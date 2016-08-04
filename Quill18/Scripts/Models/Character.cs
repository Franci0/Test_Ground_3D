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


	public void OnJobEnded (Job job)
	{
		job.unregisterJobCancelCallback (OnJobEnded);
		job.unregisterJobCompleteCallback (OnJobEnded);

		if (job != myJob) {
			Debug.LogError ("onJobEnded -- different job to end than what's his current job");
			return;
		}

		myJob = null;

	}

	public void AbbandonJob ()
	{
		nextTile = destinationTile = currentTile;
		currentTile.world.jobQueue.Enqueue (myJob);
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
						if (currentTile == myJob.tile) {
							currentTile.world.inventoryManager.PlaceInventory (myJob, inventory);
							myJob.doWork (0f);

							if (inventory.stackSize == 0) {
								inventory = null;

							} else {
								Debug.LogError ("UpdateDoJob - character is still carrying inventory, which shouldn't be. Just setting to Null now");
								inventory = null;
							}

						} else {
							destinationTile = myJob.tile;
						}

						return;

					} else {
						if (!currentTile.world.inventoryManager.PlaceInventory (currentTile, inventory)) {
							Debug.LogError ("UpdateDoJob - character tried to dump inventory to invalid tile");
							inventory = null;
						}
					}

				} else {
					if (currentTile.inventory != null &&
					    (myJob.canTakeFromStockpile || currentTile.furniture == null || !currentTile.furniture.IsStockpile ()) &&
					    myJob.DesiresInventoryType (currentTile.inventory) > 0) {

						currentTile.world.inventoryManager.PlaceInventory (
							this, 
							currentTile.inventory, 
							myJob.DesiresInventoryType (currentTile.inventory)
						);

					} else {
						Inventory desired = myJob.GetFirstDesiredInventory ();

						if (desired != null) {
							Inventory supplier = currentTile.world.inventoryManager.GetClosestInventoryOfType (
								                     desired.inventoryType, 
								                     currentTile, 
								                     desired.maxStackSize - desired.stackSize,
								                     myJob.canTakeFromStockpile);

							if (supplier == null) {
								//Debug.Log ("UpdateDoJob - No tile contains inventory of type " + desired.inventoryType + " to satisfy job requirements");
								AbbandonJob ();
								return;
							}

							destinationTile = supplier.tile;
						}
					}
				}

				return;
			}

			destinationTile = myJob.tile;

			if (/*myJob != null &&*/ currentTile == myJob.tile) {
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
				pathAStar = new Path_AStar (currentTile.world, currentTile, destinationTile);

				if (pathAStar.count () == 0) {
					Debug.LogError ("Path_AStar -- returned (0) no path to destinationTile");
					AbbandonJob ();
					return;
				}

				pathAStar.Dequeue ();

			}

			nextTile = pathAStar.Dequeue ();

			/*if (nextTile == destinationTile) {
				nextTile = currentTile;
				destinationTile = currentTile;
			}*/
		}

		float distanceToTravel = Mathf.Sqrt (Mathf.Pow (currentTile.X - nextTile.X, 2) + Mathf.Pow (currentTile.Y - nextTile.Y, 2));

		if (nextTile.isAccessible () == Accessiblity.NEVER) {
			Debug.LogError ("Charcter tried to enter in unwalkable tile");
			nextTile = null;
			pathAStar = null;
			return;
		} else if (nextTile.isAccessible () == Accessiblity.SOON) {
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
		myJob = currentTile.world.jobQueue.Dequeue ();

		if (myJob == null) {
			destinationTile = currentTile;
			return;
		}

		destinationTile = myJob.tile;
		myJob.registerJobCompleteCallback (OnJobEnded);
		myJob.registerJobCancelCallback (OnJobEnded);
		pathAStar = new Path_AStar (currentTile.world, currentTile, destinationTile);

		if (pathAStar.count () == 0) {
			Debug.LogError ("Path_AStar -- returned (0) no path to target job tile");
			AbbandonJob ();
			destinationTile = currentTile;
		}
	}
}
