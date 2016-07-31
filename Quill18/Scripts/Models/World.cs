﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine.Rendering;

public class World : IXmlSerializable
{

	public const String openness = "openness";
	public const String is_opening = "is_opening";

	public int Width{ get; protected set; }

	public int Height{ get; protected set; }


	public Path_TileGraph tileGraph;

	public JobQueue jobQueue;
	public InventoryManager inventoryManager;

	public List<Furniture> furnitures;
	public List<Character> characters;
	public List<Room> rooms;

	public Dictionary<string,Job> furnitureJobPrototypes;

	Dictionary<string,Furniture> furniturePrototypes;

	Action<Furniture> furnitureCreatedCallback;
	Action<Tile> tileChangedCallback;
	Action<Character> characterCreatedCallback;
	Action<Inventory> inventoryCreatedCallback;

	Tile[,] tiles;

	public World ()
	{
		
	}

	public World (int width, int height)
	{
		SetupWorld (width, height);
		CreateCharacter (getTileAt (width / 2, height / 2));
	}

	public XmlSchema GetSchema ()
	{
		return null;
	}

	public void WriteXml (XmlWriter writer)
	{
		writer.WriteAttributeString ("Width", Width.ToString ());
		writer.WriteAttributeString ("Height", Height.ToString ());

		writer.WriteStartElement ("Tiles");
		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {
				if (tiles [x, y].Type != TileType.Empty) {
					writer.WriteStartElement ("Tile");
					tiles [x, y].WriteXml (writer);
					writer.WriteEndElement ();
				}
			}
		}
		writer.WriteFullEndElement ();

		writer.WriteStartElement ("Furnitures");
		foreach (Furniture furniture in furnitures) {
			writer.WriteStartElement ("Furniture");
			furniture.WriteXml (writer);
			writer.WriteEndElement ();
		}
		writer.WriteFullEndElement ();

		writer.WriteStartElement ("Characters");
		foreach (Character character in characters) {
			writer.WriteStartElement ("Character");
			character.WriteXml (writer);
			writer.WriteEndElement ();
		}
		writer.WriteFullEndElement ();
	}

	public void ReadXml (XmlReader reader)
	{
		Width = int.Parse (reader.GetAttribute ("Width"));
		Height = int.Parse (reader.GetAttribute ("Height"));

		SetupWorld (Width, Height);

		while (reader.Read ()) {
			switch (reader.Name) {
			case "Tiles":
				ReadXml_Tiles (reader);
				break;
			case "Furnitures":
				ReadXml_Furnitures (reader);
				break;
			case "Characters":
				ReadXml_Characters (reader);
				break;
			}
		}

		//Debug Inventory - to remove
		Inventory inventory = new Inventory ("Steel Plate", 50, 20);
		Tile inventoryTile = getTileAt (Width / 2, Height / 2);
		inventoryManager.PlaceInventory (inventoryTile, inventory);

		if (inventoryCreatedCallback != null) {
			inventoryCreatedCallback (inventoryTile.inventory);
		}

		inventory = new Inventory ("Steel Plate", 50, 3);
		inventoryTile = getTileAt (Width / 2 + 2, Height / 2 + 2);
		inventoryManager.PlaceInventory (inventoryTile, inventory);

		if (inventoryCreatedCallback != null) {
			inventoryCreatedCallback (inventoryTile.inventory);
		}

		inventory = new Inventory ("Steel Plate", 50, 4);
		inventoryTile = getTileAt (Width / 2 + 1, Height / 2 + 2);
		inventoryManager.PlaceInventory (inventoryTile, inventory);

		if (inventoryCreatedCallback != null) {
			inventoryCreatedCallback (inventoryTile.inventory);
		}

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

	public Furniture PlaceFurniture (string furnitureType, Tile tile)
	{
		//1x1 tile
		if (!furniturePrototypes.ContainsKey (furnitureType)) {
			Debug.LogError ("FurniturePrototype doesn't contain key " + furnitureType);
			return null;
		}
		//Debug.Log (furnitureType);
		Furniture furniture = Furniture.PlaceInstance (furniturePrototypes [furnitureType], tile);

		if (furniture == null) {
			return null;
		}

		furnitures.Add (furniture);

		if (furniture.roomEnclosure) {
			Room.DoRoomFloodFill (furniture);
		}

		if (furnitureCreatedCallback != null) {
			furnitureCreatedCallback (furniture);

			if (furniture.movementCostMultiplier != 1) {
				invalidateTileGraph ();
			}
		}

		return furniture;
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
		return furniturePrototypes [furnitureType].DEFAULT__isValidPosition (tile);
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

	public Character CreateCharacter (Tile tile)
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
			character.Update (deltaTime);
		}

		foreach (Furniture furniture in furnitures) {
			furniture.Update (deltaTime);
		}
	}

	public void invalidateTileGraph ()
	{
		tileGraph = null;
	}

	public Room GetOutsideRoom ()
	{
		return rooms [0];
	}

	public void DeleteRoom (Room room)
	{
		if (room == GetOutsideRoom ()) {
			Debug.LogError ("Tried to delete OutsideRoom");
			return;
		}

		rooms.Remove (room);
		room.UnAssignAllTiles ();
	}

	public void AddRoom (Room room)
	{
		rooms.Add (room);
	}

	public void registerInventoryCreatedCallback (Action<Inventory> callback)
	{
		inventoryCreatedCallback += callback;
	}

	public void unregisterInventoryCreatedCallback (Action<Inventory> callback)
	{
		inventoryCreatedCallback -= callback;
	}

	void onTileChanged (Tile tile)
	{
		if (tileChangedCallback == null) {
			return;
		}
		tileChangedCallback (tile);

		invalidateTileGraph ();
	}

	public void OnInventoryCreated (Inventory inventory)
	{
		if (inventoryCreatedCallback != null) {
			inventoryCreatedCallback (inventory);
		}
	}

	void CreateFurniturePrototypes ()
	{
		furniturePrototypes = new Dictionary<string, Furniture> ();
		furnitureJobPrototypes = new Dictionary<string, Job> ();

		furniturePrototypes.Add (
			"Wall", 
			new Furniture (
				"Wall", 
				0, 
				1, 
				1, 
				true, 
				true
			)
		);

		furnitureJobPrototypes.Add (
			"Wall", 
			new Job (
				null, 
				FurnitureActions.JobCompleteFurnitureBuilding, 
				"Wall", 
				1f, 
				new Inventory[] { 
					new Inventory (
						"Steel Plate", 
						5, 
						0
					)
				}
			)
		);

		furniturePrototypes.Add (
			"Door", 
			new Furniture (
				"Door", 
				1.1f, 
				1, 
				1, 
				false, 
				true
			)
		);

		furniturePrototypes ["Door"].SetParameter (openness, 0);
		furniturePrototypes ["Door"].SetParameter (is_opening, 0);
		furniturePrototypes ["Door"].RegisterUpdateAction (FurnitureActions.Door_UpdateAction);
		furniturePrototypes ["Door"].isAccessible = FurnitureActions.Door_IsAccessible;

		furniturePrototypes.Add (
			"Stockpile", 
			new Furniture (
				"Stockpile", 
				1, 
				1, 
				1, 
				true, 
				false
			)
		);

		furnitureJobPrototypes.Add (
			"Stockpile", 
			new Job (
				null, 
				FurnitureActions.JobCompleteFurnitureBuilding, 
				"Stockpile", 
				-1, 
				null
			)
		);

		furniturePrototypes ["Stockpile"].RegisterUpdateAction (FurnitureActions.Stockpile_UpdateAction);
		furniturePrototypes ["Stockpile"].tint = new Color32 (186, 31, 31, 255);

	}

	void SetupWorld (int width, int height)
	{
		jobQueue = new JobQueue ();

		Width = width;
		Height = height;
		tiles = new Tile[width, height];

		rooms = new List<Room> ();
		rooms.Add (new Room ());

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				tiles [x, y] = new Tile (this, x, y);
				tiles [x, y].registerTileChangedCallback (onTileChanged);
				tiles [x, y].room = GetOutsideRoom ();
			}
		}

		CreateFurniturePrototypes ();

		characters = new List<Character> ();
		furnitures = new List<Furniture> ();
		inventoryManager = new InventoryManager ();
	}

	void ReadXml_Tiles (XmlReader reader)
	{
		if (reader.ReadToDescendant ("Tile")) {
			do {
				int x = int.Parse (reader.GetAttribute ("X"));
				int y = int.Parse (reader.GetAttribute ("Y"));
				tiles [x, y].ReadXml (reader);
			} while(reader.ReadToNextSibling ("Tile"));
		}
	}

	void ReadXml_Furnitures (XmlReader reader)
	{
		if (reader.ReadToDescendant ("Furniture")) {
			do {
				int x = int.Parse (reader.GetAttribute ("X"));
				int y = int.Parse (reader.GetAttribute ("Y"));
				Furniture furniture = PlaceFurniture (reader.GetAttribute ("FurnitureType"), tiles [x, y]);
				furniture.ReadXml (reader);
			} while(reader.ReadToNextSibling ("Furniture"));
		}
	}

	void ReadXml_Characters (XmlReader reader)
	{
		if (reader.ReadToDescendant ("Character")) {
			do {
				int x = int.Parse (reader.GetAttribute ("X"));
				int y = int.Parse (reader.GetAttribute ("Y"));
				Character character = CreateCharacter (tiles [x, y]);
				character.ReadXml (reader);
			} while(reader.ReadToNextSibling ("Character"));
		}
	}
}
