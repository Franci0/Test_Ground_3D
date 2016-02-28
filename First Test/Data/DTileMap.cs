using System.Collections.Generic;
using UnityEngine;

public class DTileMap  {

	int size_x;
	int size_z;
	DTile[,] map_data;

	List<DIsland> islands;

	public DTileMap (int size_x , int size_z) {

		this.size_x = size_x;
		this.size_z = size_z;

		map_data = new DTile[size_x , size_z];

		for( int x = 0 ; x < size_x ; x++ ) 
		{
			for( int z = 0 ; z < size_z ; z++ )
			{
				map_data[ x , z ] = new DTile( x , z , 0 );
			}
		}

		islands = new List<DIsland> ();
	}

	public int GetTileGraphicIDAt ( int x , int z ) 
	{
		return map_data[ x , z ].ID;
	}

	public void makeIsland( int left , int bottom , int width , int height) {

		DIsland island = new DIsland ( left , bottom , width , height );

		if ( !isColliding ( island ) )
		{
			islands.Add ( island );

			//Debug.Log( island.center_x + " , " + island.center_z );

			for ( int x = 0 ; x < width ; x++ )
			{
				
				for ( int z = 0 ; z < height ; z++ )
				{
					
					if ( (x == 0 || x == width - 1 || z == 0 || z == height - 1) )
						map_data[ left + x , bottom + z ] = new DTile( left + x , bottom + z , 3 );

					else if ( x == width / 2 && z == height / 2  && width > 4 && height > 4 )
					{
						map_data[ left + x , bottom + z ] = new DTile( left + x , bottom + z , 2 );

						if ( width % 2 == 0 && height % 2 == 0 )
							map_data[ left + x - 1 , bottom + z - 1 ] = new DTile( left + x - 1 , bottom + z - 1 , 2 );
						
						if ( width % 2 == 0 )
							map_data[ left + x - 1 , bottom + z ] = new DTile( left + x - 1 , bottom + z , 2 );
						
						if ( height % 2 == 0 )
							map_data[ left + x , bottom + z - 1 ] = new DTile( left + x , bottom + z - 1 , 2 );

					}
					else 
						map_data[ left + x , bottom + z ] = new DTile( left + x , bottom + z , 1 );
					
				}
			}
		}
		else
		{
			return;
		}
		foreach ( DIsland other in islands )
		{
			fuseIslands ( other );
		}
	}

	public void makeBridges()
	{
		foreach(DIsland island in islands)
		{
			DIsland other = nearestIsland(island);

			//Debug.Log("island1 center : " + island.center_x + " , " + island.center_z + "\n" + "island2 center : " + other.center_x + " , " + other.center_z);

			makeBridge( island , other );
		}
	}

	public bool isWalkable( int x , int z)
	{
		return map_data[ x , z ].walkable;
	}

	public DTile getDTileAt( int x , int z )
	{
		return map_data[ x , z ];
	}

	public int getSize_x()
	{
		return size_x;
	}

	public int getSize_z()
	{
		return size_z;
	}

	public DTile[,] getMap_Data()
	{
		return map_data;
	}

	DIsland nearestIsland(DIsland island)
	{
		int center_x = island.center_x;
		int center_z = island.center_z;

		float nearestCenter_x = Mathf.Infinity;
		float nearestCenter_z = Mathf.Infinity;

		int otherCenter_x;
		int otherCenter_z;

		DIsland nearestIsland = island;;

		foreach(DIsland other in islands)
		{
			otherCenter_x = other.center_x;
			otherCenter_z = other.center_z;

			/*Debug.Log( "island1 center : " + center_x + " , " + center_z + "\n" + "island2 center : " + otherCenter_x + " , " + otherCenter_z + "\n" 
				+ Mathf.Sqrt( ( Mathf.Pow( Mathf.Abs( center_x - otherCenter_x ) , 2 ) + Mathf.Pow( Mathf.Abs( center_z - otherCenter_z ) , 2 ) ) ) + " , " 
				+ Mathf.Sqrt( ( Mathf.Pow( Mathf.Abs( center_x - nearestCenter_x ) , 2 ) + Mathf.Pow( Mathf.Abs( center_z - nearestCenter_z ) , 2 ) ) ) );*/

			//Debug.Log( center_x + " , " + center_z + " Is Considering " + otherCenter_x + " , " + otherCenter_z + " == " + !( otherCenter_x - center_x == 0 && otherCenter_z - center_z == 0 ) );

			if( !( otherCenter_x - center_x == 0 && otherCenter_z - center_z == 0 ) && 
				( Mathf.Sqrt( ( Mathf.Pow( Mathf.Abs( center_x - otherCenter_x ) , 2 ) + Mathf.Pow( Mathf.Abs( center_z - otherCenter_z ) , 2 ) ) ) ) <= ( Mathf.Sqrt( ( Mathf.Pow( Mathf.Abs( center_x - nearestCenter_x ) , 2 ) + Mathf.Pow( Mathf.Abs( center_z - nearestCenter_z ) , 2 ) ) ) ) )
			{
				nearestCenter_x = otherCenter_x;
				nearestCenter_z = otherCenter_z;
				nearestIsland=other;
			}
		}

		//Debug.Log( "island :  " + center_x + " , " + center_z + " has found nearest island : " + nearestIsland.center_x + " , " + nearestIsland.center_z);

		return nearestIsland;
	}

	void makeBridge(DIsland island , DIsland other)
	{

		int x1 = island.center_x;
		int x2 = other.center_x;
		int z1 = island.center_z;
		int z2 = other.center_z;

		while( x1 != x2 )
		{

			if( map_data[ x1 , z1 ].ID == 0 || map_data[ x1 , z1 ].ID == 3)
			{
				map_data[ x1 , z1 ] = new DTile( x1 , z1 , 4 );
			}

			x1 += x1 <= x2 ? +1 : -1;

			//Debug.Log("New x : " + x1 );
		}

		while( z1 != z2 )
		{

			if( map_data[ x1 , z1 ].ID == 0 || map_data[ x1 , z1 ].ID == 3 )
			{
				map_data[ x1 , z1 ] = new DTile( x1 , z1 , 4 );
			}

			z1 += z1 <= z2 ? +1 : -1;

			//Debug.Log("New z : " + z1 );
		}

	}

	void fuseIslands( DIsland island )
	{

		if(island.left == 0)
		{
			for( int z = 1 ; z < island.heigth - 1 ; z++ )
			{
				map_data[ 0 , island.bottom + z ] = new DTile( 0 , island.bottom + z , 1 );
			}
		}
		if(island.bottom == 0)
		{
			for ( int x = 1 ; x < island.width - 1 ; x++ )
			{
				map_data[ island.left + x , 0 ] = new DTile( island.left + x , 0 , 1 );
			}
		}
		if(island.top == size_z )
		{
			for ( int x = 1 ; x < island.width - 1 ; x++ )
			{
				map_data[ island.left + x , size_z ] = new DTile( island.left + x , size_z , 1 );
			}
		}
		if(island.right == 0)
		{
			for ( int z = 1 ; z < island.heigth - 1 ; z++ )
			{
				map_data[ size_x , island.bottom + z ] = new DTile( size_x , island.bottom + z , 1 );
			}
		}
		if ( island.left == 0 && island.bottom == 0 )
			map_data[ 0 , 0 ] = new DTile( 0 , 0 , 1 );
		if ( island.left == 0 && island.top == size_z )
			map_data[ 0 , size_z ] = new DTile( 0 , size_z , 1 );
		if ( island.right == size_x && island.bottom == 0 )
			map_data[ size_x , 0 ] = new DTile( size_x , 0 , 1 );
		if ( island.right == size_x && island.top == size_z )
			map_data[ size_x , size_z ] = new DTile( size_x , size_z , 1 );

		for( int x = island.left ; x <= island.right ; x++ )
		{
			if( !isNearOcean( x , island.bottom) )
			{
				map_data[ x , island.bottom ] = new DTile( x , island.bottom , 1 );
			}
			if( !isNearOcean ( x , island.top ) )
			{
				map_data[ x , island.top ] = new DTile( x , island.top , 1 );
			}
		}
		for( int z = island.bottom ; z <= island.top ; z++ )
		{
			if( !isNearOcean( island.left , z ) )
			{
				map_data[ island.left , z ] = new DTile( island.left , z , 1 );
			}
			if( !isNearOcean( island.right , z ) )
			{
				map_data[ island.right , z ] = new DTile( island.right , z , 1 );
			}
		}
	}

	bool isNearOcean( int x , int z)
	{
		for( int i = x - 1 ; i <= x + 1 ; i++ )
		{
			if ( i >= 0 && i <= size_x)
			{
				for ( int j = z - 1 ; j <= z + 1 ; j++ )
				{
					if( j >= 0 && j <= size_z && !( i == x && j == z ) && map_data[ i , j ].ID == 0)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	bool isColliding (DIsland island)
	{
		foreach(DIsland island2 in islands )
		{
			if ( island.collidesWith ( island2 ) )
			{
				return true;
			}
		}
		return false;
	}

	public void generatePathfindingGraph()
	{
		for( int x = 0 ; x < size_x ; x++ )
		{
			for( int z = 0 ; z < size_z ; z++ )
			{
				if( x > 0 )
				{
					map_data[x , z].neighboors.Add( map_data[x - 1 , z] );
					if( z > 0 )
						map_data[x , z].neighboors.Add( map_data[ x - 1 , z - 1 ] );
					if( z < size_z - 1 )
						map_data[x , z].neighboors.Add( map_data[ x - 1 , z + 1 ] );
				}

				if( x < size_x - 1 )
				{
					map_data[x , z].neighboors.Add( map_data[x + 1 , z] );
					if( z > 0 )
						map_data[x , z].neighboors.Add( map_data[ x + 1 , z - 1 ] );
					if( z < size_z - 1 )
						map_data[x , z].neighboors.Add( map_data[ x + 1 , z + 1 ] );
				}

				if( z > 0 )
					map_data[x , z].neighboors.Add( map_data[ x , z - 1 ] );
				if( z < size_z - 1 )
					map_data[x , z].neighboors.Add( map_data[ x , z + 1 ] );
			}
		}
	}

}
