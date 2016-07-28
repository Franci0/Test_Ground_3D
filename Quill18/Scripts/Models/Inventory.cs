using UnityEngine;
using System.Collections;
using System;

public class Inventory
{
	public String inventoryType = "Steel Plate";
	public int maxStackSize = 50;

	public int stackSize {
		get {
			return _stackSize;
		}
		set {
			if (_stackSize != value) {
				_stackSize = value;

				if (inventoryChangedCallback != null) {
					inventoryChangedCallback (this);
				}
			}
		}
	}

	public Tile tile;
	public Character character;

	protected int _stackSize = 1;

	Action<Inventory> inventoryChangedCallback;

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

	public void RegisterInventoryChangedCallback (Action<Inventory> callback)
	{
		inventoryChangedCallback += callback;
	}

	public void UnregisterInventoryTypeChangedCallback (Action<Inventory> callback)
	{
		inventoryChangedCallback -= callback;
	}
}
