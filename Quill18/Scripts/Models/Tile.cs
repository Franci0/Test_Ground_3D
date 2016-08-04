using System;
using UnityEngine;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public enum TileType
{
	EMPTY,
	FLOOR
}

public enum Accessiblity
{
	YES,
	NEVER,
	SOON
}

public class Tile : IXmlSerializable
{
	const float baseTileMovementCost = 1;

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

	public Furniture furniture{ get; protected set; }

	public int X{ get; protected set; }

	public int Y{ get; protected set; }

	public World World{ get; protected set; }

	public Inventory inventory;

	public float movementCost {
		get {
			if (type == TileType.EMPTY) {
				return 0;
			}
			if (furniture == null) {
				return baseTileMovementCost;
			}
			return baseTileMovementCost * furniture.movementCostMultiplier;
		}
	}

	public Job pendingFurnitureJob;
	public Room room;

	Action<Tile> tileChangedCallback;
	TileType old;
	TileType type = TileType.EMPTY;

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
			return UnplaceFurniture ();
		}

		if (!furnitureInstance.isValidPosition (this)) {
			Debug.LogError ("placeFurniture - Trying to assing a furniture to a uneligible one");
			return false;
		}

		for (int x_off = X; x_off < (X + furnitureInstance.width); x_off++) {
			for (int y_off = Y; y_off < (Y + furnitureInstance.height); y_off++) {
				Tile tile = World.getTileAt (x_off, y_off);
				tile.furniture = furnitureInstance;
			}
		}

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

	public Accessiblity isAccessible ()
	{
		if (movementCost == 0) {
			return Accessiblity.NEVER;
		}
		if (furniture != null && furniture.isAccessible != null) {
			return furniture.isAccessible (furniture);
		}

		return Accessiblity.YES;
	}

	public Tile North ()
	{
		return World.getTileAt (X, Y + 1);
	}

	public Tile South ()
	{
		return World.getTileAt (X, Y - 1);
	}

	public Tile West ()
	{
		return World.getTileAt (X - 1, Y);
	}

	public Tile East ()
	{
		return World.getTileAt (X + 1, Y);
	}

	public bool PlaceInventory (Inventory inv)
	{
		if (inventory != null) {
			if (inventory.inventoryType != inv.inventoryType) {
				Debug.LogError ("Trying to assign inventory to a tile that has some of a different inventory");
				return false;
			}

			int numToMove = inv.stackSize;

			if (inventory.stackSize + numToMove > inventory.maxStackSize) {
				numToMove = inventory.maxStackSize - inventory.stackSize;
			}

			inventory.stackSize += numToMove;
			inv.stackSize -= numToMove;

			return true;
		}

		inventory = inv.Clone ();
		inventory.tile = this;
		inv.stackSize = 0;
		return true;
	}

	public bool UnplaceFurniture ()
	{
		if (furniture == null) {
			return false;
		}

		Furniture temp = furniture;

		for (int x_off = X; x_off < (X + temp.width); x_off++) {
			for (int y_off = Y; y_off < (Y + temp.height); y_off++) {
				Tile tile = World.getTileAt (x_off, y_off);
				tile.furniture = null;
			}
		}

		temp = null;

		return true;
	}

}
