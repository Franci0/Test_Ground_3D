using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class MouseOverFurnitureTypeText : MonoBehaviour
{
	Text myText;
	MouseController mouseController;

	void Start ()
	{
		myText = GetComponent<Text> ();

		if (myText == null) {
			Debug.LogError ("MouseOverFunitureTypeText: no 'Text' UI component on this object");
			this.enabled = false;
			return;
		}

		mouseController = GameObject.FindObjectOfType<MouseController> ();
		if (mouseController == null) {
			Debug.LogError ("MouseOverFunitureTypeText: Missing instance of MouseController");
			return;
		}
	}

	void Update ()
	{
		Tile tile = mouseController.GetMouseOverTile ();
		String furnitureName = "NULL";

		if (tile.furniture != null) {
			furnitureName = tile.furniture.furnitureType;
		}

		myText.text = "Furniture Type: " + furnitureName;
	}
}
