﻿using System;
using UnityEngine;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public enum TileType
{
	Empty,
	Floor
}

public class Tile : IXmlSerializable
{

	public TileType Type {
		get {
			return type;
		}
		set {
			old = type;
			type = value;
			if (tileChangedCallback != null && old != type) {
				tileChangedCallback (this);
			}
		}
	}

	public Inventory inventory{ get; protected set; }

	public Furniture furniture{ get; protected set; }

	public int X{ get; protected set; }

	public int Y{ get; protected set; }

	public World World{ get; protected set; }

	public float movementCost {
		get {
			if (type == TileType.Empty) {
				return 0;
			}
			if (furniture == null) {
				return 1;
			}
			return 1 * furniture.movementCostMultiplier;
		}
	}

	public Job pendingFurnitureJob;

	Action<Tile> tileChangedCallback;

	TileType old;

	TileType type = TileType.Empty;

	public Tile ()
	{
		
	}

	public Tile (World world, int x, int y)
	{
		this.World = world;
		this.X = x;
		this.Y = y;
		old = type;
	}

	public void registerTileChangedCallback (Action<Tile> callback)
	{
		tileChangedCallback += callback;
	}

	public void unregisterTileTypeChangedCallback (Action<Tile> callback)
	{
		tileChangedCallback -= callback;
	}

	public bool placeFurniture (Furniture furnitureInstance)
	{
		
		if (furnitureInstance == null) {
			furniture = null;
			return true;
		}

		if (furniture != null) {
			//Debug.LogError ("There is already an InstalledObject: " + installedObject.ObjectType);
			return false;
		}

		furniture = furnitureInstance;
		return true;
	}

	public bool isNeighbour (Tile tile)
	{
		if (tile == null) {
			return false;
		}

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (this.X + x == tile.X && this.Y + y == tile.Y && (x != 0 && y != 0)) {
					return true;
				}
			}
		}

		return false;
	
	}

	public Tile[] getNeighboors (bool diagOk = false)
	{
		Tile[] neighbours; 

		if (diagOk == false) {
			neighbours = new Tile[4]; //Tile Order - N E S W
		} else {
			neighbours = new Tile[8]; //Tile Order - N E S W NE SE SW NW
		}

		neighbours [0] = World.getTileAt (X, Y + 1);
		neighbours [1] = World.getTileAt (X + 1, Y);
		neighbours [2] = World.getTileAt (X, Y - 1);
		neighbours [3] = World.getTileAt (X - 1, Y);

		if (diagOk == true) {
			neighbours [4] = World.getTileAt (X + 1, Y + 1);
			neighbours [5] = World.getTileAt (X + 1, Y - 1);
			neighbours [6] = World.getTileAt (X - 1, Y - 1);
			neighbours [7] = World.getTileAt (X - 1, Y + 1);
		}

		return neighbours;

	}

	public XmlSchema GetSchema ()
	{
		return null;
	}

	public void WriteXml (XmlWriter writer)
	{
		writer.WriteAttributeString ("X", X.ToString ());
		writer.WriteAttributeString ("Y", Y.ToString ());
		writer.WriteAttributeString ("Type", ((int)Type).ToString ());
	}

	public void ReadXml (XmlReader reader)
	{
		Type = (TileType)int.Parse (reader.GetAttribute ("Type"));
	}

}