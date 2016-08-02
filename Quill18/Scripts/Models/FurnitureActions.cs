using UnityEngine;
using System.Collections;
using System;

public static class FurnitureActions
{
	public static void Door_UpdateAction (Furniture furniture, float deltaTime)
	{
		//Debug.Log ("Door_UpdateAction: " + furniture.furnitureParameters ["openess"]);

		if (furniture.GetParameter (World.is_opening) >= 1) {
			furniture.ChangeParameter (World.openness, deltaTime * 4);

			if (furniture.GetParameter (World.openness) >= 1) {
				furniture.SetParameter (World.is_opening, 0);
			}
		} else {
			if (furniture.GetParameter (World.openness) > 0) {
				furniture.ChangeParameter (World.openness, -(deltaTime * 4));
			}
		}

		furniture.SetParameter (World.openness, Mathf.Clamp01 (furniture.GetParameter (World.openness)));

		if (furniture.onChangedCallback != null) {
			furniture.onChangedCallback (furniture);
		}
	}

	public static Accessiblity Door_IsAccessible (Furniture furniture)
	{
		//Debug.Log ("Door_IsAccessible: " + furniture.furnitureParameters ["is_opening"]);
		furniture.SetParameter (World.is_opening, 1);

		if (furniture.GetParameter (World.openness) >= 1) {
			return Accessiblity.Yes;
		} else {
			return Accessiblity.Soon;
		}
	}

	public static void JobCompleteFurnitureBuilding (Job job)
	{
		WorldController.Instance.world.PlaceFurniture (job.jobObjectType, job.tile);
		job.tile.pendingFurnitureJob = null;
	}

	public static void Stockpile_UpdateAction (Furniture furniture, float deltaTime)
	{
		Inventory furnitureInventory = furniture.tile.inventory;

		if (furnitureInventory != null && furnitureInventory.stackSize >= furnitureInventory.maxStackSize) {
			furniture.ClearJobs ();
			return;
		}

		if (furniture.JobCount () > 0) {
			return;
		}

		if (furnitureInventory != null && furnitureInventory.stackSize == 0) {
			Debug.LogError ("Stockpile_UpdateAction - has a zero sized stack");
			furniture.ClearJobs ();
			return;
		}

		Inventory[] itemsDesired;

		if (furnitureInventory == null) {
			itemsDesired = Stockpile_GetItemsFromFilter ();

		} else {
			Inventory desired = furnitureInventory.Clone ();
			desired.maxStackSize -= desired.stackSize;
			desired.stackSize = 0;
			itemsDesired = new Inventory[]{ desired };
		}

		Job job = new Job (
			          furniture.tile,
			          null,
			          null,
			          0f,
			          itemsDesired
		          );

		job.canTakeFromStockpile = false;
		job.registerJobWorkedCallback (Stockpile_JobWorked);
		furniture.AddJob (job);
	}

	static void Stockpile_JobWorked (Job job)
	{
		job.tile.furniture.RemoveJob (job);

		foreach (Inventory inventory in job.inventoryRequirements.Values) {
			if (inventory.stackSize > 0) {
				job.tile.World.inventoryManager.PlaceInventory (job.tile, inventory);
				return;
			}
		}
	}

	public static Inventory[] Stockpile_GetItemsFromFilter ()
	{
		return new Inventory[] { new Inventory ("Steel Plate", 50, 0) };
	}
}
