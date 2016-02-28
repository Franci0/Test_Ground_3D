using UnityEngine;
using System.Collections.Generic;
using System;

public class World
{
	
	public int Width{ get; protected set; }

	public int Height{ get; protected set; }

	public JobQueue jobQueue;

	Tile[,] tiles;

	public Path_TileGraph tileGraph;

	Dictionary<string,Furniture> furniturePrototypes;

	Action<Furniture> furnitureCreatedCallback;
	Action<Tile> tileChangedCallback;
	Action<Character> characterCreatedCallback;

	List<Character> characters;

	public World (int width, int height)
	{
		jobQueue = new JobQueue ();

		Width = width;
		Height = height;
		tiles = new Tile[width, height];

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				tiles [x, y] = new Tile (this, x, y);
				tiles [x, y].registerTileChangedCallback (onTileChanged);
			}
		}

		instantiateFurniturePrototypes ();

		characters = new List<Character> ();
	}

	public void randomizeTiles ()
	{
		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {
				if (UnityEngine.Random.Range (0, 2) == 0) {
					tiles [x, y].Type = TileType.Empty;
				} else {
					tiles [x, y].Type = TileType.Floor;
				}
			}
		}
	}

	public Tile getTileAt (int x, int y)
	{
		if (x >= Width || y >= Height || x < 0 || y < 0) {
			return null;
		}

		return tiles [x, y];
	}

	public void placeFurniture (string furnitureType, Tile tile)
	{
		//1x1 tile
		if (!furniturePrototypes.ContainsKey (furnitureType)) {
			Debug.LogError ("FurniturePrototype doesn't contain key " + furnitureType);
			return;
		}
		//Debug.Log (furnitureType);
		Furniture furniture = Furniture.placeInstance (furniturePrototypes [furnitureType], tile);

		//Debug.Log (furnitureType + " , " + furniture.FurnitureType);

		if (furnitureCreatedCallback != null && furniture != null) {
			furnitureCreatedCallback (furniture);
		}

		invalidateTileGraph ();
	}

	public void registerFurnitureCreatedCallback (Action<Furniture> callback)
	{
		furnitureCreatedCallback += callback;
	}

	public void unregisterFurnitureCreatedCallback (Action<Furniture> callback)
	{
		furnitureCreatedCallback -= callback;
	}

	public void registerTileChangedCallback (Action<Tile> callback)
	{
		tileChangedCallback += callback;
	}

	public void unregisterTileChangedCallback (Action<Tile> callback)
	{
		tileChangedCallback -= callback;
	}

	public bool isFurniturePlacementValid (string furnitureType, Tile tile)
	{
		return furniturePrototypes [furnitureType].__isValidPosition (tile);
	}

	public Furniture getFurniturePrototype (String objectType)
	{
		if (furniturePrototypes.ContainsKey (objectType) == false) {
			Debug.LogError ("getFurniturePrototype -- furniturePrototypes doesn't contain " + objectType);
			return null;
		}
		return furniturePrototypes [objectType];
	}

	public void registerCharacterCreatedCallback (Action<Character> callback)
	{
		characterCreatedCallback += callback;
	}

	public void unregisterCharacterCreatedCallback (Action<Character> callback)
	{
		characterCreatedCallback -= callback;
	}

	public Character createCharacter (Tile tile)
	{
		Character character = new Character (tile);

		characters.Add (character);

		if (characterCreatedCallback != null) {
			characterCreatedCallback (character);
		}

		return character;
	}

	public void update (float deltaTime)
	{
		foreach (Character character in characters) {
			character.update (deltaTime);
		}
	}

	public void invalidateTileGraph ()
	{
		tileGraph = null;
	}

	void onTileChanged (Tile tile)
	{
		if (tile == null) {
			return;
		}
		tileChangedCallback (tile);

		invalidateTileGraph ();
	}

	void instantiateFurniturePrototypes ()
	{
		furniturePrototypes = new Dictionary<string, Furniture> ();
		Furniture prototype = instantiateFurniture ("Wall");//stringa temporanea, da cambiare (probabilmente da file)
		if (prototype != null) {
			furniturePrototypes.Add ("Wall", prototype);
		}
	}

	Furniture instantiateFurniture (string name)
	{
		if (name.Equals ("Wall")) {
			return Furniture.createPrototype ("Wall", 0, 1, 1, true);
		} else {
			Debug.LogError ("Prototype not found");
			return null;
		}
	}
}
