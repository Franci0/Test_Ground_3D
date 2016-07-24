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
		Room room = sourceFurniture.tile.room;

		if (room != world.GetOutsideRoom ()) {
			world.DeleteRoom (room);
		}
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
