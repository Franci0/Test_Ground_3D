using UnityEngine;
using System.Collections.Generic;
using System;

public class JobQueue
{
	Queue<Job> jobQueue;

	Action<Job> jobCreatedCallback;

	public JobQueue ()
	{
		jobQueue = new Queue<Job> ();
	}

	public void Enqueue (Job job)
	{
		jobQueue.Enqueue (job);

		if (jobCreatedCallback != null) {
			jobCreatedCallback (job);
		}

	}

	public Job Dequeue ()
	{
		if (jobQueue.Count == 0) {
			return null;
		}
		return jobQueue.Dequeue ();
	}

	public void registerJobCreationCallback (Action<Job> callback)
	{
		jobCreatedCallback += callback;
	}

	public void unregisterJobCreationCallback (Action<Job> callback)
	{
		jobCreatedCallback -= callback;
	}

}
