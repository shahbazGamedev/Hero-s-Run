﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HomingMissile : CardSpawnedObject {

	[SerializeField] ParticleSystem inFlightParticleSystem;
	[SerializeField] ParticleSystem impactParticleSystem;
	[SerializeField] AudioClip inFlightSound;
	[SerializeField] AudioClip collisionSound;
	float missileVelocity = 42f;
	//Important: if the turn value is too small, you may see the missile spin around the target without ever hitting it because the turn radius is too big.
	//A turn value of 24 for a missile velocity of 40 works well.
	float turn = 24f;
	Rigidbody homingMissile;
	Transform target;
	PlayerControl targetPlayerControl;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;

		casterTransform = getCaster( (int) data[0] );
		setCasterName( casterTransform.name );

		//We want the homing missile to self-destruct after a while
		GameObject.Destroy( gameObject, (float) data[1] );

		StartCoroutine( launchMissile() );
	}

	IEnumerator launchMissile()
	{
		yield return new WaitForSeconds(0.25f);

		homingMissile = GetComponent<Rigidbody>();
		target = getNearestTargetWithinRange( Mathf.Infinity, MaskHandler.getMaskOnlyPlayer() );
		if( target != null ) targetPlayerControl = target.GetComponent<PlayerControl>();
		//if( target != null ) print("Homing Missile target is " + target.name );
		homingMissile.isKinematic = false;
		GetComponent<AudioSource>().clip = inFlightSound;
		GetComponent<AudioSource>().Play();
		if( inFlightParticleSystem != null ) inFlightParticleSystem.gameObject.SetActive(true);

		//Add an icon on the minimap
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );
	}

	void LateUpdate()
	{
		if( homingMissile == null ) return;

		//If the player died or is now in the Idle state (because of Stasis for example) or the player now has the Cloak card active, nullify the target
		if( targetPlayerControl != null && ( targetPlayerControl.getCharacterState() == PlayerCharacterState.Dying || targetPlayerControl.getCharacterState() == PlayerCharacterState.Idle || targetPlayerControl.GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) ) )
		{
			target = null;
			targetPlayerControl = null;
		}

		if( target == null )
		{
			//If the target couldn't be found or is no longer present (maybe a player disconnected), just continue straight until you either self-destruct or hit an obstacle.
			homingMissile.velocity = transform.forward * missileVelocity;
		}
		else
		{
			homingMissile.velocity = transform.forward * missileVelocity;
			//Aim for the torso, not the feet
			Quaternion targetRotation = Quaternion.LookRotation( new Vector3( target.position.x, target.position.y + 1.4f, target.position.z ) - transform.position ); 
			homingMissile.MoveRotation( Quaternion.RotateTowards( transform.rotation, targetRotation, turn ) );
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if( homingMissile == null ) return;
		homingMissile.velocity = Vector3.zero;
		homingMissile = null;

		//Play collision sound.
		GetComponent<AudioSource>().PlayOneShot( collisionSound );

		if( impactParticleSystem != null )
		{
			impactParticleSystem.transform.SetParent( null );
			impactParticleSystem.gameObject.SetActive(true);
			GameObject.Destroy( impactParticleSystem, 5f );
		}

		destroyAllTargetsWithinBlastRadius( 15f, MaskHandler.getMaskWithPlayerWithLevelDestructible(), casterTransform );

		GameObject.Destroy( gameObject );
		
  	}

}
