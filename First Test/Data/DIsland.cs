using UnityEngine;
using System.Collections;

public class DIsland
{
	public int left;
	public int bottom;
	public int heigth;
	public int width;

	public DIsland (int left , int bottom , int width , int heigth )
	{
		this.left = left;
		this.bottom = bottom;
		this.heigth = heigth;
		this.width = width;
	}

	public int right
	{
		get
		{
			return left + width - 1;
		}
	}

	public int top
	{
		get
		{
			return bottom + heigth - 1;
		}
	}

	public int center_x
	{
		get
		{
			return ( left + right ) / 2;
		}
	}

	public int center_z
	{
		get
		{
			return ( bottom + top ) / 2;
		}
	}

	public bool collidesWith ( DIsland other )
	{
		if ( left > other.right )
		{
			return false;
		}
		if ( top < other.bottom )
		{
			return false;
		}
		if ( right < other.left )
		{
			return false;
		}
		if ( bottom > other.top )
		{
			return false;
		}
		return true;
	}
}
