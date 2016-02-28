using UnityEngine;
using System.Collections.Generic;
using System;

public class Job
{
	public Tile Tile{ get; protected set; }

	float jobTime;

	Action<Job> jobCompleteCallback;
	Action<Job> jobCancelCallback;

	//to fix
	public string jobObjectType{ get; protected set; }


	public Job (Tile tile, Action<Job> jobCompleteCallback, string jobObjectType, float jobTime = 1.0f)
	{
		this.Tile = tile;
		this.jobTime = jobTime;
		this.jobCompleteCallback += jobCompleteCallback;
		this.jobObjectType = jobObjectType;
	}

	public void registerJobCompleteCallback (Action<Job> callback)
	{
		jobCompleteCallback += callback;
	}

	public void unregisterJobCompleteCallback (Action<Job> callback)
	{
		jobCompleteCallback -= callback;
	}

	public void registerJobCancelCallback (Action<Job> callback)
	{
		jobCancelCallback += callback;
	}

	public void unregisterJobCancelCallback (Action<Job> callback)
	{
		jobCancelCallback -= callback;
	}

	public void doWork (float workTime)
	{
		//Debug.Log (jobTime + " , " + workTime);
		jobTime -= workTime;

		if (jobTime <= 0 && jobCompleteCallback != null) {
			jobCompleteCallback (this);
		}
	}

	public void cancelJob ()
	{
		if (jobCancelCallback != null) {
			jobCancelCallback (this);
		}
	}
}
