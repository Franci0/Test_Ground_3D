using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MouseOverRoomDetailsText : MonoBehaviour
{
	Text myText;
	MouseController mouseController;

	void Start ()
	{
		myText = GetComponent<Text> ();

		if (myText == null) {
			Debug.LogError ("MouseOverRoomDetailsText: no 'Text' UI component on this object");
			this.enabled = false;
			return;
		}

		mouseController = GameObject.FindObjectOfType<MouseController> ();
		if (mouseController == null) {
			Debug.LogError ("MouseOverRoomDetailsText: Missing instance of MouseController");
			return;
		}
	}

	void Update ()
	{
		Tile tile = mouseController.GetMouseOverTile ();

		if (tile == null || tile.room == null) {
			myText.text = "Room Details: ";
			return;
		}

		Room room = tile.room;
		string gasList = "";

		foreach (string gasName in room.GetGassesName()) {
			gasList += gasName + ": " + room.GetGasAmount (gasName) + " (" + room.GetGasPercentage (gasName) + "%)" + " ";
		}

		myText.text = "Room Details: " + gasList;
	}
}
