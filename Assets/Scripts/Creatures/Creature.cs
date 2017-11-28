using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Creature : MonoBehaviour {

	protected CreatureState creatureState = CreatureState.Idle;
	[Header("Other")]
	protected Transform player;
	protected CharacterController controller;
	protected Animator anim;
	protected AudioSource audioSource;
	protected const float CROSS_FADE_DURATION = 0.5f;
	//If true, the creature heads for the player as opposed to staying in his lane
	public bool followsPlayer = false;
	const float GET_PLAYER_DISTANCE = 40f;

	[Tooltip("Speed at which to lock on player.")]
	public float enemyAimSpeed = 7.6f;

	[Header("Audio")]
	public AudioSource voiceOverAudioSource;
	public AudioClip knockbackSound;

	[Header("Look At IK")]
	public float lookAtWeight = 0.8f;
	public float bodyWeight = 0.7f;
	public float headWeight = 1f;
	public float eyesWeight = 1f;
	public float clampWeight = 1f;
	public float activeDistanceIK = 24f;
	public float dotProductIK = 0.55f;
	bool lookAtActive = false;
	bool enableIK = true;

	protected void Awake ()
	{
		controller = GetComponent<CharacterController>();
		anim = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
	}

	public CreatureState getCreatureState()
	{
		return creatureState;
	}

	public void setCreatureState( CreatureState state )
	{
		creatureState = state;
		if( creatureState == CreatureState.Dying ) enableIK = false;
	}

	/*
		returns:
		-1 if creature is behind player
		+1 if creature is in front
		0 if creature is on the side
		0.5 if creature is facing player and within 60 degrees (i.e. between 30 degrees to the left and 30 degrees to the right)
	*/
	protected float getDotProduct()
	{
		Vector3 heading = player.position - transform.position;
		return Vector3.Dot( heading.normalized, transform.forward );
	}

	protected void targetPlayer()
	{
		Vector3 relativePos = player.position - transform.position;
		Quaternion desiredRotation = Quaternion.LookRotation( relativePos ); 
		desiredRotation.x = 0f;
		desiredRotation.z = 0f;
		transform.rotation = Quaternion.Lerp( transform.rotation, desiredRotation, Time.deltaTime * enemyAimSpeed );
	}

	public void deactivate()
	{
		gameObject.SetActive( false );
	}

	public virtual void knockback( Transform attacker )
	{
		if( getCreatureState() == CreatureState.Dying ) return; //Ignore. The creature is already dead.

		if( attacker != null )
		{
			GetComponent<PhotonView>().RPC("knockbackRPC", PhotonTargets.All, attacker.GetComponent<PhotonView>().viewID );
		}
		else
		{
			Debug.LogWarning("Creature-knockback: the attacker specified is null." );
		}
	}

	public virtual void shrink( Transform caster, bool value )
	{
		if( getCreatureState() == CreatureState.Dying ) return; //Ignore. The creature is already dead.

		if( caster != null )
		{
			GetComponent<PhotonView>().RPC("shrinkRPC", PhotonTargets.All, value, caster.GetComponent<PhotonView>().viewID );
		}
		else
		{
			Debug.LogWarning("Creature-shrink: the caster specified is null." );
		}
	}

	public virtual void confuse( Transform caster, bool value )
	{
		if( getCreatureState() == CreatureState.Dying ) return; //Ignore. The creature is already dead.

		if( caster != null )
		{
			GetComponent<PhotonView>().RPC("confuseRPC", PhotonTargets.All, value, caster.GetComponent<PhotonView>().viewID );
		}
		else
		{
			Debug.LogWarning("Creature-confuse: the caster specified is null." );
		}
	}

	//For SetLookAtPosition to work, there are 2 conditions:
	//The rig must be Humanoid
	//In the Animator windows, under Layers, under Settings, you must have the IK Pass toggled on.
	void OnAnimatorIK()
	{
		if( enableIK && getDotProduct() > dotProductIK )
		{
			float distance = Vector3.Distance(player.position,transform.position);
			if( distance < activeDistanceIK )			
			{
				if( !lookAtActive )
				{
 					StartCoroutine( fadeInLookAtPosition( 0.8f, 0.7f ) );
				} 
				anim.SetLookAtPosition( player.position );
				anim.SetLookAtWeight( lookAtWeight, bodyWeight, headWeight, eyesWeight, clampWeight );
			}
		}
	}

	protected IEnumerator fadeOutLookAtPosition( float finalWeight, float stayDuration, float fadeDuration )
	{
		float elapsedTime = 0;
		
		//Stay
		yield return new WaitForSeconds(stayDuration);
		
		//Fade out
		elapsedTime = 0;
		
		float initialWeight = lookAtWeight;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			lookAtWeight = Mathf.Lerp( initialWeight, finalWeight, elapsedTime/fadeDuration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < fadeDuration );
		
		lookAtWeight = finalWeight;
	
	}

	protected IEnumerator fadeInLookAtPosition( float finalWeight, float fadeDuration )
	{
		lookAtActive = true;
		float elapsedTime = 0;

		//Fade in
		elapsedTime = 0;
		
		float initialWeight = 0;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			lookAtWeight = Mathf.Lerp( initialWeight, finalWeight, elapsedTime/fadeDuration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < fadeDuration );
		
		lookAtWeight = finalWeight;
	
	}

	//Called when the player starts falling (like down a ravine). We don't want the creature to follow.
	protected void halt()
	{
		setCreatureState(CreatureState.Idle);
		//anim.CrossFadeInFixedTime( idleAnimationName, CROSS_FADE_DURATION );
	}

	protected Transform getPlayer()
	{
		if( player == null )
		{
			player = getNearestTargetWithinRange( GET_PLAYER_DISTANCE, MaskHandler.getMaskOnlyPlayer() );
			//if( player != null ) print("Zombie named: " + name + " selected target: " + player.name );
		}
		else if( player.GetComponent<PlayerSpell>().isCardActive( CardName.Cloak ) )
		{
			//Our target has activated cloak. Forget our target.
			player = null;
		}
		return player;
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
				//The player is not cloaked.
				//The game mode is not Coop.
				valid = pc.deathType == DeathType.Alive;
 				valid = valid && pc.getCharacterState() != PlayerCharacterState.Idle;
				valid = valid && !pc.GetComponent<PlayerSpell>().isCardActive(CardName.Cloak);
				break;
		}
		//if( valid ) Debug.Log("isTargetValid " + potentialTarget.name );
		return valid;
	}

}
