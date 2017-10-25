﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MultiplayerProjectile : CardSpawnedObject {

	[SerializeField] Light fireLight;
	[SerializeField] ParticleSystem fireParticleSystem;
	[SerializeField] ParticleSystem impactParticleSystem;
	[SerializeField] AudioClip inFlightSound;
	[SerializeField] AudioClip collisionSound;
	float bolt_force = 1600f;
	SentryController sentryController;

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;

		//The caster of this projectile
		casterTransform = getCaster( (int) data[0] );

		launchProjectile((Vector3) data[1], (float) data[3] );

		int sentryPhotonViewID = (int) data[2];
		if( sentryPhotonViewID == -1 ) return;
		//Find out which Sentry fired this projectile.
		//If the projectile hits a target, we can tell the Sentry to play a victory sound and animation.
		SentryController[] sentryControllers = GameObject.FindObjectsOfType<SentryController>();
		for( int i = 0; i < sentryControllers.Length; i++ )
		{
			if( sentryControllers[i].GetComponent<PhotonView>().viewID == sentryPhotonViewID )
			{
				sentryController = sentryControllers[i];
			}
		}
	}

	void launchProjectile( Vector3 direction, float selfDestructTime )
	{
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Rigidbody>().AddForce( direction.normalized * bolt_force );
		GameObject.Destroy( gameObject, selfDestructTime );
		GetComponent<AudioSource>().clip = inFlightSound;
		GetComponent<AudioSource>().Play();
		if( fireLight != null ) fireLight.enabled = true;
		if( fireParticleSystem != null ) fireParticleSystem.gameObject.SetActive(true);
	}

	void OnCollisionEnter(Collision collision)
	{
	    GetComponent<Rigidbody>().velocity = Vector3.zero;

		//Play collision sound.
		GetComponent<AudioSource>().PlayOneShot( collisionSound );

		if( impactParticleSystem != null )
		{
			impactParticleSystem.transform.SetParent( null );
			impactParticleSystem.gameObject.SetActive(true);
			GameObject.Destroy( impactParticleSystem, 5f );
		}

		if( collision.transform.GetComponent<FracturedObject>() != null ) collision.transform.GetComponent<FracturedObject>().Explode( collision.contacts[0].point, 400f );
		
		destroyValidTarget( collision.transform );

		GameObject.Destroy( gameObject );
  	}

	void destroyValidTarget( Transform potentialTarget )
	{
		bool valid = false;
   		switch (potentialTarget.gameObject.layer)
		{
			case MaskHandler.playerLayer:
				//The player is immune to projectiles while in the IDLE state.
				//The player is in the IDLE state after crossing the finish line for example.
				if( potentialTarget.GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Idle )
				{
					valid = true;
					//The projectile knocked down a player. Send him an RPC.
					if( getDotProduct( potentialTarget, transform.position ) )
					{
						//Explosion is in front of player. He falls backward.
						potentialTarget.GetComponent<PlayerControl>().killPlayer( DeathType.Obstacle );
					}
					else
					{
						//Explosion is behind player. He falls forward.
						potentialTarget.GetComponent<PlayerControl>().killPlayer( DeathType.FallForward );
					}
					//Attribute skill bonus
					casterTransform.GetComponent<PlayerControl>().incrementKillCounter();
				}
				break;
	                
	        case MaskHandler.deviceLayer:
				valid = true;
				Device dev = potentialTarget.GetComponent<Device>();
				dev.changeDeviceState(DeviceState.Broken);
                break;

	        case MaskHandler.destructibleLayer:
				valid = true;
				potentialTarget.GetComponent<CardSpawnedObject>().destroySpawnedObjectNow();
                break;

	        case MaskHandler.levelDestructibleLayer:
				valid = true;
				GameObject.Destroy( potentialTarget.gameObject );
                break;
		}
		if( valid )
		{
			 Debug.Log("destroyValidTarget " + potentialTarget.name );
			//Tell the Sentry that it was succesfull in killing a target.
			if( sentryController != null ) sentryController.targetHit();
		}		
	}

	/// <summary>
	/// Gets the dot product.
	/// </summary>
	/// <returns><c>true</c>, if the explosion is in front of the player, <c>false</c> otherwise.</returns>
	/// <param name="player">Player.</param>
	/// <param name="explosionLocation">Explosion location.</param>
	bool getDotProduct( Transform player, Vector3 explosionLocation )
	{
		Vector3 forward = player.TransformDirection(Vector3.forward);
		Vector3 toOther = explosionLocation - player.position;
		if (Vector3.Dot(forward, toOther) < 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

}
