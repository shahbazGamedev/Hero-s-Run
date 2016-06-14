using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CerberusController : BaseClass {

	[Header("Cerberus Controller")]
	[Header("General")]
	public AttackType attackType = AttackType.stand_and_breathe_fire;
	public bool applyGravity = true;
	[Header("Audio")]
	public AudioClip footstepLeftSound;
	public AudioClip footstepRightSound;
	public AudioClip ouch;
	public AudioClip fallToGround;
	public AudioClip win;

	[Header("Fire Breathing Attack")]
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
		stand_and_breathe_fire = 1,
		jump_and_attack  = 2,
		run_and_attack = 3
	}

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
		playerController = player.GetComponent<PlayerController>();
	}

	void Update ()
	{
		moveCerberus();
		handleAttackType();
		if ( Input.GetKeyDown (KeyCode.A) ) 
		{
			if( attackType == AttackType.stand_and_breathe_fire )
			{
				Debug.Log("Breathe fire");
				breatheFire();
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
		        case AttackType.stand_and_breathe_fire:
					attackDistance = 0.95f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						breatheFire();
					}
					break;
		                
		        case AttackType.jump_and_attack:
					attackDistance = 0.85f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setCerberusState( CerberusState.Attacking );
						GetComponent<Animator>().Play("attack2");
					}
					break;
		                
				case AttackType.run_and_attack:
					attackDistance = 2f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance )
					{
						followsPlayer = true;
						setCerberusState( CerberusState.Running );
						allowMove = true;
						GetComponent<Animator>().Play("run");
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

	void breatheFire()
	{
		setCerberusState( CerberusState.Attacking );
		GetComponent<Animator>().SetTrigger("blowFireAggressive");
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

	void stopBreathingFire()
	{
		centerHeadFireObject.SetActive( false );
		leftHeadFireObject.SetActive( false );
		rightHeadFireObject.SetActive( false );
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
		GetComponent<Animator>().Play("getHitAggressive");
	}

	public void victory( bool playWinSound )
	{
		if( cerberusState != CerberusState.Dying )
		{
			if( playWinSound ) GetComponent<AudioSource>().PlayOneShot( win );
			setCerberusState( CerberusState.Victory );
			GetComponent<Animator>().Play("idleLookAround");
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
		GetComponent<Animator>().Play("deathAggressive");
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
		GetComponent<Animator>().Play("idleLookAround");
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
