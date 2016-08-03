using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Collections;
using System.Runtime.InteropServices;

public class Room
{
	Dictionary<string, float> atmosphericGasses;

	List<Tile> tiles;
	World world;

	public static void DoRoomFloodFill (Furniture sourceFurniture)
	{
		World world = sourceFurniture.tile.World;
		Room oldRoom = sourceFurniture.tile.room;

		foreach (var tile in sourceFurniture.tile.getNeighboors()) {
			ActualFlooFill (tile, oldRoom, oldRoom.world);
		}

		sourceFurniture.tile.room = null;
		oldRoom.tiles.Remove (sourceFurniture.tile);

		if (!oldRoom.IsOutsideRoom ()) {
			if (oldRoom.tiles.Count > 0) {
				Debug.LogError ("oldRoom still has tiles assigned to it");
			}
			world.DeleteRoom (oldRoom);
		}
	}

	protected static void ActualFlooFill (Tile tile, Room oldRoom, World world)
	{
		if (tile == null || tile.room != oldRoom || (tile.furniture != null && tile.furniture.roomEnclosure) || tile.Type == TileType.EMPTY) {
			return;
		}

		Room newRoom = new Room (world);
		Queue<Tile> tilesToCheck = new Queue<Tile> ();
		tilesToCheck.Enqueue (tile);

		while (tilesToCheck.Count > 0) {
			Tile t = tilesToCheck.Dequeue ();

			if (t.room == oldRoom) {
				newRoom.AssignTile (t);

				Tile[] ns = t.getNeighboors ();

				foreach (var t2 in ns) {
					if (t2 == null || t2.Type == TileType.EMPTY) {
						newRoom.UnAssignAllTiles ();
						return;
					}

					if (t2 != null && t2.room == oldRoom && (t2.furniture == null || !t2.furniture.roomEnclosure)) {
						tilesToCheck.Enqueue (t2);
					}
				}
			}
		}

		newRoom.CopyGas (oldRoom);
		tile.World.AddRoom (newRoom);
	}

	public Room (World _world)
	{
		world = _world;
		tiles = new List<Tile> ();
		atmosphericGasses = new Dictionary<string, float> ();
	}

	public void AssignTile (Tile tile)
	{
		//Debug.Log ("AssignTile");
		if (tiles.Contains (tile)) {
			return;
		}

		if (tile.room != null) {
			tile.room.tiles.Remove (tile);
		}

		tile.room = this;
		tiles.Add (tile);
	}

	public void UnAssignAllTiles ()
	{
		foreach (var tile in tiles) {
			tile.room = tile.World.GetOutsideRoom ();
		}

		tiles.Clear ();
	}

	public void ChangeGas (string gasName, float amount)
	{
		if (IsOutsideRoom ()) {
			return;
		}

		if (atmosphericGasses.ContainsKey (gasName)) {
			atmosphericGasses [gasName] += amount;

		} else {
			atmosphericGasses [gasName] = amount;
		}

		if (atmosphericGasses [gasName] < 0) {
			atmosphericGasses [gasName] = 0;
		}
	}

	public float GetGasAmount (string gasName)
	{
		if (atmosphericGasses.ContainsKey (gasName)) {
			return atmosphericGasses [gasName];
		}

		return 0;
	}

	public float GetGasPercentage (string gasName)
	{
		if (!atmosphericGasses.ContainsKey (gasName)) {
			return 0;
		}

		float total = 0;
		foreach (string _gasName in atmosphericGasses.Keys) {
			total += atmosphericGasses [_gasName];
		}

		if (total == 0) {
			return 0;
		}

		return (atmosphericGasses [gasName] / total) * 100;
	}

	public bool IsOutsideRoom ()
	{
		/*if (tiles.Count == 0) {
			return true;
		}*/

		return this == world.GetOutsideRoom ();
	}

	void CopyGas (Room other)
	{
		foreach (var gasName in other.atmosphericGasses.Keys) {
			atmosphericGasses [gasName] = other.atmosphericGasses [gasName];
		}
	}

	public List<string> GetGassesName ()
	{
		List<string> arr = new List<string> ();

		foreach (var name in atmosphericGasses.Keys) {
			arr.Add (name);
		}

		return arr;
	}
}
