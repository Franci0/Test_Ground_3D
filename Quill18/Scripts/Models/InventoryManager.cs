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

		/*inventories.Add (inventory);
		return true;*/
		if (inventory.stackSize == 0 && inventories.ContainsKey (tile.inventory.inventoryType)) {
			inventories [inventory.inventoryType].Remove (inventory);
		}

		if (tileWasEmpty) {
			if (!inventories.ContainsKey (tile.inventory.inventoryType)) {
				inventories [tile.inventory.inventoryType] = new List<Inventory> ();
			}

			inventories [tile.inventory.inventoryType].Add (tile.inventory);
		}

		return true;
	}

}
