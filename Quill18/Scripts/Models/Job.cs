using UnityEngine;
using System.Collections.Generic;
using System;

public class Job
{
	public string jobObjectType{ get; protected set; }

	public Dictionary<string, Inventory> inventoryRequirements;
	public Tile Tile;

	public float jobTime{ get; protected set; }

	public bool canTakeFromStockpile = true;

	Action<Job> jobCompleteCallback;
	Action<Job> jobCancelCallback;
	Action<Job> jobWorkedCallback;

	bool acceptsAnyInventoryItem = false;

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

	public void registerJobWorkedCallback (Action<Job> callback)
	{
		jobWorkedCallback += callback;
	}

	public void unregisterJobWorkedCallback (Action<Job> callback)
	{
		jobWorkedCallback -= callback;
	}

	public void doWork (float workTime)
	{
		if (!HasAllMaterials ()) {
			//Debug.LogError ("doWork - tried to do a job that doesn't have all the material");
			if (jobWorkedCallback != null) {
				jobWorkedCallback (this);
			}

			return;
		}

		jobTime -= workTime;

		if (jobWorkedCallback != null) {
			jobWorkedCallback (this);
		}

		if (jobTime <= 0 && jobCompleteCallback != null) {
			jobCompleteCallback (this);
		}
	}

	public void CancelJob ()
	{
		if (jobCancelCallback != null) {
			jobCancelCallback (this);
		}

		Tile.World.jobQueue.Remove (this);
	}

	public virtual Job Clone ()
	{
		return new Job (this);
	}

	public bool HasAllMaterials ()
	{
		foreach (var inventory in inventoryRequirements.Values) {
			if (inventory.maxStackSize > inventory.stackSize) {
				return false;
			}
		}

		return true;
	}

	public int DesiresInventoryType (Inventory inventory)
	{
		if (acceptsAnyInventoryItem) {
			return inventory.maxStackSize;
		}

		if (!inventoryRequirements.ContainsKey (inventory.inventoryType)) {
			return 0;
		}

		if (inventoryRequirements [inventory.inventoryType].stackSize >= inventoryRequirements [inventory.inventoryType].maxStackSize) {
			return 0;
		}

		//Debug.Log (inventoryRequirements [inventory.inventoryType].maxStackSize - inventoryRequirements [inventory.inventoryType].stackSize);

		return inventoryRequirements [inventory.inventoryType].maxStackSize - inventoryRequirements [inventory.inventoryType].stackSize;
	}

	public Inventory GetFirstDesiredInventory ()
	{
		foreach (Inventory inventory in inventoryRequirements.Values) {
			if (inventory.maxStackSize > inventory.stackSize) {
				return inventory;
			}
		}

		return null;
	}

}
