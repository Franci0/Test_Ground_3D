using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MouseOverRoomIndexText : MonoBehaviour
{
	Text myText;
	MouseController mouseController;

	void Start ()
	{
		myText = GetComponent<Text> ();

		if (myText == null) {
			Debug.LogError ("MouseOverRoomIndexText: no 'Text' UI component on this object");
			this.enabled = false;
			return;
		}

		mouseController = GameObject.FindObjectOfType<MouseController> ();
		if (mouseController == null) {
			Debug.LogError ("MouseOverRoomIndexText: Missing instance of MouseController");
			return;
		}
	}

	void Update ()
	{
		Tile tile = mouseController.GetMouseOverTile ();

		myText.text = "Room Index: " + tile.World.rooms.IndexOf (tile.room).ToString ();
	}
}
