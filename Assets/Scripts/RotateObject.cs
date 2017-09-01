using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour {

[SerializeField] float rotationSpeed = 1.5f;  //This will determine max rotation speed, you can adjust in the inspector

	void Update()
	{
		//If you want to prevent rotation, just don't call this method
		rotate();
	}
	
	void rotate()
	{
		transform.Rotate(-Camera.main.transform.up, Input.GetAxis("Mouse X") * rotationSpeed, Space.World);
	}
}
