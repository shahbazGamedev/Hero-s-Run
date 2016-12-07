using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public sealed class SkeletonController : Creature, ICreature {

	[Header("Skeleton Controller")]
	[Header("General")]
	public AttackType attackType = AttackType.Short_range_sword_1;
	public bool applyGravity = true;
	[Header("Audio")]
	public AudioClip footstepLeftSound;
	public AudioClip footstepRightSound;
	public AudioClip ouch;
	public AudioClip win;
	public AudioClip swordSwoosh;
	[Tooltip("Player distance multiplier used to decide when to jump.")]
	public float jumpPlayerDistanceMultiplier = 3.3f;
	[Tooltip("Player distance multiplier used to decide when to fire a missile.")]
	public float missilePlayerDistanceMultiplier = 3f;
	[Tooltip("Player distance multiplier used to decide when to summon skeletons.")]
	public float summonSkeletonsPlayerDistanceMultiplier = 3f;
	[Tooltip("Player distance multiplier used to decide when to fire a magic missile.")]
	public float magicMissilePlayerDistanceMultiplier = 2.4f;
	[Header("Archer and Sorcerer")]
	[Tooltip("The arrow fired by the archer or the magic missile fired by the sorcerer.")]
	public GameObject missilePrefab;
	[Tooltip("Archer only. A reference to the animated arrow so we can hide it once the prefab arrow is launched.")]
	public GameObject arrow_06;
	GameObject arrow;
	[Header("Footman and warlord")]
	public GameObject weaponTrail;
	public float wakeUpDelay = 0;
	[Header("Sorcerer")]
	public ParticleSystem lightningStrike;
	public List<SkeletonController> summonedSkeletons = new List<SkeletonController>();
	public ParticleSystem summonFXPrefab;
	GameObject magicMissile;

	public enum AttackType {
		Short_range_sword_1 = 1,		//Footman and warlord
		Short_range_sword_2 = 2,		//Footman and warlord	
		Charge_and_attack = 3,			//Footman and warlord
		Bow = 4,						//Archer only
		Jump_and_attack = 6,			//Footman and warlord		
		Jump_and_long_range_attack = 7,	//Footman and warlord
		Summon_skeletons = 8,			//Sorcerer only			
		Fire_magic_missile = 9,			//Sorcerer only
		Wake_walk_attack_with_sword = 11	//Footman only
	}
	
	public float bolt_force = 900f;

	//Original setup used when reseting the Creature
	AttackType originalAttackType;

	//Movement related
	Vector3 forward;
	const float RUN_SPEED = 4.6f; //good value so feet don't slide
	float WALK_SPEED = 3.2f; //good value so feet don't slide
	float moveSpeed = 0;
	bool previouslyGrounded = true;

	new void Awake ()
	{
		base.Awake();
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

	void Start ()
	{
		if( attackType != AttackType.Wake_walk_attack_with_sword )
		{
			StartCoroutine( playIdleAnimation() );
		}
		else
		{
			//We do not want the player to collide with a dormant skeleton
			//controller.enabled = false;
			CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
			for( int i = 0; i < capsuleColliders.Length; i++ )
			{
				capsuleColliders[i].enabled = false;
			}
			anim.Play( "sleeping" );
			transform.localScale = new Vector3( 0.002f, 0.002f, 0.002f );

		}
	}

	//We don't want all the skeletons to have synced animations
	IEnumerator playIdleAnimation()
	{
		yield return new WaitForSeconds( Random.value * 3f );
		anim.CrossFadeInFixedTime( "idle", CROSS_FADE_DURATION );
	}

	void Update ()
	{
		move();
		handleAttackType();
	}

	void move()
	{
		if( creatureState == CreatureState.Running || creatureState == CreatureState.Jumping || creatureState == CreatureState.Walking )
		{
			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				targetPlayer();
			}
			//1) Get the direction of the skeleton
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			forward = forward * Time.deltaTime * moveSpeed;
			if( applyGravity ) forward.y -= 16f * Time.deltaTime;
			//3) Move the controller
			controller.Move( forward );

			if (controller.isGrounded && !previouslyGrounded )
			{
				//Creature just landed after a jump. Have it start running.
				audioSource.PlayOneShot( knockbackSound );
				anim.CrossFadeInFixedTime( "run", CROSS_FADE_DURATION );
				moveSpeed = RUN_SPEED;
				setCreatureState( CreatureState.Running );
			}
			previouslyGrounded = controller.isGrounded;
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
		        case AttackType.Short_range_sword_1:
					attackDistance = 0.7f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.9f )
					{
						followsPlayer = true;
						setCreatureState( CreatureState.Attacking );
						anim.CrossFadeInFixedTime( "attack1", CROSS_FADE_DURATION );
					}
					break;
		                
		        case AttackType.Short_range_sword_2:
					attackDistance = 0.7f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.9f )
					{
						followsPlayer = true;
						setCreatureState( CreatureState.Attacking );
						anim.CrossFadeInFixedTime( "attack2", CROSS_FADE_DURATION );
					}
					break;
		                
				case AttackType.Charge_and_attack:
					float chargeDistance = 2.3f * PlayerController.getPlayerSpeed();
					attackDistance = 0.75f * PlayerController.getPlayerSpeed();
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
								anim.CrossFadeInFixedTime( "run" , CROSS_FADE_DURATION );
							}
						}
						else
						{
							//Attack now
							setCreatureState( CreatureState.Attacking );
							anim.CrossFadeInFixedTime( "attack1" , CROSS_FADE_DURATION );
						}
					}
					break;
			
				case AttackType.Bow:
					attackDistance = missilePlayerDistanceMultiplier * PlayerController.getPlayerSpeed();
					//Only attack if the player is inside a 30 degree arc in front of skeleton
					if( distance < attackDistance && getDotProduct() > 0.8f )
					{
						setCreatureState( CreatureState.Attacking );
						fireCrossbow();
					}
					break;
				case AttackType.Fire_magic_missile:
					attackDistance = magicMissilePlayerDistanceMultiplier * PlayerController.getPlayerSpeed();
					//Only attack if the player is inside a 30 degree arc in front of skeleton
					if( distance < attackDistance && getDotProduct() > 0.8f )
					{
						setCreatureState( CreatureState.Attacking );
						fireMagicMissile();
					}
					break;
				case AttackType.Summon_skeletons:
					attackDistance = summonSkeletonsPlayerDistanceMultiplier * PlayerController.getPlayerSpeed();
					//Only attack if the player is inside a 30 degree arc in front of skeleton
					if( distance < attackDistance && getDotProduct() > 0.8f )
					{
						setCreatureState( CreatureState.Attacking );
						summonSkeletons();
					}
					break;
				case AttackType.Jump_and_attack:
					float jumpDistance = jumpPlayerDistanceMultiplier * PlayerController.getPlayerSpeed();
					attackDistance = 0.85f * PlayerController.getPlayerSpeed();
					if( distance < jumpDistance )
					{
						if( distance >= attackDistance )
						{
							if( creatureState != CreatureState.Running && creatureState != CreatureState.Jumping )
							{
								//Jump and run once you land
								followsPlayer = true;
								setCreatureState( CreatureState.Jumping );
								anim.CrossFadeInFixedTime( "jump", CROSS_FADE_DURATION );
							}
						}
						else
						{
							//Attack now
							setCreatureState( CreatureState.Attacking );
							anim.CrossFadeInFixedTime( "attack2", CROSS_FADE_DURATION );
						}
					}
					break;
				case AttackType.Jump_and_long_range_attack:
					float jumpLongDistance = jumpPlayerDistanceMultiplier * PlayerController.getPlayerSpeed();
					if( distance < jumpLongDistance )
					{
						if( creatureState != CreatureState.Running && creatureState != CreatureState.Jumping )
						{
							//Jump and run once you land
							followsPlayer = true;
							setCreatureState( CreatureState.Jumping );
							anim.CrossFadeInFixedTime( "jump", CROSS_FADE_DURATION );
							attackType = AttackType.Charge_and_attack;
						}
					}
					break;
			}
		}
	}
	
	void fireMagicMissile()
	{
		transform.LookAt( player );
		transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
		magicMissile = createMagicMissile();
		anim.CrossFadeInFixedTime("Fire Magic Missile", CROSS_FADE_DURATION );
	}

	GameObject createMagicMissile()
	{
		Transform staff = transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 Rhand_Weapon/stuff02");
		GameObject nextFireball = (GameObject)Instantiate( missilePrefab );
		nextFireball.name = "Skeleton fireball";
		nextFireball.transform.SetParent( staff, false );
		nextFireball.transform.localPosition = new Vector3( -0.8f,0,0 );
		return nextFireball;
	}

	public void Fireball_launched ( AnimationEvent eve )
	{
		magicMissile.transform.SetParent( null );
		Physics.IgnoreCollision(magicMissile.GetComponent<Collider>(), transform.GetComponent<CapsuleCollider>());
		Physics.IgnoreCollision(magicMissile.GetComponent<Collider>(), transform.GetComponent<CharacterController>());
		magicMissile.GetComponent<Rigidbody>().isKinematic = false;
		magicMissile.GetComponent<Rigidbody>().AddForce( ( new Vector3( player.position.x, player.position.y + 0.4f, player.position.z ) - magicMissile.transform.position).normalized * getAdjustedBoltForce() );
		magicMissile.GetComponent<Projectile>().launchProjectile();
		GameObject.Destroy( magicMissile, 10f );
	}

	void summonSkeletons()
	{
		anim.CrossFadeInFixedTime("Call Lightning", CROSS_FADE_DURATION );
	}

	//Called by M_skeleton_buff_spell_A
	public void Call_lightning ( AnimationEvent eve )
	{
		if( lightningStrike != null )
		{
			lightningStrike.Play();
			lightningStrike.GetComponent<AudioSource>().Play();
			for( int i = 0; i < summonedSkeletons.Count; i++ )
			{
				StartCoroutine( summonedSkeletons[i].wakeUp( wakeUpDelay ) );
			}
		}
	}

	public IEnumerator wakeUp( float delay )
	{
		yield return new WaitForSeconds( delay );
		//controller.enabled = true;
		CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
		for( int i = 0; i < capsuleColliders.Length; i++ )
		{
			capsuleColliders[i].enabled = true;
		}
		anim.Play( "wake" );
		ParticleSystem summonFX = (ParticleSystem)Instantiate( summonFXPrefab );
		summonFX.transform.position = new Vector3( transform.position.x, transform.position.y + 0.25f, transform.position.z );
		summonFX.Play();
		summonFX.GetComponent<AudioSource>().Play();
		GameObject.Destroy( summonFX, 10f );
	}
	
	public void Wake_up_completed ( AnimationEvent eve )
	{
		//Walk
		transform.localScale = new Vector3( 3f, 3f, 3f );

		followsPlayer = true;
		moveSpeed = WALK_SPEED;
		setCreatureState( CreatureState.Walking );
		if( Random.value >= 0.5f )
		{
			attackType = AttackType.Short_range_sword_1;
		}
		else
		{
			attackType = AttackType.Short_range_sword_2;
		}
	}

	void fireCrossbow()
	{
		transform.LookAt( player );
		transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
		arrow = createArrow();
		anim.SetTrigger("Fire");
	}

	GameObject createArrow()
	{
		Transform arrowHolder = transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand");
		arrow_06.SetActive( false );
		GameObject nextArrow = (GameObject)Instantiate( missilePrefab );
		nextArrow.transform.SetParent( arrowHolder, false );
		nextArrow.transform.localPosition = new Vector3( -0.03378291f,0.02765007f,-0.004029159f );
		nextArrow.transform.localRotation = Quaternion.Euler( -28f, 0, 0 );
		return nextArrow;
	}

	public void Arrow_launched ( AnimationEvent eve )
	{
		arrow.transform.SetParent( null );
		Physics.IgnoreCollision(arrow.GetComponent<Collider>(), transform.GetComponent<CapsuleCollider>());
		Physics.IgnoreCollision(arrow.GetComponent<Collider>(), transform.GetComponent<CharacterController>());
		arrow.GetComponent<Rigidbody>().isKinematic = false;
		arrow.GetComponent<Rigidbody>().AddForce( (new Vector3( player.position.x, player.position.y + 0.2f, player.position.z ) - arrow.transform.position).normalized * getAdjustedBoltForce() );
		arrow.GetComponent<Projectile>().launchProjectile();
		GameObject.Destroy( arrow, 10f );
	}

	public float getAdjustedBoltForce()
	{
		float adjustedBoltForce = bolt_force;
		switch (PlayerStatsManager.Instance.getDifficultyLevel())
		{
			case DifficultyLevel.Normal:
			adjustedBoltForce = bolt_force; //Base value is Normal, so no multiplier
			break;
				
			case DifficultyLevel.Heroic:
			adjustedBoltForce = bolt_force * 1.3f;
			break;
				
			case DifficultyLevel.Legendary:
			adjustedBoltForce = bolt_force * 1.6f;
			break;
			
		}
		return adjustedBoltForce;
	}

	public void sideCollision()
	{
		audioSource.PlayOneShot( ouch );
		anim.CrossFadeInFixedTime( "damage", CROSS_FADE_DURATION );
	}

	public void victory( bool playWinSound )
	{
		if( creatureState != CreatureState.Dying )
		{
			Debug.Log("Skeleton PlayerStateChange - victory " + gameObject.name + " " + creatureState );
			if( playWinSound ) audioSource.PlayOneShot( win );
			setCreatureState( CreatureState.Victory );
			anim.CrossFadeInFixedTime( "idle", CROSS_FADE_DURATION );
		}
	}

	//The skeleton falls over backwards, typically because the player slid into him or because of a ZNuke
	public new void knockback()
	{
		StopCoroutine( wakeUp(0) );
		base.knockback();
		anim.CrossFadeInFixedTime( "death", CROSS_FADE_DURATION );
		Debug.Log("Skeleton PlayerStateChange - knockback " + gameObject.name + " " + creatureState );
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( PlayerController._characterState == CharacterState.Dying )
		{
			//The Pendulum (bad name, yes I know) is the spike road block
			if( hit.collider.name.StartsWith("Skeleton") || hit.gameObject.CompareTag("Player") || hit.collider.name.StartsWith("Pendulum") )
			{
				//If a Skeleton collides with another Skeleton, the road block or the Hero while the player is dead, have him stop moving and play the victory sequence.
				victory( false );
			}
		}
	}

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
		SummonSkeletonsSequence.skeletonsSummoned += SkeletonsSummoned;
	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
		SummonSkeletonsSequence.skeletonsSummoned -= SkeletonsSummoned;
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
				StartCoroutine( base.fadeOutLookAtPosition( 0.2f, 2f + Random.value * 2f, 0.9f ) );
			}
		}
	}

	void SkeletonsSummoned( Transform summoner )
	{
		if( attackType == AttackType.Wake_walk_attack_with_sword )
		{
			StartCoroutine( wakeUp( wakeUpDelay ) );
		}
	}

	public void Footstep_left ( AnimationEvent eve )
	{
		audioSource.PlayOneShot( footstepLeftSound, 0.23f );
	}

	public void Footstep_right ( AnimationEvent eve )
	{
		audioSource.PlayOneShot( footstepRightSound, 0.23f );
	}

	public void Start_Weapon_Trail ( AnimationEvent eve )
	{
		weaponTrail.SetActive( true );
		audioSource.PlayOneShot( swordSwoosh );
	}

	public void Stop_Weapon_Trail ( AnimationEvent eve )
	{
		if( weaponTrail != null ) weaponTrail.SetActive( false );
	}

}
