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


	protected const int ignoreRaycastLayer = 2;
	protected const int playerLayer = 8;
	protected const int deviceLayer = 16;
	protected const int destructibleLayer = 17;
	protected const int levelDestructibleLayer = 18;
	int defaultMask;

	protected const float DELAY_BEFORE_DESTROY_EFFECTS = 1.3f;

	// Use this for initialization
	void Start ()
	{
		initialiseMask();
	}

	void initialiseMask()
	{
		defaultMask = 1 << playerLayer;
		defaultMask |= 1 << deviceLayer;
		defaultMask |= 1 << destructibleLayer;
		defaultMask |= 1 << levelDestructibleLayer;
	}

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

	//Position the object flush with the ground and try to center it in the middle of the road if possible.
	//Use the optional additionalHeight if you want to further adjust the height.
	protected void positionSpawnedObject( float additionalHeight = 0 )
	{
		RaycastHit hit;
		int originalLayer = gameObject.layer;
		gameObject.layer = ignoreRaycastLayer;
		if (Physics.Raycast( new Vector3( transform.position.x, transform.position.y + transform.localScale.y, transform.position.z ), Vector3.down, out hit, 10 * transform.localScale.y ))
		{
			if(  hit.collider.transform.parent.GetComponent<SegmentInfo>() != null )
			{
				Transform tile = hit.collider.transform.parent;
				transform.SetParent( tile );
				//Center the object in the middle of the road
				transform.localPosition = new Vector3( 0, 0, transform.localPosition.z );
				transform.SetParent( null );
			}
			//Position it flush with the ground
			float objectHalfHeight = transform.localScale.y * 0.5f;
			transform.position = new Vector3( transform.position.x, hit.point.y + objectHalfHeight + additionalHeight, transform.position.z );
		}
		//Now that our raycast is finished, reset the object's layer to its original value.
		gameObject.layer = originalLayer;
	}

	public virtual void destroySpawnedObjectNow()
	{
	}

	protected Transform getNearestTargetWithinRange( float range )
	{
		Transform nearestTarget;
		Collider[] hitColliders = Physics.OverlapSphere( transform.position, range, defaultMask );
		nearestTarget = getNearestValidTarget( hitColliders );
		return nearestTarget;
	}

	Transform getNearestValidTarget( Collider[] hitColliders )
	{
		Transform nearestTarget = null;
		float nearestDistance = 100000;
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
	        case playerLayer:
				PlayerControl pc = potentialTarget.GetComponent<PlayerControl>();
				//A player is a valid target if:
				//He is alive.
				//He is not in the Idle state. The player is in the Idle state once the race finishes for example.
				//It is not yourself.
				valid = pc.deathType == DeathType.Alive;
 				valid = valid && pc.getCharacterState() != PlayerCharacterState.Idle;
				valid = valid && casterName != potentialTarget.name;
				break;
	                
	        case deviceLayer:
				//A device is a valid target if:
				//The device is not in the Broken state.
				//Additional we want to only destroy devices that are behind the player.
				//For example: We don't want to destroy the jump pad in front of us which we'll most likely want to use.
				//We do however want to destroy the jump pad if it is behind us to prevent others players from using it.
				Device dev = potentialTarget.GetComponent<Device>();
				valid = dev.state != DeviceState.Broken;
				valid = valid && !getDotProduct( potentialTarget.position );
                break;

	        case destructibleLayer:
				//A destructible object is a valid target if:
				//The destructible object is in the Functioning state.
				//You do not own the target. For example, if you create an Ice Wall, you don't want your Sentry to destroy it.
				CardSpawnedObject cso = potentialTarget.GetComponent<CardSpawnedObject>();
				valid = cso.spawnedObjectState == SpawnedObjectState.Functioning;
				valid = valid && casterName != cso.getCasterName();
                break;

	        case levelDestructibleLayer:
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

}
