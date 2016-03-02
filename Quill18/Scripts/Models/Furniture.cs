using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Furniture : IXmlSerializable
{
	public Tile tile{ get; protected set; }

	public string FurnitureType{ get; protected set; }

	public bool LinksToNeighboors{ get; protected set; }

	public float movementCostMultiplier{ get; protected set ; }

	int width;

	int height;

	Func<Tile, bool> funcPositionValidation;

	Action<Furniture> onChangedCallback;


	static public Furniture createPrototype (string furnitureType, float movementCostMultiplier = 1f, int width = 1, int height = 1, bool linksToNeighboors = false)
	{
		Furniture furniture = new Furniture ();
		furniture.FurnitureType = furnitureType;
		furniture.movementCostMultiplier = movementCostMultiplier;
		furniture.width = width;
		furniture.height = height;
		furniture.LinksToNeighboors = linksToNeighboors;

		furniture.funcPositionValidation = furniture.__isValidPosition;

		return furniture;
	}

	static public Furniture placeInstance (Furniture prototype, Tile tile)
	{
		if (prototype.funcPositionValidation (tile) == false) {
			//Debug.LogError ("placeInstance -- position validity function returned false");
			return null;
		}

		Furniture furniture = new Furniture ();
		furniture.FurnitureType = prototype.FurnitureType;
		furniture.movementCostMultiplier = prototype.movementCostMultiplier;
		furniture.width = prototype.width;
		furniture.height = prototype.height;
		furniture.LinksToNeighboors = prototype.LinksToNeighboors;

		furniture.tile = tile;

		if (tile.placeFurniture (furniture) == false) {
			return null;
		}

		if (furniture.LinksToNeighboors == true) {
			Tile t;
			int x = tile.X;
			int y = tile.Y;

			for (int i = -1; i <= 1; i++) {
				for (int j = -1; j <= 1; j++) {
					if (i != 0 || j != 0) {
						t = tile.World.getTileAt (x + i, y + j);
						//Debug.Log (t.X + " , " + t.Y);
						if (t != null && t.furniture != null && t.furniture.onChangedCallback != null && t.furniture.FurnitureType == furniture.FurnitureType) {
							t.furniture.onChangedCallback (t.furniture);
						}
					}
				}
			}
		}

		return furniture;
	}

	public Furniture ()
	{

	}

	public XmlSchema GetSchema ()
	{
		return null;
	}

	public void WriteXml (XmlWriter writer)
	{
		writer.WriteAttributeString ("X", tile.X.ToString ());
		writer.WriteAttributeString ("Y", tile.Y.ToString ());
		writer.WriteAttributeString ("FurnitureType", FurnitureType);
		writer.WriteAttributeString ("movementCostMultiplier", movementCostMultiplier.ToString ());
	}

	public void ReadXml (XmlReader reader)
	{
		movementCostMultiplier = int.Parse (reader.GetAttribute ("movementCostMultiplier"));
	}

	public void registerOnChangedCallback (Action<Furniture> callback)
	{
		onChangedCallback += callback;
	}

	public void unregisterOnChangedCallback (Action<Furniture> callback)
	{
		onChangedCallback -= callback;
	}

	public bool __isValidPosition (Tile tile)
	{
		if (tile.Type != TileType.Floor) {
			return false;
		}

		if (tile.furniture != null) {
			return false;
		}

		return true;
	}

	public bool __isValidPosition_Door (Tile tile)
	{
		if (__isValidPosition (tile) == false) {
			return false;
		}
		return true;
	}

	public bool isValidPosition (Tile tile)
	{
		return funcPositionValidation (tile);
	}
}
