using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class WorldController : MonoBehaviour
{
	public static WorldController Instance{ get; protected set; }

	public int size_x;
	public int size_y;
	public float randomizeTileTimer = 2.0f;

	public World world{ get; protected set; }

	public Tile getTileAtWorldCoord (Vector3 coord)
	{
		int x = Mathf.FloorToInt (coord.x);
		int y = Mathf.FloorToInt (coord.y);

		return world.getTileAt (x, y);
	}

	void OnEnable ()
	{
		if (Instance != null)
			Debug.LogError ("Troppe entità WorldController");
		Instance = this;

		world = new World (size_x, size_y);

		Camera.main.transform.position = new Vector3 (size_x / 2, size_y / 2, Camera.main.transform.position.z);
	}

	void Update ()
	{
		world.update (Time.deltaTime);
	}
}
