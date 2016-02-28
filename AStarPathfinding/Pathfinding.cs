using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System;

public class Pathfinding : MonoBehaviour
{
	//public Transform seeker, target;

	PathRequestManager requestManager;

	Grid grid;

	public void startFindPath (Vector3 startPos, Vector3 targetPos)
	{
		StartCoroutine (findPath (startPos, targetPos));
	}

	void Awake ()
	{
		grid = GetComponent<Grid> ();
		requestManager = GetComponent<PathRequestManager> ();
	}

	/*void Update ()
	{
		if (Input.GetButtonDown ("Jump")) {
			findPath (seeker.position, target.position);
		}
	}*/

	IEnumerator findPath (Vector3 startPos, Vector3 targetPos)
	{
		Stopwatch sw = new Stopwatch ();
		sw.Start ();

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		Node startNode = grid.nodeFromWorldPoint (startPos);
		Node targetNode = grid.nodeFromWorldPoint (targetPos);

		if (startNode.walkable && targetNode.walkable) {

			Heap<Node> openSet = new Heap<Node> (grid.MaxSize);
			HashSet<Node> closeSet = new HashSet<Node> ();

			openSet.Add (startNode);

			while (openSet.Count > 0) {
				Node currentNode = openSet.removeFirst ();
				closeSet.Add (currentNode);

				if (currentNode == targetNode) {
					sw.Stop ();
					print ("Path found: " + sw.ElapsedMilliseconds + " ms");
					pathSuccess = true;
					break;
				}

				foreach (Node neighbour in grid.getNeighbours(currentNode)) {
					if (!neighbour.walkable || closeSet.Contains (neighbour)) {
						continue;
					}
					int newMovementCostToNeighbour = currentNode.gCost + getDistance (currentNode, neighbour) + neighbour.movementPenalty;
					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains (neighbour)) {
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.hCost = getDistance (neighbour, targetNode);
						neighbour.parent = currentNode;

						if (!openSet.Contains (neighbour)) {
							openSet.Add (neighbour);
						} else {
							openSet.updateItem (neighbour);
						}
					}
				}
			}
		}
		yield return null;
		if (pathSuccess) {
			waypoints = retracePath (startNode, targetNode);
		}
		requestManager.finishedProcessingPath (waypoints, pathSuccess);
	}

	Vector3[] retracePath (Node startNode, Node endNode)
	{
		List<Node> path = new List<Node> ();
		Node currentNode = endNode;
		while (currentNode != startNode) {
			path.Add (currentNode);
			currentNode = currentNode.parent;
		}
		Vector3[] waypoints = simplifyPath (path);
		Array.Reverse (waypoints);

		return waypoints;
	}

	Vector3[] simplifyPath (List<Node> path)
	{
		List<Vector3> waypoints = new List<Vector3> ();
		Vector2 directionOld = Vector2.zero;

		for (int i = 1; i < path.Count; i++) {
			Vector2 directionNew = new Vector2 (path [i - 1].gridX - path [i].gridX, path [i - 1].gridY - path [i].gridY);
			if (directionNew != directionOld) {
				waypoints.Add (path [i].worldPosition);
			}
			directionOld = directionNew;
		}

		return waypoints.ToArray ();
	}

	int getDistance (Node a, Node b)//Chebichev
	{
		int distX = Mathf.Abs (a.gridX - b.gridX);
		int distY = Mathf.Abs (a.gridY - b.gridY);

		if (distX > distY) {
			return 14 * distY + 10 * (distX - distY);
		}
		return 14 * distX + 10 * (distY - distX);
	}

}
