using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
	public Transform target;
	float speed = 20;
	Vector3[] path;
	int targetIndex;

	public void OnPathFound (Vector3[] newPath, bool pathSuccessfull)
	{
		if (pathSuccessfull) {
			path = newPath;
			StopCoroutine ("followPath");
			StartCoroutine ("followPath");
		}
	}

	public void OnDrawGizmos ()
	{
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube (path [i], Vector3.one);

				if (i == targetIndex) {
					Gizmos.DrawLine (transform.position, path [i]);
				} else {
					Gizmos.DrawLine (path [i - 1], path [i]);
				}
			}
		}
	}

	void Start ()
	{
		PathRequestManager.requestPath (transform.position, target.position, OnPathFound);
	}

	IEnumerator followPath ()
	{
		Vector3 currentWaypoint = path [0];
		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex++;
				if (targetIndex >= path.Length) {
					yield break;
				}
				currentWaypoint = path [targetIndex];
			}

			transform.position = Vector3.MoveTowards (transform.position, currentWaypoint, speed * Time.deltaTime);
			yield return null;
		}
	}
}
