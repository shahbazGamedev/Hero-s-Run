using System.Collections;
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

	protected Transform getPlayerByViewID( int photonViewId )
	{
		Transform player = null;
		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewId )
			{
				//We found the player
				player = PlayerRace.players[i].transform;
				break;
			}
		}
		return player;
	}

	protected void spawnDecalOnTheGround( GameObject decalPrefab, Vector3 decalPosition, Quaternion decalRotation, float lifespan, float heightOffset = 0.02f )
	{
		if ( decalPrefab != null )
		{		
			RaycastHit hit;
			if (Physics.Raycast( decalPosition, Vector3.down, out hit, 10, ~MaskHandler.getMaskWithPlayersWithCreaturesWithIgnore() ))
			{		
				GameObject decal = GameObject.Instantiate( decalPrefab, decalPosition, decalRotation );
	  			decal.transform.position = new Vector3( decalPosition.x, hit.point.y + heightOffset, decalPosition.z );
				Debug.Log( "There is ground underneath the decal called " + decalPrefab.name + ". The collider underneath is: " + hit.collider.name + " at height: " + hit.point.y );
				Destroy( decal, lifespan );
			}
			else
			{
				Debug.LogWarning( "There is no ground underneath the decal called: " + decalPrefab.name + " at position " + decalPosition );
			}
		}
		else
		{
			Debug.LogError( "The ground decal specified is null." );
		}
	}

	protected float getHeightAdjusment( GameObject target )
	{
		float heightAdjustment = 0;
		if( target.CompareTag("Player") )
		{
			PlayerControl pc = target.GetComponent<PlayerControl>();

			if( pc.getCharacterState() == PlayerCharacterState.Sliding || pc.getCharacterState() == PlayerCharacterState.Turning_and_sliding )
			{
				//The player is sliding.
				heightAdjustment = 0.3f;
			}
			else
			{
				//The transform position of the player is at his feet. Let's aim at his torso.
				heightAdjustment = 1.2f;
			}
		}
		else if( target.CompareTag("Zombie") )
		{
			ZombieController zc = target.GetComponent<ZombieController>();

			if( zc.getCreatureState() == CreatureState.Crawling )
			{
				//The zombie is crawling.
				heightAdjustment = 0.3f;
			}
			else
			{
				//The transform position of the zombie is at his feet. Let's aim at his torso.
				heightAdjustment = 1.2f;
			}
		}
		return heightAdjustment; 
	}

	protected void addSkillBonus( int skillPoints, string skillTextID )
	{
		if( casterTransform != null )
		{
			if( casterTransform.GetComponent<PhotonView>().isMine && casterTransform.GetComponent<PlayerAI>() == null )
			{
				SkillBonusHandler.Instance.addSkillBonus( skillPoints, skillTextID );
			}
		}
		else
		{
			Debug.LogError("CardSpawnedObject-addSkillBonus called for " + name + " with text ID " + skillTextID + ", but casterTransform is null. Ignoring." ); 
		}
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

		//Are we trying to spawn the object in a dead end?
		//Important: we need the +6f to start above ground, or else the raycast may start below the road and miss (especially on a slope).
		if (Physics.Raycast( new Vector3( transform.position.x, transform.position.y + 6f, transform.position.z ), Vector3.down, out hit, 20f, ~MaskHandler.getMaskWithPlayersWithCreaturesWithIgnore() ))
		{
			if(  hit.collider.transform.root.GetComponent<SegmentInfo>() != null )
			{
				Transform tile = hit.collider.transform.root;
				transform.SetParent( tile );
		
				Transform currentDeadEnd = tile.Find("deadEnd");
				if( currentDeadEnd != null )
				{
					//We are trying to spawn the object in either a left or right turn tile
					//Are we trying to spawn it inside the dead end collider or outside?
					if( currentDeadEnd.GetComponent<BoxCollider>().bounds.Contains( transform.TransformPoint( 0, 2f, 0 ) ) )
					{
						//Yes, this would be inside the deadEnd collider
						//Is this a right-turn tile or a left-turn tile?
						if( currentDeadEnd.GetComponent<deadEnd>().deadEndType == DeadEndType.Right )
						{
							transform.localPosition = new Vector3( 5, 0, 0 );
						}
						else if( currentDeadEnd.GetComponent<deadEnd>().deadEndType == DeadEndType.Left )
						{
							transform.localPosition = new Vector3( -5, 0, 0 );
						}
						transform.localRotation = Quaternion.Euler(0, 90, 0 );
					}
					else
					{
						//No, this would be outside the deadEnd collider
						transform.localPosition = new Vector3( 0, 0, transform.localPosition.z );
					}
				}
				else
				{
					//We are trying to spawn the object in none-turn tile
					//Center the object in the middle of the road
					transform.localPosition = new Vector3( 0, 0, transform.localPosition.z );
				}
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
		if (Physics.Raycast( new Vector3( objectToCenter.position.x, objectToCenter.position.y + 2f, objectToCenter.position.z ), Vector3.down, out hit, 15f, ~MaskHandler.getMaskWithPlayersWithCreaturesWithIgnore() ))
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

	protected Transform getNearestTargetWithinRange( float range, int mask, bool useDotProduct )
	{
		Transform nearestTarget;
		Collider[] hitColliders = Physics.OverlapSphere( transform.position, range, mask );
		nearestTarget = getNearestValidTarget( hitColliders, useDotProduct );
		return nearestTarget;
	}

	Transform getNearestValidTarget( Collider[] hitColliders, bool useDotProduct )
	{
		Transform nearestTarget = null;
		float nearestDistance = Mathf.Infinity;
		for( int i =0; i < hitColliders.Length; i++ )
		{
			//Is the target valid?
			if( !isTargetValid( hitColliders[i].transform ) ) continue;

			if( useDotProduct && !getDotProduct( hitColliders[i].transform.position ) ) continue;

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
				//The player is not cloaked.
				//The game mode is not Coop.
				valid = pc.deathType == DeathType.Alive;
 				valid = valid && pc.getCharacterState() != PlayerCharacterState.Idle;
				valid = valid && casterName != potentialTarget.name;
				valid = valid && !pc.GetComponent<PlayerSpell>().isCardActive(CardName.Cloak);
				valid = valid && !GameManager.Instance.isCoopPlayMode();
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

	        case MaskHandler.creatureLayer:
				ICreature creatureController = potentialTarget.GetComponent<ICreature>();
				if( creatureController != null && creatureController.getCreatureState() != CreatureState.Dying && creatureController.getCreatureState() != CreatureState.Immobilized )
				{
					valid = true;
				}
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

	/// <summary>
	/// Destroys all valid targets within the blast radius.
	/// </summary>
	/// <returns>The number of players or zombies affected by the blast.</returns>
	/// <param name="blastRadius">Blast radius.</param>
	/// <param name="mask">Mask.</param>
	/// <param name="caster">Caster.</param>
	protected int destroyAllTargetsWithinBlastRadius( float blastRadius, int mask, Transform caster = null )
	{
		//number of players or zombies (in coop) successfully targeted by this blast.
		int numberOfBlastVictims = 0;

		//To add a dramatic effect, make all of the objects that have the Movable layer and a rigidbody move because of the shockwave.
		//Also affect level destructible objects.
		float halfRadius = blastRadius * 0.5f;
		Collider[] movableHitColliders = Physics.OverlapSphere( transform.position, blastRadius, MaskHandler.getMaskMovableAndLevelDestructible() );
		for( int i = 0; i < movableHitColliders.Length; i++ )
		{
			if( movableHitColliders[i].attachedRigidbody != null )
			{
				if( movableHitColliders[i].gameObject.layer == MaskHandler.movableLayer )
				{
					//Handle movables such as barrels, crates, and cones.
					movableHitColliders[i].attachedRigidbody.AddExplosionForce( EXPLOSION_FORCE, movableHitColliders[i].transform.position, halfRadius, EXPLOSION_FORCE );
				}
				else
				{
					//Handle level destructibles such as bridges
					movableHitColliders[i].attachedRigidbody.isKinematic = false;
					movableHitColliders[i].attachedRigidbody.AddExplosionForce( EXPLOSION_FORCE, movableHitColliders[i].transform.position, halfRadius, EXPLOSION_FORCE );
					GameObject.Destroy( movableHitColliders[i].gameObject, 3.5f );
				}
			}
		}

		Collider[] hitColliders = Physics.OverlapSphere( transform.position, blastRadius, mask );

		for( int i = 0; i < hitColliders.Length; i++ )
		{
			if( isTargetValid( hitColliders[i].transform ) )
			{
				bool isPlayerOrCreature = destroyValidTarget( hitColliders[i].transform, blastRadius, caster );
				if( isPlayerOrCreature ) numberOfBlastVictims++;
			}
		}
		return numberOfBlastVictims;
	}

	bool destroyValidTarget( Transform potentialTarget, float blastRadius, Transform caster = null )
	{
		bool valid = false;
		bool isPlayerOrCreature = false;
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
						assessPlayerDamage( potentialTarget, blastRadius, caster );
						isPlayerOrCreature = true;
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

	        case MaskHandler.creatureLayer:
				valid = true;
				potentialTarget.GetComponent<ICreature>().knockback( caster, false );
				isPlayerOrCreature = true;
               break;
		}
		#if UNITY_EDITOR
		if( valid )
		{
			 Debug.Log("destroyValidTarget " + potentialTarget.name );
		}
		#endif
		return isPlayerOrCreature;		
	}

	//If the player-to-explosion distance is within MAXIMUM_IMPACT_DISTANCE_PERCENTAGE of the blast radius, the player gets maximum damage.
	//If the player-to-explosion distance is bigger than MAXIMUM_IMPACT_DISTANCE_PERCENTAGE of the blast radius, the amount of damage decreases linearly based on the distance.
	void assessPlayerDamage( Transform potentialTarget, float blastRadius, Transform caster = null )
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
			//Attribute skill bonus
			if( caster != null ) caster.GetComponent<PlayerControl>().incrementKillCounter();
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
				//Attribute skill bonus
				if( caster != null ) caster.GetComponent<PlayerControl>().incrementKillCounter();
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
