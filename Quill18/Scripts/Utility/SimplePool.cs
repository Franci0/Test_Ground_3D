using UnityEngine;
using System.Collections.Generic;

public class SimplePool
{
	const int DEFAULT_POOL_SIZE = 3;

	class Pool
	{
		int nextId = 1;
		Stack<GameObject> inactive;
		GameObject prefab;

		public Pool (GameObject prefab, int initialQty)
		{
			this.prefab = prefab;
			inactive = new Stack<GameObject> (initialQty);
		}

		public GameObject spawn (Vector3 pos, Quaternion rot)
		{
			GameObject go;
			if (inactive.Count == 0) {
				go = (GameObject)GameObject.Instantiate (prefab, pos, rot);
				go.name = prefab.name + " (" + (nextId++) + ")";
				go.AddComponent<PoolMember> ().myPool = this;
			} else {
				go = inactive.Pop ();
				if (go == null) {
					return spawn (pos, rot);
				}
			}
			go.transform.position = pos;
			go.transform.rotation = rot;
			go.SetActive (true);
			return go;
		}

		public void despawn (GameObject go)
		{
			go.SetActive (false);
			inactive.Push (go);
		}
	}

	class PoolMember : MonoBehaviour
	{
		public Pool myPool;
	}

	static Dictionary<GameObject,Pool> pools;

	static void init (GameObject prefab = null, int qty = DEFAULT_POOL_SIZE)
	{
		if (pools == null) {
			pools = new Dictionary<GameObject, Pool> ();
		}
		if (prefab != null && pools.ContainsKey (prefab) == false) {
			pools [prefab] = new Pool (prefab, qty);
		}
	}

	static public void preload (GameObject prefab, int qty = 1)
	{
		init (prefab, qty);
		GameObject[] gos = new GameObject[qty];
		for (int i = 0; i < qty; i++) {
			gos [i] = spawn (prefab, Vector3.zero, Quaternion.identity);
		}

		for (int i = 0; i < qty; i++) {
			despawn (gos [i]);
		}
	}

	static public GameObject spawn (GameObject prefab, Vector3 pos, Quaternion rot)
	{
		init (prefab);
		return pools [prefab].spawn (pos, rot);
	}

	static public void despawn (GameObject go)
	{
		PoolMember pm = go.GetComponent<PoolMember> ();
		if (pm == null) {
			Debug.Log ("Object '" + go.name + "' wasn't spawned from a pool. Destroying it instead");
			GameObject.Destroy (go);
		} else {
			pm.myPool.despawn (go);
		}
	}

}
