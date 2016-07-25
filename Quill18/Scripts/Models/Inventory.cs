using UnityEngine;
using System.Collections;
using System;

public class Inventory
{
	public String inventoryType = "Steel Plate";
	public int maxStackSize = 50;
	public int stackSize = 1;

	public Tile tile;
	public Character character;

	public Inventory ()
	{
		
	}

	public  virtual Inventory Clone ()
	{
		return new Inventory (this);
	}

	protected Inventory (Inventory other)
	{
		inventoryType = other.inventoryType;
		maxStackSize = other.maxStackSize;
		stackSize = other.stackSize;
	}
}
