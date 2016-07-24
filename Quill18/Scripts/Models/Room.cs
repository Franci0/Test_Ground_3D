using UnityEngine;
using System.Collections.Generic;

public class Room
{
	public float atmosO2 = 0;
	public float atmosN = 0;
	public float atmosCO2 = 0;

	List<Tile> tiles;

	public static void DoRoomFloodFill (Furniture sourceFurniture)
	{
		World world = sourceFurniture.tile.World;
		Room oldRoom = sourceFurniture.tile.room;

		foreach (var tile in sourceFurniture.tile.getNeighboors()) {
			ActualFlooFill (tile, oldRoom);
		}

		sourceFurniture.tile.room = null;
		oldRoom.tiles.Remove (sourceFurniture.tile);

		if (oldRoom != world.GetOutsideRoom ()) {
			if (oldRoom.tiles.Count > 0) {
				Debug.LogError ("oldRoom still has tiles assigned to it");
			}
			world.DeleteRoom (oldRoom);
		}
	}

	protected static void ActualFlooFill (Tile tile, Room oldRoom)
	{
		if (tile == null || tile.room != oldRoom || (tile.furniture != null && tile.furniture.roomEnclosure) || tile.Type == TileType.Empty) {
			return;
		}

		Room newRoom = new Room ();
		Queue<Tile> tilesToCheck = new Queue<Tile> ();
		tilesToCheck.Enqueue (tile);

		while (tilesToCheck.Count > 0) {
			Tile t = tilesToCheck.Dequeue ();

			if (t.room == oldRoom) {
				newRoom.AssignTile (t);

				Tile[] ns = t.getNeighboors ();

				foreach (var t2 in ns) {
					if (t2 == null || t2.Type == TileType.Empty) {
						newRoom.UnAssignAllTiles ();
						return;
					}

					if (t2 != null && t2.room == oldRoom && (t2.furniture == null || !t2.furniture.roomEnclosure)) {
						tilesToCheck.Enqueue (t2);
					}
				}
			}
		}

		tile.World.AddRoom (newRoom);
	}

	public Room ()
	{
		tiles = new List<Tile> ();
	}

	public void AssignTile (Tile tile)
	{
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
}
