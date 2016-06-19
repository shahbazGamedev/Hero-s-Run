using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DemonController : BaseClass {

	[Header("Demon Controller")]
	[Header("General")]
	public AttackType attackType = AttackType.stand_and_normal_attack;
	public bool applyGravity = true;
	[Header("Audio")]
	public AudioClip footstepLeftSound;
	public AudioClip footstepRightSound;
	public AudioClip ouch;
	public AudioClip fallToGround;
	public AudioClip win;
	[Header("Particle Systems")]
	public ParticleSystem sparksLeftHoof;
	public ParticleSystem sparksRightHoof;
	public GameObject weaponTrail;

	public enum DemonState {
		Idle = 1,
		Running = 2,
		Walking = 3,
		Attacking = 4,
		Dying = 5,
		Victory = 6
	}

	public enum AttackType {
		stand_and_normal_attack = 1,
		stand_and_big_attack = 2,
		charge_and_attack = 3,
		walk_and_attack = 4
	}
	
	PlayerController playerController;
	Transform player;
	const float CROSS_FADE_DURATION = 0.5f;

	//Movement related
	DemonState demonState = DemonState.Idle;
	CharacterController controller;
	Vector3 forward;
	const float RUN_SPEED = 4.6f; //good value so feet don't slide
	float WALK_SPEED = 2.1f; //good value so feet don't slide
	float moveSpeed = 0;
	//If true, the demon heads for the player as opposed to staying in his lane
	bool followsPlayer = false;
	bool allowMove = false;


	void Awake ()
	{
		controller = GetComponent<CharacterController>();
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = player.gameObject.GetComponent<PlayerController>();
	}

	void Update ()
	{
		moveDemon();
		handleAttackType();
	}

	void moveDemon()
	{
		if( demonState == DemonState.Running || demonState == DemonState.Walking && allowMove )
		{

			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				transform.LookAt( player );
				transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
			}
			//1) Get the direction of the demon
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			forward = forward * Time.deltaTime * moveSpeed;
			if( applyGravity ) forward.y -= 16f * Time.deltaTime;
			//3) Move the controller
			controller.Move( forward );
		}
	}

	void handleAttackType()
	{
		if( demonState != DemonState.Attacking && demonState != DemonState.Dying && demonState != DemonState.Victory )
		{
			float distance = Vector3.Distance(player.position,transform.position);
			float attackDistance;
		    switch (attackType)
			{
		        case AttackType.stand_and_normal_attack:
					attackDistance = 0.76f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setDemonState( DemonState.Attacking );
						GetComponent<Animator>().CrossFadeInFixedTime( "Attack" , CROSS_FADE_DURATION );
					}
					break;
		                
		        case AttackType.stand_and_big_attack:
					attackDistance = 0.95f * PlayerController.getPlayerSpeed();
					if( distance < attackDistance && getDotProduct() > 0.98f )
					{
						setDemonState( DemonState.Attacking );
						GetComponent<Animator>().CrossFadeInFixedTime( "Skill" , CROSS_FADE_DURATION );
					}
					break;
		                
				case AttackType.charge_and_attack:
					float chargeDistance = 2.3f * PlayerController.getPlayerSpeed();
					attackDistance = 0.97f * PlayerController.getPlayerSpeed();
					if( distance < chargeDistance )
					{
						if( distance >= attackDistance )
						{
							//Charge
							followsPlayer = true;
							moveSpeed = RUN_SPEED;
							setDemonState( DemonState.Running );
							allowMove = true;
							GetComponent<Animator>().CrossFadeInFixedTime( "Run" , CROSS_FADE_DURATION );
						}
						else
						{
							//Attack now
							setDemonState( DemonState.Attacking );
							GetComponent<Animator>().CrossFadeInFixedTime( "Skill" , CROSS_FADE_DURATION );
						}
					}
					break;
				
				case AttackType.walk_and_attack:
					float walkDistance = 2.5f * PlayerController.getPlayerSpeed();
					attackDistance = 0.97f * PlayerController.getPlayerSpeed();
					if( distance < walkDistance )
					{
						if( distance >= attackDistance )
						{
							//Walk
							followsPlayer = true;
							moveSpeed = WALK_SPEED;
							setDemonState( DemonState.Walking );
							allowMove = true;
							GetComponent<Animator>().CrossFadeInFixedTime( "Walk" , CROSS_FADE_DURATION );
						}
						else
						{
							//Attack now
							setDemonState( DemonState.Attacking );
							GetComponent<Animator>().CrossFadeInFixedTime( "Skill" , CROSS_FADE_DURATION );
						}
					}
					break;
			}
		}
	}

	/*
		returns:
		-1 if demon is behind player
		+1 if demon is in front
		0 if demon is on the side
		0.5 if demon is facing player and within 60 degrees (i.e. between 30 degrees to the left and 30 degrees to the right)
	*/
	float getDotProduct()
	{
		Vector3 heading = player.position - transform.position;
		return Vector3.Dot( heading.normalized, transform.forward );
	}

	public DemonState getDemonState()
	{
		return demonState;
	}

	public void setDemonState( DemonState state )
	{
		demonState = state;
	}

	public void sideCollision()
	{
		GetComponent<AudioSource>().PlayOneShot( ouch );
		GetComponent<Animator>().CrossFadeInFixedTime( "Damage" , CROSS_FADE_DURATION );
	}

	public void victory( bool playWinSound )
	{
		if( demonState != DemonState.Dying )
		{
			if( playWinSound ) GetComponent<AudioSource>().PlayOneShot( win );
			setDemonState( DemonState.Victory );
			GetComponent<Animator>().CrossFadeInFixedTime( "Idle" , CROSS_FADE_DURATION );
		}
	}

	//The demon falls over backwards, typically because the player slid into him or because of a ZNuke
	public void knockbackDemon()
	{
		setDemonState( DemonState.Dying );
		controller.enabled = false;
		CapsuleCollider[] capsuleColliders = GetComponentsInChildren<CapsuleCollider>();
		for( int i = 0; i < capsuleColliders.Length; i++ )
		{
			capsuleColliders[i].enabled = false;
		}
		GetComponent<Animator>().SetTrigger("Knockback");
		GetComponent<AudioSource>().PlayOneShot( fallToGround );
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( PlayerController._characterState == CharacterState.Dying )
		{
			//The Pendulum (bad name, yes I know) is the spike road block
			if( hit.collider.name.StartsWith("Demon") || hit.collider.name.StartsWith("Hero") || hit.collider.name.StartsWith("Pendulum") )
			{
				//If a demon collides with another demon, the road block or the Hero while the player is dead, have him stop moving and play the victory sequence.
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
				Debug.Log("Demon PlayerStateChange - player is dead and nearby");
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

	public void resetDemon()
	{
		setDemonState( DemonState.Idle );
		GetComponent<Animator>().CrossFadeInFixedTime( "Idle" , CROSS_FADE_DURATION );
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
		GetComponent<AudioSource>().PlayOneShot( footstepLeftSound, 0.4f );
		sparksLeftHoof.Play();
	}

	public void Footstep_right ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( footstepRightSound, 0.4f );
		sparksRightHoof.Play();
	}

	public void Start_Weapon_Trail ( AnimationEvent eve )
	{
		weaponTrail.SetActive( true );
	}

	public void Stop_Weapon_Trail ( AnimationEvent eve )
	{
		weaponTrail.SetActive( false );
	}


}
