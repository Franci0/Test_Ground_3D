using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class World : IXmlSerializable
{
	
	public int Width{ get; protected set; }

	public int Height{ get; protected set; }

	public JobQueue jobQueue;
	public List<Furniture> furnitures;
	public Path_TileGraph tileGraph;
	public List<Character> characters;

	Tile[,] tiles;
	Dictionary<string,Furniture> furniturePrototypes;
	Action<Furniture> furnitureCreatedCallback;
	Action<Tile> tileChangedCallback;
	Action<Character> characterCreatedCallback;

	public World ()
	{
		
	}

	public World (int width, int height)
	{
		SetupWorld (width, height);
		Character character = CreateCharacter (getTileAt (width / 2, height / 2));
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
				writer.WriteStartElement ("Tile");
				tiles [x, y].WriteXml (writer);
				writer.WriteEndElement ();
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

	public Furniture placeFurniture (string furnitureType, Tile tile)
	{
		//1x1 tile
		if (!furniturePrototypes.ContainsKey (furnitureType)) {
			Debug.LogError ("FurniturePrototype doesn't contain key " + furnitureType);
			return null;
		}
		//Debug.Log (furnitureType);
		Furniture furniture = Furniture.placeInstance (furniturePrototypes [furnitureType], tile);

		if (furniture == null) {
			return null;
		}

		furnitures.Add (furniture);

		if (furnitureCreatedCallback != null) {
			furnitureCreatedCallback (furniture);
			invalidateTileGraph ();
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
			character.update (deltaTime);
		}
	}

	public void invalidateTileGraph ()
	{
		tileGraph = null;
	}

	void onTileChanged (Tile tile)
	{
		if (tileChangedCallback == null) {
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

	void SetupWorld (int width, int height)
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
		furnitures = new List<Furniture> ();
	}

	void ReadXml_Tiles (XmlReader reader)
	{
		while (reader.Read ()) {
			
			if (reader.Name != "Tile") {
				return;
			}

			int x = int.Parse (reader.GetAttribute ("X"));
			int y = int.Parse (reader.GetAttribute ("Y"));
			tiles [x, y].ReadXml (reader);
		}
	}

	void ReadXml_Furnitures (XmlReader reader)
	{
		while (reader.Read ()) {

			if (reader.Name != "Furniture") {
				return;
			}

			int x = int.Parse (reader.GetAttribute ("X"));
			int y = int.Parse (reader.GetAttribute ("Y"));

			Furniture furniture = placeFurniture (reader.GetAttribute ("FurnitureType"), tiles [x, y]);
			furniture.ReadXml (reader);
		}
	}

	void ReadXml_Characters (XmlReader reader)
	{
		while (reader.Read ()) {

			if (reader.Name != "Character") {
				return;
			}

			int x = int.Parse (reader.GetAttribute ("X"));
			int y = int.Parse (reader.GetAttribute ("Y"));

			Character character = CreateCharacter (tiles [x, y]);
			character.ReadXml (reader);
		}
	}
}
