using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public sealed class SkeletonController : Creature, ICreature {

	[Header("Skeleton Controller")]
	[Header("General")]
	public AttackType attackType = AttackType.short_range_Spear_1;
	public bool applyGravity = true;
	[Header("Audio")]
	public AudioClip footstepLeftSound;
	public AudioClip footstepRightSound;
	public AudioClip ouch;
	public AudioClip fallToGround;
	public AudioClip win;
	public AudioClip swordSwoosh;
	[Header("Barrel")]
	[Tooltip("The breakable barrel that the skeleton will push on top of the player.")]
	public Rigidbody barrel;
	[Tooltip("The forward (based on the skeleton's transform) force to apply on the barrel.")]
	public float barrelForwardForce = 1300f; //Based on 10 kilograms
	[Tooltip("Player distance multiplier used to decide to throw barrel.")]
	public float barrelPlayerDistanceMultiplier = 1.45f;
	[Tooltip("Player distance multiplier used to decide when to jump.")]
	public float jumpPlayerDistanceMultiplier = 3.3f;
	[Tooltip("Player distance multiplier used to decide when to fire missile.")]
	public float missilePlayerDistanceMultiplier = 3f;
	[Tooltip("Player distance multiplier used to decide when to fire missile.")]
	public float spell1PlayerDistanceMultiplier = 3f;
	[Tooltip("Player distance multiplier used to decide when to fire missile.")]
	public float spell2PlayerDistanceMultiplier = 2.4f;
	[Tooltip("Player distance multiplier used to decide when to fire missile.")]
	public float spell3PlayerDistanceMultiplier = 2.4f;
	[Tooltip("Whether or not the skeleton should play a diabolical laughter before pushing the barrel.")]
	public bool playSkeletonTaunt = false;
	[Tooltip("Speed at which to lock on player.")]
	public float enemyAimSpeed = 7.6f;
	[Tooltip("The bolt fired by the crossbow.")]
	public GameObject missilePrefab;
	GameObject arrow;
	public GameObject arrow_06;
	GameObject fireball;
	[Header("Other")]
	public GameObject weaponTrail;
	public ParticleSystem lightningStrike;
	public ParticleSystem earthquakeFX;

	public enum AttackType {
		short_range_Spear_1 = 1,
		short_range_Spear_2 = 2,
		long_range_Spear = 3,
		Crossbow = 4,
		Throw_Barrel = 5,
		jump_and_attack = 6,
		jump_and_long_range_attack = 7,
		cast_spell_1 = 8,
		cast_spell_2 = 9,
		cast_spell_3 = 10
	}
	
	const float BOLT_FORCE = 900f;
	const float CROSS_FADE_DURATION = 0.5f;

	//Only use for the scout skeleton with the crossbow
	Vector3 initialBoltPositionOffset = new Vector3( 0f, 0.47f, 0.46f );

	Transform player;

	//Movement related
	CharacterController controller;
	Vector3 forward;
	float runSpeed = 4.6f; //good value so feet don't slide
	//If true, the skeleton heads for the player as opposed to staying in his lane
	bool followsPlayer = false;
	bool previouslyGrounded = true;

	void Awake ()
	{
		controller = GetComponent<CharacterController>();
		player = GameObject.FindGameObjectWithTag("Player").transform;

	}

	void Start ()
	{
		StartCoroutine( playIdleAnimation() );
	}

	//We don't want all the skeletons to have synced animations
	IEnumerator playIdleAnimation()
	{
		yield return new WaitForSeconds( Random.value * 3f );
		GetComponent<Animator>().CrossFadeInFixedTime( "idle", CROSS_FADE_DURATION );
	}

	void Update ()
	{
		moveSkeleton();
		handleAttackType();
		#if UNITY_EDITOR
		handleKeyboard();
		#endif

	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.M) ) 
		{
			fireCrossbow();
		}
	}

	void moveSkeleton()
	{
		if( creatureState == CreatureState.Running || creatureState == CreatureState.Jumping )
		{
			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				targetPlayer();
			}
			//1) Get the direction of the skeleton
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			forward = forward * Time.deltaTime * runSpeed;
			if( applyGravity ) forward.y -= 16f * Time.deltaTime;
			//3) Move the controller
			controller.Move( forward );

			if (controller.isGrounded && !previouslyGrounded )
			{
				GetComponent<AudioSource>().PlayOneShot( fallToGround );
				GetComponent<Animator>().CrossFadeInFixedTime( "run", CROSS_FADE_DURATION );
				setCreatureState( CreatureState.Running );
			}
			previouslyGrounded = controller.isGrounded;
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
		if( creatureState != CreatureState.Attacking && creatureState != CreatureState.Dying && creatureState != CreatureState.Victory )
		{
			float distance = Vector3.Distance(player.position,transform.position);
			float attackDistance;
		    switch (attackType)
			{
		        case AttackType.short_range_Spear_1:
					attackDistance = 0.7f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setCreatureState( CreatureState.Attacking );
						GetComponent<Animator>().CrossFadeInFixedTime( "attack1", CROSS_FADE_DURATION );
					}
					break;
		                
		        case AttackType.short_range_Spear_2:
					attackDistance = 0.7f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setCreatureState( CreatureState.Attacking );
						GetComponent<Animator>().CrossFadeInFixedTime( "attack2", CROSS_FADE_DURATION );
					}
					break;
		                
				case AttackType.long_range_Spear:
					attackDistance = 2f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						followsPlayer = true;
						setCreatureState( CreatureState.Running );
						GetComponent<Animator>().Play( "run" );
					}
					break;
			
				case AttackType.Crossbow:
					attackDistance = missilePlayerDistanceMultiplier * PlayerController.getPlayerSpeed();
					//Only attack if the player is inside a 30 degree arc in front of skeleton
					if( distance < attackDistance && getDotProduct() > 0.8f )
					{
						setCreatureState( CreatureState.Attacking );
						fireCrossbow();
					}
					break;
				case AttackType.cast_spell_1:
					attackDistance = spell1PlayerDistanceMultiplier * PlayerController.getPlayerSpeed();
					//Only attack if the player is inside a 30 degree arc in front of skeleton
					if( distance < attackDistance && getDotProduct() > 0.8f )
					{
						setCreatureState( CreatureState.Attacking );
						castFireballSpell();
					}
					break;
				case AttackType.cast_spell_2:
					attackDistance = spell2PlayerDistanceMultiplier * PlayerController.getPlayerSpeed();
					//Only attack if the player is inside a 30 degree arc in front of skeleton
					if( distance < attackDistance && getDotProduct() > 0.8f )
					{
						setCreatureState( CreatureState.Attacking );
						castLightningSpell();
					}
					break;
				case AttackType.cast_spell_3:
					attackDistance = spell3PlayerDistanceMultiplier * PlayerController.getPlayerSpeed();
					//Only attack if the player is inside a 30 degree arc in front of skeleton
					if( distance < attackDistance && getDotProduct() > 0.8f )
					{
						setCreatureState( CreatureState.Attacking );
						castEarthquakeSpell();
					}
					break;
				case AttackType.Throw_Barrel:
					attackDistance = barrelPlayerDistanceMultiplier * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						setCreatureState( CreatureState.Attacking );
						throwBarrel();
					}
					break;
				case AttackType.jump_and_attack:
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
								GetComponent<Animator>().CrossFadeInFixedTime( "jump", CROSS_FADE_DURATION );
							}
						}
						else
						{
							//Attack now
							setCreatureState( CreatureState.Attacking );
							GetComponent<Animator>().CrossFadeInFixedTime( "attack2", CROSS_FADE_DURATION );
						}
					}
					break;
				case AttackType.jump_and_long_range_attack:
					float jumpLongDistance = jumpPlayerDistanceMultiplier * PlayerController.getPlayerSpeed();
					if( distance < jumpLongDistance )
					{
						if( creatureState != CreatureState.Running && creatureState != CreatureState.Jumping )
						{
							//Jump and run once you land
							followsPlayer = true;
							setCreatureState( CreatureState.Jumping );
							GetComponent<Animator>().CrossFadeInFixedTime( "jump", CROSS_FADE_DURATION );
							attackType = AttackType.long_range_Spear;
						}
					}
					break;
			}
		}
	}

	/*
		returns:
		-1 if creature is behind player
		+1 if creature is in front
		0 if creature is on the side
		0.5 if creature is facing player and within 60 degrees (i.e. between 30 degrees to the left and 30 degrees to the right)
	*/
	float getDotProduct()
	{
		Vector3 heading = player.position - transform.position;
		return Vector3.Dot( heading.normalized, transform.forward );
	}
	
	void castLightningSpell()
	{
		GetComponent<Animator>().CrossFadeInFixedTime("Call Lightning", CROSS_FADE_DURATION );
		Debug.LogWarning("SkeletonController - castLightningSpell" );
	}

	void castEarthquakeSpell()
	{
		GetComponent<Animator>().CrossFadeInFixedTime("Earthquake Spell", CROSS_FADE_DURATION );
		Debug.LogWarning("SkeletonController - castEarthquakeSpell" );
	}

	void castFireballSpell()
	{
		transform.LookAt( player );
		transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
		fireball = createFireball();
		GetComponent<Animator>().CrossFadeInFixedTime("Fire Magic Missile", CROSS_FADE_DURATION );
		//Debug.LogWarning("Skeleton castFireballSpell" );
	}

	GameObject createFireball()
	{
		Transform staff = transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 Rhand_Weapon/stuff02");
		GameObject nextFireball = (GameObject)Instantiate( missilePrefab );
		nextFireball.name = "Skeleton fireball";
		nextFireball.transform.SetParent( staff, false );
		nextFireball.transform.localPosition = new Vector3( -0.8f,0,0 );
		//Debug.LogWarning("Skeleton createFireball" );
		return nextFireball;
	}

	public void Fireball_launched ( AnimationEvent eve )
	{
		fireball.transform.SetParent( null );
		Physics.IgnoreCollision(fireball.GetComponent<Collider>(), transform.GetComponent<CapsuleCollider>());
		Physics.IgnoreCollision(fireball.GetComponent<Collider>(), transform.GetComponent<CharacterController>());
		fireball.GetComponent<Rigidbody>().isKinematic = false;
		fireball.GetComponent<Rigidbody>().AddForce( ( new Vector3( player.position.x, player.position.y + 0.35f, player.position.z ) - fireball.transform.position).normalized * getAdjustedBoltForce() );
		fireball.GetComponent<Projectile>().launchProjectile();
		GameObject.Destroy( fireball, 10f );
	}

	void fireCrossbow()
	{
		transform.LookAt( player );
		transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
		arrow = createArrow();
		GetComponent<Animator>().SetTrigger("Fire");
	}

	GameObject createArrow()
	{
		Transform arrowHolder = transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand");
		arrow_06.SetActive( false );
		GameObject nextArrow = (GameObject)Instantiate( missilePrefab );
		nextArrow.name = "Skeleton arrow";
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
		arrow.GetComponent<Rigidbody>().AddForce( (player.position - arrow.transform.position).normalized * getAdjustedBoltForce() );
		arrow.GetComponent<Projectile>().launchProjectile();
		GameObject.Destroy( arrow, 10f );
	}

	public float getAdjustedBoltForce()
	{
		float adjustedBoltForce = BOLT_FORCE;
		switch (PlayerStatsManager.Instance.getDifficultyLevel())
		{
			case DifficultyLevel.Normal:
			adjustedBoltForce = BOLT_FORCE; //Base value is Normal, so no multiplier
			break;
				
			case DifficultyLevel.Heroic:
			adjustedBoltForce = BOLT_FORCE * 1.3f;
			break;
				
			case DifficultyLevel.Legendary:
			adjustedBoltForce = BOLT_FORCE * 1.6f;
			break;
			
		}
		return adjustedBoltForce;
	}

	void throwBarrel()
	{
		if( playSkeletonTaunt ) GetComponent<AudioSource>().PlayOneShot( win, 0.7f );
		//Push barrels in the direction of the skeleton and add a small upward force
		Vector3 forces = transform.forward * barrelForwardForce + new Vector3( 0, 400f, 0 );
		barrel.isKinematic = false;
		barrel.AddForce( forces );
		barrel.AddTorque( new Vector3( 0, 300f, 0 ) );
		GetComponent<Animator>().CrossFadeInFixedTime( "attack2", CROSS_FADE_DURATION );
	}



	public void sideCollision()
	{
		GetComponent<AudioSource>().PlayOneShot( ouch );
		GetComponent<Animator>().CrossFadeInFixedTime( "damage", CROSS_FADE_DURATION );
	}

	public void victory( bool playWinSound )
	{
		if( creatureState != CreatureState.Dying )
		{
			if( playWinSound ) GetComponent<AudioSource>().PlayOneShot( win );
			setCreatureState( CreatureState.Victory );
			GetComponent<Animator>().CrossFadeInFixedTime( "idle", CROSS_FADE_DURATION );
		}
	}

	//The skeleton falls over backwards, typically because the player slid into him or because of a ZNuke
	public void knockback()
	{
		setCreatureState( CreatureState.Dying );
		controller.enabled = false;
		//The piker has two capsule colliders. The scout, only one.
		CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
		for( int i = 0; i < capsuleColliders.Length; i++ )
		{
			capsuleColliders[i].enabled = false;
		}
		GetComponent<Animator>().CrossFadeInFixedTime( "death", CROSS_FADE_DURATION );
		GetComponent<AudioSource>().PlayOneShot( fallToGround );
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( PlayerController._characterState == CharacterState.Dying )
		{
			//The Pendulum (bad name, yes I know) is the spike road block
			if( hit.collider.name.StartsWith("Skeleton") || hit.collider.name.StartsWith("Hero") || hit.collider.name.StartsWith("Pendulum") )
			{
				//If a Skeleton collides with another Skeleton, the road block or the Hero while the player is dead, have him stop moving and play the victory sequence.
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
				Debug.Log("Skeleton PlayerStateChange - player is dead and nearby");
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

	public void Footstep_left ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( footstepLeftSound, 0.23f );
	}

	public void Footstep_right ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( footstepRightSound, 0.23f );
	}

	public void Start_Weapon_Trail ( AnimationEvent eve )
	{
		weaponTrail.SetActive( true );
		GetComponent<AudioSource>().PlayOneShot( swordSwoosh );
	}

	public void Stop_Weapon_Trail ( AnimationEvent eve )
	{
		weaponTrail.SetActive( false );
	}

	//Called by M_skeleton_buff_spell_A
	public void Call_lightning ( AnimationEvent eve )
	{
		if( lightningStrike != null )
		{
			lightningStrike.Play();
			lightningStrike.GetComponent<AudioSource>().Play();
			lightningStrike.GetComponent<Light>().enabled = true;
			Invoke("closeLight", 1f);
		}
		Debug.LogError("SkeletonController - Call_lightning");
	}

	//Called by M_skeleton_buff_spell_A
	public void Call_Earthquake ( AnimationEvent eve )
	{
		if( earthquakeFX != null )
		{
			earthquakeFX.Play();
			earthquakeFX.GetComponent<AudioSource>().Play();
			//earthquakeFx.GetComponent<Light>().enabled = true;
			//Invoke("closeLight", 1f);
		}
		Debug.LogError("SkeletonController - Call_lightning");
	}

	void closeLight()
	{
		lightningStrike.GetComponent<Light>().enabled = false;
	}
}
