using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.IO;

public class WorldController : MonoBehaviour
{
	public static WorldController Instance{ get; protected set; }

	static bool loadWorld = false;

	public int size_x;
	public int size_y;
	public float randomizeTileTimer = 2.0f;

	public World world{ get; protected set; }

	public Tile getTileAtWorldCoord (Vector3 coord)
	{
		int x = Mathf.FloorToInt (coord.x);
		int y = Mathf.FloorToInt (coord.y);
		return world.getTileAt (x, y);
	}

	public void NewWorld ()
	{
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public void SaveWorld ()
	{
		XmlSerializer serializer = new XmlSerializer (typeof(World));
		TextWriter writer = new StringWriter ();
		serializer.Serialize (writer, world);
		writer.Close ();
		PlayerPrefs.SetString ("SaveGame00", writer.ToString ());
	}

	public void LoadWorld ()
	{
		loadWorld = true;
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	void OnEnable ()
	{
		if (Instance != null) {
			Debug.LogError ("Troppe entità WorldController");
		}

		Instance = this;
		if (loadWorld == true) {
			loadWorld = false;
			CreateWorldFromSaveFile ();
		} else {
			CreateEmptyWorld ();
		}
	}

	void Update ()
	{
		world.update (Time.deltaTime);
	}

	void CreateEmptyWorld ()
	{
		world = new World (size_x, size_y);
		Camera.main.transform.position = new Vector3 (size_x / 2, size_y / 2, Camera.main.transform.position.z);
	}

	void CreateWorldFromSaveFile ()
	{
		XmlSerializer serializer = new XmlSerializer (typeof(World));
		TextReader reader = new StringReader (PlayerPrefs.GetString ("SaveGame00"));
		world = (World)serializer.Deserialize (reader);
		reader.Close ();

		Camera.main.transform.position = new Vector3 (size_x / 2, size_y / 2, Camera.main.transform.position.z);
	}
}
