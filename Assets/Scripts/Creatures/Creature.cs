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
	}


}
