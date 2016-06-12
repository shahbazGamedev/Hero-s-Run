using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CerberusController : BaseClass {

	[Header("Cerberus Controller")]
	[Header("General")]
	public AttackType attackType = AttackType.fire_attack;
	public bool applyGravity = true;
	[Header("Audio")]
	public AudioClip footstepLeftSound;
	public AudioClip footstepRightSound;
	public AudioClip ouch;
	public AudioClip fallToGround;
	public AudioClip win;
	[Header("Clothing")]
	[Tooltip("There is 30% chance that each of the following cloth item will be hidden. This is so not all cerberuss look alike.")]
	public GameObject clothItem1;
	public GameObject clothItem2;
	[Header("Barrel")]
	[Tooltip("The breakable barrel that the cerberus will push on top of the player.")]
	public Rigidbody barrel;
	[Tooltip("Whether or not the cerberus should play a diabolical laughter before pushing the barrel.")]
	public bool playCerberusTaunt = false;

	public GameObject leftHeadFireObject;
	public GameObject centerHeadFireObject;
	public GameObject rightHeadFireObject;
	public ParticleSystem leftHeadFire;
	public ParticleSystem centerHeadFire;
	public ParticleSystem rightHeadFire;
	public AudioClip fireBreath;

	public enum CerberusState {
		Idle = 1,
		Running = 2,
		Attacking = 3,
		Dying = 4,
		Victory = 5
	}

	public enum AttackType {
		fire_attack = 1,
		short_range_Spear_2 = 2,
		long_range_Spear = 3,
		Crossbow = 4,
		Throw_Barrel = 5
	}
	
	const float BOLT_FORCE = 600f;

	//Only use for the scout cerberus with the crossbow
	GameObject boltPrefab;
	Vector3 initialBoltPositionOffset = new Vector3( 0f, 0.47f, 0.46f );

	PlayerController playerController;
	Transform player;

	//Movement related
	CerberusState cerberusState = CerberusState.Idle;
	CharacterController controller;
	Vector3 forward;
	float runSpeed = 4.5f; //good value so feet don't slide
	//If true, the cerberus heads for the player as opposed to staying in his lane
	bool followsPlayer = false;
	bool allowMove = false;


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

	//We don't want all cerberuss to look the same
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
		moveCerberus();
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

	void moveCerberus()
	{
		if( cerberusState == CerberusState.Running && allowMove )
		{
			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				transform.LookAt( player );
				transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
			}
			//1) Get the direction of the cerberus
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
		if( cerberusState != CerberusState.Attacking && cerberusState != CerberusState.Dying && cerberusState != CerberusState.Victory )
		{
			float distance = Vector3.Distance(player.position,transform.position);
			float attackDistance;
		    switch (attackType)
			{
		        case AttackType.fire_attack:
					attackDistance = 0.95f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setCerberusState( CerberusState.Attacking );
						GetComponent<Animator>().Play("blowFireAggressive");
						centerHeadFireObject.SetActive( true );
						centerHeadFire.Play();
						leftHeadFireObject.SetActive( true );
						leftHeadFire.Play();
						rightHeadFireObject.SetActive( true );
						rightHeadFire.Play();
						GetComponent<AudioSource>().clip = fireBreath;
						GetComponent<AudioSource>().Play();
						Invoke( "stopBreathingFire", 2.8f );
					}
					break;
		                
		        case AttackType.short_range_Spear_2:
					attackDistance = 0.85f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setCerberusState( CerberusState.Attacking );
						GetComponent<Animator>().Play("attack2");
					}
					break;
		                
				case AttackType.long_range_Spear:
					attackDistance = 2f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						followsPlayer = true;
						setCerberusState( CerberusState.Running );
						allowMove = true;
						GetComponent<Animator>().Play("run");
					}
					break;
			
				case AttackType.Crossbow:
					attackDistance = 2.5f * PlayerController.getPlayerSpeed();
					//Only attack if the player is inside a 30 degree arc in front of cerberus
					if( distance < attackDistance && getDotProduct() > 0.85f )
					{
						setCerberusState( CerberusState.Attacking );
						fireCrossbow();
					}
					break;
				case AttackType.Throw_Barrel:
					attackDistance = 1.45f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						setCerberusState( CerberusState.Attacking );
						throwBarrel();
					}
					break;
			}
		}
	}

	/*
		returns:
		-1 if cerberus is behind player
		+1 if cerberus is in front
		0 if cerberus is on the side
		0.5 if cerberus is facing player and within 60 degrees (i.e. between 30 degrees to the left and 30 degrees to the right)
	*/
	float getDotProduct()
	{
		Vector3 heading = player.position - transform.position;
		return Vector3.Dot( heading.normalized, transform.forward );
	}

	void stopBreathingFire()
	{
		centerHeadFireObject.SetActive( false );
		leftHeadFireObject.SetActive( false );
		rightHeadFireObject.SetActive( false );
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
		//destroy the bolt after 8 seconds
		GameObject.Destroy( bolt, 8f );
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
		if( playCerberusTaunt ) GetComponent<AudioSource>().PlayOneShot( win );
		//Push barrels in the direction of the cerberus and add a small upward force
		Vector3 forces = transform.forward * 1300f + new Vector3( 0, 400f, 0 );
		barrel.AddForce( forces );
		barrel.AddTorque( new Vector3( 0, 300f, 0 ) );
		GetComponent<Animator>().Play("attack2");
	}


	public CerberusState getCerberusState()
	{
		return cerberusState;
	}

	public void setCerberusState( CerberusState state )
	{
		cerberusState = state;
	}

	public void sideCollision()
	{
		GetComponent<AudioSource>().PlayOneShot( ouch );
		GetComponent<Animator>().Play("damage");
	}

	public void victory( bool playWinSound )
	{
		if( cerberusState != CerberusState.Dying )
		{
			if( playWinSound ) GetComponent<AudioSource>().PlayOneShot( win );
			setCerberusState( CerberusState.Victory );
			StartCoroutine( playVictoryAnimation() );
		}
	}

	IEnumerator playVictoryAnimation()
	{
		GetComponent<Animator>().Play("idle");
		yield return new WaitForSeconds( Random.value * 2f );
		if( Random.value < 0.5f )
		{
			GetComponent<Animator>().Play("fun1");
		}
		else
		{
			GetComponent<Animator>().Play("fun2");
		}
	}

	//The cerberus falls over backwards, typically because the player slid into him or because of a ZNuke
	public void knockbackCerberus()
	{
		setCerberusState( CerberusState.Dying );
		controller.enabled = false;
		//The piker has two capsule colliders. The scout, only one.
		CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
		for( int i = 0; i < capsuleColliders.Length; i++ )
		{
			capsuleColliders[i].enabled = false;
		}
		GetComponent<Animator>().Play("death");
		GetComponent<AudioSource>().PlayOneShot( fallToGround );
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( PlayerController._characterState == CharacterState.Dying )
		{
			//The Pendulum (bad name, yes I know) is the spike road block
			if( hit.collider.name.StartsWith("Cerberus") || hit.collider.name.StartsWith("Hero") || hit.collider.name.StartsWith("Pendulum") )
			{
				//If a cerberus collides with another cerberus, the road block or the Hero while the player is dead, have him stop moving and play the victory sequence.
				victory( false );
			}
		}
	}

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
		GameManager.gameStateEvent += GameStateChange;
	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
		GameManager.gameStateEvent -= GameStateChange;
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
				Debug.Log("Cerberus PlayerStateChange - player is dead and nearby");
			}
		}
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Paused )
		{
			allowMove = false;
			controller.enabled = false;
			
		}
		else if( newState == GameState.Checkpoint )
		{
			allowMove = false;
			controller.enabled = false;
		}
		else if( newState == GameState.Normal )
		{
			allowMove = true;
			controller.enabled = true;
		}
	}


	public void resetCerberus()
	{
		setCerberusState( CerberusState.Idle );
		GetComponent<Animator>().Play("idle");
		gameObject.SetActive( false );
		followsPlayer = false;
		controller.enabled = true;
		CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
		for( int i = 0; i < capsuleColliders.Length; i++ )
		{
			capsuleColliders[i].enabled = true;
		}
		allowMove = false;
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
