using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class MouseController : MonoBehaviour
{

	public GameObject cursorPrefab;

	public float maxCameraDistance = 50f;
	public float minCameraDistance = 3f;

	GameObject go;

	Vector3 lastFramePosition;
	Vector3 currFramePosition;
	Vector3 dragStartPosition;

	Tile tileUnderMouse;
	Vector3 cursorPosition;

	Vector3 diff;
	int start_x;
	int end_x;
	int start_y;
	int end_y;
	int tmpInt;

	Tile tile;

	List<GameObject> dragCursors;

	BuildModeController buildModeController;
	FurnitureSpriteController furnitureSpriteController;

	bool isDragging = false;

	enum MouseMode
	{
		SELECT,
		BUILD
	}

	MouseMode currentMode = MouseMode.SELECT;

	public Vector3 GetMousePosition ()
	{
		return currFramePosition;
	}

	public Tile GetMouseOverTile ()
	{
		return WorldController.Instance.getTileAtWorldCoord (currFramePosition);//WorldController.Instance.world.getTileAt (Mathf.FloorToInt (currFramePosition.x), Mathf.FloorToInt (currFramePosition.y));
	}

	public void StartBuildMode ()
	{
		currentMode = MouseMode.BUILD;
	}

	void Start ()
	{
		dragCursors = new List<GameObject> ();
		buildModeController = GameObject.FindObjectOfType<BuildModeController> ();
		furnitureSpriteController = GameObject.FindObjectOfType<FurnitureSpriteController> ();
	}

	void Update ()
	{
		currFramePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		currFramePosition.z = 0;

		if (Input.GetKeyUp (KeyCode.Escape)) {
			
			if (currentMode == MouseMode.BUILD) {
				currentMode = MouseMode.SELECT;

			} else if (currentMode == MouseMode.SELECT) {
				Debug.Log ("Show Game Menu?");
			}
		}

		updateDrag ();
		updateCameraMovement ();

		lastFramePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition); //Per camera ortografica -- raycast per la prospettiva
		lastFramePosition.z = 0;
	}

	/*
	void updateCursor ()
	{
		tileUnderMouse = WorldController.Instance.getTileAtWorldCoord (currFramePosition);
		if (tileUnderMouse != null) {
			cursor.SetActive (true);
			cursorPosition = new Vector3 (tileUnderMouse.X, tileUnderMouse.Y, 0);
			cursor.transform.position = cursorPosition;
		} else {
			cursor.SetActive (false);
		}
	}
	*/

	void updateDrag ()
	{
		if (EventSystem.current.IsPointerOverGameObject ()) {
			return;
		}

		while (dragCursors.Count > 0) {
			go = dragCursors [0];
			dragCursors.RemoveAt (0);
			SimplePool.despawn (go);
		}

		if (currentMode != MouseMode.BUILD) {
			return;
		}

		if (Input.GetMouseButtonDown (0)) {
			dragStartPosition = currFramePosition;
			isDragging = true;
		} else if (!isDragging) {
			dragStartPosition = currFramePosition;
		}

		if (Input.GetMouseButtonUp (1) || Input.GetKeyUp (KeyCode.Escape)) {
			isDragging = false;
		}

		if (!buildModeController.IsObjectDraggable ()) {
			dragStartPosition = currFramePosition;
		}

		calculateAreaDrag ();

		for (int x = start_x; x <= end_x; x++) {
			for (int y = start_y; y <= end_y; y++) {
				tile = WorldController.Instance.world.getTileAt (x, y);

				if (tile != null) {
					if (buildModeController.buildMode == BuildMode.FURNITURE) {
						ShowFurnitureSpriteAtTile (buildModeController.objectType, tile);

					} else {
						go = SimplePool.spawn (cursorPrefab, new Vector3 (x, y, 0), Quaternion.identity);
						go.transform.SetParent (this.transform, true);
						dragCursors.Add (go);
					}
				}
			}
		}

		if (isDragging && Input.GetMouseButtonUp (0)) {
			calculateAreaDrag ();
			isDragging = false;

			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					tile = WorldController.Instance.world.getTileAt (x, y);
					if (tile != null) {
						buildModeController.doBuild (tile);
					}
				}
			}
		}
	}

	void updateCameraMovement ()
	{
		if (Input.GetMouseButton (2) || Input.GetMouseButton (1)) {
			diff = lastFramePosition - currFramePosition;
			Camera.main.transform.Translate (diff);
		}

		Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis ("Mouse ScrollWheel");
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, minCameraDistance, maxCameraDistance);
	}

	void calculateAreaDrag ()
	{
		start_x = Mathf.FloorToInt (dragStartPosition.x + 0.5f);
		end_x = Mathf.FloorToInt (currFramePosition.x + 0.5f);
		start_y = Mathf.FloorToInt (dragStartPosition.y + 0.5f);
		end_y = Mathf.FloorToInt (currFramePosition.y + 0.5f);

		if (end_x < start_x) {
			tmpInt = start_x;
			start_x = end_x;
			end_x = tmpInt;
		}
		if (end_y < start_y) {
			tmpInt = start_y;
			start_y = end_y;
			end_y = tmpInt;
		}
	}

	void ShowFurnitureSpriteAtTile (string furnitureType, Tile tile)
	{
		go = new GameObject ();
		go.transform.SetParent (this.transform, true);
		dragCursors.Add (go);

		SpriteRenderer sr = go.AddComponent<SpriteRenderer> ();
		sr.sprite = furnitureSpriteController.GetSpriteForFurniture (furnitureType);
		sr.sortingLayerName = "Jobs";

		if (WorldController.Instance.world.IsFurniturePlacementValid (furnitureType, tile)) {
			sr.color = new Color (0.5f, 1f, 0.5f, 0.25f);
		} else {
			sr.color = new Color (1f, 0.5f, 0.5f, 0.25f);
		}

		Furniture furniturePrototype = World.worldInstance.GetFurniturePrototype (furnitureType);
		go.transform.position = new Vector3 (tile.X + ((furniturePrototype.width - 1) / 2f), tile.Y + ((furniturePrototype.height - 1) / 2f), 0);
	}
}
