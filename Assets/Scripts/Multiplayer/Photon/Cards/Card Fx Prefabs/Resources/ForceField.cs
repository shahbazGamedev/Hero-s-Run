using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ForceField : CardSpawnedObject {
	
	[SerializeField] ParticleSystem activeForceFieldFx;
	[SerializeField] AudioClip collisionSound;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Read the data
		object[] data = this.gameObject.GetPhotonView ().instantiationData;
		
		GetComponent<BoxCollider>().isTrigger = true;

		//Remember who the caster is
		casterName = data[0].ToString();

		//Destroy the force field when the spell expires
		float spellDuration = (float) data[1];
		StartCoroutine( destroySpawnedObject( spellDuration, DELAY_BEFORE_DESTROY_EFFECTS ) );

		//Adjust the height
		float height = (float) data[2];
		transform.localScale = new Vector3( 8f, height, 1f );

		//Display the force field icon on the minimap
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position it flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject( height * 0.5f );

		StartCoroutine( activate( 1f ) );
	}

	void OnCollisionEnter(Collision collision)
	{
		//Play collision sound at point of impact
		AudioSource.PlayClipAtPoint( collisionSound, collision.contacts[0].point );
  	}

	IEnumerator activate( float delayBeforeActivation )
	{
		yield return new WaitForSeconds(delayBeforeActivation);
		setSpawnedObjectState(SpawnedObjectState.Functioning);
		GetComponent<BoxCollider>().isTrigger = false;
		activeForceFieldFx.Play();
	}

	public override void destroySpawnedObjectNow()
	{
		StartCoroutine( destroySpawnedObject( 0, DELAY_BEFORE_DESTROY_EFFECTS ) );
	}

	IEnumerator destroySpawnedObject( float delayBeforeExpires, float delayBeforeDestroyEffects )
	{
		yield return new WaitForSeconds(delayBeforeExpires);
		GetComponent<BoxCollider>().isTrigger = true;
		setSpawnedObjectState(SpawnedObjectState.BeingDestroyed);
		StopCoroutine( "activate" );
		activeForceFieldFx.Stop();
		yield return new WaitForSeconds(delayBeforeDestroyEffects);
		Destroy( gameObject );
	}
}