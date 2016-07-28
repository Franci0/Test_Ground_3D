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
		if (job.jobTime < 0) {
			job.doWork (0f);
			return;	
		}

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

	public void Remove (Job job)
	{
		List<Job> jobs = new List<Job> (jobQueue);

		if (!jobs.Contains (job)) {
			Debug.LogError ("Trying to remove a job doesn't exist on the queue");
			return;
		}

		jobs.Remove (job);
		jobQueue = new Queue<Job> (jobs);
	}

}
