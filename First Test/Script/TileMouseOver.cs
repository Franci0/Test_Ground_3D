using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(TileMap))]
public class TileMouseOver : MonoBehaviour {

	Vector3 currentTileCoord;

	TileMap tileMap;

	DTileMap dataTileMap;

	public Transform cube;
	Transform cubeParent;
	public Material[] materials = new Material[2];

	public bool mouseOverIsActive;
	public bool useDjkstra;

	public int tries = 100;

	void Start() 
	{
		tileMap = GetComponent<TileMap>();
		currentTileCoord.y = cube.transform.position.y;
		cubeParent = cube.parent;
		cube.GetComponent<Renderer>().material = materials[0];

		if( !mouseOverIsActive )
		{
			dataTileMap = tileMap.getDTileMap();

			//Debug.Log(dataTileMap);

			int new_x;
			int new_z;

			for(int i = 0 ; i < tries ; i++)
			{
				new_x = Random.Range( 0 , dataTileMap.getSize_x() );
				new_z = Random.Range( 0 , dataTileMap.getSize_z() );
				//Debug.Log( "Coord : " + new_x + " , " + new_z );

				if( dataTileMap.isWalkable( new_x , new_z ) )
				{
					cube.transform.position = new Vector3( new_x + cube.localScale.x , currentTileCoord.y , new_z + cube.localScale.z );
					break;
				}
			}
		}

	}

	void Update() 
	{
		
		if(mouseOverIsActive)
		{
			Ray ray= Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;

			if(GetComponent<Collider>().Raycast(ray, out hitInfo, Mathf.Infinity)) {
	            
				int x = (int)(hitInfo.point.x / tileMap.tileSize);
				int z = (int)(hitInfo.point.z / tileMap.tileSize);

				currentTileCoord.x = x + 0.5f;
				currentTileCoord.z = z + 0.5f;
				
				cube.transform.position = new Vector3 (currentTileCoord.x * tileMap.tileSize , cube.transform.position.y , currentTileCoord.z * tileMap.tileSize);
			}
		}
	}

	void OnMouseUpAsButton()
	{
		
		Ray ray= Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo;

		if( GetComponent<Collider>().Raycast( ray, out hitInfo, Mathf.Infinity ) )
		{
			DTile hitTile = dataTileMap.getDTileAt( (int) hitInfo.point.x , (int) hitInfo.point.z );
			if(hitTile.walkable)
			{
				if( !useDjkstra )
					cube.position = new Vector3( hitTile.x + cubeParent.position.x , currentTileCoord.y , hitTile.z + cubeParent.position.z );
				else
				{
					generatePathTo( (int) ( hitTile.x + cubeParent.position.x ) , (int) ( hitTile.z + cubeParent.position.z ) );
				}
			}
		}
	}

	void generatePathTo( int x , int z )
	{
		cube.GetComponent<UnitManager>().path = null;
		
		Dictionary<DTile , float> dist = new Dictionary<DTile , float>();
		Dictionary<DTile , DTile> prev = new Dictionary<DTile, DTile>();

		List<DTile> unvisited = new List<DTile>();
		List<DTile> currentPath = new List<DTile>();

		DTile source = dataTileMap.getDTileAt( (int) cube.position.x , (int) cube.position.z );
		DTile target = dataTileMap.getDTileAt( x , z );

		dist[source] = 0;
		prev[source] = null;

		foreach(DTile v in dataTileMap.getMap_Data() )
		{
			if(v != source )
			{
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}
			unvisited.Add( v );
		}

		while( unvisited.Count>0 )
		{
			DTile u = null;

			foreach( DTile iter in unvisited )
			{
				if( u == null || dist[iter] < dist[u] )
					u = iter;
			}

			if( u == target )
				break;

			unvisited.Remove( u );

			foreach( DTile v in u.neighboors )
			{
				float alt = dist[u] + u.distanceTo( v ) * v.weight;

				if( alt < dist[v] )
				{
					dist[v] = alt;
					prev[v] = u;
				}
			}
		}

		if( prev[target] == null )
		{
			return;
		}
	
		DTile next = target;

		while( next != null )
		{
			currentPath.Add( next );
			next = prev[ next ];
		}

		currentPath.Reverse();

		cube.GetComponent<UnitManager>().path = currentPath.ToArray();
	}

}
