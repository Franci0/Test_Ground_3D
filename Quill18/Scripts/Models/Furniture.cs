using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Furniture : IXmlSerializable
{
	public Tile tile{ get; protected set; }

	public string furnitureType{ get; protected set; }

	public bool linksToNeighboors{ get; protected set; }

	public float movementCostMultiplier{ get; protected set ; }

	public bool roomEnclosure{ get; protected set ; }

	public Dictionary<string,float> furnitureParameters;
	public Action<Furniture,float> updateActions;
	public Func<Furniture,Accessiblity> isAccessible;
	public Action<Furniture> onChangedCallback;

	int width;
	int height;
	Func<Tile, bool> funcPositionValidation;

	public Furniture ()
	{
		furnitureParameters = new Dictionary<string, float> ();
	}

	public Furniture (string _furnitureType, float _movementCostMultiplier = 1f, int _width = 1, int _height = 1, bool _linksToNeighboors = false, bool _roomEnclosure = false)
	{
		furnitureParameters = new Dictionary<string, float> ();
		furnitureType = _furnitureType;
		movementCostMultiplier = _movementCostMultiplier;
		roomEnclosure = _roomEnclosure;
		width = _width;
		height = _height;
		linksToNeighboors = _linksToNeighboors;
		funcPositionValidation = __isValidPosition;
	}

	protected Furniture (Furniture other)
	{
		if (other.updateActions != null) {
			updateActions = (Action<Furniture,float>)other.updateActions.Clone ();
		}

		furnitureParameters = new Dictionary<string, float> (other.furnitureParameters);
		furnitureType = other.furnitureType;
		movementCostMultiplier = other.movementCostMultiplier;
		roomEnclosure = other.roomEnclosure;
		width = other.width;
		height = other.height;
		linksToNeighboors = other.linksToNeighboors;
		isAccessible = other.isAccessible;
	}

	public static Furniture PlaceInstance (Furniture prototype, Tile tile)
	{
		if (prototype.funcPositionValidation (tile) == false) {
			//Debug.LogError ("placeInstance -- position validity function returned false");
			return null;
		}

		Furniture furniture = prototype.Clone ();
		furniture.tile = tile;

		if (tile.placeFurniture (furniture) == false) {
			return null;
		}

		if (furniture.linksToNeighboors == true) {
			Tile t;
			int x = tile.X;
			int y = tile.Y;

			for (int i = -1; i <= 1; i++) {
				for (int j = -1; j <= 1; j++) {
					if (i != 0 || j != 0) {
						t = tile.World.getTileAt (x + i, y + j);
						//Debug.Log (t.X + " , " + t.Y);
						if (t != null && t.furniture != null && t.furniture.onChangedCallback != null && t.furniture.furnitureType == furniture.furnitureType) {
							t.furniture.onChangedCallback (t.furniture);
						}
					}
				}
			}
		}

		return furniture;
	}

	public void Update (float deltaTime)
	{
		if (updateActions != null) {
			updateActions (this, deltaTime);
		}
	}

	public void WriteXml (XmlWriter writer)
	{
		writer.WriteAttributeString ("X", tile.X.ToString ());
		writer.WriteAttributeString ("Y", tile.Y.ToString ());
		writer.WriteAttributeString ("FurnitureType", furnitureType);
		//writer.WriteAttributeString ("movementCostMultiplier", movementCostMultiplier.ToString ());

		foreach (string key in furnitureParameters.Keys) {
			writer.WriteStartElement ("Param");
			writer.WriteAttributeString ("name", key);
			writer.WriteAttributeString ("value", furnitureParameters [key].ToString ());
			writer.WriteEndElement ();
		}
	}

	public void ReadXml (XmlReader reader)
	{
		//movementCostMultiplier = int.Parse (reader.GetAttribute ("movementCostMultiplier"));

		if (reader.ReadToDescendant ("Param")) {
			do {
				string key = reader.GetAttribute ("name");
				float value = float.Parse (reader.GetAttribute ("value"));
				furnitureParameters [key] = value;
			} while(reader.ReadToNextSibling ("Param"));
		}
	}

	public void registerOnChangedCallback (Action<Furniture> callback)
	{
		onChangedCallback += callback;
	}

	public void unregisterOnChangedCallback (Action<Furniture> callback)
	{
		onChangedCallback -= callback;
	}

	public virtual Furniture Clone ()
	{
		return new Furniture (this);
	}

	public XmlSchema GetSchema ()
	{
		return null;
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
