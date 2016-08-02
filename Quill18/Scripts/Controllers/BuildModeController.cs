using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class BuildModeController : MonoBehaviour
{
	TileType buildModeTile = TileType.Floor;
	bool buildModeIsObject = false;
	bool devActive = false;
	string objectType;
	GameObject furniturePreview;
	FurnitureSpriteController furnitureSpriteController;
	MouseController mouseController;

	public void setMode_BuildFurniture (string furnitureType)
	{
		buildModeIsObject = true;
		devActive = false;
		objectType = furnitureType;
	}

	public void setMode_BuildFloor ()
	{
		buildModeIsObject = false;
		devActive = false;
		buildModeTile = TileType.Floor;
	}

	public void setMode_Bulldoze ()
	{
		buildModeIsObject = false;
		devActive = false;
		buildModeTile = TileType.Empty;
	}

	public void doBuild (Tile tile)
	{
		if (buildModeIsObject == true) {

			string furnitureType = objectType;

			if (devActive == true) {
				WorldController.Instance.world.PlaceFurniture (furnitureType, tile);
				return;
			}

			/*foreach (Job job in WorldController.Instance.World.jobList) {
				if (job != null && job.Tile.Equals (tmpTile)) {
					Debug.Log ("Tile occupied by another job");
					continue;
				}
			}*/

			if (tile.pendingFurnitureJob == null && WorldController.Instance.world.isFurniturePlacementValid (furnitureType, tile)) {

				Job job;

				if (WorldController.Instance.world.furnitureJobPrototypes.ContainsKey (furnitureType)) {
					job = WorldController.Instance.world.furnitureJobPrototypes [furnitureType].Clone ();
				} else {
					Debug.LogError ("There is no furnitureJobPrototype for '" + furnitureType + "'");
					job = new Job (tile, FurnitureActions.JobCompleteFurnitureBuilding, furnitureType, 0.1f, null);
				}

				job.furniturePrototype =	WorldController.Instance.world.getFurniturePrototype (furnitureType);

				job.tile = tile;

				tile.pendingFurnitureJob = job;

				job.registerJobCancelCallback ((theJob) => {
					theJob.tile.pendingFurnitureJob = null;
				});

				WorldController.Instance.world.jobQueue.Enqueue (job);
			}
		} else {
			tile.Type = buildModeTile;
		}
	}

	public void setMode_DevBuildFurniture (string furnitureType)
	{
		buildModeIsObject = true;
		devActive = true;
		objectType = furnitureType;
	}

	public bool IsObjectDraggable ()
	{
		if (!buildModeIsObject) {
			return true;
		}

		Furniture furniturePrototype = WorldController.Instance.world.getFurniturePrototype (objectType);
		return furniturePrototype.width == 1 && furniturePrototype.height == 1;
	}

	void Start ()
	{
		furnitureSpriteController = GameObject.FindObjectOfType<FurnitureSpriteController> ();
		mouseController = GameObject.FindObjectOfType<MouseController> ();
		furniturePreview = new GameObject ();
		furniturePreview.transform.SetParent (this.transform);
		furniturePreview.AddComponent<SpriteRenderer> ().sortingLayerName = "Jobs";
		furniturePreview.SetActive (false);
	}

	void Update ()
	{
		if (buildModeIsObject && objectType != null && objectType != "") {
			ShowFurnitureSpriteAtTile (objectType, mouseController.GetMouseOverTile ());
		}
	}

	void ShowFurnitureSpriteAtTile (string furnitureType, Tile tile)
	{
		furniturePreview.SetActive (true);
		SpriteRenderer sr = furniturePreview.GetComponent<SpriteRenderer> ();
		sr.sprite = furnitureSpriteController.getSpriteForFurniture (furnitureType);

		if (WorldController.Instance.world.isFurniturePlacementValid (furnitureType, tile)) {
			sr.color = new Color (0.5f, 1f, 0.5f, 0.25f);
		} else {
			sr.color = new Color (1f, 0.5f, 0.5f, 0.25f);
		}

		Furniture furniturePrototype = tile.World.getFurniturePrototype (furnitureType);
		furniturePreview.transform.position = new Vector3 (tile.X + ((furniturePrototype.width - 1) / 2f), tile.Y + ((furniturePrototype.height - 1) / 2f), 0);
	}


}
