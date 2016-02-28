using System.Collections.Generic;
using UnityEngine;

public class DTile 
{
	
	public int x;
	public int z;
	public bool walkable = false;
	public int ID;
	public string name;
	public float weight;
	public List<DTile> neighboors;
	public DTile next;

	public DTile( int x , int z , int ID )
	{
		this.x = x;
		this.z = z;
		this.ID = ID;

		walkable = (ID != 0 && ID != 2);

		switch(ID)
		{
		case 0 :
			name = "Ocean";
			weight = Mathf.Infinity;
			break;
		case 1 :
			name = "Plain";
			weight = 2;
			break;
		case 2 :
			name = "Mountain";
			weight = Mathf.Infinity;
			break;
		case 3 :
			name = "Beach";
			weight = 3;
			break;
		case 4 :
			name = "Bridge";
			weight = 1;
			break;
		}

		neighboors = new List<DTile>();

	}

	public float distanceTo( DTile dTile )
	{
		return Vector2.Distance( new Vector2( x , z ) , new Vector2( dTile.x , dTile.z ) );
	}

}
