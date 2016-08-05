using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager
{
	public Dictionary<String,List<Inventory>> inventories;

	public InventoryManager ()
	{
		inventories = new Dictionary<String,List<Inventory>> ();
	}

	public bool PlaceInventory (Tile tile, Inventory inventory)
	{
		bool tileWasEmpty = tile.inventory == null;

		if (!tile.PlaceInventory (inventory)) {
			return false;
		}
		
		CleanupInventory (inventory);

		if (tileWasEmpty) {
			if (!inventories.ContainsKey (tile.inventory.inventoryType)) {
				inventories [tile.inventory.inventoryType] = new List<Inventory> ();
			}

			inventories [tile.inventory.inventoryType].Add (tile.inventory);
			World.worldInstance.OnInventoryCreated (tile.inventory);
		}

		return true;
	}

	public bool PlaceInventory (Job job, Inventory inventory)
	{
		if (!job.inventoryRequirements.ContainsKey (inventory.inventoryType)) {
			Debug.LogError ("PlaceInventory - trying to add inventory to a job that is doesn't want");
			return false;
		}

		job.inventoryRequirements [inventory.inventoryType].stackSize += inventory.stackSize;

		if (job.inventoryRequirements [inventory.inventoryType].maxStackSize < job.inventoryRequirements [inventory.inventoryType].stackSize) {
			inventory.stackSize = job.inventoryRequirements [inventory.inventoryType].stackSize - job.inventoryRequirements [inventory.inventoryType].maxStackSize;
			job.inventoryRequirements [inventory.inventoryType].stackSize = job.inventoryRequirements [inventory.inventoryType].maxStackSize;
		} else {
			inventory.stackSize = 0;
		}

		CleanupInventory (inventory);

		return true;
	}

	public Inventory GetClosestInventoryOfType (string inventoryType, Tile tile, int desiredAmount, bool canTakeFromStockpile)
	{
		if (!inventories.ContainsKey (inventoryType)) {
			Debug.LogError ("GetClosestInventoryOfType - No items of desired type");
			return null;
		}

		foreach (Inventory inventory in inventories[inventoryType]) {
			if (inventory.tile != null && (canTakeFromStockpile || inventory.tile.furniture == null || !inventory.tile.furniture.IsStockpile ())) {
				return inventory;
			}
		}

		return null;
	}

	public bool PlaceInventory (Character character, Inventory sourceInventory, int amount = -1)
	{
		if (amount < 0) {
			amount = sourceInventory.stackSize;
		} else {
			amount = Mathf.Min (amount, sourceInventory.stackSize);
		}

		if (character.inventory == null) {
			character.inventory = sourceInventory.Clone ();
			character.inventory.stackSize = 0;
			inventories [character.inventory.inventoryType].Add (character.inventory);

		} else if (character.inventory.inventoryType != sourceInventory.inventoryType) {
			Debug.LogError ("PlaceInventory - character is trying to pick up a missmatched inventory type");
			return false;
		}

		character.inventory.stackSize += amount;

		if (character.inventory.maxStackSize < character.inventory.stackSize) {
			sourceInventory.stackSize = character.inventory.stackSize - character.inventory.maxStackSize;
			character.inventory.stackSize = character.inventory.maxStackSize;

		} else {
			sourceInventory.stackSize -= amount;
		}

		CleanupInventory (sourceInventory);

		return true;
	}

	void CleanupInventory (Inventory inventory)
	{
		if (inventory.stackSize == 0) {
			if (inventories.ContainsKey (inventory.inventoryType)) {
				inventories [inventory.inventoryType].Remove (inventory);
			}

			if (inventory.tile != null) {
				inventory.tile.inventory = null;
				inventory.tile = null;
			}

			if (inventory.character != null) {
				inventory.character.inventory = null;
				inventory.character = null;
			}
		}
	}

}
