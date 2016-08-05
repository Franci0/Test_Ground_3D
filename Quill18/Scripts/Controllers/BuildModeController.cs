using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public enum BuildMode
{
	FLOOR,
	FURNITURE,
	DECONSTRUCT
}

public class BuildModeController : MonoBehaviour
{
	public BuildMode buildMode = BuildMode.FLOOR;
	public string objectType;
	TileType buildModeTile = TileType.FLOOR;
	bool devActive = false;
	MouseController mouseController;

	/*GameObject furniturePreview;
	FurnitureSpriteController furnitureSpriteController;*/
	//MouseController mouseController;

	public void setMode_BuildFurniture (string furnitureType)
	{
		buildMode = BuildMode.FURNITURE;
		devActive = false;
		objectType = furnitureType;
		mouseController.StartBuildMode ();
	}

	public void setMode_BuildFloor ()
	{
		buildMode = BuildMode.FLOOR;
		devActive = false;
		buildModeTile = TileType.FLOOR;
		mouseController.StartBuildMode ();
	}

	public void setMode_RemoveFloor ()
	{
		buildMode = BuildMode.FLOOR;
		devActive = false;
		buildModeTile = TileType.EMPTY;
		mouseController.StartBuildMode ();
	}

	public void setMode_Deconstruct ()
	{
		buildMode = BuildMode.DECONSTRUCT;
		devActive = false;
		mouseController.StartBuildMode ();
	}

	public void doBuild (Tile tile)
	{
		if (buildMode == BuildMode.FURNITURE) {

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

			if (tile.pendingFurnitureJob == null && WorldController.Instance.world.IsFurniturePlacementValid (furnitureType, tile)) {

				Job job;

				if (WorldController.Instance.world.furnitureJobPrototypes.ContainsKey (furnitureType)) {
					job = WorldController.Instance.world.furnitureJobPrototypes [furnitureType].Clone ();
				} else {
					Debug.LogError ("There is no furnitureJobPrototype for '" + furnitureType + "'");
					job = new Job (tile, FurnitureActions.JobCompleteFurnitureBuilding, furnitureType, 0.1f, null);
				}

				job.furniturePrototype =	WorldController.Instance.world.GetFurniturePrototype (furnitureType);

				job.tile = tile;

				tile.pendingFurnitureJob = job;

				job.RegisterJobStoppedCallback ((theJob) => {
					theJob.tile.pendingFurnitureJob = null;
				});

				WorldController.Instance.world.jobQueue.Enqueue (job);
			}

		} else if (buildMode == BuildMode.FLOOR) {
			tile.Type = buildModeTile;

		} else if (buildMode == BuildMode.DECONSTRUCT) {
			if (tile.furniture != null) {
				tile.furniture.Deconstruct ();
			}

		} else {
			Debug.LogError ("doBuild - Unimplemented BuildMode");
		}
	}

	public void setMode_DevBuildFurniture (string furnitureType)
	{
		buildMode = BuildMode.FURNITURE;
		devActive = true;
		objectType = furnitureType;
		mouseController.StartBuildMode ();
	}

	public bool IsObjectDraggable ()
	{
		if (buildMode == BuildMode.FLOOR || buildMode == BuildMode.DECONSTRUCT) {
			return true;
		}

		Furniture furniturePrototype = WorldController.Instance.world.GetFurniturePrototype (objectType);
		return furniturePrototype.width == 1 && furniturePrototype.height == 1;
	}

	void Start ()
	{
		mouseController = FindObjectOfType<MouseController> ();
	}

	/*void Update ()
	{
		if (buildModeIsObject && objectType != null && objectType != "") {
			ShowFurnitureSpriteAtTile (objectType, mouseController.GetMouseOverTile ());
		
		} else {
			furniturePreview.SetActive (false);
		}
	}*/

	/*void ShowFurnitureSpriteAtTile (string furnitureType, Tile tile)
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
	}*/


}
