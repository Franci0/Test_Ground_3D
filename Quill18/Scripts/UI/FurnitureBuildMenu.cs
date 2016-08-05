using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FurnitureBuildMenu : MonoBehaviour
{
	public GameObject buildFurnitureButtonPrefab;

	void Start ()
	{
		BuildModeController buildModeController = FindObjectOfType<BuildModeController> ();

		foreach (string furnitureName in World.worldInstance.furniturePrototypes.Keys) {
			GameObject go = Instantiate (buildFurnitureButtonPrefab);
			go.transform.SetParent (transform);
			go.name = "Button - Build " + furnitureName;
			Text text = go.GetComponentInChildren<Text> ();
			text.text = "Build " + furnitureName;
			Button button = go.GetComponent<Button> ();
			string furnitureID = furnitureName;
			button.onClick.AddListener (delegate {
				buildModeController.setMode_BuildFurniture (furnitureID);
			});
		}
	}
}
