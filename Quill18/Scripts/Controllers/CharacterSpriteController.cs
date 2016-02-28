using UnityEngine;
using System.Collections.Generic;

public class CharacterSpriteController : MonoBehaviour
{
	Dictionary<Character,GameObject> characterGameObjectMap;
	Dictionary<string,Sprite> characterSprites;

	World world {
		get{ return WorldController.Instance.world; }
	}

	void Start ()
	{
		loadSprites ();

		characterGameObjectMap = new Dictionary<Character, GameObject> ();

		world.registerCharacterCreatedCallback (OnCharacterCreated);

		Character character = world.createCharacter (world.getTileAt (world.Width / 2, world.Height / 2));

		//character.setDestination (world.getTileAt (world.Width / 2 + 5, world.Height / 2));
	}

	void loadSprites ()
	{
		characterSprites = new Dictionary<string, Sprite> ();

		Sprite[] sprites = Resources.LoadAll<Sprite> ("Sprites/Avatar");

		foreach (Sprite s in sprites) {
			characterSprites [s.name] = s;
		}
	}

	void OnCharacterCreated (Character character)
	{
		GameObject character_go = new GameObject ();

		characterGameObjectMap.Add (character, character_go);

		character_go.name = "Character";
		character_go.transform.position = new Vector3 (character.X, character.Y, 0);
		character_go.transform.SetParent (this.transform, true);

		SpriteRenderer sr = character_go.AddComponent<SpriteRenderer> ();
		sr.sprite = characterSprites ["Avatar"];
		sr.sortingLayerName = "Characters";


		character.registerOnChangedCallback (OnCharacterChanged);

	}

	void OnCharacterChanged (Character character)
	{
		if (characterGameObjectMap.ContainsKey (character) == false) {
			Debug.LogError ("OnCharacterChanged -- characterGameObjectMap doesn't contain key: " + character);
			return;
		}

		GameObject character_go = characterGameObjectMap [character];

		character_go.transform.position = new Vector3 (character.X, character.Y, 0);
	}

}
