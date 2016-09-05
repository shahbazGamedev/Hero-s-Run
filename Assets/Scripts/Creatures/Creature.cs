using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Creature : BaseClass {

	protected CreatureState creatureState = CreatureState.Idle;
	[Header("Other")]
	protected Transform player;
	protected CharacterController controller;
	protected Animator anim;
	protected const float CROSS_FADE_DURATION = 0.5f;
	//If true, the creature heads for the player as opposed to staying in his lane
	public bool followsPlayer = false;
	[Tooltip("Speed at which to lock on player.")]
	public float enemyAimSpeed = 7.6f;
	[Header("Audio")]
	public AudioClip knockbackSound;
	[Header("Look At IK")]
	public float lookAtWeight = 0.8f;
	public float bodyWeight = 0.7f;
	public float headWeight = 1f;
	public float eyesWeight = 1f;
	public float clampWeight = 1f;
	bool lookAtActive = false;
	bool enableIK = true;

	protected void Awake ()
	{
		controller = GetComponent<CharacterController>();
		player = GameObject.FindGameObjectWithTag("Player").transform;
		anim = GetComponent<Animator>();
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

	public void resetCreature()
	{
		setCreatureState( CreatureState.Idle );
		anim.CrossFadeInFixedTime( "idle", CROSS_FADE_DURATION );
		gameObject.SetActive( false );
		followsPlayer = false;
		controller.enabled = true;
		CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
		for( int i = 0; i < capsuleColliders.Length; i++ )
		{
			capsuleColliders[i].enabled = true;
		}
		if( GetComponent<Rigidbody>() != null ) GetComponent<Rigidbody>().isKinematic = true;
		enableIK = true;
	}

	//The creature falls over backwards, typically because the player slid into him or because of a ZNuke
	public void knockback()
	{
		setCreatureState( CreatureState.Dying );
		controller.enabled = false;
		//Some creatures (usually the ones carrying a weapon) have more than one capsule colliders.
		CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
		for( int i = 0; i < capsuleColliders.Length; i++ )
		{
			capsuleColliders[i].enabled = false;
		}
		GetComponent<AudioSource>().PlayOneShot( knockbackSound );
	}

	//For SetLookAtPosition to work, there are 2 conditions:
	//The rig must be Humanoid
	//In the Animator windows, under Layers, under Settings, you must have the IK Pass toggled on.
	void OnAnimatorIK()
	{
		if( enableIK && getDotProduct() > 0.55f )
		{
			float distance = Vector3.Distance(player.position,transform.position);
			if( distance < 24f )			
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

}
