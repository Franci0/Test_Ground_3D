using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class TileSpriteController : MonoBehaviour
{
	public Sprite floorSprite;
	public Sprite emptySprite;

	Dictionary<Tile,GameObject> tileGameObjectMap;

	World world {
		get{ return WorldController.Instance.world; }
	}

	void Start ()
	{
		tileGameObjectMap = new Dictionary<Tile, GameObject> ();

		for (int x = 0; x < world.Width; x++) {
			for (int y = 0; y < world.Height; y++) {

				Tile tile_data = world.getTileAt (x, y);	

				GameObject tile_go = new GameObject ();

				tileGameObjectMap.Add (tile_data, tile_go);

				tile_go.name = "tile_" + x + "_" + y;
				tile_go.transform.position = new Vector3 (tile_data.X, tile_data.Y, 0);
				tile_go.transform.SetParent (this.transform, true);

				SpriteRenderer sr = tile_go.AddComponent<SpriteRenderer> ();
				sr.sprite = emptySprite;
				sr.sortingLayerName = "Tiles";
			}
		}

		world.registerTileChangedCallback (OnTileChanged);
	}

	void destroyAllTileGameObjects ()
	{
		while (tileGameObjectMap.Count > 0) {
			Tile tile_data = tileGameObjectMap.Keys.First ();
			GameObject tile_go = tileGameObjectMap.Values.First ();

			tileGameObjectMap.Remove (tile_data);

			tile_data.unregisterTileTypeChangedCallback (OnTileChanged);

			Destroy (tile_go);
		}
	}

	void OnTileChanged (Tile tile_data)
	{
		if (tileGameObjectMap.ContainsKey (tile_data) == false) {
			Debug.LogError ("tileGameObjectMap doesn't contain tile_data");
			return;
		}

		GameObject tile_go = tileGameObjectMap [tile_data];

		if (tile_go == null) {
			Debug.LogError ("tileGameObjectMap's returned tile_go is null");
			return;
		}

		if (tile_data.Type == TileType.Floor)
			tile_go.GetComponent<SpriteRenderer> ().sprite = floorSprite;
		else if (tile_data.Type == TileType.Empty)
			tile_go.GetComponent<SpriteRenderer> ().sprite = emptySprite;
		else
			Debug.LogError ("OnTileTypeChanged - Unkown TileType");
	}

}
