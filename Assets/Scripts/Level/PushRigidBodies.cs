using UnityEngine;
using System.Collections;

//On trigger enter by the hero, adds a small force and torque to the game objects with a rigid body as well as play a sound.
public class PushRigidBodies : MonoBehaviour {

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero")
		{
			Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody>();
			for( int i = 0; i < rigidBodies.Length; i++ )
			{
				rigidBodies[i].AddForce( 0, 20f, 120f );
				rigidBodies[i].AddTorque( 30f, 40f, 30f );
			}
			GetComponent<AudioSource>().PlayDelayed( 0.3f );
		}
	}

}
