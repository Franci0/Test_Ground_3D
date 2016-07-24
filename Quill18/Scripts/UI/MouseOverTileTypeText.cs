using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MouseOverTileTypeText : MonoBehaviour
{
	Text myText;
	MouseController mouseController;

	void Start ()
	{
		myText = GetComponent<Text> ();

		if (myText == null) {
			Debug.LogError ("MouseOverTileTypeText: no 'Text' UI component on this object");
			this.enabled = false;
			return;
		}

		mouseController = GameObject.FindObjectOfType<MouseController> ();
		if (mouseController == null) {
			Debug.LogError ("MouseOverTileTypeText: Missing instance of MouseController");
			return;
		}
	}

	void Update ()
	{
		Tile tile = mouseController.GetMouseOverTile ();
		myText.text = "Tile Type: " + tile.Type.ToString ();
	}
}
