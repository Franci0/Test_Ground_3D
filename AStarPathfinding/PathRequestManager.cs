using UnityEngine;
using System.Collections.Generic;
using System;

public class PathRequestManager : MonoBehaviour
{
	Queue<PathRequest> pathRequestQueue = new Queue<PathRequest> ();
	PathRequest currentPathRequest;

	static PathRequestManager instance;
	Pathfinding pathfinding;
	bool isProcessingPath;

	public static void requestPath (Vector3 pathStart, Vector3 pathEnd, Action<Vector3[],bool> callback)
	{
		PathRequest newRequest = new PathRequest (pathStart, pathEnd, callback);
		instance.pathRequestQueue.Enqueue (newRequest);
		instance.tryProcessNext ();
	}

	public void finishedProcessingPath (Vector3[] path, bool success)
	{
		currentPathRequest.callback (path, success);
		isProcessingPath = false;
		tryProcessNext ();
	}

	void tryProcessNext ()
	{
		if (!isProcessingPath && pathRequestQueue.Count > 0) {
			isProcessingPath = true;
			currentPathRequest = pathRequestQueue.Dequeue ();
			pathfinding.startFindPath (currentPathRequest.pathStart, currentPathRequest.pathEnd);
		}
	}

	void Awake ()
	{
		instance = this;
		pathfinding = GetComponent<Pathfinding> ();
	}

	struct PathRequest
	{
		public Vector3 pathStart;
		public Vector3 pathEnd;
		public Action<Vector3[],bool> callback;

		public PathRequest (Vector3 _pathStart, Vector3 _pathEnd, Action<Vector3[],bool> _callback)
		{
			pathStart = _pathStart;
			pathEnd = _pathEnd;
			callback = _callback;
		}
	}

}
