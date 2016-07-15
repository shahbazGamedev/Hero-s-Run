using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WraithController : BaseClass, ICreature {

	[Header("Wraith Controller")]
	[Header("General")]
	public AttackType attackType = AttackType.stand_and_normal_attack;
	[Tooltip("Speed at which to lock on player.")]
	public float enemyAimSpeed = 7f;
	[Header("Audio")]
	public AudioClip ouch;
	public AudioClip fallToGround;
	public AudioClip win;
	public AudioClip weaponSwoosh;
	[Header("Particle Systems")]
	public Sprite wraithPortrait;
	[Header("Weapons")]
	[Tooltip("The mesh includes both an axe and a scythe. Both have two LOD levels. We need references in order to disable the unused ones.")]
	public WeaponType weaponType = WeaponType.Axe;
	public GameObject weaponAxeLOD0;
	public GameObject weaponAxeLOD1;
	public GameObject weaponTrailAxe;
	public GameObject weaponScytheLOD0;
	public GameObject weaponScytheLOD1;
	public GameObject weaponTrailScythe;


	public enum AttackType {
		stand_and_normal_attack = 1,
		stand_and_big_attack = 2,
		charge_and_attack = 3,
		walk_and_attack = 4,
		walk_and_talk = 5,
		do_nothing = 6
	}

	public enum WeaponType {
		Axe = 1,
		Scythe = 2
	}
	
	Transform player;
	const float CROSS_FADE_DURATION = 0.5f;

	//Movement related
	CreatureState wraithState = CreatureState.Idle;
	CharacterController controller;
	Vector3 forward;
	const float RUN_SPEED = 4.6f; //good value so feet don't slide
	float WALK_SPEED = 3.2f; //good value so feet don't slide
	float moveSpeed = 0;
	//If true, the wraith heads for the player as opposed to staying in his lane
	bool followsPlayer = false;

	void Awake ()
	{
		controller = GetComponent<CharacterController>();
		player = GameObject.FindGameObjectWithTag("Player").transform;
		if( attackType == AttackType.walk_and_talk )
		{
			//transform.Find("wraith_weapon").gameObject.SetActive( false );
		}
		configureSelectedWeapon();
	}

	void configureSelectedWeapon()
	{
		if( weaponType == WeaponType.Scythe )
		{
			weaponAxeLOD0.SetActive( false );
			weaponAxeLOD1.SetActive( false );
			weaponScytheLOD0.SetActive( true );
			weaponScytheLOD1.SetActive( true );
		}
		else
		{
			weaponAxeLOD0.SetActive( true );
			weaponAxeLOD1.SetActive( true );
			weaponScytheLOD0.SetActive( false );
			weaponScytheLOD1.SetActive( false );
		}
	}

	void Update ()
	{
		moveWraith();
		handleAttackType();
	}

	void moveWraith()
	{
		if( wraithState == CreatureState.Running || wraithState == CreatureState.Walking )
		{
			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				targetPlayer();
			}
			//1) Get the direction of the wraith
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			forward = forward * Time.deltaTime * moveSpeed;
			//3) Move the controller
			controller.Move( forward );
		}
	}

	void targetPlayer()
	{
		Vector3 relativePos = player.position - transform.position;
		Quaternion desiredRotation = Quaternion.LookRotation( relativePos ); 
		desiredRotation.x = 0f;
		desiredRotation.z = 0f;
		transform.rotation = Quaternion.Lerp( transform.rotation, desiredRotation, Time.deltaTime * enemyAimSpeed );

	}

	void handleAttackType()
	{
		if( wraithState != CreatureState.Attacking && wraithState != CreatureState.Dying && wraithState != CreatureState.Victory )
		{
			float distance = Vector3.Distance(player.position,transform.position);
			float attackDistance;
		    switch (attackType)
			{
		        case AttackType.stand_and_normal_attack:
					attackDistance = 0.52f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setCreatureState( CreatureState.Attacking );
						GetComponent<Animator>().CrossFadeInFixedTime( "attack2" , CROSS_FADE_DURATION );
					}
					break;
		                
		        case AttackType.stand_and_big_attack:
					attackDistance = 0.52f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setCreatureState( CreatureState.Attacking );
						GetComponent<Animator>().CrossFadeInFixedTime( "attack1" , CROSS_FADE_DURATION );
					}
					break;
		                
				case AttackType.charge_and_attack:
					float chargeDistance = 2.3f * PlayerController.getPlayerSpeed();
					attackDistance = 0.97f * PlayerController.getPlayerSpeed();
					if( distance < chargeDistance )
					{
						if( distance >= attackDistance )
						{
							//Charge
							followsPlayer = true;
							moveSpeed = RUN_SPEED;
							setCreatureState( CreatureState.Running );
							GetComponent<Animator>().CrossFadeInFixedTime( "move" , CROSS_FADE_DURATION );
						}
						else
						{
							//Attack now
							setCreatureState( CreatureState.Attacking );
							GetComponent<Animator>().CrossFadeInFixedTime( "attack1" , CROSS_FADE_DURATION );
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
							GetComponent<Animator>().CrossFadeInFixedTime( "move" , CROSS_FADE_DURATION );
						}
						else
						{
							//Attack now
							setCreatureState( CreatureState.Attacking );
							GetComponent<Animator>().CrossFadeInFixedTime( "attack1", CROSS_FADE_DURATION );
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
							GetComponent<Animator>().Play( "move" );
							if( Random.value > 0.5f )
							{
								speak( "VO_DE_ANOTHER_MEETING", 3.9f, false );
							}
							else
							{
								speak( "VO_DE_BAD_SONG", 3.9f, false );
							}
							Invoke("stopWalking", 6.8f );
						}
					}
					break;
			}
		}
	}

	/*
		returns:
		-1 if wraith is behind player
		+1 if wraith is in front
		0 if wraith is on the side
		0.5 if wraith is facing player and within 60 degrees (i.e. between 30 degrees to the left and 30 degrees to the right)
	*/
	float getDotProduct()
	{
		Vector3 heading = player.position - transform.position;
		return Vector3.Dot( heading.normalized, transform.forward );
	}

	void stopWalking()
	{
		attackType = AttackType.do_nothing;
		setCreatureState( CreatureState.Idle );
		GetComponent<Animator>().CrossFadeInFixedTime( "idle" , CROSS_FADE_DURATION );
	}

	public CreatureState getCreatureState()
	{
		return wraithState;
	}

	public void setCreatureState( CreatureState state )
	{
		wraithState = state;
	}

	public void sideCollision()
	{
		GetComponent<AudioSource>().PlayOneShot( ouch );
		GetComponent<Animator>().CrossFadeInFixedTime( "hit" , CROSS_FADE_DURATION );
	}

	public void victory( bool playWinSound )
	{
		if( wraithState != CreatureState.Dying )
		{
			if( playWinSound ) GetComponent<AudioSource>().PlayOneShot( win );
			setCreatureState( CreatureState.Victory );
			GetComponent<Animator>().CrossFadeInFixedTime( "idle" , CROSS_FADE_DURATION );
		}
	}

	//The wraith falls over backwards, typically because the player slid into him or because of a ZNuke
	public void knockback()
	{
		setCreatureState( CreatureState.Dying );
		controller.enabled = false;
		CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
		for( int i = 0; i < capsuleColliders.Length; i++ )
		{
			capsuleColliders[i].enabled = false;
		}
		GetComponent<Animator>().SetTrigger("Knockback");
		GetComponent<AudioSource>().PlayOneShot( fallToGround );
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( PlayerController._characterState == CharacterState.Dying )
		{
			//The Pendulum (bad name, yes I know) is the spike road block
			if( hit.collider.name.StartsWith("Wraith") || hit.collider.name.StartsWith("Hero") || hit.collider.name.StartsWith("Pendulum") )
			{
				//If a wraith collides with another wraith, the road block or the Hero while the player is dead, have him stop moving and play the victory sequence.
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
			float distance = Vector3.Distance(player.position,transform.position);
			float nearby = 4f;
			if( distance < nearby )
			{
				victory( false );
				Debug.Log("Wraith PlayerStateChange - player is dead and nearby");
			}
		}
	}

	public void resetCreature()
	{
		setCreatureState( CreatureState.Idle );
		GetComponent<Animator>().CrossFadeInFixedTime( "idle", CROSS_FADE_DURATION );
		gameObject.SetActive( false );
		followsPlayer = false;
		controller.enabled = true;
		CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
		for( int i = 0; i < capsuleColliders.Length; i++ )
		{
			capsuleColliders[i].enabled = true;
		}
	}

	public void Start_Weapon_Trail ( AnimationEvent eve )
	{
		if( weaponType == WeaponType.Scythe ) weaponTrailScythe.SetActive( true );
		if( weaponType == WeaponType.Axe ) weaponTrailAxe.SetActive( true );
		GetComponent<AudioSource>().PlayOneShot( weaponSwoosh );
	}

	public void Stop_Weapon_Trail ( AnimationEvent eve )
	{
		if( weaponType == WeaponType.Scythe ) weaponTrailScythe.SetActive( false );
		if( weaponType == WeaponType.Axe ) weaponTrailAxe.SetActive( false );
	}

	//the voiceOverID is used both as text ID and as the name of the audio clip. They need to be identical.
	public void speak( string voiceOverID, float textDisplayDuration, bool hasVoiceOver )
	{
		//DialogManager.dialogManager.activateDisplayGeneric( LocalizationManager.Instance.getText( voiceOverID ), wraithPortrait, textDisplayDuration );
		//if( hasVoiceOver ) GetComponent<AudioSource>().PlayOneShot( DialogManager.dialogManager.getVoiceOver( voiceOverID ) );
	}
}
