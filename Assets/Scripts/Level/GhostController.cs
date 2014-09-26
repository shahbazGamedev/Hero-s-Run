using UnityEngine;
using System.Collections;

public class GhostController : MonoBehaviour {
	
	bool allowMove = false;  //The NPC will not start moving until the PlayerTrigger event has been received
	bool allowControllerUpdate = true;
	public AnimationClip walkAnim;
	public AnimationClip idleAnim;
	public AnimationClip hitAnim;	//If true, the zombie heads for the player (as opposed to staying in its lane).
	public bool followsPlayer = false;

	float walkSpeed = 25;
	Animation anim;
	CharacterController controller;
	public Vector3 forward;
	public float moveDuration = 12f; //Only move for a short amount of time so the cows don't wander off into the wild ...
	public bool applyGravity = true;
	Transform player;

	// Use this for initialization
	void Start () {
		anim = (Animation) GetComponent("Animation");
		//controller = (CharacterController) GetComponent("CharacterController");
		//playAnim( idleAnim );
		player = GameObject.FindGameObjectWithTag("Player").transform;
		followsPlayer = true;
		allowControllerUpdate = true;
	}
	
	// Update is called once per frame
	void Update () {
		move ();
	}

	void move()
	{
		if( allowMove && allowControllerUpdate )
		{
			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				transform.LookAt( player );
				transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
			}
			//1) Get the direction
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			forward = forward * Time.smoothDeltaTime * walkSpeed;
			if( applyGravity ) forward.y -= 16f * Time.deltaTime;
			//3) Move the controller
			//controller.Move( forward );
			transform.position = transform.position + forward;
		}
		
	}

	public void playHitAnim()
	{
		anim[hitAnim.name].wrapMode = WrapMode.Once;
		anim.CrossFade( hitAnim.name, 0.25f );
		anim.CrossFadeQueued(walkAnim.name, 0.4f );
		audio.Play();
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
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Start_Ghost )
		{
			//anim.CrossFade(walkAnim.name, 0.5f );
			audio.Play();
			allowMove = true;
			Invoke( "stopMoving", moveDuration );
		}
		else if( eventType == GameEvent.Stop_Ghost )
		{
			stopMoving();
		} 
	}

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			print ("Player is touching ghost");
			followsPlayer = false;
			allowControllerUpdate = false;

		}
	}

	void stopMoving()
	{
		allowMove = false;
		//anim.CrossFade(idleAnim.name, 0.7f );
		audio.Stop();
	}

	void GameStateChange( GameState newState )
	{
		if( anim != null )
		{
			if( newState == GameState.Paused )
			{
				anim.enabled = false;
				allowControllerUpdate = false;
				//controller.enabled = false;
				
			}
			else if( newState == GameState.Normal )
			{
				anim.enabled = true;
				allowControllerUpdate = true;
				//controller.enabled = true;
			}
		}
	}
}
