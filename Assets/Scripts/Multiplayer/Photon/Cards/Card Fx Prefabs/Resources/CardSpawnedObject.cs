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
				destroyValidTarget( hitColliders[i].transform );
			}
		}
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
