using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
	public Transform target;
	public float distanceUp;
	public float distanceback;
	public float minimumHeight;

	//Not used Directly
	private Vector3 positionVelocity;

	void FixedUpdate()
	{
		//Calculate NewPosition to place camera
		Vector3 newPosition = target.position + (target.forward * distanceback);
		newPosition.y = Mathf.Max (newPosition.y + distanceUp, minimumHeight);

		//Move the Camera
		transform.position = Vector3.SmoothDamp (transform.position, newPosition, ref positionVelocity, 0.18f);

		//Rotate the camera to look at where the drone is pointing
//		Vector3 focalPoint = target.position + (target.forward * 5);
		transform.LookAt (target);
	}
}
