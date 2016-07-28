using UnityEngine;
using System.Collections.Generic;

public class JobSpriteController : MonoBehaviour
{
	FurnitureSpriteController fsc;

	Dictionary<Job,GameObject> jobGameObjectMap;

	void Start ()
	{
		jobGameObjectMap = new Dictionary<Job, GameObject> ();

		fsc = GameObject.FindObjectOfType<FurnitureSpriteController> ();

		WorldController.Instance.world.jobQueue.registerJobCreationCallback (onJobCreated);

	}

	void onJobCreated (Job job)
	{
		if (job.jobObjectType == null) {
			return;
		}

		if (jobGameObjectMap.ContainsKey (job)) {
			Debug.LogError ("onJobCreated -- job_go already exists -- job re-enqueued?");
			return;
		}

		GameObject job_go = new GameObject ();

		jobGameObjectMap.Add (job, job_go);

		job_go.name = "JOB_" + job.jobObjectType + "_" + job.Tile.X + "_" + job.Tile.Y;
		job_go.transform.position = new Vector3 (job.Tile.X, job.Tile.Y, 0);
		job_go.transform.SetParent (this.transform, true);

		SpriteRenderer sr = job_go.AddComponent<SpriteRenderer> ();
		sr.sprite = fsc.getSpriteForFurniture (job.jobObjectType);
		sr.sortingLayerName = "Jobs";
		sr.color = new Color (0.5f, 1f, 0.5f, 0.25f);

		if (job.jobObjectType == "Door") {
			Tile westTile = job.Tile.World.getTileAt (job.Tile.X - 1, job.Tile.Y);
			Tile eastTile = job.Tile.World.getTileAt (job.Tile.X + 1, job.Tile.Y);

			if (westTile != null && eastTile != null && westTile.furniture != null && eastTile.furniture != null && westTile.furniture.furnitureType == "Wall" && eastTile.furniture.furnitureType == "Wall") {
				job_go.transform.rotation = Quaternion.Euler (0, 0, 90);
			}
		}

		job.registerJobCompleteCallback (onJobEnded);
		job.registerJobCancelCallback (onJobEnded);
	}

	void onJobEnded (Job job)
	{
		GameObject job_go = jobGameObjectMap [job];

		job.unregisterJobCancelCallback (onJobEnded);
		job.unregisterJobCompleteCallback (onJobEnded);

		Destroy (job_go);
	}

}
