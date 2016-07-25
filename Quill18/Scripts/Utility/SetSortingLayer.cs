using UnityEngine;
using System.Collections;
using System;

public class SetSortingLayer : MonoBehaviour
{
	public String sortingLayerName = "default";

	void Start ()
	{
		GetComponent<Renderer> ().sortingLayerName = sortingLayerName;
	}
}
