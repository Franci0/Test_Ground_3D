using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof(MenuController))]
public class AutomaticMenuController : Editor
{

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		if (GUILayout.Button ("Adjust Size")) {
			((MenuController)target).adjustSize ();
		}
	}

}
