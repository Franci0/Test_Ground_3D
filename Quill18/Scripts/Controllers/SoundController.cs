﻿using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour
{
	float soundCooldown = 0f;

	void Start ()
	{
		WorldController.Instance.world.registerFurnitureCreatedCallback (onFurnitureCreated);
		WorldController.Instance.world.registerTileChangedCallback (onTileChanged);
	}

	void Update ()
	{
		soundCooldown -= Time.deltaTime;
	}

	void onTileChanged (Tile tile_data)
	{
		if (soundCooldown > 0) {
			return;
		}
		AudioClip audioClip = Resources.Load<AudioClip> ("Sounds/Floor_OnCreated");
		AudioSource.PlayClipAtPoint (audioClip, Camera.main.transform.position);
		soundCooldown = 0.1f;
	}

	void onFurnitureCreated (Furniture furniture)
	{
		if (soundCooldown > 0) {
			return;
		}

		AudioClip audioClip = Resources.Load<AudioClip> ("Sounds/" + furniture.FurnitureType + "_OnCreated");

		if (audioClip == null) {
			Debug.LogError ("Sound for " + furniture.FurnitureType + " not found!");
			audioClip = Resources.Load<AudioClip> ("Sounds/Wall_OnCreated");
		}

		AudioSource.PlayClipAtPoint (audioClip, Camera.main.transform.position);
		soundCooldown = 0.1f;
	}
}
