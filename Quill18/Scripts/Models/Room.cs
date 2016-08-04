using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Collections;
using System.Runtime.InteropServices;
using Priority_Queue;

public class Room
{
	Dictionary<string, float> atmosphericGasses;

	List<Tile> tiles;
	World world;

	public static void DoRoomFloodFill (Tile sourceTile, bool onlyIfOutside = false)
	{
		World world = sourceTile.world;
		Room oldRoom = sourceTile.room;

		if (oldRoom != null) {

			foreach (var tile in sourceTile.getNeighboors()) {
				if (tile.room != null && (!onlyIfOutside || tile.room.IsOutsideRoom ())) {
					ActualFlooFill (tile, oldRoom);
				}
			}

			sourceTile.room = null;
			oldRoom.tiles.Remove (sourceTile);

			if (!oldRoom.IsOutsideRoom ()) {
				if (oldRoom.tiles.Count > 0) {
					Debug.LogError ("oldRoom still has tiles assigned to it");
				}
				world.DeleteRoom (oldRoom);
			}

		} else {
			ActualFlooFill (sourceTile, null);
		}
	}

	protected static void ActualFlooFill (Tile tile, Room oldRoom)
	{
		if (tile == null || tile.room != oldRoom || (tile.furniture != null && tile.furniture.roomEnclosure) || tile.Type == TileType.EMPTY) {
			return;
		}

		Room newRoom = new Room (tile.world);
		Queue<Tile> tilesToCheck = new Queue<Tile> ();
		tilesToCheck.Enqueue (tile);
		bool isConnectedToSpace = false;

		while (tilesToCheck.Count > 0) {
			Tile t = tilesToCheck.Dequeue ();

			if (t.room != newRoom) {
				newRoom.AssignTile (t);

				Tile[] ns = t.getNeighboors ();

				foreach (var t2 in ns) {
					if (t2 == null || t2.Type == TileType.EMPTY) {
						isConnectedToSpace = true;

						/*if (oldRoom != null) {
							newRoom.UnAssignAllTiles ();
							return;
						}*/

					} else if (t2.room != newRoom && (t2.furniture == null || !t2.furniture.roomEnclosure)) {
						tilesToCheck.Enqueue (t2);
						
					}
				}
			}
		}

		if (isConnectedToSpace) {
			newRoom.ReturnTilesToOutsideRoom ();
			return;
		}

		if (oldRoom != null) {
			newRoom.CopyGas (oldRoom);

		} else {
			newRoom.MergeGasses (oldRoom);
		}

		tile.world.AddRoom (newRoom);
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

	public void ReturnTilesToOutsideRoom ()
	{
		foreach (var tile in tiles) {
			tile.room = tile.world.GetOutsideRoom ();
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

	void MergeGasses (Room oldRoom)
	{
		
	}
}
