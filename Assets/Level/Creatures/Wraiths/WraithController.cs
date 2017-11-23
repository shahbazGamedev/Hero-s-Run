using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

public sealed class WraithController : Creature, ICreature {

	[Header("Wraith Controller")]
	[Header("General")]
	public AttackType attackType = AttackType.stand_and_normal_attack;
	[Header("Audio")]
	public AudioClip charge;
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
	[Range(0,1f)]
	public float percentageWillAttack = 0.5f;
	public float floatDuration = 5f;
	public enum WeaponType {
		Axe = 1,
		Scythe = 2
	}

	//Original setup used when reseting the Creature
	AttackType originalAttackType;

	//Movement related
	Vector3 forward;
	float moveSpeed = 0;
	const float WALK_SPEED = 3.2f;
	const float NORMAL_CHARGE_SPEED = 35f;

	//We add a motion blur to the camera when the wraith is charging
	Camera mainCamera;

	new void Awake ()
	{
		base.Awake();
		configureSelectedWeapon();
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		saveOriginalSetup();
	}

	new void saveOriginalSetup()
	{
		base.saveOriginalSetup();
		originalAttackType = attackType;
	}

	new public void resetCreature()
	{
		base.resetCreature();
		attackType = originalAttackType;
	}

	public void sideCollision ()
	{
	}

	void configureSelectedWeapon()
	{
		if( attackType != AttackType.do_nothing )
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
		if( creatureState == CreatureState.Running || creatureState == CreatureState.Walking )
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

	void handleAttackType()
	{
		if( creatureState != CreatureState.Attacking && creatureState != CreatureState.Dying && creatureState != CreatureState.Victory )
		{
			float distance = Vector3.Distance(getPlayer().position,transform.position);
			float attackDistance;
			float playerSpeed = getPlayerController().getSpeed();
		    switch (attackType)
			{
		        case AttackType.stand_and_normal_attack:
					attackDistance = 0.64f * playerSpeed;
					if( distance < attackDistance )
					{
						setCreatureState( CreatureState.Attacking );
						anim.CrossFadeInFixedTime( "attack2" , CROSS_FADE_DURATION );
					}
					break;
		                
		        case AttackType.stand_and_big_attack:
					attackDistance = 0.64f * playerSpeed;
					if( distance < attackDistance )
					{
						setCreatureState( CreatureState.Attacking );
						anim.CrossFadeInFixedTime( "attack1" , CROSS_FADE_DURATION );
					}
					break;
		                
				case AttackType.charge_and_attack:
					float chargeDistance = 2.3f * playerSpeed;
					attackDistance = 0.65f * playerSpeed;
					if( distance < chargeDistance )
					{
						if( distance >= attackDistance )
						{
							if( creatureState != CreatureState.Running )
							{
								//Charge
								mainCamera.GetComponent<MotionBlur>().enabled = true;			
								followsPlayer = true;
								moveSpeed = NORMAL_CHARGE_SPEED;
								setCreatureState( CreatureState.Running );
								anim.CrossFadeInFixedTime( "move" , CROSS_FADE_DURATION );
								audioSource.PlayOneShot( charge );
							}
						}
						else
						{
							//Attack now
							mainCamera.GetComponent<MotionBlur>().enabled = false;			
							setCreatureState( CreatureState.Attacking );
							anim.CrossFadeInFixedTime( "attack1" , CROSS_FADE_DURATION );
						}
					}
					break;
				
				case AttackType.walk_and_talk:
					float startWalkingDistance = 3.4f * playerSpeed;
					if( distance < startWalkingDistance )
					{
						if( getCreatureState() != CreatureState.Glide )
						{
							followsPlayer = false;
							setCreatureState( CreatureState.Glide );
							anim.Play( "move" );
							//percentageWillAttack of the time, the wraith will turn to face the player and attack. The rest of the time, the wraith will just continue straight.
							if( Random.value < percentageWillAttack )
							{
								Invoke("removeDotProduct", floatDuration * 0.65f );
								LeanTween.moveLocal( gameObject, new Vector3( transform.localPosition.x - 12f, transform.localPosition.y, transform.localPosition.z ), floatDuration ).setOnComplete(turnToFacePlayer).setOnCompleteParam(gameObject).setEase(LeanTweenType.easeInOutQuad);
							}
							else
							{
								LeanTween.moveLocal( gameObject, new Vector3( transform.localPosition.x - 24f, transform.localPosition.y + 0.3f, transform.localPosition.z ), floatDuration * 2f );
							}
						}
					}
					break;
			}
		}
	}

	void removeDotProduct()
	{
		dotProductIK = 0;
	}

	void turnToFacePlayer()
	{
		LeanTween.rotateLocal( gameObject, new Vector3( 0, 160f, 0 ), 0.3f ).setDelay( 0.1f ).setOnComplete(nowFacingPlayer).setOnCompleteParam(gameObject).setEase(LeanTweenType.easeOutQuad);
	}

	void nowFacingPlayer()
	{
		dotProductIK = 0.6f;
		attackType = AttackType.stand_and_big_attack;
	}

	void stopWalking()
	{
		attackType = AttackType.do_nothing;
		setCreatureState( CreatureState.Idle );
		anim.CrossFadeInFixedTime( "idle" , CROSS_FADE_DURATION );
	}

	public void victory( bool playWinSound )
	{
		if( creatureState != CreatureState.Dying )
		{
			mainCamera.GetComponent<MotionBlur>().enabled = false;			
			if( playWinSound ) audioSource.PlayOneShot( win );
			setCreatureState( CreatureState.Victory );
			anim.CrossFadeInFixedTime( "idle" , CROSS_FADE_DURATION );
		}
	}

	//The wraith falls over backwards, typically because the player slid into him or because of a ZNuke
	public new void knockback( Transform attacker )
	{
		base.knockback( attacker );
		anim.SetTrigger("Knockback");
		GetComponent<Rigidbody>().isKinematic = false;
		mainCamera.GetComponent<MotionBlur>().enabled = false;			
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( getPlayerController().getCharacterState() == PlayerCharacterState.Dying )
		{
			if( hit.collider.name.StartsWith("Wraith") || hit.gameObject.CompareTag("Player") )
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

	void PlayerStateChange( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			Stop_Weapon_Trail ( null );
			float distance = Vector3.Distance(getPlayer().position,transform.position);
			float nearby = 5f;
			if( distance < nearby )
			{
				victory( false );
				StartCoroutine( base.fadeOutLookAtPosition( 0.2f, 2f + Random.value * 2f, 0.9f ) );
			}
		}
	}

	public void Start_Weapon_Trail ( AnimationEvent eve )
	{
		if( weaponType == WeaponType.Scythe ) weaponTrailScythe.SetActive( true );
		if( weaponType == WeaponType.Axe ) weaponTrailAxe.SetActive( true );
		audioSource.PlayOneShot( weaponSwoosh );
	}

	public void Stop_Weapon_Trail ( AnimationEvent eve )
	{
		if( weaponTrailScythe != null && weaponType == WeaponType.Scythe ) weaponTrailScythe.SetActive( false );
		if(weaponTrailAxe != null && weaponType == WeaponType.Axe ) weaponTrailAxe.SetActive( false );
	}


}
