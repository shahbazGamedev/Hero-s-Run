using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

public class WraithController : BaseClass, ICreature {

	[Header("Wraith Controller")]
	[Header("General")]
	public AttackType attackType = AttackType.stand_and_normal_attack;
	[Tooltip("Speed at which to lock on player.")]
	public float enemyAimSpeed = 7f;
	[Header("Audio")]
	public AudioClip charge;
	public AudioClip screech;
	public AudioClip win;
	public AudioClip weaponSwoosh;
	[Header("Weapons")]
	[Tooltip("The mesh includes both an axe and a scythe. Both have two LOD levels. We need references in order to disable the unused ones. Note that if the attack type is walk_and_talk, no weapon is displayed.")]
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
	float moveSpeed = 0;
	const float WALK_SPEED = 3.2f;
	const float NORMAL_CHARGE_SPEED = 35f;

	//If true, the wraith heads for the player as opposed to staying in his lane
	bool followsPlayer = false;
	//We add a motion blur to the camera when the wraith is charging
	Camera mainCamera;

	void Awake ()
	{
		controller = GetComponent<CharacterController>();
		player = GameObject.FindGameObjectWithTag("Player").transform;
		configureSelectedWeapon();
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}

	void configureSelectedWeapon()
	{
		if( attackType != AttackType.walk_and_talk )
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
		else
		{
			weaponAxeLOD0.SetActive( false );
			weaponAxeLOD1.SetActive( false );
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
					attackDistance = 0.65f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						setCreatureState( CreatureState.Attacking );
						GetComponent<Animator>().CrossFadeInFixedTime( "attack2" , CROSS_FADE_DURATION );
					}
					break;
		                
		        case AttackType.stand_and_big_attack:
					attackDistance = 0.65f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						setCreatureState( CreatureState.Attacking );
						GetComponent<Animator>().CrossFadeInFixedTime( "attack1" , CROSS_FADE_DURATION );
					}
					break;
		                
				case AttackType.charge_and_attack:
					float chargeDistance = 2.3f * PlayerController.getPlayerSpeed();
					attackDistance = 0.65f * PlayerController.getPlayerSpeed();
					if( distance < chargeDistance )
					{
						if( distance >= attackDistance )
						{
							if( wraithState != CreatureState.Running )
							{
								//Charge
								mainCamera.GetComponent<MotionBlur>().enabled = true;			
								followsPlayer = true;
								moveSpeed = getAdjustedChargeSpeed();
								setCreatureState( CreatureState.Running );
								GetComponent<Animator>().CrossFadeInFixedTime( "move" , CROSS_FADE_DURATION );
								GetComponent<AudioSource>().PlayOneShot( charge );
							}
						}
						else
						{
							//Attack now
							mainCamera.GetComponent<MotionBlur>().enabled = false;			
							setCreatureState( CreatureState.Attacking );
							GetComponent<Animator>().CrossFadeInFixedTime( "attack1" , CROSS_FADE_DURATION );
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
							Invoke("stopWalking", 6.8f );
						}
					}
					break;
			}
		}
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

	public float getAdjustedChargeSpeed()
	{
		float adjustedChargeSpeed = NORMAL_CHARGE_SPEED;
		switch (PlayerStatsManager.Instance.getDifficultyLevel())
		{
			case DifficultyLevel.Normal:
			adjustedChargeSpeed = NORMAL_CHARGE_SPEED; //Base value is Normal, so no multiplier
			break;
				
			case DifficultyLevel.Heroic:
			adjustedChargeSpeed = NORMAL_CHARGE_SPEED * 1.1f;
			break;
				
			case DifficultyLevel.Legendary:
			adjustedChargeSpeed = NORMAL_CHARGE_SPEED * 1.3f;
			break;
			
		}
		return adjustedChargeSpeed;
	}

	public void victory( bool playWinSound )
	{
		if( wraithState != CreatureState.Dying )
		{
			mainCamera.GetComponent<MotionBlur>().enabled = false;			
			if( playWinSound ) GetComponent<AudioSource>().PlayOneShot( win );
			setCreatureState( CreatureState.Victory );
			GetComponent<Animator>().CrossFadeInFixedTime( "idle" , CROSS_FADE_DURATION );
		}
	}

	//The wraith falls over backwards, typically because the player slid into him or because of a ZNuke
	public void knockback()
	{
		setCreatureState( CreatureState.Dying );
		mainCamera.GetComponent<MotionBlur>().enabled = false;			
		controller.enabled = false;
		CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
		for( int i = 0; i < capsuleColliders.Length; i++ )
		{
			capsuleColliders[i].enabled = false;
		}
		GetComponent<Animator>().SetTrigger("Knockback");
		GetComponent<AudioSource>().PlayOneShot( screech );
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( PlayerController._characterState == CharacterState.Dying )
		{
			if( hit.collider.name.StartsWith("Wraith") || hit.collider.name.StartsWith("Hero") )
			{
				//If a wraith collides with another wraith or the Hero while the player is dead, have him stop moving and play the victory sequence.
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
}
