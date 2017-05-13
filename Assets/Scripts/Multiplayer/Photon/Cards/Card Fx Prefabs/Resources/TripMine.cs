using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Trip mine.
/// For the OnTriggerEnter method to be called, the BoxCollider component which is used as the trigger must be above the MeshCollider component.
/// </summary>
public class TripMine : CardSpawnedObject {
	
	[SerializeField] ParticleSystem explosionEffect;
	float blastRadius;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;

		casterName = data[0].ToString();

		float delayBeforeSpellExpires = (float) data[1];
		GameObject.Destroy( gameObject, delayBeforeSpellExpires );

		blastRadius = (float) data[2];

		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position the trip mine flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject( 0 );
	}

	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") && other.gameObject.name != casterName )
		{
			startDetonationCountdown();
		}
	}

	void startDetonationCountdown()
	{
		GetComponent<AudioSource>().Play();
		Invoke( "detonate", 0.94f ); //the bomb beeps lasts 0.94 seconds
	}

	void detonate()
	{
		destroyAllTargetsWithinBlastRadius( blastRadius, true );
		explode();
		GameObject.Destroy( gameObject );
	}

	void explode()
	{
		ParticleSystem effect = GameObject.Instantiate( explosionEffect, transform.position, transform.rotation );
		effect.GetComponent<AudioSource>().Play ();
		effect.Play();
		//Destroy the particle effect after a few seconds
		GameObject.Destroy( effect.gameObject, 3f );
	}

	void Update()
	{
		#if UNITY_EDITOR
		handleKeyboard();
		#endif
	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.Y) ) 
		{
			startDetonationCountdown();
		}
	}
}