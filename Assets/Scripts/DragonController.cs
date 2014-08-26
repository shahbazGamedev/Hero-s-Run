﻿using UnityEngine;
using System.Collections;

public class DragonController : BaseClass {
	
	enum DragonState {
		None = 0,
		Fly = 1,
		Arrive = 2,
		Leave = 3,
		FireBreathing = 4
	}

	//Components
	Animation dragonAnimation;

	public ParticleSystem appearFx;
	public AudioClip appearSound;

	public ParticleSystem fairyDustFx;
	public AudioClip fairyDustSound;

	Transform player;
	PlayerController playerController;

	DragonState dragonState = DragonState.None;


	// The distance in the x-z plane to the target
	const float DEFAULT_DISTANCE = 0.7f;
	float distance = DEFAULT_DISTANCE;

	// the height we want the fairy to be above the player
	public const float DEFAULT_HEIGHT = 2.1f;
	float height = DEFAULT_HEIGHT;

	//Where to position the fairt relative to the player when appearing next to player
	Vector3 fairyRelativePos = new Vector3(-0.6f , DEFAULT_HEIGHT , DEFAULT_DISTANCE );

	// How much we 
	const float DEFAULT_HEIGHT_DAMPING = 6f;
	float heightDamping = DEFAULT_HEIGHT_DAMPING;

	const float DEFAULT_ROTATION_DAMPING = 3f;
	float rotationDamping = DEFAULT_ROTATION_DAMPING;

	const float DEFAULT_Y_ROTATION_OFFSET = 168f;
	float yRotationOffset = DEFAULT_Y_ROTATION_OFFSET;

	const float DEFAULT_X_ROTATION = 9f;
	float xRotation = DEFAULT_X_ROTATION;

	Vector3 xOffset = new Vector3( 0.6f, 0, 0 );

	bool followsPlayer = false;
	public Vector3 forward;
	float flyingSpeed = 5f;
	CharacterController controller;
	public GameObject dragonFire;
	public GameObject dragonFireSpotlight;

	void Awake()
	{
		//Get a copy of the components
		dragonAnimation = GetComponent<Animation>();
		controller = GetComponent<CharacterController>();
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = (PlayerController) player.gameObject.GetComponent(typeof(PlayerController));

	}

	void Start()
	{
		if( playerController.getCurrentTileType() == TileType.Opening ) Invoke ("Arrive", 10f );
	}

	// Update is called once per frame
	void LateUpdate ()
	{
		if( ( GameManager.Instance.getGameState() == GameState.Normal || GameManager.Instance.getGameState() == GameState.Checkpoint ) && dragonState == DragonState.Fly )
		{
			float distance = Vector3.Distance(player.position,transform.position);
			
			if( distance < 45f )
			{
				breatheFire( "Attack_001");
			}

			moveDragon();
		}
	}

	void moveDragon()
	{

		//0) Target the player but we only want the Y rotation
		if( followsPlayer )
		{
			transform.LookAt( player );
			transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
		}
		//1) Get the direction of the zombie
		forward = transform.TransformDirection(Vector3.forward);			
		//2) Scale vector based on flying speed
		forward = forward * Time.deltaTime * flyingSpeed;
		//3) Move the controller
		controller.Move( forward );

	}


	public void Arrive( )
	{
		print ("*******Dragon arrive");
		Vector3 arrivalStartPos = new Vector3( -42f, 9f, PlayerController.getPlayerSpeed() * 9f );
		Vector3 exactPos = player.TransformPoint(arrivalStartPos);
		transform.position = exactPos;
		transform.rotation = Quaternion.Euler( 0, player.transform.eulerAngles.y + 90f, transform.eulerAngles.z );
		playAnimation("Run", WrapMode.Loop );
		dragonState = DragonState.Fly;

		//StartCoroutine("MoveToPosition", timeToArrive );
	}
	

	private void breatheFire( string attackAnim )
	{
		dragonState = DragonState.FireBreathing;
		dragonAnimation[attackAnim].speed = 0.8f;
		dragonAnimation[attackAnim].wrapMode = WrapMode.Once;
		dragonAnimation.CrossFade(attackAnim);				
		Invoke( "breatheFireNow", dragonAnimation[attackAnim].length * 0.6f);
		Invoke( "resumeFlying", dragonAnimation[attackAnim].length);

	}
	
	private void breatheFireNow ()
	{
		dragonFireSpotlight.light.enabled = true;
		dragonFire.particleSystem.Play();
		audio.loop = false;
		audio.Play();
	}

	private void resumeFlying ()
	{
		dragonFireSpotlight.light.enabled = false;
		dragonAnimation.CrossFade("Run");				
		dragonState = DragonState.Fly;
	}

	private IEnumerator MoveToPosition( float timeToArrive )
	{
		//Step 1 - Take position in front of player
		float startTime = Time.time;
		float elapsedTime = 0;
		float startYrot = transform.eulerAngles.y;
		Vector3 startPosition = transform.position;
		
		while ( elapsedTime <= timeToArrive )
		{
			elapsedTime = Time.time - startTime;
			
			//Percentage of time completed 
			float fracJourney = elapsedTime / timeToArrive;
			
			float yRot = Mathf.LerpAngle( startYrot, player.eulerAngles.y + 180f, fracJourney );
			transform.eulerAngles = new Vector3 ( transform.eulerAngles.x, yRot, transform.eulerAngles.z );
			
			Vector3 exactPos = player.TransformPoint(fairyRelativePos);
			transform.position = Vector3.Lerp( startPosition, exactPos, fracJourney );
			
			//Tilt the fairy down
			transform.rotation = Quaternion.Euler( -8f, transform.eulerAngles.y, transform.eulerAngles.z );
			
			yield return _sync();  
			
		}
		dragonState = DragonState.Fly;
	}


	private void playAnimation( string animationName, WrapMode mode )
	{
		dragonAnimation[ animationName ].wrapMode = mode;
		dragonAnimation[ animationName ].speed = 1f;
		dragonAnimation.CrossFade(animationName, 0.1f);
	}
}
