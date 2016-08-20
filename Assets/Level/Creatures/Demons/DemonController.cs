﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class DemonController : Creature, ICreature {

	[Header("Demon Controller")]
	[Header("General")]
	public AttackType attackType = AttackType.stand_and_normal_attack;
	public bool applyGravity = true;
	[Header("Audio")]
	public AudioClip footstepLeftSound;
	public AudioClip footstepRightSound;
	public AudioClip ouch;
	public AudioClip fallToGround;
	public AudioClip win;
	public AudioClip swordSwoosh;
	[Header("Particle Systems")]
	public ParticleSystem sparksLeftHoof;
	public ParticleSystem sparksRightHoof;
	[Header("Other")]
	public GameObject weaponTrail;
	public Sprite demonPortrait;
	[Tooltip("List of text IDs that the demon will say when the attack type is walk_and_talk.")]
	public List<string> demonText = new List<string>(3);
	static int demonTextPointer = 0;

	public enum AttackType {
		stand_and_normal_attack = 1,
		stand_and_big_attack = 2,
		charge_and_attack = 3,
		walk_and_attack = 4,
		walk_and_talk = 5,
		do_nothing = 6
	}
	
	//Movement related
	Vector3 forward;
	const float RUN_SPEED = 4.6f; //good value so feet don't slide
	float WALK_SPEED = 3.2f; //good value so feet don't slide
	float moveSpeed = 0;

	void Awake ()
	{
		base.Awake();
		if( attackType == AttackType.walk_and_talk )
		{
			transform.Find("demon_weapon").gameObject.SetActive( false );
		}
	}

	void Update ()
	{
		moveDemon();
		handleAttackType();
	}

	void moveDemon()
	{
		if( creatureState == CreatureState.Running || creatureState == CreatureState.Walking )
		{
			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				targetPlayer();
			}
			//1) Get the direction of the demon
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
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
			float distance = Vector3.Distance(player.position,transform.position);
			float attackDistance;
		    switch (attackType)
			{
		        case AttackType.stand_and_normal_attack:
					attackDistance = 0.76f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						setCreatureState( CreatureState.Attacking );
						anim.CrossFadeInFixedTime( "Attack" , CROSS_FADE_DURATION );
					}
					break;
		                
		        case AttackType.stand_and_big_attack:
					attackDistance = 0.95f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						setCreatureState( CreatureState.Attacking );
						anim.CrossFadeInFixedTime( "Skill" , CROSS_FADE_DURATION );
					}
					break;
		                
				case AttackType.charge_and_attack:
					float chargeDistance = 2.3f * PlayerController.getPlayerSpeed();
					attackDistance = 0.97f * PlayerController.getPlayerSpeed();
					if( distance < chargeDistance )
					{
						if( distance >= attackDistance )
						{
							if( creatureState != CreatureState.Running )
							{
								//Charge
								followsPlayer = true;
								moveSpeed = RUN_SPEED;
								setCreatureState( CreatureState.Running );
								anim.CrossFadeInFixedTime( "Run" , CROSS_FADE_DURATION );
							}
						}
						else
						{
							//Attack now
							setCreatureState( CreatureState.Attacking );
							anim.CrossFadeInFixedTime( "Skill" , CROSS_FADE_DURATION );
						}
					}
					break;
				
				case AttackType.walk_and_attack:
					float walkDistance = 2.5f * PlayerController.getPlayerSpeed();
					attackDistance = 0.97f * PlayerController.getPlayerSpeed();
					if( distance < walkDistance )
					{
						if( distance >= attackDistance )
						{
							//Walk
							followsPlayer = true;
							moveSpeed = WALK_SPEED;
							setCreatureState( CreatureState.Walking );
							anim.CrossFadeInFixedTime( "Walk" , CROSS_FADE_DURATION );
						}
						else
						{
							//Attack now
							setCreatureState( CreatureState.Attacking );
							anim.CrossFadeInFixedTime( "Skill" , CROSS_FADE_DURATION );
						}
					}
					break;

				case AttackType.walk_and_talk:
					float startWalkingDistance = 3.3f * PlayerController.getPlayerSpeed();
					if( distance < startWalkingDistance )
					{
						if( getCreatureState() != CreatureState.Walking )
						{
							//Walk
							followsPlayer = false;
							moveSpeed = WALK_SPEED;
							setCreatureState( CreatureState.Walking );
							anim.Play( "Walk" );
							speak( getNextText(), 3.9f, false );
							Invoke("stopWalking", 6.8f );
						}
					}
					break;
			}
		}
	}

	string getNextText()
	{
		string nextText = demonText[demonTextPointer];
		demonTextPointer++;
		if( demonTextPointer == demonText.Count ) demonTextPointer = 0;
		return nextText;
	}

	void stopWalking()
	{
		attackType = AttackType.do_nothing;
		setCreatureState( CreatureState.Idle );
		anim.CrossFadeInFixedTime( "Stand" , CROSS_FADE_DURATION );
	}

	public void sideCollision()
	{
		GetComponent<AudioSource>().PlayOneShot( ouch );
		anim.CrossFadeInFixedTime( "Damage" , CROSS_FADE_DURATION );
	}

	public void victory( bool playWinSound )
	{
		if( creatureState != CreatureState.Dying )
		{
			if( playWinSound ) GetComponent<AudioSource>().PlayOneShot( win );
			setCreatureState( CreatureState.Victory );
			anim.CrossFadeInFixedTime( "idle" , CROSS_FADE_DURATION );
		}
	}

	//The demon falls over backwards, typically because the player slid into him or because of a ZNuke
	public void knockback()
	{
		setCreatureState( CreatureState.Dying );
		controller.enabled = false;
		CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
		for( int i = 0; i < capsuleColliders.Length; i++ )
		{
			capsuleColliders[i].enabled = false;
		}
		anim.SetTrigger("Knockback");
		GetComponent<AudioSource>().PlayOneShot( fallToGround );
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( PlayerController._characterState == CharacterState.Dying )
		{
			//The Pendulum (bad name, yes I know) is the spike road block
			if( hit.collider.name.StartsWith("Demon") || hit.collider.name.StartsWith("Hero") || hit.collider.name.StartsWith("Pendulum") )
			{
				//If a demon collides with another demon, the road block or the Hero while the player is dead, have him stop moving and play the victory sequence.
				victory( false );
			}
		}
	}

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			Stop_Weapon_Trail ( null );
			float distance = Vector3.Distance(player.position,transform.position);
			float nearby = 4f;
			if( distance < nearby )
			{
				victory( false );
				Debug.Log("Demon PlayerStateChange - player is dead and nearby");
			}
		}
	}


	public void Footstep_left ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( footstepLeftSound, 0.4f );
		sparksLeftHoof.Play();
	}

	public void Footstep_right ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( footstepRightSound, 0.4f );
		sparksRightHoof.Play();
	}

	public void Start_Weapon_Trail ( AnimationEvent eve )
	{
		weaponTrail.SetActive( true );
		GetComponent<AudioSource>().PlayOneShot( swordSwoosh );
	}

	public void Stop_Weapon_Trail ( AnimationEvent eve )
	{
		if( weaponTrail != null ) weaponTrail.SetActive( false );
	}


	//the voiceOverID is used both as text ID and as the name of the audio clip. They need to be identical.
	public void speak( string voiceOverID, float textDisplayDuration, bool hasVoiceOver )
	{
		DialogManager.dialogManager.activateDisplayGeneric( LocalizationManager.Instance.getText( voiceOverID ), demonPortrait, textDisplayDuration );
		if( hasVoiceOver ) GetComponent<AudioSource>().PlayOneShot( DialogManager.dialogManager.getVoiceOver( voiceOverID ) );
	}
}
