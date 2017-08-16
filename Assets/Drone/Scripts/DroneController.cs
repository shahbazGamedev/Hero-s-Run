using UnityEngine;
using System.Collections;

public class DroneController : MonoBehaviour 
{
	//values that controlls the drone
	public float acceleration;
	public float rotationRate;

	//values that fakes a nice turning motion
	public float turnRotationAngle;
	public float turnRoationSeekSpeed;

	//Reference variables not directly used
	private float rotationVelocity;
	private float groundAngleVelocity;

	private Rigidbody rBody;

	void Awake()
	{
		rBody = GetComponent<Rigidbody> ();

	}

	 
	void FixedUpdate()
	{
		//Check to see if we are touching the ground
		if (Physics.Raycast (transform.position, transform.up * -1, 3f)) 
		{
			//we are on ground, enable the accelerator and increase drag
			rBody.drag = 2;

			//calculate forward force
			Vector3 forwardForce = transform.forward * acceleration * Input.GetAxis ("Vertical");

			//Correct force for the deltatime and drone mass
			forwardForce = forwardForce * Time.deltaTime * rBody.mass;

			rBody.AddForce (forwardForce);
		} else {
			//We aren't on the ground and we dont want to jusy halt in mid-air, reduce drag
			rBody.drag = 0;
		}

		//Drone can turn in the air or ground
		Vector3 turnTorgue = Vector3.up * rotationRate * Input.GetAxis ("Horizontal");
		//Correct force for deltatime and drone mass
		turnTorgue = turnTorgue * Time.deltaTime * rBody.mass;
		rBody.AddTorque (turnTorgue);

		//"fake" rotate when the drone is turning
		Vector3 newRotation = transform.eulerAngles;
		newRotation.z = Mathf.SmoothDampAngle (newRotation.z, Input.GetAxis ("Horizontal") * -turnRotationAngle, ref rotationVelocity, turnRoationSeekSpeed);
		transform.eulerAngles = newRotation;
	}
}
