using UnityEngine;
using System.Collections.Generic;
using System;

public class Job
{
	public string jobObjectType{ get; protected set; }

	public Dictionary<string, Inventory> inventoryRequirements;
	public Tile tile;

	public float jobTime{ get; protected set; }

	public bool canTakeFromStockpile = true;

	public Furniture furniturePrototype;
	public Furniture furniture;

	protected float jobTimeRequired;
	protected bool jobRepeats = false;

	Action<Job> jobCompletedCallback;
	Action<Job> jobStoppedCallback;
	Action<Job> jobWorkedCallback;

	bool acceptsAnyInventoryItem = false;

	public Job (Tile _tile, Action<Job> _jobCompleteCallback, string _jobObjectType, float _jobTime, Inventory[] _inventoryRequirements, bool _jobRepeats = false)
	{
		tile = _tile;
		jobTimeRequired = jobTime = _jobTime;
		jobCompletedCallback += _jobCompleteCallback;
		jobObjectType = _jobObjectType;
		jobRepeats = _jobRepeats;

		inventoryRequirements = new Dictionary<string, Inventory> ();

		if (_inventoryRequirements != null) {
			foreach (var inventory in _inventoryRequirements) {
				inventoryRequirements [inventory.inventoryType] = inventory.Clone ();

			}
		}

	}

	protected Job (Job other)
	{
		tile = other.tile;
		jobTimeRequired = jobTime = other.jobTime;
		jobCompletedCallback = other.jobCompletedCallback;
		jobObjectType = other.jobObjectType;
		jobRepeats = other.jobRepeats;

		inventoryRequirements = new Dictionary<string, Inventory> ();

		if (other.inventoryRequirements != null) {
			foreach (var inventory in other.inventoryRequirements.Values) {
				inventoryRequirements [inventory.inventoryType] = inventory.Clone ();
			}
		}
	}

	public void RegisterJobCompletedCallback (Action<Job> callback)
	{
		jobCompletedCallback += callback;
	}

	public void UnregisterJobCompletedCallback (Action<Job> callback)
	{
		jobCompletedCallback -= callback;
	}

	public void RegisterJobStoppedCallback (Action<Job> callback)
	{
		jobStoppedCallback += callback;
	}

	public void UnregisterJobStoppedCallback (Action<Job> callback)
	{
		jobStoppedCallback -= callback;
	}

	public void RegisterJobWorkedCallback (Action<Job> callback)
	{
		jobWorkedCallback += callback;
	}

	public void UnregisterJobWorkedCallback (Action<Job> callback)
	{
		jobWorkedCallback -= callback;
	}

	public void DoWork (float workTime)
	{
		if (!HasAllMaterials ()) {
			if (jobWorkedCallback != null) {
				jobWorkedCallback (this);
			}

			return;
		}

		jobTime -= workTime;

		if (jobWorkedCallback != null) {
			jobWorkedCallback (this);
		}

		if (jobTime <= 0) {
			if (jobCompletedCallback != null) {
				jobCompletedCallback (this);
			}

			if (!jobRepeats && jobStoppedCallback != null) {
				jobStoppedCallback (this);

			} else {
				jobTime += jobTimeRequired;
			}
		}
	}

	public void CancelJob ()
	{
		if (jobStoppedCallback != null) {
			jobStoppedCallback (this);
		}

		World.worldInstance.jobQueue.Remove (this);
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
