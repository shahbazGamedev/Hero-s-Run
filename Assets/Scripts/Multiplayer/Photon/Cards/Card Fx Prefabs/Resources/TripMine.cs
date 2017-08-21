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
		//Note that the Trip Mine prefab has its MeshRenderer disabled and trigger box collider.
		//We will enable them only when the card gets activated by the lockstep manager.
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.CARD, gameObject, CardName.Trip_Mine );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	public override void activateCard()
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;

		casterName = data[0].ToString();

		float delayBeforeExpires = (float) data[1];
		GameObject.Destroy( gameObject, delayBeforeExpires );

		blastRadius = (float) data[2];

		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position the trip mine flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject( 0 );

		//We can now make the trip mine visible and triggerable
		GetComponent<BoxCollider>().enabled = true;
		transform.FindChild("Sci-fi Mine base").GetComponent<MeshRenderer>().enabled = true;
		transform.FindChild("Sci-fi Mine leg").GetComponent<MeshRenderer>().enabled = true;
	}

	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Player") && other.name != casterName )
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
}