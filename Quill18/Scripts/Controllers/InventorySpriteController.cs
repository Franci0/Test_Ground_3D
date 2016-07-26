using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class InventorySpriteController : MonoBehaviour
{
	public GameObject invetoryUIPrefab;

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

		foreach (string inventoryType in world.inventoryManager.inventories.Keys) {
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

		if (inventory.maxStackSize > 1 && inventory.stackSize > 1) {
			GameObject ui_go = Instantiate (invetoryUIPrefab);
			ui_go.transform.SetParent (inventory_go.transform);
			ui_go.transform.localPosition = Vector3.zero;
			ui_go.GetComponentInChildren<Text> ().text = inventory.stackSize.ToString ();
		}

		inventory.registerInventoryChangedCallback (OnInventoryChanged);

	}

	void OnInventoryChanged (Inventory inventory)
	{
		if (inventoryGameObjectMap.ContainsKey (inventory) == false) {
			Debug.LogError ("OnInventoryChanged -- inventoryGameObjectMap doesn't contain key: " + inventory);
			return;
		}

		GameObject inventory_go = inventoryGameObjectMap [inventory];
		Text text = inventory_go.GetComponentInChildren<Text> ();

		if (text != null) {
			text.text = inventory.stackSize.ToString ();
		}
	}

}
