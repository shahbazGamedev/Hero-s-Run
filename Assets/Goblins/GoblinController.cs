using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoblinController : BaseClass {

	[Header("Goblin Controller")]
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
	[Tooltip("There is 30% chance that each of the following cloth item will be hidden. This is so not all goblins look alike.")]
	public GameObject clothItem1;
	public GameObject clothItem2;
	[Header("Barrel")]
	[Tooltip("The breakable barrel that the goblin will push on top of the player.")]
	public Rigidbody barrel;
	[Tooltip("Whether or not the goblin should play a diabolical laughter before pushing the barrel.")]
	public bool playGoblinTaunt = false;

	public enum GoblinState {
		Idle = 1,
		Running = 2,
		Attacking = 3,
		Dying = 4,
		Victory = 5
	}

	public enum AttackType {
		short_range_Spear_1 = 1,
		short_range_Spear_2 = 2,
		long_range_Spear = 3,
		Crossbow = 4,
		Throw_Barrel = 5
	}
	
	const float BOLT_FORCE = 400f;

	//Only use for the scout goblin with the crossbow
	GameObject boltPrefab;
	Vector3 initialBoltPositionOffset = new Vector3( 0f, 0.47f, 0.46f );

	PlayerController playerController;
	Transform player;

	//Movement related
	GoblinState goblinState = GoblinState.Idle;
	CharacterController controller;
	Vector3 forward;
	float runSpeed = 4.5f; //good value so feet don't slide
	//If true, the goblin heads for the player as opposed to staying in his lane
	bool followsPlayer = false;

	void Awake ()
	{
		controller = GetComponent<CharacterController>();
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = player.gameObject.GetComponent<PlayerController>();

		randomizeLook ();

		if( attackType == AttackType.Crossbow )
		{
			boltPrefab = Resources.Load( "Level/Props/Magic Bolt") as GameObject;
		}
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
		if ( Input.GetKeyDown (KeyCode.A) ) 
		{
			if( attackType == AttackType.Crossbow )
			{
				Debug.Log("KeyInput - Fire bolt");
				fireCrossbow();
			}
		}
	}

	void moveGoblin()
	{
		if( goblinState == GoblinState.Running )
		{
			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				transform.LookAt( player );
				transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
			}
			//1) Get the direction of the goblin
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			forward = forward * Time.deltaTime * runSpeed;
			if( applyGravity ) forward.y -= 16f * Time.deltaTime;
			//3) Move the controller
			controller.Move( forward );
		}
	}

	void handleAttackType()
	{
		if( goblinState != GoblinState.Attacking && goblinState != GoblinState.Dying && goblinState != GoblinState.Victory )
		{
			float distance = Vector3.Distance(player.position,transform.position);
			float attackDistance;
		    switch (attackType)
			{
		        case AttackType.short_range_Spear_1:
					attackDistance = 0.85f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setGoblinState( GoblinState.Attacking );
						GetComponent<Animator>().Play("attack1");
					}
					break;
		                
		        case AttackType.short_range_Spear_2:
					attackDistance = 0.85f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setGoblinState( GoblinState.Attacking );
						GetComponent<Animator>().Play("attack2");
					}
					break;
		                
				case AttackType.long_range_Spear:
					attackDistance = 2.5f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						followsPlayer = true;
						setGoblinState( GoblinState.Running );
						GetComponent<Animator>().Play("run");
					}
					break;
			
				case AttackType.Crossbow:
					attackDistance = 2.5f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						setGoblinState( GoblinState.Attacking );
						fireCrossbow();
					}
					break;
				case AttackType.Throw_Barrel:
					attackDistance = 1.5f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						setGoblinState( GoblinState.Attacking );
						throwBarrel();
					}
					break;
			}
		}
	}

	/*
		returns:
		-1 if goblin is behind player
		+1 if goblin is in front
		0 if goblin is on the side
		0.5 if goblin is facing player and within 30 degrees
	*/
	float getDotProduct()
	{
		Vector3 heading = player.position - transform.position;
		return Vector3.Dot( heading.normalized, transform.forward );
	}

	void fireCrossbow()
	{
		if( gameObject.activeSelf ) StartCoroutine( fireCrossbowNow() );
	}

	IEnumerator fireCrossbowNow()
	{
		yield return new WaitForSeconds( Random.value * 1f );
		GameObject bolt = (GameObject)Instantiate(boltPrefab);
		transform.LookAt( player );
		bolt.transform.rotation = Quaternion.Euler( transform.eulerAngles.x, transform.eulerAngles.y, 0 );
		transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
		Vector3 initialBoltPosition = transform.TransformPoint( initialBoltPositionOffset );
		bolt.transform.position = initialBoltPosition;
		GetComponent<Animator>().Play("attack");
		Physics.IgnoreCollision(bolt.GetComponent<Collider>(), transform.GetComponent<CapsuleCollider>());
		Physics.IgnoreCollision(bolt.GetComponent<Collider>(), transform.GetComponent<CharacterController>());
		bolt.GetComponent<Rigidbody>().AddForce(bolt.transform.forward * getAdjustedBoltForce() );
		//destroy the bolt after 10 seconds
		GameObject.Destroy( bolt, 10f );
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
		if( playGoblinTaunt ) GetComponent<AudioSource>().PlayOneShot( win );
		//Push barrels in the direction of the goblin and add a small upward force
		Vector3 forces = transform.forward * 1300f + new Vector3( 0, 400f, 0 );
		barrel.AddForce( forces );
		barrel.AddTorque( new Vector3( 0, 300f, 0 ) );
		GetComponent<Animator>().Play("attack2");
	}


	public GoblinState getGoblinState()
	{
		return goblinState;
	}

	public void setGoblinState( GoblinState state )
	{
		goblinState = state;
		if( goblinState == GoblinState.Victory )
		{
			StartCoroutine( playVictoryAnimation() );
		}
	}

	public void sideCollision()
	{
		GetComponent<AudioSource>().PlayOneShot( ouch );
		GetComponent<Animator>().Play("damage");
	}

	public void victory( bool playWinSound )
	{
		if( playWinSound ) GetComponent<AudioSource>().PlayOneShot( win );
		setGoblinState( GoblinState.Victory );
	}

	IEnumerator playVictoryAnimation()
	{
		yield return new WaitForSeconds( Random.value * 0.3f );
		if( Random.value < 0.5f )
		{
			GetComponent<Animator>().Play("fun1");
		}
		else
		{
			GetComponent<Animator>().Play("fun2");
		}
	}

	//The goblin falls over backwards, typically because the player slid into him or because of a ZNuke
	public void knockbackGoblin()
	{
		setGoblinState( GoblinState.Dying );
		controller.enabled = false;
		GetComponent<CapsuleCollider>().enabled = false;
		GetComponent<Animator>().Play("death");
		GetComponent<AudioSource>().PlayOneShot( fallToGround );
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( PlayerController._characterState == CharacterState.Dying )
		{
			if( hit.collider.name.StartsWith("Goblin") || hit.collider.name.StartsWith("Hero"))
			{
				//If a goblin collides with another goblin or the Hero while the player is dead, have him stop moving and play the victory sequence.
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
			Debug.Log("Goblin PlayerStateChange - player is dead");
		}
	}

	public void resetGoblin()
	{
		setGoblinState( GoblinState.Idle );
		GetComponent<Animator>().Play("idle");
		gameObject.SetActive( false );
		followsPlayer = false;
		controller.enabled = true;
		GetComponent<CapsuleCollider>().enabled = true;

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
