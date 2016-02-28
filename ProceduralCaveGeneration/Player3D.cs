using UnityEngine;
using System.Collections;

public class Player3D : MonoBehaviour
{
	Rigidbody rigidBody;
	Vector3 velocity;

	void Start ()
	{
		rigidBody = GetComponent<Rigidbody> ();
	}

	void Update ()
	{
		velocity = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * 10;
	}

	void FixedUpdate ()
	{
		rigidBody.MovePosition (rigidBody.position + velocity * Time.deltaTime);
	}

}
