using UnityEngine;
using System.Collections;

public class HoverAudio : MonoBehaviour
{
	public AudioSource hoverSound;
	private float hoverPitch;
	private const float LowPitch = 0.8f;
	private const float HighPitch = 2.5f;
	private const float SpeedToRevs = 0.5f;
	Vector3 myVelocity;
	Rigidbody droneRigidbody;

	 
	void Awake ()
	{
		droneRigidbody = GetComponent<Rigidbody> ();
	}

	void FixedUpdate () 
	{
		myVelocity = droneRigidbody.velocity;
		float movementSpeed = transform.InverseTransformDirection(myVelocity).z;
		float engineRevs = Mathf.Abs (movementSpeed) * SpeedToRevs;
		hoverSound.pitch = Mathf.Clamp (engineRevs, LowPitch, HighPitch);

	}
}
