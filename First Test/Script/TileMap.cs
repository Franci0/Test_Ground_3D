using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TileMap : MonoBehaviour {

	public int size_x = 100;
	public int size_z = 50;
	public float tileSize = 1.0f;
	public Texture2D terrainTiles;
	public int tileResolution = 16;

	public int islandsNum = 10;
	public int minIsland_x = 4;
	public int maxIsland_x = 16;
	public int minIsland_z = 4;
	public int maxIsland_z = 16;

	public float bumpness = 0f;

	DTileMap map;

	void Start () {
		BuildMesh ();
	}

	Color[][] ChopUpTiles () {

		int numTilesRow = terrainTiles.width / tileResolution;
		int numRows = terrainTiles.height / tileResolution;

		Color[][] tiles = new Color[numTilesRow * numRows][];

		for (int z=0; z<numRows; z++) {
			for (int x=0; x<numTilesRow; x++) {
				tiles [z * numTilesRow + x] = terrainTiles.GetPixels (x * tileResolution, z * tileResolution, tileResolution, tileResolution);
			}
		}
		return tiles;
	}

	void BuildTexture () {

		map = new DTileMap (size_x, size_z);
		//Debug.Log(map);

		int textWidth = size_x * tileResolution;
		int textHeight = size_z * tileResolution;

		Texture2D texture = new Texture2D (textWidth, textHeight);

		Color[][] tiles = ChopUpTiles ();
        
        makeIslands(islandsNum , map);

		map.makeBridges();

		map.generatePathfindingGraph();

		for (int z=0; z<size_z; z++) {
			for (int x=0; x<size_x; x++) {
				Color[] color = tiles [map.GetTileGraphicIDAt(x , z)];
				texture.SetPixels (x * tileResolution, z * tileResolution, tileResolution, tileResolution, color);
			}
		}
        
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.Apply ();

		MeshRenderer mesh_renderer = GetComponent<MeshRenderer> ();
		mesh_renderer.sharedMaterials [0].mainTexture = texture;
	}

	public void BuildMesh () {

		int vsize_x = size_x + 1;
		int vsize_z = size_z + 1;

		int numVerts = vsize_x * vsize_z;
		int numTiles = size_x * size_z;
		int numTris = numTiles * 2;

		Vector3[] vertices = new Vector3[numVerts];
		Vector3[] normals = new Vector3[numVerts];
		Vector2[] uv = new Vector2[numVerts];

		int[] triangles = new int[numTris * 3];

		int x, z;
		for (z=0; z<vsize_z; z++) {
			for (x=0; x<vsize_x; x++) {
				vertices [z * vsize_x + x] = new Vector3 ( x * tileSize, Random.Range ( -bumpness , bumpness ) , z * tileSize );
				normals [z * vsize_x + x] = Vector3.forward;
				uv [z * vsize_x + x] = new Vector2 ( (float) x / size_x, (float) z / size_z );
			}
		}

		for (z=0; z<size_z; z++) {
			for (x=0; x<size_x; x++) {
				int squareIndex = z * size_x + x;
				int triOffset = squareIndex * 6;

				triangles [triOffset + 0] = z * vsize_x + x + vsize_x + 1;
				triangles [triOffset + 1] = z * vsize_x + x;
				triangles [triOffset + 2] = z * vsize_x + x + vsize_x;

				triangles [triOffset + 3] = z * vsize_x + x + 1;
				triangles [triOffset + 4] = z * vsize_x + x;
				triangles [triOffset + 5] = z * vsize_x + x + vsize_x + 1;
			}
		}

		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uv;

		MeshFilter mesh_filter = GetComponent<MeshFilter> ();
		MeshCollider mesh_collider = GetComponent<MeshCollider> ();

		mesh_filter.mesh = mesh;
		mesh_collider.sharedMesh = mesh;

		BuildTexture ();

	}

    public void makeIslands(int size , DTileMap map)
    {
        for (int i = 0; i < islandsNum; i++)
        {
			int width = Random.Range( minIsland_x , maxIsland_x + 1 );
			int height = Random.Range (minIsland_z , maxIsland_z + 1 );
            map.makeIsland(Random.Range(0, size_x - width), Random.Range(0, size_z - height), width, height);
        }
    }

	public DTileMap getDTileMap()
	{
		//Debug.Log(map);
		return map;
	}
}