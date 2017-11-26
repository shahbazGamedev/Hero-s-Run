using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public sealed class GoblinController : Creature, ICreature {

	[Header("Goblin Controller")]
	[Header("General")]
	public AttackType attackType = AttackType.short_range_Spear_1;
	public bool applyGravity = true;
	[Header("Audio")]
	public AudioClip footstepLeftSound;
	public AudioClip footstepRightSound;
	public AudioClip ouch;
	public AudioClip win;
	[Header("Clothing")]
	[Tooltip("There is 30% chance that each of the following cloth item will be hidden. This is so not all goblins look alike.")]
	public GameObject clothItem1;
	public GameObject clothItem2;
	[Header("Pushed/thrown object like a barrel or a big snow ball")]
	[Tooltip("The rigid body of the object that the goblin will push on top of the player.")]
	Rigidbody pushedObject;
	public Transform locationOfPushedObject;
	public GameObject pushedObjectPrefab;
	[Tooltip("The forward (based on the goblin's transform) force to apply on the barrel.")]
	public float barrelForwardForce = 1300f; //Based on 10 kilograms
	[Tooltip("Player distance multiplier used to decide to throw barrel.")]
	public float barrelPlayerDistanceMultiplier = 1.45f;
	[Tooltip("Player distance multiplier used to decide when to jump.")]
	public float jumpPlayerDistanceMultiplier = 3.3f;
	[Tooltip("Whether or not the goblin should play a diabolical laughter before pushing the barrel.")]
	public bool playGoblinTaunt = false;
	[Tooltip("The bolt fired by the crossbow.")]
	public GameObject boltPrefab;

	public enum AttackType {
		short_range_Spear_1 = 1,
		short_range_Spear_2 = 2,
		long_range_Spear = 3,
		Crossbow = 4,
		Push_Object = 5,
		jump_and_attack = 6,
		jump_and_long_range_attack = 7
	}
	
	const float BOLT_FORCE = 700f;

	//Only use for the scout goblin with the crossbow
	Vector3 initialBoltPositionOffset = new Vector3( 0f, 0.47f, 0.46f );

	//Original setup used when reseting the Creature
	AttackType originalAttackType;

	//Movement related
	Vector3 forward;
	float runSpeed = 4.6f; //good value so feet don't slide
	bool previouslyGrounded = true;

	new void Awake ()
	{
		base.Awake();
		randomizeLook();
	}

	void Start ()
	{
		StartCoroutine( playIdleAnimation() );
	}

	//We don't want all the goblins to have synced animations
	IEnumerator playIdleAnimation()
	{
		yield return new WaitForSeconds( Random.value * 3f );
		anim.CrossFadeInFixedTime( "fun1", CROSS_FADE_DURATION );
	}

	//We don't want all goblins to look the same
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
		moveGoblin();
		handleAttackType();
	}

	void moveGoblin()
	{
		if( creatureState == CreatureState.Running || creatureState == CreatureState.Jumping )
		{
			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				targetPlayer();
			}
			//1) Get the direction of the goblin
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			forward = forward * Time.deltaTime * runSpeed;
			if( applyGravity ) forward.y -= 16f * Time.deltaTime;
			//3) Move the controller
			controller.Move( forward );

			if (controller.isGrounded && !previouslyGrounded )
			{
				audioSource.PlayOneShot( knockbackSound );
				anim.CrossFadeInFixedTime( "run", CROSS_FADE_DURATION );
				setCreatureState( CreatureState.Running );
			}
			previouslyGrounded = controller.isGrounded;
		}


	}

	void handleAttackType()
	{
		if( creatureState != CreatureState.Attacking && creatureState != CreatureState.Dying && creatureState != CreatureState.Victory )
		{
			float distance = Vector3.Distance(getPlayer().position,transform.position);
			float attackDistance;
			float playerSpeed = getPlayer().GetComponent<PlayerRun>().getRunSpeed();
		    switch (attackType)
			{
		        case AttackType.short_range_Spear_1:
					attackDistance = 0.85f * playerSpeed;
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setCreatureState( CreatureState.Attacking );
						anim.CrossFadeInFixedTime( "attack1", CROSS_FADE_DURATION );
					}
					break;
		                
		        case AttackType.short_range_Spear_2:
					attackDistance = 0.85f * playerSpeed;
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setCreatureState( CreatureState.Attacking );
						anim.CrossFadeInFixedTime( "attack2", CROSS_FADE_DURATION );
					}
					break;
		                
				case AttackType.long_range_Spear:
					attackDistance = 2f * playerSpeed;
					if( distance < attackDistance )
					{
						followsPlayer = true;
						setCreatureState( CreatureState.Running );
						anim.Play( "run" );
					}
					break;
			
				case AttackType.Crossbow:
					attackDistance = 2.2f * playerSpeed;
					//Only attack if the player is inside a 30 degree arc in front of goblin
					if( distance < attackDistance && getDotProduct() > 0.85f )
					{
						setCreatureState( CreatureState.Attacking );
						fireCrossbow();
					}
					break;
				case AttackType.Push_Object:
					attackDistance = barrelPlayerDistanceMultiplier * playerSpeed;
					if( distance < attackDistance )
					{
						setCreatureState( CreatureState.Attacking );
						pushObject();
					}
					break;
				case AttackType.jump_and_attack:
					float jumpDistance = jumpPlayerDistanceMultiplier * playerSpeed;
					attackDistance = 0.85f * playerSpeed;
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
				case AttackType.jump_and_long_range_attack:
					float jumpLongDistance = jumpPlayerDistanceMultiplier * playerSpeed;
					if( distance < jumpLongDistance )
					{
						if( creatureState != CreatureState.Running && creatureState != CreatureState.Jumping )
						{
							//Jump and run once you land
							followsPlayer = true;
							setCreatureState( CreatureState.Jumping );
							anim.CrossFadeInFixedTime( "jump", CROSS_FADE_DURATION );
							attackType = AttackType.long_range_Spear;
						}
					}
					break;
			}
		}
	}
	
	void fireCrossbow()
	{
		if( gameObject.activeSelf ) StartCoroutine( fireCrossbowNow() );
	}

	IEnumerator fireCrossbowNow()
	{
		yield return new WaitForSeconds( Random.value * 1f );
		GameObject bolt = (GameObject)Instantiate(boltPrefab);
		transform.LookAt( getPlayer() );
		bolt.transform.rotation = Quaternion.Euler( transform.eulerAngles.x, transform.eulerAngles.y, 0 );
		transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
		Vector3 initialBoltPosition = transform.TransformPoint( initialBoltPositionOffset );
		bolt.transform.position = initialBoltPosition;
		anim.CrossFadeInFixedTime( "attack", CROSS_FADE_DURATION );
		Physics.IgnoreCollision(bolt.GetComponent<Collider>(), transform.GetComponent<CapsuleCollider>());
		Physics.IgnoreCollision(bolt.GetComponent<Collider>(), transform.GetComponent<CharacterController>());
		bolt.GetComponent<Rigidbody>().AddForce(bolt.transform.forward * BOLT_FORCE );
		bolt.GetComponent<Projectile>().launchProjectile();
		//destroy the bolt after 8 seconds
		GameObject.Destroy( bolt, 8f );
	}

	void pushObject()
	{
		if( playGoblinTaunt ) audioSource.PlayOneShot( win, 0.7f );
		//Push barrels in the direction of the goblin and add a small upward force
		Vector3 forces = transform.forward * barrelForwardForce + new Vector3( 0, 400f, 0 );
		pushedObject.isKinematic = false;
		pushedObject.AddForce( forces );
		pushedObject.AddTorque( new Vector3( 0, 300f, 0 ) );
		anim.CrossFadeInFixedTime( "push", CROSS_FADE_DURATION );
	}



	public void sideCollision()
	{
		audioSource.PlayOneShot( ouch );
		anim.CrossFadeInFixedTime( "damage", CROSS_FADE_DURATION );
	}

	public void shrink( Transform caster, bool value )
	{
	}

	public void victory( bool playWinSound )
	{
		if( creatureState != CreatureState.Dying )
		{
			if( playWinSound ) audioSource.PlayOneShot( win );
			setCreatureState( CreatureState.Victory );
			StartCoroutine( playVictoryAnimation() );
		}
	}

	IEnumerator playVictoryAnimation()
	{
		anim.CrossFadeInFixedTime( "idle", CROSS_FADE_DURATION );
		yield return new WaitForSeconds( Random.value * 2f );
		if( Random.value < 0.5f )
		{
			anim.CrossFadeInFixedTime( "fun1", CROSS_FADE_DURATION );
		}
		else
		{
			anim.CrossFadeInFixedTime( "fun2", CROSS_FADE_DURATION );
		}
	}

	//The goblin falls over backwards, typically because the player slid into him or because of a ZNuke
	public new void knockback( Transform attacker )
	{
		base.knockback( attacker );
		anim.CrossFadeInFixedTime( "death", CROSS_FADE_DURATION );
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( getPlayer().GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying )
		{
			//The Pendulum (bad name, yes I know) is the spike road block
			if( hit.collider.name.StartsWith("Goblin") || hit.gameObject.CompareTag("Player") || hit.collider.name.StartsWith("Pendulum") )
			{
				//If a goblin collides with another goblin, the road block or the Hero while the player is dead, have him stop moving and play the victory sequence.
				victory( false );
			}
		}
	}

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
		createPushedObject();
	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
		destroyPushedObject();
	}

	void createPushedObject()
	{
		if( locationOfPushedObject != null && pushedObjectPrefab != null )
		{
			GameObject go = (GameObject)Instantiate(pushedObjectPrefab);
			go.transform.SetParent( transform.parent );
			go.name = "Fence";
			go.transform.localPosition = locationOfPushedObject.localPosition;
			go.transform.localRotation = locationOfPushedObject.localRotation;
			pushedObject = go.GetComponent<Rigidbody>();
		}
	}

	void destroyPushedObject()
	{
		if( locationOfPushedObject != null && pushedObjectPrefab != null )
		{
			GameObject.Destroy( pushedObject.gameObject );
			pushedObject = null;
		}
	}

	void PlayerStateChange( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			float distance = Vector3.Distance(getPlayer().position,transform.position);
			float nearby = 4f;
			if( distance < nearby )
			{
				victory( false );
				Debug.Log("Goblin PlayerStateChange - player is dead and nearby");
			}
		}
		else if( newState == PlayerCharacterState.Falling )
		{
			halt();
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

}
