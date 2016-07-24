﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

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

	public Vector3 GetMousePosition ()
	{
		return currFramePosition;
	}

	public Tile GetMouseOverTile ()
	{
		return WorldController.Instance.world.getTileAt (Mathf.FloorToInt (currFramePosition.x), Mathf.FloorToInt (currFramePosition.y));
	}

	void Start ()
	{
		dragCursors = new List<GameObject> ();
	}

	void Update ()
	{
		currFramePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		currFramePosition.z = 0;

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

		if (Input.GetMouseButtonDown (0)) {
			dragStartPosition = currFramePosition;
		}

		while (dragCursors.Count > 0) {
			go = dragCursors [0];
			dragCursors.RemoveAt (0);
			SimplePool.despawn (go);
		}

		if (Input.GetMouseButton (0)) {
			calculateAreaDrag ();

			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					tile = WorldController.Instance.world.getTileAt (x, y);
					if (tile != null) {
						go = SimplePool.spawn (cursorPrefab, new Vector3 (x, y, 0), Quaternion.identity);
						go.transform.SetParent (this.transform, true);
						dragCursors.Add (go);
					}
				}
			}
		}

		if (Input.GetMouseButtonUp (0)) {
			calculateAreaDrag ();

			BuildModeController bmc = GameObject.FindObjectOfType<BuildModeController> ();

			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					tile = WorldController.Instance.world.getTileAt (x, y);
					if (tile != null) {
						bmc.doBuild (tile);
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
		start_x = Mathf.FloorToInt (dragStartPosition.x);
		end_x = Mathf.FloorToInt (currFramePosition.x);
		start_y = Mathf.FloorToInt (dragStartPosition.y);
		end_y = Mathf.FloorToInt (currFramePosition.y);

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
}
