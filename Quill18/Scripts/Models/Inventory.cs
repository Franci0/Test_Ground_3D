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

	public Inventory (string _inventoryType, int _maxStackSize, int _stackSize)
	{
		inventoryType = _inventoryType;
		maxStackSize = _maxStackSize;
		stackSize = _stackSize;
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
