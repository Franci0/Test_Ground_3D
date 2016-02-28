using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildModeController : MonoBehaviour
{
	TileType buildModeTile = TileType.Floor;
	bool buildModeIsObject = false;
	bool devActive = false;
	string buildModeObjectType;

	public void setMode_BuildFurniture (string furnitureType)
	{
		buildModeIsObject = true;
		devActive = false;
		buildModeObjectType = furnitureType;
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

			string furnitureType = buildModeObjectType;

			if (devActive == true) {
				WorldController.Instance.world.placeFurniture (furnitureType, tile);
				return;
			}

			/*foreach (Job job in WorldController.Instance.World.jobList) {
				if (job != null && job.Tile.Equals (tmpTile)) {
					Debug.Log ("Tile occupied by another job");
					continue;
				}
			}*/

			if (tile.pendingFurnitureJob == null && WorldController.Instance.world.isFurniturePlacementValid (furnitureType, tile) == true) {

				Job job = new Job (tile, (theJob) => {	
					WorldController.Instance.world.placeFurniture (furnitureType, theJob.Tile);
					tile.pendingFurnitureJob = null;
				}, furnitureType);

				tile.pendingFurnitureJob = job;

				job.registerJobCancelCallback ((theJob) => {
					theJob.Tile.pendingFurnitureJob = null;
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
		buildModeObjectType = furnitureType;
	}

	/*public void doPathfindingTest ()
	{
		Path_TileGraph tileGraph = new Path_TileGraph (WorldController.Instance.world);
	}*/

}
