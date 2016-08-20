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
	[Header("Look At IK")]
	public float lookAtWeight = 0.8f;
	public float bodyWeight = 0.7f;
	public float headWeight = 1f;
	public float eyesWeight = 1f;
	public float clampWeight = 1f;
	bool lookAtActive = false;


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
	}

	public void sideCollision ()
	{
	}

	void configureSelectedWeapon()
	{
		if( attackType == AttackType.charge_and_attack || attackType == AttackType.stand_and_big_attack || attackType == AttackType.stand_and_normal_attack )
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
			float distance = Vector3.Distance(player.position,transform.position);
			float attackDistance;
		    switch (attackType)
			{
		        case AttackType.stand_and_normal_attack:
					attackDistance = 0.65f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						setCreatureState( CreatureState.Attacking );
						anim.CrossFadeInFixedTime( "attack2" , CROSS_FADE_DURATION );
					}
					break;
		                
		        case AttackType.stand_and_big_attack:
					attackDistance = 0.65f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						setCreatureState( CreatureState.Attacking );
						anim.CrossFadeInFixedTime( "attack1" , CROSS_FADE_DURATION );
					}
					break;
		                
				case AttackType.charge_and_attack:
					float chargeDistance = 2.3f * PlayerController.getPlayerSpeed();
					attackDistance = 0.65f * PlayerController.getPlayerSpeed();
					if( distance < chargeDistance )
					{
						if( distance >= attackDistance )
						{
							if( creatureState != CreatureState.Running )
							{
								//Charge
								mainCamera.GetComponent<MotionBlur>().enabled = true;			
								followsPlayer = true;
								moveSpeed = getAdjustedChargeSpeed();
								setCreatureState( CreatureState.Running );
								anim.CrossFadeInFixedTime( "move" , CROSS_FADE_DURATION );
								GetComponent<AudioSource>().PlayOneShot( charge );
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
					float startWalkingDistance = 3.3f * PlayerController.getPlayerSpeed();
					if( distance < startWalkingDistance )
					{
						if( getCreatureState() != CreatureState.Walking )
						{
							//Walk
							followsPlayer = false;
							moveSpeed = WALK_SPEED;
							setCreatureState( CreatureState.Walking );
							anim.Play( "move" );
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
		anim.CrossFadeInFixedTime( "idle" , CROSS_FADE_DURATION );
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
		if( creatureState != CreatureState.Dying )
		{
			mainCamera.GetComponent<MotionBlur>().enabled = false;			
			if( playWinSound ) GetComponent<AudioSource>().PlayOneShot( win );
			setCreatureState( CreatureState.Victory );
			anim.CrossFadeInFixedTime( "idle" , CROSS_FADE_DURATION );
		}
	}

	//The wraith falls over backwards, typically because the player slid into him or because of a ZNuke
	public new void knockback()
	{
		base.knockback();
		anim.SetTrigger("Knockback");
		GetComponent<Rigidbody>().isKinematic = false;
		mainCamera.GetComponent<MotionBlur>().enabled = false;			
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
			Stop_Weapon_Trail ( null );
			float distance = Vector3.Distance(player.position,transform.position);
			float nearby = 5f;
			if( distance < nearby )
			{
				victory( false );
				StartCoroutine( fadeOutLookAtPosition( 0.2f, 2f + Random.value * 2f, 0.9f ) );
			}
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
		if( weaponTrailScythe != null && weaponType == WeaponType.Scythe ) weaponTrailScythe.SetActive( false );
		if(weaponTrailAxe != null && weaponType == WeaponType.Axe ) weaponTrailAxe.SetActive( false );
	}

	//For SetLookAtPosition to work, there are 2 conditions:
	//The rig must be Humanoid
	//In the Animator windows, under Layers, under Settings, you must have the IK Pass toggled on.
	void OnAnimatorIK()
	{
		if( getDotProduct() > 0.55f )
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

	IEnumerator fadeOutLookAtPosition( float finalWeight, float stayDuration, float fadeDuration )
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

	IEnumerator fadeInLookAtPosition( float finalWeight, float fadeDuration )
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
