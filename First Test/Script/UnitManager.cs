using UnityEngine;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{

	public DTile[] path;
	DTile[] oldPath;

	public TileMap tileMap;
	public int moveSpeed = 2;

	int i = 1;

	float x_Offset;
	float z_Offset;

	void Start ()
	{
		x_Offset = transform.parent.position.x;
		z_Offset = transform.parent.position.z;
	}

	void Update()
	{
		if(path != null)
		{
			if( !path.Equals(oldPath) )
			{
				i = 1;
				oldPath = path;
			}
			for( int j = 0 ; j < path.Length - 1 ; j++ )
			{
				Debug.DrawLine( new Vector3( path[j].x + x_Offset , 0 , path[j].z + z_Offset ) , 
					new Vector3( path[j + 1].x + x_Offset , 0 , path[j + 1].z + z_Offset  ) , Color.black );
			}
		}
	}

	public void moveNextTile()
	{
		int j = moveSpeed;
		while( j > 0 && i < path.Length)
		{
			transform.position = new Vector3( path[i].x + x_Offset , transform.position.y , path[i].z + z_Offset );
			j -= (int) path[i].weight;
			i++;
		}
	}
}
