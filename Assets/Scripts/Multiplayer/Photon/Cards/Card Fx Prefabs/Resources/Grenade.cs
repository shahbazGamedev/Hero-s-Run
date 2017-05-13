using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Grenade.
/// </summary>
public class Grenade : CardSpawnedObject {
	
	[SerializeField] ParticleSystem explosionEffect;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;

		casterName = data[0].ToString();

		StartCoroutine( startDetonationCountdown( (float) data[1] ) );
	}

	IEnumerator startDetonationCountdown( float blastRadius )
	{
		GetComponent<AudioSource>().Play();
		//the bomb beeps lasts 0.94 seconds
		yield return new WaitForSeconds(0.94f);

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

}