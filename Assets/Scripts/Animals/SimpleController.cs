using UnityEngine;
using System.Collections;

public class SimpleController : MonoBehaviour {
	
	bool allowMove = false;  //The NPC will not start moving until the PlayerTrigger event has been received
	public AnimationClip walkAnim;
	public AnimationClip idleAnim;
	public AnimationClip hitAnim;
	public float walkSpeed = 1.18f;	//1.18 is a good value for the cow
	Animation anim;
	CharacterController controller; //The controller is optional. Without a controller, there is no collision handling.
	public Vector3 forward;
	public float moveDuration = 12f; //Only move for a short amount of time so the cows don't wander off into the wild ...
	public bool applyGravity = true;

	// Use this for initialization
	void Start () {
		anim = (Animation) GetComponent("Animation");
		controller = (CharacterController) GetComponent("CharacterController");
		playAnim( idleAnim );
	}
	
	// Update is called once per frame
	void Update () {
		move ();
	}

	void move()
	{
		if( allowMove )
		{
			//1) Get the direction
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			forward = forward * Time.smoothDeltaTime * walkSpeed;
			if( applyGravity ) forward.y -= 16f * Time.deltaTime;
			//3) Move the controller
			if( controller != null )
			{
				controller.Move( forward );
			}
			else
			{
				transform.position = transform.position + forward;
			}
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
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}
	
	void OnDisable()
	{
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
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

	void stopMoving()
	{
		allowMove = false;
		anim.CrossFade(idleAnim.name, 0.7f );
	}

}
