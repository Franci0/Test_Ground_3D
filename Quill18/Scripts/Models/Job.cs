using UnityEngine;
using System.Collections.Generic;
using System;

public class Job
{
	public Tile Tile;

	float jobTime;

	Action<Job> jobCompleteCallback;
	Action<Job> jobCancelCallback;

	public string jobObjectType{ get; protected set; }

	Dictionary<string, Inventory> inventoryRequirements;

	public Job (Tile _tile, Action<Job> _jobCompleteCallback, string _jobObjectType, float _jobTime, Inventory[] _inventoryRequirements)
	{
		Tile = _tile;
		jobTime = _jobTime;
		jobCompleteCallback += _jobCompleteCallback;
		jobObjectType = _jobObjectType;

		inventoryRequirements = new Dictionary<string, Inventory> ();

		if (_inventoryRequirements != null) {
			foreach (var inventory in _inventoryRequirements) {
				inventoryRequirements [inventory.inventoryType] = inventory.Clone ();
			}
		}

	}

	protected Job (Job other)
	{
		Tile = other.Tile;
		jobTime = other.jobTime;
		jobCompleteCallback = other.jobCompleteCallback;
		jobObjectType = other.jobObjectType;

		inventoryRequirements = new Dictionary<string, Inventory> ();

		if (other.inventoryRequirements != null) {
			foreach (var inventory in other.inventoryRequirements.Values) {
				inventoryRequirements [inventory.inventoryType] = inventory.Clone ();
			}
		}
	}

	public void registerJobCompleteCallback (Action<Job> callback)
	{
		jobCompleteCallback += callback;
	}

	public void unregisterJobCompleteCallback (Action<Job> callback)
	{
		jobCompleteCallback -= callback;
	}

	public void registerJobCancelCallback (Action<Job> callback)
	{
		jobCancelCallback += callback;
	}

	public void unregisterJobCancelCallback (Action<Job> callback)
	{
		jobCancelCallback -= callback;
	}

	public void doWork (float workTime)
	{
		//Debug.Log (jobTime + " , " + workTime);
		jobTime -= workTime;

		if (jobTime <= 0 && jobCompleteCallback != null) {
			jobCompleteCallback (this);
		}
	}

	public void cancelJob ()
	{
		if (jobCancelCallback != null) {
			jobCancelCallback (this);
		}
	}

	public virtual Job Clone ()
	{
		return new Job (this);
	}
}
