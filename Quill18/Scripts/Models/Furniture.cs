﻿using UnityEngine;
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

	public Func<Furniture,Accessiblity> isAccessible;
	public Action<Furniture> onChangedCallback;

	public Color tint = Color.white;

	protected Dictionary<string,float> furnitureParameters;
	protected Action<Furniture,float> updateActions;

	int width;
	int height;
	Func<Tile, bool> funcPositionValidation;
	List<Job> jobs;

	public Furniture ()
	{
		furnitureParameters = new Dictionary<string, float> ();
		jobs = new List<Job> ();
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
		funcPositionValidation = DEFAULT__isValidPosition;
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
		tint = other.tint;
		linksToNeighboors = other.linksToNeighboors;
		isAccessible = other.isAccessible;
		jobs = new List<Job> ();
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

	public bool DEFAULT__isValidPosition (Tile tile)
	{
		if (tile.Type != TileType.Floor) {
			return false;
		}

		if (tile.furniture != null) {
			return false;
		}

		return true;
	}

	public bool isValidPosition (Tile tile)
	{
		return funcPositionValidation (tile);
	}

	public float GetParameter (String key, float default_value = 0)
	{
		if (!furnitureParameters.ContainsKey (key)) {
			return default_value;
		}

		return furnitureParameters [key];
	}

	public void SetParameter (String key, float value)
	{
		furnitureParameters [key] = value;
	}

	public void ChangeParameter (String key, float value)
	{
		if (!furnitureParameters.ContainsKey (key)) {
			furnitureParameters [key] = value;
		}
		furnitureParameters [key] += value;
	}

	public void RegisterUpdateAction (Action<Furniture,float> action)
	{
		updateActions += action;
	}

	public void UnregisterUpdateAction (Action<Furniture,float> action)
	{
		updateActions -= action;
	}

	public int JobCount ()
	{
		return jobs.Count;
	}

	public void AddJob (Job job)
	{
		jobs.Add (job);
		tile.World.jobQueue.Enqueue (job);
	}

	public void RemoveJob (Job job)
	{
		jobs.Remove (job);
		job.CancelJob ();
		//tile.World.jobQueue.Remove (job);
	}

	public void ClearJobs ()
	{
		foreach (var job in jobs) {
			RemoveJob (job);
			//tile.World.jobQueue.Remove (job);
		}
	}

	public bool IsStockpile ()
	{
		return furnitureType == "Stockpile";
	}
}
