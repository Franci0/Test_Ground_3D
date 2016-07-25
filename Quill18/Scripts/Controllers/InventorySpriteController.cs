using UnityEngine;
using System.Collections.Generic;
using System;

public class InventorySpriteController : MonoBehaviour
{
	Dictionary<Inventory,GameObject> inventoryGameObjectMap;
	Dictionary<string,Sprite> inventorySprites;

	World world {
		get{ return WorldController.Instance.world; }
	}

	void Start ()
	{
		loadSprites ();

		inventoryGameObjectMap = new Dictionary<Inventory, GameObject> ();

		world.registerInventoryCreatedCallback (OnInventoryCreated);

		foreach (String inventoryType in world.inventoryManager.inventories.Keys) {
			foreach (Inventory inventory in world.inventoryManager.inventories[inventoryType]) {
				OnInventoryCreated (inventory);
			}
		}
	}

	void loadSprites ()
	{
		inventorySprites = new Dictionary<string, Sprite> ();

		Sprite[] sprites = Resources.LoadAll<Sprite> ("Sprites/Inventory/");

		foreach (Sprite s in sprites) {
			inventorySprites [s.name] = s;
		}
	}

	void OnInventoryCreated (Inventory inventory)
	{
		GameObject inventory_go = new GameObject ();

		inventoryGameObjectMap.Add (inventory, inventory_go);

		inventory_go.name = inventory.inventoryType;
		inventory_go.transform.position = new Vector3 (inventory.tile.X, inventory.tile.Y, 0);
		inventory_go.transform.SetParent (this.transform, true);

		SpriteRenderer sr = inventory_go.AddComponent<SpriteRenderer> ();
		sr.sprite = inventorySprites [inventory.inventoryType];
		sr.sortingLayerName = "Inventory";


		//inventory.registerOnChangedCallback (OnInvetoryChanged);

	}

	void OnInventoryChanged (Inventory inventory)
	{
		if (inventoryGameObjectMap.ContainsKey (inventory) == false) {
			Debug.LogError ("OnInventoryChanged -- inventoryGameObjectMap doesn't contain key: " + inventory);
			return;
		}

		GameObject inventory_go = inventoryGameObjectMap [inventory];

		inventory_go.transform.position = new Vector3 (inventory.tile.X, inventory.tile.Y, 0);
	}

}
