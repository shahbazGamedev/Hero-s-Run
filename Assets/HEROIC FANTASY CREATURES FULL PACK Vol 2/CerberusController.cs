using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class CerberusController : Creature, ICreature {

	[Header("Cerberus Controller")]
	[Header("General")]
	public AttackType attackType = AttackType.controlled_by_script;
	public bool applyGravity = true;
	[Header("Audio")]
	public AudioClip footstepLeftSound;
	public AudioClip footstepRightSound;
	public AudioClip angrySnarl;
	public AudioClip angryBark;

	[Header("Fire Breathing Attack")]
	public GameObject leftHeadFireObject;
	public GameObject centerHeadFireObject;
	public GameObject rightHeadFireObject;
	public ParticleSystem leftHeadFire;
	public ParticleSystem centerHeadFire;
	public ParticleSystem rightHeadFire;
	public AudioClip fireBreath;
	public Light fireBreathingLight;

	public enum AttackType {
		stand_and_breathe_fire = 1,
		controlled_by_script  = 2
	}


	//Movement related
	Vector3 forward;
	float WALK_SPEED = 2.1f; //good value so feet don't slide
	float moveSpeed = 0;

	void Update ()
	{
		moveCerberus();
		handleAttackType();
		#if UNITY_EDITOR
		if ( Input.GetKeyDown (KeyCode.A) ) 
		{
			if( attackType == AttackType.stand_and_breathe_fire )
			{
				breatheFire();
			}
		}
		#endif
	}

	public void sideCollision ()
	{
	}

	public void victory ( bool playWinSound )
	{
	}

	void moveCerberus()
	{
		if( ( creatureState == CreatureState.Walking ) )
		{
			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				transform.LookAt( getPlayer() );
				transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
			}
			//1) Get the direction of the cerberus
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on move speed
			forward = forward * Time.deltaTime * moveSpeed;
			if( applyGravity ) forward.y -= 16f * Time.deltaTime;
			//3) Move the controller
			controller.Move( forward );
		}
	}

	void handleAttackType()
	{
		if( creatureState != CreatureState.Attacking && creatureState != CreatureState.Dying && creatureState != CreatureState.Victory )
		{
			float distance = Vector3.Distance(getPlayer().position,transform.position);
			float attackDistance;
		    switch (attackType)
			{
		        case AttackType.stand_and_breathe_fire:
					attackDistance = 0.95f * getPlayerController().getSpeed();
					if( distance < attackDistance  )
					{
						breatheFire();
					}
					break;
			}
		}
	}

	public void walk()
	{
		followsPlayer = true;
		moveSpeed = WALK_SPEED;
		setCreatureState( CreatureState.Walking );
		anim.CrossFadeInFixedTime("walk", 0.7f);
	}

	public void idle()
	{
		setCreatureState( CreatureState.Idle );
		anim.CrossFadeInFixedTime("idleBreathe", 0.5f);
	}

	void breatheFire()
	{
		setCreatureState( CreatureState.Attacking );
		anim.CrossFadeInFixedTime("blowFireAggressive", 0.5f);
		centerHeadFireObject.SetActive( true );
		centerHeadFire.Play();
		leftHeadFireObject.SetActive( true );
		leftHeadFire.Play();
		rightHeadFireObject.SetActive( true );
		rightHeadFire.Play();
		GetComponent<AudioSource>().clip = fireBreath;
		GetComponent<AudioSource>().Play();
		StartCoroutine( Utilities.fadeInLight( fireBreathingLight, 0.6f, 6f ) );
		Invoke( "stopBreathingFire", 2.8f );
	}

	void stopBreathingFire()
	{
		centerHeadFireObject.SetActive( false );
		leftHeadFireObject.SetActive( false );
		rightHeadFireObject.SetActive( false );
		StartCoroutine( Utilities.fadeOutLight( fireBreathingLight, 0.2f ) );
	}

	//The cerberus falls over backwards, typically because of a ZNuke
	public new void knockback()
	{
		base.knockback();
		stopBreathingFire();
		anim.Play("deathAggressive");
	}

	public new void resetCreature()
	{
		//Only reset Cerebus if the player died on the same tile as Cerebus.
		if( transform.parent.name == getPlayer().GetComponent<PlayerController>().getCurrentTileName() )
		{
			setCreatureState( CreatureState.Idle );
			anim.Play("idleLookAround");
			gameObject.SetActive( false );
			followsPlayer = false;
			controller.enabled = true;
			CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
			for( int i = 0; i < capsuleColliders.Length; i++ )
			{
				capsuleColliders[i].enabled = true;
			}
		}
	}

	//walk
	public void Footstep_left ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( footstepLeftSound, 0.5f );
	}

	//walk
	public void Footstep_right ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( footstepRightSound, 0.5f );
	}

	//biteNormal
	public void snarl ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( angrySnarl );
	}

	//biteAggressive
	public void bark ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( angryBark );
	}

	//biteAggressive
	public void End_bite_aggressive ( AnimationEvent eve )
	{
		breatheFire();
	}

}
