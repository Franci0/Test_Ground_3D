﻿using UnityEngine;
using System.Collections.Generic;
using System;

public class Heap<T> where T : IHeapItem<T>
{
	T[] items;
	int currentItemCount;

	public int Count{ get { return currentItemCount; } }

	public Heap (int maxHeapSize)
	{
		items = new T[maxHeapSize];
	}

	public void Add (T item)
	{
		item.HeapIndex = currentItemCount;
		items [currentItemCount] = item;
		sortUp (item);
		currentItemCount++;
	}

	public T removeFirst ()
	{
		T firstItem = items [0];
		currentItemCount--;
		items [0] = items [currentItemCount];
		items [0].HeapIndex = 0;
		sortDown (items [0]);
		return firstItem;
	}

	public void updateItem (T item)
	{
		sortUp (item);
	}

	public bool Contains (T item)
	{
		return Equals (items [item.HeapIndex], item);
	}

	void sortDown (T item)
	{
		while (true) {
			int childIndexLeft = item.HeapIndex * 2 + 1;
			int childIndexRight = item.HeapIndex * 2 + 2;
			int swapIndex = 0;

			if (childIndexLeft < currentItemCount) {
				swapIndex = childIndexLeft;

				if (childIndexRight < currentItemCount) {
					if (items [childIndexLeft].CompareTo (items [childIndexRight]) < 0) {
						swapIndex = childIndexRight;
					}
				}

				if (item.CompareTo (items [swapIndex]) < 0) {
					swap (item, items [swapIndex]);
				} else {
					return;
				}
			} else {
				return;
			}
		}
	}

	void sortUp (T item)
	{
		int parentIndex = (item.HeapIndex - 1) / 2;
		while (true) {
			T parentItem = items [parentIndex];
			if (item.CompareTo (parentItem) > 0) {
				swap (item, parentItem);
			} else {
				break;
			}
			parentIndex = (item.HeapIndex - 1) / 2;
		}
	}

	void swap (T itemA, T itemB)
	{
		items [itemA.HeapIndex] = itemB;
		items [itemB.HeapIndex] = itemA;
		int itemAIndex = itemA.HeapIndex;
		itemA.HeapIndex = itemB.HeapIndex;
		itemB.HeapIndex = itemAIndex;
	}

}

public interface IHeapItem<T> : IComparable<T>
{
	int HeapIndex{ get; set; }



}
