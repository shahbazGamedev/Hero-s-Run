using UnityEngine;
using System.Collections;

public class Thruster : MonoBehaviour 
{
	public float thrusterStrengh;
	public float thrusterDistance;
	public Transform[] thrusters;
	private Rigidbody rbody;

	void Awake()
	{
		rbody = GetComponent<Rigidbody> ();
	}

	void FixedUpdate()
	{
		RaycastHit hit;
		foreach (Transform thruster in thrusters) 
		{
			Vector3 downwardForce;
			float distancePercentage;

			if (Physics.Raycast (thruster.position, thruster.up * -1, out hit, thrusterDistance)) 
			{
				//Thruster is Within the thrusterDistance to the ground, how far away
				distancePercentage = 1-(hit.distance/thrusterDistance);

				//Calculate how much force to push
				downwardForce = transform.up * thrusterStrengh * distancePercentage;

				//correct the force for the mass of the drone and deltaTime
				downwardForce = downwardForce * Time.deltaTime * rbody.mass;

				//Apply the force where the thruster is
				rbody.AddForceAtPosition (downwardForce, thruster.position);
			}
		}
	}
}
