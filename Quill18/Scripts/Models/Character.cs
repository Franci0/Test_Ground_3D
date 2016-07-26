using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Character : IXmlSerializable
{
	public float X { 
		get { 
			return Mathf.Lerp (currentTile.X, nextTile.X, movementPercentage); 
		} 
	}

	public float Y { 
		get { 
			return Mathf.Lerp (currentTile.Y, nextTile.Y, movementPercentage); 
		} 
	}

	public Tile currentTile{ get; protected set; }

	public Inventory inventory;

	Tile _destinationTile;

	Tile destinationTile {
		get { 
			return _destinationTile;
		}
		set {
			if (_destinationTile != value) {
				_destinationTile = value;
				pathAStar = null;
			}
		}
	}

	Tile nextTile;
	Path_AStar pathAStar;
	float movementPercentage;
	Job myJob;
	float speed = 4f;
	Action<Character> characterChangedCallback;

	public Character ()
	{
		
	}

	public Character (Tile tile)
	{
		currentTile = destinationTile = nextTile = tile;
	}

	public void Update (float deltaTime)
	{
		UpdateDoJob (deltaTime);
		UpdateDoMovement (deltaTime);

		if (characterChangedCallback != null) {
			characterChangedCallback (this);
		}
	}

	public void setDestination (Tile tile)
	{	
		if (currentTile.isNeighbour (tile) == false) {
			Debug.Log ("Character::setDestination -- the destination tile isn't a neighbour");
		}
		destinationTile = tile;
	}

	public void registerOnChangedCallback (Action<Character> callback)
	{
		characterChangedCallback += callback;
	}

	public void unregisterOnChangedCallback (Action<Character> callback)
	{
		characterChangedCallback -= callback;
	}


	public void onJobEnded (Job job)
	{
		if (job != myJob) {
			Debug.LogError ("onJobEnded -- different job to end than what's his current job");
			return;
		}

		myJob = null;

	}

	public void AbbandonJob ()
	{
		nextTile = destinationTile = currentTile;
		currentTile.World.jobQueue.Enqueue (myJob);
		myJob = null;
	}

	public XmlSchema GetSchema ()
	{
		return null;
	}

	public void WriteXml (XmlWriter writer)
	{
		writer.WriteAttributeString ("X", currentTile.X.ToString ());
		writer.WriteAttributeString ("Y", currentTile.Y.ToString ());
	}

	public void ReadXml (XmlReader reader)
	{
		
	}

	void UpdateDoJob (float deltaTime)
	{
		if (myJob == null) {
			GetNewJob ();
		}

		if (myJob != null) {
			if (!myJob.HasAllMaterials ()) {
				if (inventory != null) {
					if (myJob.DesiresInventoryType (inventory) > 0) {
						if (currentTile == myJob.Tile) {
							currentTile.World.inventoryManager.PlaceInventory (myJob, inventory);

							if (inventory.stackSize == 0) {
								inventory = null;

							} else {
								Debug.LogError ("UpdateDoJob - character is still carrying inventory, which shouldn't be. Just setting to Null now");
								inventory = null;
							}

						} else {
							destinationTile = myJob.Tile;
						}

						return;

					} else {
						if (!currentTile.World.inventoryManager.PlaceInventory (currentTile, inventory)) {
							Debug.LogError ("UpdateDoJob - character tried to dump inventory to invalid tile");
							inventory = null;
						}
					}

				} else {
					if (currentTile.inventory != null && myJob.DesiresInventoryType (currentTile.inventory) > 0) {
						currentTile.World.inventoryManager.PlaceInventory (this, currentTile.inventory, myJob.DesiresInventoryType (currentTile.inventory));

					} else {
						Inventory desired = myJob.GetFirstDesiredInventory ();

						if (desired != null) {
							Inventory supplier = currentTile.World.inventoryManager.GetClosestInventoryOfType (
								                     desired.inventoryType, 
								                     currentTile, 
								                     desired.maxStackSize - desired.stackSize);

							if (supplier == null) {
								Debug.Log ("UpdateDoJob - No tile contains inventory of type " + desired.inventoryType + " to satisfy job requirements");
								AbbandonJob ();
								return;
							}

							destinationTile = supplier.tile;
						}
					}
				}

				return;
			}

			destinationTile = myJob.Tile;

			if (/*myJob != null &&*/ currentTile == myJob.Tile) {
				myJob.doWork (deltaTime);
			}
		}
	}

	void UpdateDoMovement (float deltaTime)
	{
		if (currentTile == destinationTile) {
			pathAStar = null;
			return;
		}

		if (nextTile == null || nextTile == currentTile) {
			if (pathAStar == null || pathAStar.count () == 0) {
				pathAStar = new Path_AStar (currentTile.World, currentTile, destinationTile);

				if (pathAStar.count () == 0) {
					Debug.LogError ("Path_AStar -- returned (0) no path to destinationTile");
					AbbandonJob ();
					return;
				}

				pathAStar.Dequeue ();

			}

			nextTile = pathAStar.Dequeue ();

			/*if (nextTile == currentTile) {
				Debug.LogError ("update_DoMovement -- Dequeue -- nextTile is currentTile");
			}*/
		}
		//Debug.Log ("CurrentTile: " + currentTile.X + " , " + currentTile.Y + "\n" + "NextTile: " + nextTile.X + " , " + nextTile.Y);

		float distanceToTravel = Mathf.Sqrt (Mathf.Pow (currentTile.X - nextTile.X, 2) + Mathf.Pow (currentTile.Y - nextTile.Y, 2));

		if (nextTile.isAccessible () == Accessiblity.Never) {
			Debug.LogError ("Charcter tried to enter in unwalkable tile");
			nextTile = null;
			pathAStar = null;
			return;
		} else if (nextTile.isAccessible () == Accessiblity.Soon) {
			return;
		}

		float distanceThisFrame = speed / nextTile.movementCost * deltaTime;
		float percentageThisFrame = distanceThisFrame / distanceToTravel;
		movementPercentage += percentageThisFrame;

		if (movementPercentage >= 1) {
			currentTile = nextTile;
			movementPercentage = 0;
		}
	}

	void GetNewJob ()
	{
		myJob = currentTile.World.jobQueue.Dequeue ();

		if (myJob == null) {
			destinationTile = currentTile;
			return;
		}

		destinationTile = myJob.Tile;
		myJob.registerJobCompleteCallback (onJobEnded);
		myJob.registerJobCancelCallback (onJobEnded);
		pathAStar = new Path_AStar (currentTile.World, currentTile, destinationTile);

		if (pathAStar.count () == 0) {
			Debug.LogError ("Path_AStar -- returned (0) no path to target job tile");
			AbbandonJob ();
			destinationTile = currentTile;
		}
	}
}
