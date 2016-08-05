using UnityEngine;
using System.Collections.Generic;
using System;

public class Path_TileGraph
{
	public Dictionary<Tile,Path_Node<Tile>> nodes;

	public Path_TileGraph (World world)
	{
		nodes = new Dictionary<Tile,Path_Node<Tile>> ();

		for (int x = 0; x < world.Width; x++) {
			for (int y = 0; y < world.Height; y++) {
				Tile tile = world.getTileAt (x, y);
				//movementCost==0 -> impassable
				//if (tile.movementCost > 0) {
				Path_Node<Tile> node = new Path_Node<Tile> ();
				node.data = tile;
				nodes.Add (tile, node);
				//}
			}
		}

		//Debug.Log ("Path_TileGraph -- Created " + nodes.Count + " nodes");
		int edgesCount = 0;

		foreach (Tile tile in nodes.Keys) {
			Path_Node<Tile> node = nodes [tile];
			List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>> (); //Heavy cumputation?
			Tile[] neighbours = tile.getNeighboors (true);

			for (int i = 0; i < neighbours.Length; i++) {
				if (neighbours [i] != null && neighbours [i].movementCost > 0) {
					if (isClippingCorner (tile, neighbours [i]) == true) {
						continue;
					}
					Path_Edge<Tile> edge = new Path_Edge<Tile> ();
					edge.cost = neighbours [i].movementCost;
					edge.node = nodes [neighbours [i]];
					edges.Add (edge);
					edgesCount++;
				}
			}

			node.egdes = edges.ToArray ();
		}

		//Debug.Log ("Path_TileGraph -- Created " + edgesCount + " edges");
	}

	bool isClippingCorner (Tile current, Tile neighbour)
	{
		int dX = current.X - neighbour.X;
		int dY = current.Y - neighbour.Y;
				
		if (Mathf.Abs (dX) + Mathf.Abs (dY) == 2) {
			if (World.worldInstance.getTileAt (current.X - dX, current.Y).movementCost == 0) {
				return true;
			}
			if (World.worldInstance.getTileAt (current.X, current.Y - dY).movementCost == 0) {
				return true;
			}
		}

		return false;
	}
}
