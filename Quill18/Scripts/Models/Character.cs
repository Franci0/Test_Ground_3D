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

	Tile destinationTile;
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

	public void update (float deltaTime)
	{
		update_DoJob (deltaTime);
		update_DoMovement (deltaTime);

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

	public void abbandonJob ()
	{
		nextTile = destinationTile = currentTile;
		pathAStar = null;
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

	void update_DoJob (float deltaTime)
	{
		if (myJob == null) {
			myJob = currentTile.World.jobQueue.Dequeue ();

			if (myJob != null) {
				destinationTile = myJob.Tile;
				myJob.registerJobCompleteCallback (onJobEnded);
				myJob.registerJobCancelCallback (onJobEnded);
			}
		}

		if (currentTile == destinationTile) {
			//if (pathAStar != null && pathAStar.count () == 1) {
			if (myJob != null) {
				myJob.doWork (deltaTime);
			}
		}
	}

	void update_DoMovement (float deltaTime)
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
					abbandonJob ();
					pathAStar = null;
					return;
				}
			}

			nextTile = pathAStar.Dequeue ();

			/*if (nextTile == currentTile) {
				Debug.LogError ("update_DoMovement -- Dequeue -- nextTile is currentTile");
			}*/
		}
		//Debug.Log ("CurrentTile: " + currentTile.X + " , " + currentTile.Y + "\n" + "NextTile: " + nextTile.X + " , " + nextTile.Y);

		float distanceToTravel = Mathf.Sqrt (Mathf.Pow (currentTile.X - nextTile.X, 2) + Mathf.Pow (currentTile.Y - nextTile.Y, 2));
		float distanceThisFrame = speed * deltaTime;
		float percentageThisFrame = distanceThisFrame / distanceToTravel;
		movementPercentage += percentageThisFrame;

		if (movementPercentage >= 1) {
			currentTile = nextTile;
			movementPercentage = 0;
		}
	}
}
