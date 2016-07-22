using UnityEngine;
using System.Collections.Generic;
using System;
using Priority_Queue;
using System.Linq;

public class Path_AStar
{
	Stack<Tile> path;

	public Path_AStar (World world, Tile tileStart, Tile tileEnd)
	{
		if (world.tileGraph == null) {
			world.tileGraph = new Path_TileGraph (world);
		}

		Dictionary<Tile,Path_Node<Tile>> nodes = world.tileGraph.nodes;

		if (nodes.ContainsKey (tileStart) == false) {
			Debug.LogError ("Path_AStar -- tileStart isn't in nodes");
			return;
		}
		if (nodes.ContainsKey (tileEnd) == false) {
			Debug.LogError ("Path_AStar -- tileEnd isn't in nodes");
			return;
		}

		Path_Node<Tile> start = nodes [tileStart];
		Path_Node<Tile> goal = nodes [tileEnd];

		List<Path_Node<Tile>> closedSet = new List<Path_Node<Tile>> ();

		SimplePriorityQueue<Path_Node<Tile>> openSet = new SimplePriorityQueue<Path_Node<Tile>> ();
		openSet.Enqueue (start, heuristicCostEstimate (start, goal));

		Dictionary<Path_Node<Tile>,Path_Node<Tile>> cameFrom = new Dictionary<Path_Node<Tile>, Path_Node<Tile>> ();

		Dictionary<Path_Node<Tile>,float> gScore = new Dictionary<Path_Node<Tile>, float> ();

		foreach (Path_Node<Tile> node in nodes.Values) {
			gScore [node] = Mathf.Infinity;
		}

		gScore [start] = 0;

		Dictionary<Path_Node<Tile>,float> fScore = new Dictionary<Path_Node<Tile>, float> ();

		foreach (Path_Node<Tile> node in nodes.Values) {
			fScore [node] = Mathf.Infinity;
		}

		fScore [start] = heuristicCostEstimate (start, goal);

		while (openSet.Count > 0) {
			Path_Node<Tile> current = openSet.Dequeue ();

			if (current == goal) {
				//Debug.Log (cameFrom.Count);
				reconstructPath (cameFrom, current);
				return;
			}

			closedSet.Add (current);

			foreach (Path_Edge<Tile> edge_neighbour in current.egdes) {
				Path_Node<Tile> neighbour = edge_neighbour.node;
				
				if (closedSet.Contains (neighbour) == true) {
					continue;
				}

				float movementCostToNeighbour = neighbour.data.movementCost * distanceBetween (current, neighbour);
				float tentativeGScore = gScore [current] + movementCostToNeighbour;

				if (openSet.Contains (neighbour) && tentativeGScore >= gScore [neighbour]) {
					continue;
				}

				cameFrom [neighbour] = current;
				gScore [neighbour] = tentativeGScore;
				fScore [neighbour] = gScore [neighbour] + heuristicCostEstimate (neighbour, goal);

				if (openSet.Contains (neighbour) == false) {
					openSet.Enqueue (neighbour, fScore [neighbour]);
				} else {
					openSet.UpdatePriority (neighbour, fScore [neighbour]);
				}

			}
		}
		//failure state
	}

	public Tile Dequeue ()
	{
		Tile first = path.Pop ();
		//Debug.Log ("Path first: " + first.X + " , " + first.Y);
		return first;
	}

	public int count ()
	{
		if (path == null) {
			return 0;
		}
		return path.Count;
	}

	void reconstructPath (Dictionary<Path_Node<Tile>,Path_Node<Tile>> cameFrom, Path_Node<Tile> current)
	{
		path = new Stack<Tile> ();
		path.Push (current.data);
		//Debug.Log ("Path data: " + current.data.X + " , " + current.data.Y);

		while (cameFrom.ContainsKey (current) == true) {
			current = cameFrom [current];
			path.Push (current.data);
			//Debug.Log ("Path data: " + current.data.X + " , " + current.data.Y);
		}

		//path.Pop ();
	}

	float distanceBetween (Path_Node<Tile> a, Path_Node<Tile> b)
	{
		if (Mathf.Abs (a.data.X - b.data.X) + Mathf.Abs (a.data.Y - b.data.Y) == 1) {
			return 1f;
		} 
		if (Mathf.Abs (a.data.X - b.data.X) == 1 && Mathf.Abs (a.data.Y - b.data.Y) == 1) {
			return 1.41421356237f;
		}
		return Mathf.Sqrt (Mathf.Pow (a.data.X - b.data.X, 2) + Mathf.Pow (a.data.Y - b.data.Y, 2));
	}

	float heuristicCostEstimate (Path_Node<Tile> a, Path_Node<Tile> b)
	{
		return Mathf.Sqrt (Mathf.Pow (a.data.X - b.data.X, 2) + Mathf.Pow (a.data.Y - b.data.Y, 2));
	}


}
