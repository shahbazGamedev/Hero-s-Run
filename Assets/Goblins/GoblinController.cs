using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoblinController : BaseClass {

	public enum GoblinState {
		Idle = 1,
		Running = 2,
		Attacking = 3,
		Dying = 4,
		Victory = 5
	}

	Transform player;
	PlayerController playerController;
	CharacterController controller;
	Vector3 forward;
	float walkSpeed = 1.65f; //good value so feet don't slide
	GoblinState goblinState = GoblinState.Idle;
	bool hasAttackedPlayer = false;
	public bool applyGravity = true;

	public AudioClip ouch;
	public AudioClip fallToGround;
	public AudioClip win;

	public GameObject clothItem1;
	public GameObject clothItem2;

	//For stats and achievements
	static EventCounter goblin_bowling = new EventCounter( GameCenterManager.ZombieBowling, 3, 3000 );

	//If true, the goblin heads for the player (as opposed to staying in its lane).
	public bool followsPlayer = false;

	//True if the CharacterController is allowed to move and false otherwise (because the game is paused for example).
	public bool allowMove = true;

	// Use this for initialization
	void Awake () {

		controller = (CharacterController) GetComponent("CharacterController");
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = (PlayerController) player.gameObject.GetComponent(typeof(PlayerController));
		randomizeLook ();
	}

	//We don't want all goblins to look the same
	void randomizeLook ()
	{
		if( Random.value < 0.3f ) clothItem1.SetActive( false );
		if( Random.value < 0.3f ) clothItem2.SetActive( false );
	}

	void Update ()
	{
		moveGoblin();
		if( !hasAttackedPlayer )
		{
			float distance = Vector3.Distance(player.position,transform.position);

			float attackDistance = 0.64f * PlayerController.getPlayerSpeed();
			if( distance < attackDistance )
			{
				triggerShortRangeAttack();
			}
		}
	}

	void moveGoblin()
	{
		if( allowMove && goblinState == GoblinState.Running )
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
			forward = forward * Time.deltaTime * walkSpeed;
			if( applyGravity ) forward.y -= 16f * Time.deltaTime;
			//3) Move the controller
			controller.Move( forward );
		}
	}

	public GoblinState getGoblinState()
	{
		return goblinState;
	}

	public void setGoblinState( GoblinState state )
	{
		goblinState = state;
	}

	//The goblin falls over backwards, typically because the player slid into him.
	public void fallToBack()
	{
		goblin_bowling.incrementCounter();
		setGoblinState( GoblinState.Dying );
		controller.enabled = false;
		CapsuleCollider capsuleCollider = (CapsuleCollider) GetComponent("CapsuleCollider");
		capsuleCollider.enabled = false;
		GetComponent<Animator>().Play("death");
		GetComponent<AudioSource>().PlayOneShot( fallToGround );
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
		GetComponent<Animator>().Play("fun2");
	}

	public void knockbackGoblin()
	{
		setGoblinState( GoblinState.Dying );
		controller.enabled = false;
		CapsuleCollider capsuleCollider = (CapsuleCollider) GetComponent("CapsuleCollider");
		capsuleCollider.enabled = false;
		GetComponent<Animator>().Play("death");
		GetComponent<AudioSource>().PlayOneShot( fallToGround );
		Debug.Log("KNOCKBACK " + gameObject.name );

	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( PlayerController._characterState == CharacterState.Dying )
		{
			if( hit.collider.name.StartsWith("Goblin") )
			{
				//If a goblin collides with another goblin while the player is dead, have him stop moving and play the victory sequence.
				victory( false );
			}
		}
	}

	void triggerShortRangeAttack()
	{
		hasAttackedPlayer = true;
		if( Random.value < 0.5f )
		{
			GetComponent<Animator>().Play("attack1");
		}
		else
		{
			GetComponent<Animator>().Play("attack2");
		}
	}

}
