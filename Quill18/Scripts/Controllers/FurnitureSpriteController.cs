using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

public class FurnitureSpriteController : MonoBehaviour
{

	Dictionary<Furniture,GameObject> furnitureGameObjectMap;
	Dictionary<string,Sprite> furnitureSprites;

	World world {
		get{ return WorldController.Instance.world; }
	}

	public Sprite GetSpriteForFurniture (Furniture furniture)
	{
		//Debug.Log ("getSpriteForFurniture");
		string spriteName = furniture.furnitureType;

		if (furniture.linksToNeighboors == false) {

			if (furniture.furnitureType == "Door") {
				if (furniture.GetParameter (World.openness) < 0.1f) {
					spriteName = "Door";
				} else if (furniture.GetParameter (World.openness) < 0.5f) {
					spriteName = "Door_openness_1";
				} else if (furniture.GetParameter (World.openness) < 0.9f) {
					spriteName = "Door_openness_2";
				} else {
					spriteName = "Door_openness_3";
				}
			}

			if (spriteName != "Door") {
				//Debug.Log (spriteName);
			}

			return furnitureSprites [spriteName];
		}

		if (spriteName == "Wall") {
			spriteName += "s0";
		}

		spriteName += "_";

		int x = furniture.tile.X;
		int y = furniture.tile.Y;

		bool exit = false;

		Tile t;

		for (int i = -1; i <= 1; i = i + 2) {
			for (int j = -1; j <= 1; j = j + 2) {
				t = world.getTileAt (x + i, y + j);
				if (t != null && t.furniture != null && t.furniture.furnitureType == furniture.furnitureType) {

					t = world.getTileAt (x + i, y);
					if (t != null && t.furniture != null && t.furniture.furnitureType == furniture.furnitureType) {
						t = world.getTileAt (x, y + j);
						if (t != null && t.furniture != null && t.furniture.furnitureType == furniture.furnitureType) {
							spriteName += "Full_";
							exit = true;
							break;
						}
					}
				}
			}
			if (exit == true) {
				break;
			}
		}

		t = world.getTileAt (x - 1, y);
		if (t != null && t.furniture != null && t.furniture.furnitureType == furniture.furnitureType) {
			spriteName += "W";
		}
		t = world.getTileAt (x + 1, y);
		if (t != null && t.furniture != null && t.furniture.furnitureType == furniture.furnitureType) {
			spriteName += "E";
		}
		t = world.getTileAt (x, y + 1);
		if (t != null && t.furniture != null && t.furniture.furnitureType == furniture.furnitureType) {
			spriteName += "N";
		}
		t = world.getTileAt (x, y - 1);
		if (t != null && t.furniture != null && t.furniture.furnitureType == furniture.furnitureType) {
			spriteName += "S";
		}

		//Debug.Log (spriteName);
		if (furnitureSprites.ContainsKey (spriteName) == false) {
			Debug.LogError ("getSpriteForFurniture -- No sprite with name: " + spriteName);
			return null;
		}
		//Debug.Log (furnitureSprites [spriteName].name);

		return furnitureSprites [spriteName];
	}

	public Sprite GetSpriteForFurniture (string objectType)
	{
		//Debug.Log ("getSpriteForFurniture");
		if (furnitureSprites.ContainsKey (objectType) == true) {
			return furnitureSprites [objectType];
		}

		if (furnitureSprites.ContainsKey (objectType + "s0_") == true) {
			return furnitureSprites [objectType + "s0_"];
		}

		if (furnitureSprites.ContainsKey (objectType + "_")) {
			return furnitureSprites [objectType + "_"];
		}

		Debug.LogError ("getSpriteForFurniture -- No sprite with name: " + objectType);
		return null;
	}

	void Start ()
	{
		LoadSprites ();

		furnitureGameObjectMap = new Dictionary<Furniture, GameObject> ();

		world.RegisterFurnitureCreatedCallback (OnFurnitureCreated);

		foreach (Furniture furniture in world.furnitures) {
			OnFurnitureCreated (furniture);
		}
	}

	void OnFurnitureCreated (Furniture furniture)
	{
		//only 1x1 object
		//Debug.Log ("OnFurnitureCreated");
		//Debug.Log (furniture.furnitureType);

		GameObject furniture_go = new GameObject ();

		furnitureGameObjectMap.Add (furniture, furniture_go);

		furniture_go.name = furniture.furnitureType + "_" + furniture.tile.X + "_" + furniture.tile.Y;
		furniture_go.transform.position = new Vector3 (furniture.tile.X + ((furniture.width - 1) / 2f), furniture.tile.Y + ((furniture.height - 1) / 2f), 0);
		furniture_go.transform.SetParent (this.transform, true);

		if (furniture.furnitureType == "Door") {
			Tile westTile = world.getTileAt (furniture.tile.X - 1, furniture.tile.Y);
			Tile eastTile = world.getTileAt (furniture.tile.X + 1, furniture.tile.Y);

			if (westTile != null && eastTile != null && westTile.furniture != null && eastTile.furniture != null && westTile.furniture.furnitureType == "Wall" && eastTile.furniture.furnitureType == "Wall") {
				furniture_go.transform.rotation = Quaternion.Euler (0, 0, 90);
			}
		}

		SpriteRenderer sr = furniture_go.AddComponent<SpriteRenderer> ();
		sr.sprite = GetSpriteForFurniture (furniture);
		sr.sortingLayerName = "Furnitures";
		sr.color = furniture.tint;

		//Debug.Log (furniture_go.GetComponent<SpriteRenderer> ().sprite);

		furniture.RegisterOnChangedCallback (OnFurnitureChanged);
		furniture.RegisterOnRemovedCallback (OnFurnitureRemoved);

	}

	void OnFurnitureChanged (Furniture furniture)
	{
		//Debug.LogError ("OnFurnitureChanged not implemented yet");
		//Debug.Log ("OnFurnitureChanged");
		if (furnitureGameObjectMap.ContainsKey (furniture) == false) {
			Debug.LogError ("OnFurnitureChanged -- furnitureGameObjectMap doesn't contain key: " + furniture);
			return;
		}

		GameObject furniture_go = furnitureGameObjectMap [furniture];

		//Debug.Log (furniture_go);

		//Debug.Log (furniture_go.GetComponent<SpriteRenderer> ());

		SpriteRenderer sr = furniture_go.GetComponent<SpriteRenderer> ();

		sr.sprite = GetSpriteForFurniture (furniture);
		sr.sortingLayerName = "Furnitures";
		sr.color = furniture.tint;

	}

	void LoadSprites ()
	{
		furnitureSprites = new Dictionary<string, Sprite> ();

		Sprite[] sprites = Resources.LoadAll<Sprite> ("Sprites/Furniture");

		foreach (Sprite sprite in sprites) {
			//Debug.Log (sprite.name);
			furnitureSprites [sprite.name] = sprite;
		}

	}

	void OnFurnitureRemoved (Furniture furniture)
	{
		if (furnitureGameObjectMap.ContainsKey (furniture) == false) {
			Debug.LogError ("OnFurnitureRemoved -- furnitureGameObjectMap doesn't contain key: " + furniture);
			return;
		}

		GameObject furniture_go = furnitureGameObjectMap [furniture];
		Destroy (furniture_go);
		furnitureGameObjectMap.Remove (furniture);
	}

}
