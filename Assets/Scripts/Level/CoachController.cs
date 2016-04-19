using UnityEngine;
using System.Collections;

public class CoachController : MonoBehaviour {
	
	bool allowMove = false;  //The NPC will not start moving until the PlayerTrigger event has been received
	bool allowControllerUpdate = true;
	public AnimationClip walkAnim;
	public AnimationClip idleAnim;
	public AnimationClip hitAnim;
	public float walkSpeed = 1.18f;	//1.18 is a good value for the cow
	Animation anim;
	CharacterController controller;
	public Vector3 forward;
	public float moveDuration = 12f; //Only move for a short amount of time so the cows don't wander off into the wild ...
	public bool applyGravity = true;
	public GameObject leftHorse;
	public GameObject rightHorse;
	public GameObject coachDriver;
	PlayerController playerController;

	// Use this for initialization
	void Start () {
		anim = (Animation) GetComponent("Animation");
		controller = (CharacterController) GetComponent("CharacterController");
		playAnim( idleAnim );
		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		playerController = playerObject.GetComponent<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () {
		move ();
	}

	void move()
	{
		if( allowMove && allowControllerUpdate )
		{
			//1) Get the direction
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			forward = forward * Time.smoothDeltaTime * walkSpeed;
			if( applyGravity ) forward.y -= 16f * Time.deltaTime;
			//3) Move the controller
			controller.Move( forward );
		}
		
	}

	public void playHitAnim()
	{
		anim[hitAnim.name].wrapMode = WrapMode.Once;
		anim.CrossFade( hitAnim.name, 0.25f );
		anim.CrossFadeQueued(walkAnim.name, 0.4f );
		GetComponent<AudioSource>().Play();
	}

	void playAnim( AnimationClip clip )
	{
		anim[clip.name].wrapMode = WrapMode.Loop;
		anim[clip.name].speed = 1f;
		anim.Play(clip.name);
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
		PlayerController.playerStateChanged += PlayerStateChange;
		coachDriver.GetComponent<Animation>().Play("CoachDriverDriving");
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
		PlayerController.playerStateChanged -= PlayerStateChange;
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Start_Moving )
		{
			anim.CrossFade(walkAnim.name, 0.5f );
			GetComponent<AudioSource>().Play();
			allowMove = true;
			Invoke( "stopMoving", moveDuration );
		}
		else if( eventType == GameEvent.Stop_Moving )
		{
			stopMoving();
		} 
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			print ("Player collided with coach horses.");
			//The player collided with the front of the horses
			//Stop the coach
			//Horses go to idles
			//Coach driver plays a victory animation
			coachDriver.GetComponent<Animation>().Play("CoachDriverRunOverPlayer");
			coachDriver.GetComponent<Animation>().PlayQueued("CoachDriverIdle", QueueMode.CompleteOthers);
			stopMoving();
		}
	}

	void stopMoving()
	{
		allowMove = false;
		leftHorse.GetComponent<Animation>().Stop();
		rightHorse.GetComponent<Animation>().CrossFade("idle_01", 0.2f );
		anim.CrossFade(idleAnim.name, 0.5f );
		GetComponent<AudioSource>().Stop();
	}
	
	void GameStateChange( GameState newState )
	{
		if( anim != null )
		{
			if( newState == GameState.Paused )
			{
				anim.enabled = false;
				allowControllerUpdate = false;
				controller.enabled = false;
				
			}
			else if( newState == GameState.Normal )
			{
				anim.enabled = true;
				allowControllerUpdate = true;
				controller.enabled = true;
			}
		}
	}
}
