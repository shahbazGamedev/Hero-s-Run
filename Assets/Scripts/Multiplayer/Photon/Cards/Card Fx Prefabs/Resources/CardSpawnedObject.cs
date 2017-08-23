﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnedObjectState
{
		Initialising = 1,
		Functioning = 2,
		BeingDestroyed = 3
}

public class CardSpawnedObject : MonoBehaviour {

	[Header("Minimap")]
	public Sprite  minimapIcon;

	[Header("Materials")]
	public Material onCreate;
	public Material onDestroy;
	public Material onFunctioning;

	//My owner (i.e. the caster who created this object)
	public string casterName;
	protected GameObject casterGameObject;
	protected Transform casterTransform;
	public SpawnedObjectState spawnedObjectState = SpawnedObjectState.Initialising;
	protected const float DELAY_BEFORE_DESTROY_EFFECTS = 1.3f;

	const float MAXIMUM_IMPACT_DISTANCE_PERCENTAGE = 0.2f;
	const float EXPLOSION_FORCE = 1500f;

	protected void setSpawnedObjectState( SpawnedObjectState newState )
	{
		spawnedObjectState = newState;
	}

	protected SpawnedObjectState getSpawnedObjectState()
	{
		return spawnedObjectState;
	}
	
	protected void setCasterName( string casterName )
	{
		this.casterName = casterName;
	}

	protected string getCasterName()
	{
		return casterName;
	}

	public virtual void activateCard()
	{
	}

	protected Transform getCaster( int casterPhotonViewId )
	{
		Transform caster = null;
		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == casterPhotonViewId )
			{
				//We found the caster
				caster = PlayerRace.players[i].transform;
				break;
			}
		}
		return caster;
	}

	/// <summary>
	/// Positions the spawned object in the center of the tile, flush with the ground if possible. The tile will become the parent of the object.
	/// </summary>
	/// <param name="additionalHeight">Use the optional additionalHeight if you want to further adjust the height.</param>
	protected void positionSpawnedObject( float additionalHeight = 0 )
	{
		RaycastHit hit;
		int originalLayer = gameObject.layer;
		gameObject.layer = MaskHandler.ignoreRaycastLayer;
		//Important: we need the +2f to start a bit above ground, or else the raycast may start a tad below the road and miss.
		if (Physics.Raycast( new Vector3( transform.position.x, transform.position.y + 2f, transform.position.z ), Vector3.down, out hit, 15f ))
		{
			if(  hit.collider.transform.root.GetComponent<SegmentInfo>() != null )
			{
				Transform tile = hit.collider.transform.root;
				transform.SetParent( tile );
				//Center the object in the middle of the road
				transform.localPosition = new Vector3( 0, 0, transform.localPosition.z );
			}
			//Position it flush with the ground
			transform.position = new Vector3( transform.position.x, hit.point.y + additionalHeight, transform.position.z );
		}
		//Now that our raycast is finished, reset the object's layer to its original value.
		gameObject.layer = originalLayer;
	}

	/// <summary>
	/// Positions the specified object in the center of the tile, flush with the ground if possible. The tile will become the parent of the object.
	/// </summary>
	/// <param name="additionalHeight">Use the optional additionalHeight if you want to further adjust the height.</param>
	protected void positionSpecifiedObject( Transform objectToCenter, float additionalHeight = 0 )
	{
		RaycastHit hit;
		int originalLayer = objectToCenter.gameObject.layer;
		objectToCenter.gameObject.layer = MaskHandler.ignoreRaycastLayer;
		//Important: we need the +2f to start a bit above ground, or else the raycast may start a tad below the road and miss.
		if (Physics.Raycast( new Vector3( objectToCenter.position.x, objectToCenter.position.y + 2f, objectToCenter.position.z ), Vector3.down, out hit, 15f ))
		{
			if(  hit.collider.transform.root.GetComponent<SegmentInfo>() != null )
			{
				Transform tile = hit.collider.transform.root;
				objectToCenter.SetParent( tile );
				//Center the object in the middle of the road
				objectToCenter.localPosition = new Vector3( 0, 0, objectToCenter.localPosition.z );
			}
			//Position it flush with the ground
			objectToCenter.position = new Vector3( objectToCenter.position.x, hit.point.y + additionalHeight, objectToCenter.position.z );
		}
		//Now that our raycast is finished, reset the object's layer to its original value.
		objectToCenter.gameObject.layer = originalLayer;
	}

	public virtual void destroySpawnedObjectNow()
	{
	}

	protected Transform getNearestTargetWithinRange( float range, int mask )
	{
		Transform nearestTarget;
		Collider[] hitColliders = Physics.OverlapSphere( transform.position, range, mask );
		nearestTarget = getNearestValidTarget( hitColliders );
		return nearestTarget;
	}

	Transform getNearestValidTarget( Collider[] hitColliders )
	{
		Transform nearestTarget = null;
		float nearestDistance = Mathf.Infinity;
		for( int i =0; i < hitColliders.Length; i++ )
		{
			//Is the target valid?
			if( !isTargetValid( hitColliders[i].transform ) ) continue;

			//Calculate the distance between this object and the potential target.
			float distanceToTarget = Vector3.Distance( transform.position, hitColliders[i].transform.position );

			//Is it the closest target?
			if( distanceToTarget < nearestDistance )
			{
				nearestTarget = hitColliders[i].transform;
				nearestDistance = distanceToTarget;
			}
		}
		return nearestTarget;
	}

	bool isTargetValid( Transform potentialTarget )
	{
		bool valid = false;
   		switch (potentialTarget.gameObject.layer)
		{
	        case MaskHandler.playerLayer:
				PlayerControl pc = potentialTarget.GetComponent<PlayerControl>();
				//A player is a valid target if:
				//He is alive.
				//He is not in the Idle state. The player is in the Idle state once the race finishes for example.
				//It is not yourself.
				valid = pc.deathType == DeathType.Alive;
 				valid = valid && pc.getCharacterState() != PlayerCharacterState.Idle;
				valid = valid && casterName != potentialTarget.name;
				valid = valid && !pc.GetComponent<PlayerSpell>().isCardActive(CardName.Cloak);
				break;
	                
	        case MaskHandler.deviceLayer:
				//A device is a valid target if:
				//The device is not in the Broken state.
				//Additional we want to only destroy devices that are behind the player.
				//For example: We don't want to destroy the jump pad in front of us which we'll most likely want to use.
				//We do however want to destroy the jump pad if it is behind us to prevent others players from using it.
				Device dev = potentialTarget.GetComponent<Device>();
				valid = dev.state != DeviceState.Broken;
				valid = valid && !getDotProduct( potentialTarget.position );
                break;

	        case MaskHandler.destructibleLayer:
				//A destructible object is a valid target if:
				//The destructible object is in the Functioning state.
				//You do not own the target. For example, if you create an Ice Wall, you don't want your Sentry to destroy it.
				CardSpawnedObject cso = potentialTarget.GetComponent<CardSpawnedObject>();
				valid = cso.spawnedObjectState == SpawnedObjectState.Functioning;
				valid = valid && casterName != cso.getCasterName();
                break;

	        case MaskHandler.levelDestructibleLayer:
				//A destructible object that is part of the level and does not have any special functionality such
				//as a simple wall or a bridge.
				valid = true;
                break;
		}
		//if( valid ) Debug.Log("isTargetValid " + potentialTarget.name );
		return valid;
	}

	/// <summary>
	/// Gets the dot product.
	/// </summary>
	/// <returns><c>true</c>, if the target is in front of this object, <c>false</c> otherwise.</returns>
	/// <param name="targetLocation">Target location.</param>
	bool getDotProduct( Vector3 targetLocation )
	{
		//The caster transform could be null. This is the case for Grenade and Trip Mine for example.
		//Since they destroy indiscriminately regardless of whether the explosion is in front or behind, return false.
		if( casterTransform == null ) return false;
		Vector3 forward = casterTransform.TransformDirection(Vector3.forward);
		Vector3 toOther = targetLocation - transform.position;
		if (Vector3.Dot(forward, toOther) < 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	protected void destroyAllTargetsWithinBlastRadius( float blastRadius, bool includePlayers )
	{
		//To add a dramatic effect, make all of the objects that have the Movable layer and a rigidbody move because of the shockwave.
		float halfRadius = blastRadius * 0.5f;
		Collider[] movableHitColliders = Physics.OverlapSphere( transform.position, blastRadius, MaskHandler.getMaskOnlyMovable() );
		for( int i = 0; i < movableHitColliders.Length; i++ )
		{
			if( movableHitColliders[i].attachedRigidbody != null )
			{
				movableHitColliders[i].attachedRigidbody.AddExplosionForce( EXPLOSION_FORCE, movableHitColliders[i].transform.position, halfRadius, EXPLOSION_FORCE );
			}
		}

		Collider[] hitColliders;
		if( includePlayers )
		{
			hitColliders = Physics.OverlapSphere( transform.position, blastRadius, MaskHandler.getMaskWithPlayerWithLevelDestructible() );
		}
		else
		{
			hitColliders = Physics.OverlapSphere( transform.position, blastRadius, MaskHandler.getMaskWithoutPlayerWithLevelDestructible() );
		}


		for( int i = 0; i < hitColliders.Length; i++ )
		{
			if( isTargetValid( hitColliders[i].transform ) )
			{
				destroyValidTarget( hitColliders[i].transform, blastRadius );
			}
		}
	}

	void destroyValidTarget( Transform potentialTarget, float blastRadius )
	{
		bool valid = false;
   		switch (potentialTarget.gameObject.layer)
		{
			case MaskHandler.playerLayer:
				//The player is immune while in the IDLE or DYING states.
				//The player is in the IDLE state after crossing the finish line for example.
				if( potentialTarget.GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Idle && potentialTarget.GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Dying )
				{
					//Ignore the caster
					if( potentialTarget.name != casterName )
					{
						valid = true;
						assessPlayerDamage( potentialTarget, blastRadius );
					}
				}
				break;
	                
	        case MaskHandler.deviceLayer:
				valid = true;
				Device dev = potentialTarget.GetComponent<Device>();
				dev.changeDeviceState(DeviceState.Broken);
                break;

	        case MaskHandler.destructibleLayer:
				valid = true;
				if( potentialTarget.GetComponent<FracturedObject>() != null ) potentialTarget.GetComponent<FracturedObject>().Explode( potentialTarget.transform.position, 400f );
				Destroy( potentialTarget.gameObject );
                break;

	        case MaskHandler.levelDestructibleLayer:
				valid = true;
				if( potentialTarget.GetComponent<FracturedObject>() != null ) potentialTarget.GetComponent<FracturedObject>().Explode( potentialTarget.transform.position, 15f );
				Destroy( potentialTarget.gameObject );
               break;
		}
		if( valid )
		{
			 Debug.Log("destroyValidTarget " + potentialTarget.name );
		}		
	}

	//If the player-to-explosion distance is within MAXIMUM_IMPACT_DISTANCE_PERCENTAGE of the blast radius, the player gets maximum damage.
	//If the player-to-explosion distance is bigger than MAXIMUM_IMPACT_DISTANCE_PERCENTAGE of the blast radius, the amount of damage decreases linearly based on the distance.
	void assessPlayerDamage( Transform potentialTarget, float blastRadius )
	{
		float distance = Vector3.Distance( potentialTarget.position, transform.position );

		if( distance <= MAXIMUM_IMPACT_DISTANCE_PERCENTAGE * blastRadius )
		{
			//The player is really close. He is getting maximum damage and it will kill him.
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
		}
		else
		{
			distance = distance - MAXIMUM_IMPACT_DISTANCE_PERCENTAGE * blastRadius;

			//Slope is (y2 - y1)/(x2 - x1)
			float slope = ( 0 - PlayerHealth.DEFAULT_HEALTH ) / blastRadius - MAXIMUM_IMPACT_DISTANCE_PERCENTAGE * blastRadius;

			//Linear equation is f(x) = a.x + b
			int damageReceivedByPlayer = (int) ( slope * distance + PlayerHealth.DEFAULT_HEALTH );

			print("assessPlayerDamage " + damageReceivedByPlayer + " for " + potentialTarget.name );

			//Will this damage kill the player?
			if ( potentialTarget.GetComponent<PlayerHealth>().getHealth() > damageReceivedByPlayer )
			{
				//No it won't. Simply deduct the damage and make the player stumble.
				potentialTarget.GetComponent<PlayerHealth>().deductHealth( damageReceivedByPlayer );
				potentialTarget.GetComponent<PlayerControl>().stumble();
			}
			else
			{
				//Yes it will.
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
			}
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

	protected IEnumerator dimVolume( AudioSource audioSource, float duration )
	{
		float elapsedTime = 0;	
		float startVolume = audioSource.volume;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			audioSource.volume = Mathf.Lerp( startVolume, 0, elapsedTime/duration );
			yield return new WaitForEndOfFrame();  
			
		} while ( elapsedTime < duration );
	}

}
