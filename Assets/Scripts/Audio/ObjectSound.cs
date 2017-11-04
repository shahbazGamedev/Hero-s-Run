using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectSound : MonoBehaviour {

	[SerializeField] AudioClip onCollisionClip;
	[SerializeField] float maximumImpactStrength = 300f;

	void OnCollisionEnter(Collision collision)
	{
		//Debug.Log("ObjectSound: Name: " + name + " Root: " + transform.root.name + " Relative Velocity Sqr Magnitude: " + collision.relativeVelocity.sqrMagnitude );
		float impactStrength = collision.relativeVelocity.sqrMagnitude;
		if( !GetComponent<AudioSource>().isPlaying && impactStrength > 0 )
		{
			//The collision.relativeVelocity.sqrMagnitude ranges between 0 and 300 or so.
			//The higher the impact strength, the louder the sound.
			impactStrength = Mathf.Clamp( impactStrength, 0, maximumImpactStrength );
			GetComponent<AudioSource>().PlayOneShot( onCollisionClip, impactStrength/maximumImpactStrength );
		}
	}
}
