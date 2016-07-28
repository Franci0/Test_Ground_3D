using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class MenuController : MonoBehaviour
{

	public float childHeight = 35f;

	void Start ()
	{
		adjustSize ();
	}

	void Update ()
	{
		adjustSize ();
	}

	public void adjustSize ()
	{
		Vector2 size = this.GetComponent<RectTransform> ().sizeDelta;
		size.y = this.transform.childCount * childHeight;
		this.GetComponent<RectTransform> ().sizeDelta = size;
	}
}
