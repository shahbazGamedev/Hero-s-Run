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
	[Header("Clothing")]
	[Tooltip("There is 30% chance that each of the following cloth item will be hidden. This is so not all skeletons look alike.")]
	public GameObject clothItem1;
	public GameObject clothItem2;
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
	[Tooltip("Whether or not the skeleton should play a diabolical laughter before pushing the barrel.")]
	public bool playSkeletonTaunt = false;
	[Tooltip("Speed at which to lock on player.")]
	public float enemyAimSpeed = 7.6f;
	[Tooltip("The bolt fired by the crossbow.")]
	public GameObject boltPrefab;
	GameObject arrow;
	public GameObject arrow_06;

	public enum AttackType {
		short_range_Spear_1 = 1,
		short_range_Spear_2 = 2,
		long_range_Spear = 3,
		Crossbow = 4,
		Throw_Barrel = 5,
		jump_and_attack = 6,
		jump_and_long_range_attack = 7
	}
	
	const float BOLT_FORCE = 700f;
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

		randomizeLook ();
	}

	void Start ()
	{
		//StartCoroutine( playIdleAnimation() );
	}

	//We don't want all the skeletons to have synced animations
	IEnumerator playIdleAnimation()
	{
		yield return new WaitForSeconds( Random.value * 3f );
		GetComponent<Animator>().CrossFadeInFixedTime( "fun1", CROSS_FADE_DURATION );
	}

	//We don't want all skeletons to look the same
	void randomizeLook ()
	{
		if( Random.value < 0.3f )
		{
			if( clothItem1 != null ) clothItem1.SetActive( false );
		}
		if( Random.value < 0.3f )
		{
			if( clothItem2 != null ) clothItem2.SetActive( false );
		}
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
		if ( Input.GetKeyDown (KeyCode.RightArrow) ) 
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
					attackDistance = 0.85f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setCreatureState( CreatureState.Attacking );
						GetComponent<Animator>().CrossFadeInFixedTime( "attack1", CROSS_FADE_DURATION );
					}
					break;
		                
		        case AttackType.short_range_Spear_2:
					attackDistance = 0.85f * PlayerController.getPlayerSpeed();
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
		GameObject nextArrow = (GameObject)Instantiate( boltPrefab );
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
		arrow.GetComponent<Rigidbody>().AddForce( transform.forward * getAdjustedBoltForce() );
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
			StartCoroutine( playVictoryAnimation() );
		}
	}

	IEnumerator playVictoryAnimation()
	{
		GetComponent<Animator>().CrossFadeInFixedTime( "idle", CROSS_FADE_DURATION );
		yield return new WaitForSeconds( Random.value * 2f );
		if( Random.value < 0.5f )
		{
			GetComponent<Animator>().CrossFadeInFixedTime( "fun1", CROSS_FADE_DURATION );
		}
		else
		{
			GetComponent<Animator>().CrossFadeInFixedTime( "fun2", CROSS_FADE_DURATION );
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

}
