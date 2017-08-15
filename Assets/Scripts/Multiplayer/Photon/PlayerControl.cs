using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public enum DeathType {
		Alive = 0,
		Obstacle = 1,
		Cliff = 2,
		Flame = 3,
		Trap = 4,
		Enemy = 5,
		Water = 6,
		Turned_Too_Late = 7,
		Lava = 8,
		Fireball = 9,
		Zombie = 10,
		VortexTrap = 11,
		SpecialFall = 12,
		GreatFall = 13,
		FallForward = 14,
		Turned_Wrong_Way = 15,
		Exited_Without_Turning = 16,
		NO_MORE_HEALTH = 17

}

public enum PlayerCharacterState {
	None = -1,
	Idle = 0,
	SideMove = 1,
	Sliding = 2,
	Running = 3,
	Jumping = 4,
	Dying = 5,
	Stumbling = 6,
	Winning = 7,
	Flying = 8,
	StartRunning = 9,
	Falling = 10,
	Turning = 11,
	Turning_and_sliding = 12,
	Ziplining = 13
}

public class PlayerControl : Photon.PunBehaviour {

	#region Components cached for performance 	
	Animator anim;
	PlayerCamera playerCamera;
	PlayerVisuals playerVisuals;
	PlayerSounds playerSounds;
	PlayerVoiceOvers playerVoiceOvers;
	PlayerCollisions playerCollisions;
	PlayerAI playerAI;
	CapsuleCollider capsuleCollider;
	PlayerRun playerRun;
	PlayerSpell playerSpell;
	PlayerRace playerRace;
	PlayerHealth playerHealth;
	#endregion

	#region Hash IDs for player animations	
	//Hash IDs for player animations. These are used to improve performance.
	int RunTrigger = Animator.StringToHash("Run");
	int FallTrigger = Animator.StringToHash("Fall");
	int LandTrigger = Animator.StringToHash("Land");
	int Double_JumpTrigger = Animator.StringToHash("Double Jump");
	int JumpTrigger = Animator.StringToHash("Jump");
	int Slide_DownTrigger = Animator.StringToHash("Slide Down");
	int Slide_UpTrigger = Animator.StringToHash("Slide Up");
	int DeathWallTrigger = Animator.StringToHash("DeathWall");
	int StumbleTrigger = Animator.StringToHash("Stumble");
	int FallForwardTrigger = Animator.StringToHash("Fall_Forward");
	int Idle_LookTrigger = Animator.StringToHash("Idle_Look");
	int Finish_LineTrigger = Animator.StringToHash("Finish_Line");
	#endregion

	#region Falling variables
	const float MIN_DISTANCE_FOR_FALL = 3f;
	//When the player falls, we start calculating his fall distance. If the fall distance is greater than FALL_TO_DEATH_DISTANCE, then the player will die.
	float fallStartYPos = 0;
	const float FALL_TO_DEATH_DISTANCE = 60f;
	//If the fall distance is less than DISTANCE_FOR_LAND_ANIMATION meters, the hero plays the Land animation; above, he plays the Stumble animation.
	const float DISTANCE_FOR_LAND_ANIMATION = 34f;
	#endregion

	#region Jumping and gravity variables
	bool jumping = false;
	float jumpSpeed = 10f;
	public float distanceToGround = 0;
	//The gravity for the character
	const float DEFAULT_GRAVITY = 16f;
	float gravity = DEFAULT_GRAVITY;
	float gravityModifier = 1.8f;
	//when in the air, you can queue a jump request so that you will jump again
	//as soon as you touch the ground
	//You can only queue one move at any given time
	bool queueJump = false;
	//jumpStarted is used for one frame to prevent isGrounded from preventing the player to jump.
	//A jump event can come at any time.
	bool jumpStarted = false;
	#endregion

	#region Sliding variables
	//Time slide started
	float slideStartTime;
	float slideDuration = 0.85f; //in seconds
	//when in the air, you can queue a slide request so that you will slide
	//as soon as you touch the ground
	bool queueSlide = false;
	#endregion

	#region General movement variables
	//Use to move the player along the X, Y and Z axis
	public Vector3 moveDirection;
	Vector3 forward;
	//We set the anim speed to 0 when we pause, so we need to remember the value when we unpause
	float animSpeedAtTimeOfPause;
	//The state of the character i.e. running, jumping, sliding, etc.
	public PlayerCharacterState playerCharacterState;
	//True if the player is allowed to move, false otherwise. This flag is useful during camera cut-scenes to prevent the player from moving.
	bool playerMovementEnabled = true;
 	//Are inputs allowed?
	bool playerControlsEnabled = true;
	#endregion

	#region Powerup variables
	//When the player is using a slow time power up, this slows down time by a factor of 2.
	//However, in order for the game to not feel sluggish, we use a smaller value called SLOW_DOWN_FACTOR
	//for the accelerometer and when changing lanes.
	const float SLOW_DOWN_FACTOR = 1.5f;
	#endregion

	#region Zipline variables
	public bool isInZiplineTrigger;
	Transform ziplineAttachPoint;
	#endregion

	#region Lane variables
	public enum Lanes {
		Left = -1,
		Center = 0,
		Right = 1,
	}
	static float laneLimit = 2f;
	//Due to rounding errors, the player may not reach exactly the lane limit. If there is less than 1% of the distance
	//remaining, assume that he did reach the lane limit which will allow us to finalize the side move.
	float adjustedLaneLimit = laneLimit * 0.99f;
	public Lanes currentLane = Lanes.Center;
	Lanes desiredLane = Lanes.Center;
	int myLane = 0; //0 is uninitialized, 1 is the nearest, 2 is in the center and 3 is the furthest
	public float sideMoveSpeed = 5.8f; //At what speed do you change lanes.
	bool useLanes = true; //if true, forces player to remain in the 3 lanes
	Vector3 playerPositionWhenSideMoveStarted;
	#endregion

	#region Turning corners variables
	//Used to indicate if the player is in a zone where a turn is required
	bool isInDeadEnd = false;
	//What type of turn is supported in that deadEnd: Left only, Right only or Left and Right
	DeadEndType currentDeadEndType;
	//Used to indicate whether or not the player turned correctly in a dead end.
	bool deadEndTurnDone = false;
	//The dead end trigger that the player is in
	Collider deadEndTrigger;
	//Used to indicate that the player wants to turn. The actual turn will be delayed until the player is aligned with a lane.
	bool wantToTurn = false;
	//Used to indicate whether the player is making a side-move or turn to the right or to the left
	bool isGoingRight = false;
	#endregion

	#region Current tile variables
	Vector3 currentTilePos = Vector3.zero;
	GameObject currentTile;
	public float tileRotationY = 0; //Since we use this value often, we will store it.
 	#endregion

	#region Death variables
	public DeathType deathType = DeathType.Alive;
	//When the player dies, we change the center and radius.
	//We need to be able to reset these values when the player is revived.
	Vector3 controllerOriginalCenter;
	float PRONE_CAPSULE_CENTER_Y = 0.57f;
	string tileWherePlayerDied = string.Empty;
	int numberOfTimesDiedDuringRace = 0; //Used by PlayerStatistics to determine if the player had a perfect race, that is, he did not die a single time.
	#endregion

	#region Other variables
	GenerateLevel generateLevel;
	#endregion

	#region Events
	//Event management used to notify other classes when the character state has changed
	public delegate void MultiplayerStateChanged( PlayerCharacterState value );
	public static event MultiplayerStateChanged multiplayerStateChanged;
	#endregion

	#region Cached localized strings
	//Cache the string to avoid the runtime lookup
	string backInTheGameString;
	#endregion

	void Awake ()
	{
		generateLevel = GameObject.FindObjectOfType<GenerateLevel>();
	}

	// Use this for initialization
	void Start ()
	{
		//Cache components for performance
		anim = GetComponent<Animator>();
		capsuleCollider = GetComponent<CapsuleCollider>();
		controllerOriginalCenter = capsuleCollider.center;
		playerCamera = GetComponent<PlayerCamera>();
		playerVisuals = GetComponent<PlayerVisuals>();
		playerSounds = GetComponent<PlayerSounds>();
		playerVoiceOvers = GetComponent<PlayerVoiceOvers>();
		playerCollisions = GetComponent<PlayerCollisions>();
		playerAI = GetComponent<PlayerAI>(); //Null for everyone except bots
		playerRun = GetComponent<PlayerRun>();
		playerSpell = GetComponent<PlayerSpell>();
		playerRace = GetComponent<PlayerRace>();
		playerHealth = GetComponent<PlayerHealth>();

		//Cache the string to avoid the runtime lookup
		backInTheGameString = LocalizationManager.Instance.getText( "MINIMAP_BACK_IN_GAME" );

		//The character is in idle while waiting to run. 
		setCharacterState( PlayerCharacterState.Idle );

		//Calculate the ground height
		RaycastHit hit;

		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hit, 10f ))
		{
			transform.position = new Vector3( transform.position.x, hit.point.y, transform.position.z);
			//Debug.Log( "There is ground underneath the player on Start. The collider is: " + hit.collider.name + " at height: " + hit.point.y );
			//Also adjust the camera height
			if( this.photonView.isMine && playerAI == null )
			{
				Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y + hit.point.y, Camera.main.transform.position.z);
			}
			playerCamera.positionCameraNow();
		}
		else
		{
			Debug.LogError( "There is no ground below the player during PlayerControl Start. Check LevelNetworkingManager to see if the spawn position height values are correct." );
		}
		playerCamera.playCutscene(CutsceneType.Checkpoint);

		getFirstTileInfo();

		//We may not have been spawned in the center lane. Make sure the lane values are accurate.
		recalculateCurrentLane();

		//We want the cutscene camera which is a child of the player to use the same skybox as the main camera.
		//The bot does not have a cutscene camera.
		if( playerAI == null )
		{
			Transform cutSceneCamera = transform.FindChild("CutsceneCamera");
			Skybox skyBox = cutSceneCamera.GetComponent<Skybox>();
			skyBox.material = LevelManager.Instance.getLevelData().skyBoxMaterial;
		}

		//Tell the MasterClient that we are ready to go. Our level has been loaded and our player created.
		//The MasterClient will initiate the countdown
		this.photonView.RPC("readyToGo", PhotonTargets.MasterClient );	
	}

	//The player control needs info about the tile the player is on.
	void getFirstTileInfo()
	{	
		GameObject firstTile = generateLevel.getFirstTile();
		currentTile = firstTile;
		tileRotationY = firstTile.transform.eulerAngles.y;
		currentTilePos = firstTile.transform.position;
		playerRace.tilesLeftBeforeReachingEnd = generateLevel.getNumberOfTiles();
	}

	void OnEnable()
	{
		HUDMultiplayer.startRunningEvent += StartRunningEvent;
		GameManager.gameStateEvent += GameStateChange;
	}

	void OnDisable()
	{
		HUDMultiplayer.startRunningEvent -= StartRunningEvent;
		GameManager.gameStateEvent -= GameStateChange;
	}

	void StartRunningEvent()
	{
		startRunning();
	}

	public void startRunning()
	{	
		//The player starts off running
		setAnimationTrigger(RunTrigger);
		setCharacterState( PlayerCharacterState.StartRunning );
		setCharacterState( PlayerCharacterState.Running );
	
		//When the GameState is NORMAL, we display the HUD
		if( this.photonView.isMine && GetComponent<PlayerAI>() == null ) GameManager.Instance.setGameState( GameState.Normal );

		enablePlayerControl( true );
	}

	#region Pause/Resume
	void GameStateChange( GameState previousState, GameState newState )
	{
		//Ignore game state changes if we are not the owner
		if ( !this.photonView.isMine ) return;
		if( newState == GameState.Normal )
		{
			if( previousState == GameState.Paused )
			{
				//If in an online mode, unpause the player via RPC
				if( GameManager.Instance.isOnlinePlayMode() )
				{
					this.photonView.RPC( "unpauseRemotePlayers", PhotonTargets.AllViaServer );
				}
				//If in an offline mode, resume time
				else
				{
					Time.timeScale = 1f;
					AudioListener.pause = false;
				}
			}
		}
		else if( newState == GameState.Paused )
		{
			//If in an online mode, only pause this player but the rest of the world continues as normal
			if( GameManager.Instance.isOnlinePlayMode() )
			{
				//The reason that we pass by the Master Client and don't send the RPC directly to all the clients is
				//that this will allow the Master Client to send his position and rotation.
				//This will ensure that all clients have the same position and rotation when paused.			
				this.photonView.RPC( "pauseRemotePlayersMaster", PhotonTargets.MasterClient );
			}
			//If in an offline mode, stop time
			else
			{
				Time.timeScale = 0;
				AudioListener.pause = true;
			}
		}
	}

	[PunRPC]
	public void pauseRemotePlayersMaster()
	{
		this.photonView.RPC( "pauseRemotePlayers", PhotonTargets.All, transform.position, transform.eulerAngles.y, PhotonNetwork.time );
	}

	[PunRPC]
	public void pauseRemotePlayers( Vector3 positionAtTimeOfPause, float yRotationAtTimeOfpause, double timeRPCSent )
	{
		//Debug.Log("pauseRemotePlayers RPC received for: " +  gameObject.name + " isMasterClient: " + PhotonNetwork.isMasterClient + " isMine: " + this.photonView.isMine + " isLocal: " + PhotonNetwork.player.IsLocal + " view ID: " + this.photonView.viewID + " owner ID: " + this.photonView.ownerId );		
		//Debug.Log("pauseRemotePlayers-positionAtTimeOfPause: " + positionAtTimeOfPause + " yRotationAtTimeOfpause: " + yRotationAtTimeOfpause );		
		//Debug.Log("pauseRemotePlayers-current position: " +  transform.position + " current rotation: " + transform.eulerAngles.y );
		//float realDistanceDelta = Vector3.Distance( transform.position, positionAtTimeOfPause);
		//double predictedDistanceDelta = (PhotonNetwork.time - timeRPCSent) * playerRun.getRunSpeed();
		//Debug.Log("pauseRemotePlayers-real distance delta: " +  realDistanceDelta + " predictedDistanceDelta " + predictedDistanceDelta );
		//Debug.Log("pauseRemotePlayers-distancePrediction accuracy: " + ((predictedDistanceDelta - realDistanceDelta) * 100).ToString("N1") + "%" );
		transform.position = positionAtTimeOfPause;
		transform.eulerAngles = new Vector3( transform.eulerAngles.x, yRotationAtTimeOfpause, transform.eulerAngles.z );
		recalculateCurrentLane();
		pausePlayer( true );
	}

	[PunRPC]
	public void unpauseRemotePlayers()
	{
		//We don't want one of the clients to unpause faster than the others. If a client got this RPC call 100ms faster than
		//another client, with a player running at 20 meters/second, this will lead to an immediate desynchronization of 2 meters.
		//This is why we use lockstep to unpause.
		LockstepManager.Instance.addActionToQueue( new LockstepManager.LockstepAction( LockstepActionType.UNPAUSE, gameObject ) );
	}

	public void pausePlayer( bool isPaused )
	{
		if( isPaused )
		{
			animSpeedAtTimeOfPause = anim.speed;
			anim.speed = 0;
			enablePlayerControl( false );
			enablePlayerMovement( false );
		}
		else
		{
			enablePlayerControl( true );
			enablePlayerMovement( true );
			anim.speed = animSpeedAtTimeOfPause;
		}
	}
	#endregion

	void FixedUpdate()
	{
		if( getCharacterState() != PlayerCharacterState.Flying )
		{
			calculateFallDistance();
			if( playerMovementEnabled )
			{
				calculateDistanceToGround();
				moveCharacter();
	
				//Verify if the player is falling.
				//Also ignore if we are already falling or dying.
				if( playerCharacterState != PlayerCharacterState.Falling && playerCharacterState != PlayerCharacterState.Dying && playerCharacterState != PlayerCharacterState.Ziplining )
				{
					//Verify how far is the ground
					if( distanceToGround > MIN_DISTANCE_FOR_FALL )
					{
						bool isLeftFootOnGround = true;
						Vector3 leftFootPosition = transform.TransformPoint(new Vector3( -0.12f ,0 ,0.1f ));
						bool isRightFootOnGround = true;
						Vector3 rightFootPosition = transform.TransformPoint(new Vector3( 0.12f ,0 ,-0.1f ));
	
						//Test left foot
						//There might be a small crack between the tiles. We don't want the player to fall if this is the case.
						//So also check 10cm in front of the player (with left foot test) and 10cm in back of the player (with right foot test) before deciding to fall.	
						if ( !Physics.Raycast(leftFootPosition, Vector3.down, MIN_DISTANCE_FOR_FALL ))
						{
							//Ground is further than MIN_DISTANCE_FOR_FALL meters.
							//Left foot is not on the ground
							isLeftFootOnGround = false;
						}
						//Test right foot
						if ( !Physics.Raycast(rightFootPosition, Vector3.down, MIN_DISTANCE_FOR_FALL ))
						{
							//Ground is further than MIN_DISTANCE_FOR_FALL meters.
							//Right foot is not on the ground
							isRightFootOnGround = false;
						}
						if( !isLeftFootOnGround && !isRightFootOnGround )
						{
							fall( false );
						}
					}
				}
	
				if( useLanes )
				{
					verifyIfDesiredLaneReached();
				}
				else
				{
					verifyIfDesiredLaneReached2();
				}
			}
		}
	}

	void moveCharacter()
	{

		verifySlide();

		if (distanceToGround < 0.1f && !jumpStarted)
		{
			//We are on the ground. Do not apply a downward force.
			moveDirection.y = 0f;

			//If we we were falling and just landed,reset values and go back to running state.
			if( playerCharacterState == PlayerCharacterState.Falling )
			{
				land();
			}

			//if we were jumping and just landed, reset values and go back to the running state
			if (jumping) 
			{
				if( playerCollisions.getGroundType() != "Water" )
				{
					playerVisuals.playDustPuff( false );
				}
				setAnimationTrigger(LandTrigger);
				jumping = false;
				if ( queueSlide )
				{
					queueSlide = false;
					gravity = DEFAULT_GRAVITY;
					startSlide();
				}
				else if ( queueJump )
				{
					setCharacterState( PlayerCharacterState.Running );
					queueJump = false;
					jump( false );
				}
				else
				{
					//Resume running, but only if we are not dying
					if( playerCharacterState != PlayerCharacterState.Dying )
					{
						setCharacterState( PlayerCharacterState.Running );
					}
				}
			}
		}
		else
		{
			//We are not on the ground, apply gravity
			moveDirection.y -= gravity * Time.deltaTime;
		}

		// Move the controller
		if( playerCharacterState != PlayerCharacterState.Ziplining )
		{
			//1) Get the direction of the player
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			forward = forward * Time.deltaTime * playerRun.getRunSpeed();
			//3) Add Y component for gravity. Both the x and y components are stored in moveDirection.
			forward.Set( forward.x, moveDirection.y * Time.deltaTime, forward.z );
			//4) Get a unit vector that is orthogonal to the direction of the player
			Vector3 relativePos = new Vector3(1 , 0 , 0 );
			Vector3 xPos = transform.TransformPoint(relativePos);
			Vector3 xVector = xPos - transform.position;
			//5) Scale the X component
			xVector = xVector * Time.deltaTime * moveDirection.x;
			//6) Clamp to the max distance we can travel perpendicularly without
			//exiting the left or right lanes.
			if( useLanes) xVector =  Vector3.ClampMagnitude(xVector, Mathf.Abs(getMaxDist(moveDirection.x)));
			//7) Add the X component to the forward direction
			forward = forward + xVector;
			//8) Move the controller
			capsuleCollider.attachedRigidbody.velocity = forward * 25f;
			jumpStarted = false;
		}
		//Note: if the player is ziplining. He is moved by a LeanTween function.
	}

	//Returns the maximum distance the player can travel perpendicularly without
	//exiting the left or right lanes.
	private float getMaxDist( float totalX )
	{
		float maxDist = 0;
		float playerRotationY = Mathf.Floor ( transform.eulerAngles.y );
		//Player is facing straight.
		if( playerRotationY == 0 )
		{
			if( totalX >=0 )
			{
				maxDist = currentTilePos.x + laneLimit - transform.position.x;
			}
			else
			{
				maxDist = currentTilePos.x - laneLimit - transform.position.x;
			}
			
		}
		//Player is facing right.
		else if( playerRotationY == 90f || playerRotationY == -270f )
		{
			if( totalX >=0 )
			{
				maxDist = currentTilePos.z - laneLimit - transform.position.z;
			}
			else
			{
				maxDist = currentTilePos.z + laneLimit - transform.position.z;
			}
		}
		//Player is facing left.
		else if( playerRotationY == -90f || playerRotationY == 270f )
		{
			if( totalX >=0 )
			{
				maxDist = currentTilePos.z + laneLimit - transform.position.z;
			}
			else
			{
				maxDist = currentTilePos.z - laneLimit - transform.position.z;
			}
		}
		return maxDist;
	}

	/// <summary>
	/// Calculates the distance to ground. This value is used to determine if you can jump.
	/// </summary>
	void calculateDistanceToGround()
	{
        RaycastHit hit;
		//We add 0.1f just to be sure our raycast starts a tad higher than the player's feet
		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hit, 10f))
		{
            distanceToGround = hit.distance;
			//print ("PlayerControl-calculateDistanceToGround: " + hit.collider.name );
		}
		else
		{
			//Ground is further than 10 meters or possibly there is no collider below the player.
			//Just set an Arbitrarily big value.
			distanceToGround = 1000f;
			//print ("PlayerControl-calculateDistanceToGround: " + distanceToGround );
		}
        
	}

	#region Jump and Double Jump
	/// <summary>
	/// Makes the player jump. If doingDoubleJump is set to true, the player will do a higher double-jump.
	/// During a normal jump, the run speed is reduced by runSpeedJumpMultiplier.
	/// During a double-jump, the run speed is capped to MAX_RUN_SPEED_FOR_DOUBLE_JUMP to avoid the player hitting a wall instead of clearing the obstacle.
	/// </summary>
	/// <param name="doingDoubleJump">If set to <c>true</c> player will do a double-jump.</param>
	/// <param name="doubleJumpSpeed">Double jump speed.</param>
	public void jump( bool doingDoubleJump, float doubleJumpSpeed = 0 )
	{
		if( jumping )
		{
			//Delay the second jump request until we are on the ground
			//Cancel any slide queue since we can only queue one movement at a time
			queueJump = true;
			queueSlide = false;
		}

		//Only allow a jump if we are not already jumping and if we are on the ground.
		//However, if the ground type below the player is of type Collapsing, still allow him to jump.
		//The Collapsing tag is used in the CollapsingBridge code.
		if (playerCharacterState != PlayerCharacterState.Jumping && ( distanceToGround < 0.5f || playerCollisions.getGroundType() == "Collapsing" ) )
		{
			//Hack - put moveDirection.x to zero in case finalizeSideMove was never called because of a collision
			moveDirection.x = 0;
			
			//Make sure the lane data is correct in case a collision forced us out of our lane
			recalculateCurrentLane();

			//We are allowed to jump from the slide state.
			playerSounds.stopAudioSource();							//stop the sliding sound if any
			playerVisuals.playDustPuff( false );					//stop the dust puff that loops while we are sliding
			playerVisuals.playWaterSplashWhileSliding( false );

			jumping = true;
			jumpStarted = true;

			setCharacterState( PlayerCharacterState.Jumping );
			if( doingDoubleJump )
			{
				moveDirection.y = doubleJumpSpeed;
				setAnimationTrigger(Double_JumpTrigger);
			}
			else
			{
				moveDirection.y = jumpSpeed;
				setAnimationTrigger(JumpTrigger);
			}
			//for debugging
			//remove jump sound for now because it is annoying
			//playSound( jumpingSound, false );
		}
	}
	#endregion

	#region Fall and Land
	public void fall( bool useFallLoop )
	{
		if( playerCharacterState == PlayerCharacterState.Falling || playerCharacterState == PlayerCharacterState.Jumping ) return; //ignore, we are already falling or jumping

		//Give a little downward impetus
		moveDirection.y = -1f;
		//Remember at what height the player started to fall because this will help us calculate the fall distance.
		fallStartYPos = transform.position.y;
		gravity = DEFAULT_GRAVITY * 2f;
		playerCamera.heightDamping = PlayerCamera.DEFAULT_HEIGHT_DAMPING * 9f;
		setCharacterState(PlayerCharacterState.Falling);
		if( useFallLoop )
		{
			//Going straight to fall loop is better when respawning and falling from the sky
			anim.Play( "Fall_Loop");
		}
		else
		{
			//This fall animation blends with the run animation
			setAnimationTrigger(FallTrigger);
		}
		//playSound( fallingSound, false );
		print ( "fall started " + distanceToGround + " " + MIN_DISTANCE_FOR_FALL + " " + playerCharacterState );
	}

	public void land()
	{
		//Reset values that we changed in the fall() method
		playerVisuals.playDustPuff( true, false );
		playerCamera.lockCamera( false );
		gravity = DEFAULT_GRAVITY;
		playerCamera.heightDamping = PlayerCamera.DEFAULT_HEIGHT_DAMPING;
		setCharacterState( PlayerCharacterState.Running );
		float fallDistance = fallStartYPos - transform.position.y;
		if( fallDistance < DISTANCE_FOR_LAND_ANIMATION )
		{
			anim.CrossFadeInFixedTime( "Land", 0.25f );
		}
		else
		{
			anim.CrossFadeInFixedTime( "Stumble", 0.25f );
		}
		print ( "player landed. Fall distance was: " + 	fallDistance );
		//Was I dead before?
		if( deathType != DeathType.Alive )
		{
			resurrectEnd();
		}
	}

	void calculateFallDistance()
	{
		if( playerCharacterState == PlayerCharacterState.Falling )
		{
			//Calculate the fall distance
			float fallDistance = fallStartYPos - transform.position.y;
			if( fallDistance > FALL_TO_DEATH_DISTANCE )
			{
				killPlayer(DeathType.Cliff);
			}
		}
	}
	#endregion

	#region Sliding
	public void startSlide()
	{
		if( playerCharacterState != PlayerCharacterState.SideMove && playerCharacterState != PlayerCharacterState.Falling )
		{
			if( jumping )
			{
				//Delay the slide until we are on the ground
				queueSlide = true;
				//Cancel any queued jump
				queueJump = false;
				//Since the player wants to slide, let's make the character fall back
				//down faster than normal by increasing gravity
				gravity = DEFAULT_GRAVITY  * gravityModifier;
			
			}
			else
			{
				//Slides cant be prolonged indefinitely. This is why you can reset the start time.
				slideStartTime = Time.time;
				//Don't reset values for nothing if we are extending the slide
				if( playerCharacterState != PlayerCharacterState.Sliding )
				{
					changeColliderAxis( Axis.Z );
					setCharacterState( PlayerCharacterState.Sliding );
					if( playerCollisions.getGroundType() == "Water" )
					{
						playerVisuals.playWaterSplashWhileSliding( true );
					}
					else
					{
						playerVisuals.playDustPuff( true );
					}

					setAnimationTrigger(Slide_DownTrigger);
				}
			}
		}
	}

	void verifySlide()
	{
		if ( playerCharacterState == PlayerCharacterState.Sliding || playerCharacterState == PlayerCharacterState.Turning_and_sliding )
		{
			//For as long as we have a collider above the character's head, prolong the slide duration
			if ( checkAbove() )
			{
				slideStartTime = slideStartTime + Time.deltaTime;
				
			}
			if ( (Time.time - slideStartTime) > slideDuration )
			{
				//We are stopping sliding
				playerVisuals.playDustPuff( false );
				playerVisuals.playWaterSplashWhileSliding( false );

				if( playerCharacterState == PlayerCharacterState.Turning_and_sliding )
				{
					setCharacterState( PlayerCharacterState.Sliding );
				}
				else
				{
					setCharacterState( PlayerCharacterState.Running );
				}
				setAnimationTrigger(Slide_UpTrigger);
				//Reset capsule collider to upright position
				changeColliderAxis( Axis.Y );
			}
		}
	}	

	//Returns true if there is a collider less than 1.5 meters above the player
	bool checkAbove()
	{
 		//Temporarily use Ignore Raycast layer so we don't detect the player
		gameObject.layer = 2;
        if (Physics.Raycast(transform.position, Vector3.up, 1.5f ))
		{
 			gameObject.layer = 8;
            return true;
		}
		else
		{
			gameObject.layer = 8;
 			return false;
		}
       
	}
	#endregion

	#region Turnining and changing lanes

	void turnCorner( bool isGoingRight )
	{
		if ( playerCharacterState == PlayerCharacterState.Running || playerCharacterState == PlayerCharacterState.Jumping || playerCharacterState == PlayerCharacterState.Sliding || playerCharacterState == PlayerCharacterState.SideMove || playerCharacterState == PlayerCharacterState.Stumbling )
		{
			this.isGoingRight = isGoingRight;

			//Hack - put moveDirection.x to zero in case finalizeSideMove was never called because of a collision
			moveDirection.x = 0;

			//Make sure the lane data is correct in case a collision forced us out of our lane
			recalculateCurrentLane();

			//You can only turn once
			if (!deadEndTurnDone)
			{
				float sideMoveInitiatedZ = getTurnInitiatedPosition();

				//We have 3 cases:
				//Case 1: Player turned early or normally
				//Condition: player turned before -1.3 meters (0 is center of tile).
				//What we want to do: wait until player reaches the next available lane (left, center of right) and then turn

				//Case 2: Player turned late
				//Condition: player turned after +1.3 meters (0 is center of tile), but before +1.9 meters. Math is 1.3 lane width + 0.5 is player radius + 0.1 meters is margin
				//What we want to do: turn immediately

				//Case 3: Player turned too late
				//Condition: player turned after +1.9 meters (0 is center of tile).
				//What we want to do: kill the player. He missed the turn.

				//Case 3: Player turned too late, we want to kill him
				if (sideMoveInitiatedZ > 2f )
				{	
					Debug.LogWarning("turnCorner: game over - player turned too late." );
					killPlayer ( DeathType.Turned_Too_Late );
					return;
				}

				//Case 2: Player turned late, we want to turn now
				if ( sideMoveInitiatedZ > laneLimit && sideMoveInitiatedZ < 2f )
				{	
					if ( isGoingRight )
					{
						//Verify if the player is doing a side-move in an allowed direction
						if (currentDeadEndType == DeadEndType.Right || currentDeadEndType == DeadEndType.LeftRight || currentDeadEndType == DeadEndType.RightStraight)
						{
							//Turn is valid
							desiredLane = Lanes.Left;
							turnNow();
						}
						else
						{
							//Player turned the wrong way
							Debug.LogWarning("turnCorner: game over - player turned wrong way too late." );
							killPlayer ( DeathType.Turned_Too_Late );
						}				
					}
					else
					{
						if (currentDeadEndType == DeadEndType.Left || currentDeadEndType == DeadEndType.LeftRight  || currentDeadEndType == DeadEndType.LeftStraight)
						{
							//Turn is valid
							desiredLane = Lanes.Right;
							turnNow();
						}
						else
						{
							//Player turned the wrong way
							Debug.LogWarning("turnCorner: game over - player turned wrong way too late." );
							killPlayer ( DeathType.Turned_Too_Late );
						}
					}
					return;
				}

				//Case 1: Player turned early or normally
				if (sideMoveInitiatedZ < laneLimit )
				{	
					if ( isGoingRight )
					{
						//Verify if the player is doing a side-move in an allowed direction
						if (currentDeadEndType == DeadEndType.Right || currentDeadEndType == DeadEndType.LeftRight || currentDeadEndType == DeadEndType.RightStraight)
						{
							//Turn is valid
							setDesiredLane( sideMoveInitiatedZ );
						}
						else
						{
							//Player turned the wrong way
							Debug.LogWarning("turnCorner: game over - player turned wrong way." );
							killPlayer ( DeathType.Turned_Wrong_Way );
						}				
					}
					else
					{
						if (currentDeadEndType == DeadEndType.Left || currentDeadEndType == DeadEndType.LeftRight || currentDeadEndType == DeadEndType.LeftStraight )
						{
							//Turn is valid
							setDesiredLane( sideMoveInitiatedZ );
						}
						else
						{
							//Player turned the wrong way
							Debug.LogWarning("turnCorner: game over - player turned wrong way." );
							killPlayer ( DeathType.Turned_Wrong_Way );
						}
					}
				}
			}
			else
			{
				Debug.LogWarning("turnCorner: turn denied since player has already turned." );
			}
		}
	}

	void turnNow()
	{
		wantToTurn = false;
		deadEndTurnDone = true;
		currentLane = desiredLane;
		if( currentDeadEndType == DeadEndType.LeftRight )
		{
			Debug.Log("PlayerController-turnNow: player turned " + isGoingRight + " at T-Junction." + currentTile.name );
			//Now that we know which way the player is turning, tell the level generator right away
			//so he can activate the proper tiles and, if needed, move the tiles on the Right of the T-Junction to 
			//the Left assuming the player decided to turn that way.
			//We want to do this early to avoid the tiles popping into view.
			generateLevel.playerTurnedAtTJunction( isGoingRight, currentTile );
		}

		playerSounds.playSideMoveSound();

		float playerRotY = transform.eulerAngles.y;
		
		if ( isGoingRight )
		{
			transform.rotation = Quaternion.Euler( 0,playerRotY + 90f,0 );
			tileRotationY = tileRotationY + 90f;
		}
		else
		{
			transform.rotation = Quaternion.Euler( 0,playerRotY - 90f,0 );
			tileRotationY = tileRotationY - 90f;
		}

		if( playerCharacterState == PlayerCharacterState.Turning_and_sliding )
		{
			setCharacterState( PlayerCharacterState.Sliding );
		}
		else
		{
			setCharacterState( PlayerCharacterState.Running );
		}
		//Debug.Log ("turnNow completed " + isGoingRight + " " + transform.eulerAngles.y + " " + playerCharacterState );

	}

	//move the player to the left lane if false,
	//and to the right lane if true
	void changeLane( bool isGoingRight )
	{
		//You can only change lanes while running
		//You can also change your mind 
		if ( playerCharacterState == PlayerCharacterState.Running || ( playerCharacterState == PlayerCharacterState.SideMove && this.isGoingRight != isGoingRight ) )
		{
			this.isGoingRight = isGoingRight;

			//Hack - put moveDirection.x to zero in case finalizeSideMove was never called because of a collision
			moveDirection.x = 0;

			//Make sure the lane data is correct in case a collision forced us out of our lane
			recalculateCurrentLane();

			float currentSideMoveSpeed;
			if( Time.timeScale < 1f )
			{
				//Player is using a slow time power up.
				currentSideMoveSpeed = sideMoveSpeed * SLOW_DOWN_FACTOR;
			}
			else
			{
				//Time is normal.
				currentSideMoveSpeed = sideMoveSpeed;
			}

			if ( currentLane == Lanes.Center )
			{
				if ( isGoingRight )
				{
					desiredLane = Lanes.Right;
					setCharacterState( PlayerCharacterState.SideMove );
					moveDirection.x = currentSideMoveSpeed;
					playerSounds.playSideMoveSound();
					//Debug.Log ("changeLane completed " + isGoingRight + " to lane " + desiredLane );

				}
				else
				{
					desiredLane = Lanes.Left;
					setCharacterState( PlayerCharacterState.SideMove );
					moveDirection.x = -currentSideMoveSpeed;
					playerSounds.playSideMoveSound();
					//Debug.Log ("changeLane completed " + isGoingRight + " to lane " + desiredLane );
				}
			}
			else if ( currentLane == Lanes.Right && !isGoingRight )
			{
				desiredLane = Lanes.Center;
				setCharacterState( PlayerCharacterState.SideMove );
				moveDirection.x = -currentSideMoveSpeed;
				playerSounds.playSideMoveSound();
				//Debug.Log ("changeLane completed " + isGoingRight + " to lane " + desiredLane );
			}
			else if ( currentLane == Lanes.Left && isGoingRight )
			{
				desiredLane = Lanes.Center;
				setCharacterState( PlayerCharacterState.SideMove );
				moveDirection.x = currentSideMoveSpeed;
				playerSounds.playSideMoveSound();
				//Debug.Log ("changeLane completed " + isGoingRight + " to lane " + desiredLane );
			}
		}
	}
	
	//move the player to the left lane if false,
	//and to the right lane if true
	void changeLane2( bool isGoingRight )
	{
		//You can only change lanes while running
		//You can also change your mind 
		if ( playerCharacterState == PlayerCharacterState.Running || ( playerCharacterState == PlayerCharacterState.SideMove && this.isGoingRight != isGoingRight ) )
		{
			playerPositionWhenSideMoveStarted = transform.position;
			this.isGoingRight = isGoingRight;

			//Hack - put moveDirection.x to zero in case finalizeSideMove was never called because of a collision
			moveDirection.x = 0;

			//Make sure the lane data is correct in case a collision forced us out of our lane
			recalculateCurrentLane();

			float currentSideMoveSpeed;
			if( Time.timeScale < 1f )
			{
				//Player is using a slow time power up.
				currentSideMoveSpeed = sideMoveSpeed * SLOW_DOWN_FACTOR;
			}
			else
			{
				//Time is normal.
				currentSideMoveSpeed = sideMoveSpeed;
			}

			if ( isGoingRight )
			{
				desiredLane = Lanes.Right;
				setCharacterState( PlayerCharacterState.SideMove );
				moveDirection.x = currentSideMoveSpeed;
				playerSounds.playSideMoveSound();
				//Debug.Log ("changeLane completed " + isGoingRight + " to lane " + desiredLane );

			}
			else
			{
				desiredLane = Lanes.Left;
				setCharacterState( PlayerCharacterState.SideMove );
				moveDirection.x = -currentSideMoveSpeed;
				playerSounds.playSideMoveSound();
				//Debug.Log ("changeLane completed " + isGoingRight + " to lane " + desiredLane );
			}
		}
	}

	void finalizeSideMove()
	{
		if (jumping)
		{
			setCharacterState( PlayerCharacterState.Jumping );
		}
		else
		{
			setCharacterState( PlayerCharacterState.Running );					
		}
		moveDirection.x = 0;
		currentLane = desiredLane;
	}


	void setDesiredLane( float sideMoveInitiatedZ )
	{
		if( playerCharacterState == PlayerCharacterState.Sliding )
		{
			setCharacterState( PlayerCharacterState.Turning_and_sliding );
		}
		else
		{
			setCharacterState( PlayerCharacterState.Turning );
		}

		if (sideMoveInitiatedZ < -laneLimit )
		{
			//Lane 1 is the nearest to the player
			//Depending on whether the player is turning left or right
			//This will be either the left or the right lane
			myLane = 1;
			if (isGoingRight)
			{
				desiredLane = Lanes.Right;
			}
			else
			{
				desiredLane = Lanes.Left;				
			}
		}
		else if (sideMoveInitiatedZ <= 0)
		{
			//Lane 2 is always the center lane
			myLane = 2;
			desiredLane = Lanes.Center;
		}
		else
		{
			//Lane 3 is the furthest to the player
			//Depending on whether the player is turning left or right
			//This will be either the left or the right lane
			myLane = 3;
			if (isGoingRight)
			{
				desiredLane = Lanes.Left;
			}
			else
			{
				desiredLane = Lanes.Right;				
			}
		}
		wantToTurn = true;

	}

	//Returns the distance between the player and the center of the tile
	float getTurnInitiatedPosition()
	{
		float playerRotationY = Mathf.Floor( transform.eulerAngles.y );
		if( playerRotationY == 0 )
		{
			return transform.position.z - currentTilePos.z;
		}
		else if( playerRotationY == 90f || playerRotationY == -270f )
		{
			return transform.position.x - currentTilePos.x;
		}
		else if( playerRotationY == -90f || playerRotationY == 270f )
		{
			return -(transform.position.x - currentTilePos.x);
		}
		else
		{
			Debug.LogError("getTurnInitiatedPosition: returning 0");
			return 0;
		}
	}

	public void sideSwipe( bool isGoingRight )
	{
		if (isInDeadEnd )
		{	
			//We want to turn the corner
			turnCorner ( isGoingRight );
		}
		else
		{
			//we want to change lanes
			if( useLanes )
			{
				changeLane ( isGoingRight );
			}
			else
			{
				changeLane2 ( isGoingRight );
			}

		}
	}

	void verifyIfDesiredLaneReached()
	{
		if ( playerCharacterState == PlayerCharacterState.SideMove || playerCharacterState == PlayerCharacterState.Stumbling )
		{
			float playerRotationY = Mathf.Floor( transform.eulerAngles.y );
			if ( currentLane == Lanes.Center )
			{
				if ( isGoingRight )
				{
					//I want to reach the right lane
					//Facing straight
					if( playerRotationY == 0 )
					{
						if( transform.position.x - currentTilePos.x >= adjustedLaneLimit )
						{
							transform.position = new Vector3 (  currentTilePos.x + laneLimit, transform.position.y, transform.position.z );
							finalizeSideMove();
						}
					
					}
					//Facing right
					else if( playerRotationY == 90f ||  playerRotationY == -270f )
					{
						if( transform.position.z - currentTilePos.z <= -adjustedLaneLimit )
						{
							transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z - laneLimit );
							finalizeSideMove();
						}
					}
					//Facing left
					else
					{
						if( transform.position.z - currentTilePos.z >= adjustedLaneLimit )
						{
							transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z + laneLimit );
							finalizeSideMove();
						}
					}
				}
				else
				{
					//I want to reach the left lane
					//Facing straight
					if( playerRotationY == 0 )
					{
						if( transform.position.x - currentTilePos.x <= -adjustedLaneLimit )
						{
							transform.position = new Vector3 (  currentTilePos.x -laneLimit, transform.position.y, transform.position.z );
							finalizeSideMove();
						}
					
					}
					//Facing right
					else if( playerRotationY == 90f ||  playerRotationY == -270f )
					{
						if( transform.position.z - currentTilePos.z >= adjustedLaneLimit )
						{
							transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z + laneLimit );
							finalizeSideMove();
						}
					}
					//Facing left
					else
					{
						if( transform.position.z - currentTilePos.z <= -adjustedLaneLimit )
						{
							transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z - laneLimit );
							finalizeSideMove();
						}
					}
				}
				
			}
			else if ( currentLane == Lanes.Left )
			{
				//I want to reach the center lane
				//Facing straight
				if( playerRotationY == 0 )
				{
					if( transform.position.x - currentTilePos.x >= 0 )
					{
						transform.position = new Vector3 (  currentTilePos.x, transform.position.y, transform.position.z );
						finalizeSideMove();
					}
				
				}
				//Facing right
				else if( playerRotationY == 90f ||  playerRotationY == -270f )
				{
					if( transform.position.z - currentTilePos.z <= 0 )
					{
						transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z );
						finalizeSideMove();
					}
				}
				//Facing left
				else
				{
					if( transform.position.z - currentTilePos.z >= 0 )
					{
						transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z );
						finalizeSideMove();
					}
				}
			}
			else if ( currentLane == Lanes.Right )
			{
				//I want to reach the center lane
				//Facing straight
				if( playerRotationY == 0 )
				{
					if( transform.position.x - currentTilePos.x <= 0 )
					{
						transform.position = new Vector3 (  currentTilePos.x, transform.position.y, transform.position.z );
						finalizeSideMove();
					}					
				}
				//Facing right
				else if( playerRotationY == 90f || playerRotationY == -270f )
				{
					if( transform.position.z - currentTilePos.z >= 0 )
					{
						transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z );
						finalizeSideMove();
					}
				}
				//Facing left
				else
				{
					if( transform.position.z - currentTilePos.z <= 0 )
					{
						transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z );
						finalizeSideMove();
					}
				}
			}
		}
	}

	void verifyIfDesiredLaneReached2()
	{
		if ( playerCharacterState == PlayerCharacterState.SideMove || playerCharacterState == PlayerCharacterState.Stumbling )
		{
			float playerRotationY = Mathf.Floor( transform.eulerAngles.y );
			if ( isGoingRight )
			{
				//I want to reach the right lane
				//Facing straight
				if( playerRotationY == 0 )
				{
					if( transform.position.x - playerPositionWhenSideMoveStarted.x >= adjustedLaneLimit )
					{
						transform.position = new Vector3 (  playerPositionWhenSideMoveStarted.x + laneLimit, transform.position.y, transform.position.z );
						finalizeSideMove();
					}
				
				}
				//Facing right
				else if( playerRotationY == 90f ||  playerRotationY == -270f )
				{
					if( transform.position.z - playerPositionWhenSideMoveStarted.z <= -adjustedLaneLimit )
					{
						transform.position = new Vector3 (  transform.position.x, transform.position.y, playerPositionWhenSideMoveStarted.z - laneLimit );
						finalizeSideMove();
					}
				}
				//Facing left
				else
				{
					if( transform.position.z - playerPositionWhenSideMoveStarted.z >= adjustedLaneLimit )
					{
						transform.position = new Vector3 (  transform.position.x, transform.position.y, playerPositionWhenSideMoveStarted.z + laneLimit );
						finalizeSideMove();
					}
				}
			}
			else
			{
				//I want to reach the left lane
				//Facing straight
				if( playerRotationY == 0 )
				{
					if( transform.position.x - playerPositionWhenSideMoveStarted.x <= -adjustedLaneLimit )
					{
						transform.position = new Vector3 (  playerPositionWhenSideMoveStarted.x -laneLimit, transform.position.y, transform.position.z );
						finalizeSideMove();
					}
				
				}
				//Facing right
				else if( playerRotationY == 90f ||  playerRotationY == -270f )
				{
					if( transform.position.z - playerPositionWhenSideMoveStarted.z >= adjustedLaneLimit )
					{
						transform.position = new Vector3 (  transform.position.x, transform.position.y, playerPositionWhenSideMoveStarted.z + laneLimit );
						finalizeSideMove();
					}
				}
				//Facing left
				else
				{
					if( transform.position.z - playerPositionWhenSideMoveStarted.z <= -adjustedLaneLimit )
					{
						transform.position = new Vector3 (  transform.position.x, transform.position.y, playerPositionWhenSideMoveStarted.z - laneLimit );
						finalizeSideMove();
					}
				}
			}
		}
	}

	//Called every frame to ensure that the critical current lane value is always accurate
	public void recalculateCurrentLane()
	{
		float min = -0.65f;
		float max = 0.65f;
		float relativePos = 0;
		Lanes calculatedLane;

		if( tileRotationY == 0 )
		{
			//X axis
			relativePos = transform.position.x - currentTilePos.x;	
		}
		else if( tileRotationY == 90f || tileRotationY == -270f )
		{
			//Z axis facing left
			relativePos = currentTilePos.z - transform.position.z;
		}
		else if( tileRotationY == -90f || tileRotationY == 270f )
		{
			//Z axis facing right
			relativePos = transform.position.z - currentTilePos.z;
		}
		if( relativePos > min && relativePos < max)
		{
			calculatedLane = Lanes.Center;
		}
		else if( relativePos < min )
		{
			calculatedLane = Lanes.Left;
		}
		else
		{
			calculatedLane = Lanes.Right;		
		}
		if( calculatedLane != currentLane )
		{
			//Debug.LogWarning("recalculateCurrentLane changed current lane from: " + currentLane + " to: " + calculatedLane + " relative pos " + relativePos );
			currentLane = calculatedLane;
			desiredLane = currentLane;
		}
	}
	#endregion

	#region Zipline
	public void attachToZipline()
	{
		SegmentInfo si = currentTile.GetComponent<SegmentInfo>();
		if( si != null )
		{
			List<SegmentInfo.BezierData>  curveList = si.curveList;
			SegmentInfo.BezierData bezierData = curveList[0];
			setCharacterState( PlayerCharacterState.Ziplining );
			enablePlayerControl( false );
			enablePlayerMovement( false );
			//It is possible for a change lane to be in progress when we attach to the zipline.
			//To avoid drifting along the X axis when we detach from the zipline, make sure we reset moveDirection.
			moveDirection = Vector3.zero;
			capsuleCollider.attachedRigidbody.isKinematic = true;
			setAnimationTrigger(Idle_LookTrigger);
			ziplineAttachPoint = transform.FindChild("Zipline Attach Point");
			Debug.Log("attachToZipline for: " +  gameObject.name + " isMasterClient: " + PhotonNetwork.isMasterClient + " isMine: " + this.photonView.isMine + " view ID: " + this.photonView.viewID + " owner ID: " + this.photonView.ownerId );
			ziplineAttachPoint.localPosition = new Vector3( 0, 2.15f, 0 );
			ziplineAttachPoint.localEulerAngles = new Vector3( 0, 0, 0 );
			ziplineAttachPoint.GetComponent<AudioSource>().Play();
			ziplineAttachPoint.SetParent(null);
			transform.SetParent( ziplineAttachPoint );
			transform.eulerAngles =  new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,6f);
			//A set of points that define one or many bezier paths (the paths should be passed in multiples of 4, which correspond to each individual bezier curve)
			//It goes in the order: startPoint,endControl,startControl,endPoint
			LTBezierPath ltBezier = new LTBezierPath( new Vector3[] { bezierData.bezierStart.position, bezierData.bezierControl2.position, bezierData.bezierControl1.position, bezierData.bezierEnd.position } );
			LeanTween.move(ziplineAttachPoint.gameObject, ltBezier.pts, 3.5f).setOrientToPath(true).setEase(LeanTweenType.easeOutQuad);
			playerCamera.playCutscene(CutsceneType.Ziplining);
		}
	}

	public void detachFromZipline()
	{
		if( getCharacterState() == PlayerCharacterState.Ziplining )
		{
			LeanTween.cancel( gameObject );
			transform.SetParent( null );
			ziplineAttachPoint.SetParent( transform, false );
			ziplineAttachPoint.localScale = new Vector3( 1f, 1f, 1f ); 	//Just because of rounding when changing parent
			ziplineAttachPoint.GetComponent<AudioSource>().Stop();
			playerCamera.reactivateMaincamera();
			isInZiplineTrigger = false; //Reset in case player died inside trigger
			capsuleCollider.attachedRigidbody.isKinematic = false;
			transform.localScale = new Vector3( 1f, 1f, 1f ); 	//Just because of rounding when changing parent
			//The player might have died while ziplining.
			//managePlayerDeath calls detachFromZipline().
			//We only do the last steps if the player is alive.
			if( deathType == DeathType.Alive )
			{
				enablePlayerControl( true );
				enablePlayerMovement( true );
				transform.eulerAngles = new Vector3(0,270f,0); //we turned left while ziplining
				//We may have switched lanes because of the position change. Make sure the lane values are accurate.
				recalculateCurrentLane();
				fall( true );
			}
			else
			{
				//If the player is dead, it looks nicer with the camera locked.
				playerCamera.lockCamera( true );
			}
		}
	}
	#endregion

	#region Animation
	public void playVictoryAnimation()
	{
		setAnimationTrigger( Finish_LineTrigger );
	}

	void setAnimationTrigger( int animationTrigger )
	{
		anim.SetTrigger( animationTrigger );
	}
	#endregion

	#region Player Death
	/// <summary>
	/// This is the official way to kill the player. Do not call playerDiedRPC directly.
	/// This method verifies that it is the master client before sending the playerDiedRPC.
	/// The playerDiedRPC call is sent to All.
	/// </summary>
	/// <param name="deathTypeValue">Death type value.</param>
	public void killPlayer( DeathType deathTypeValue )
	{
		//Only proceed if the player is the master client and he is not dying already
		if ( PhotonNetwork.isMasterClient && playerCharacterState != PlayerCharacterState.Dying )
		{
			Debug.Log("PlayerControl-killPlayer : " + deathTypeValue + " name " + gameObject.name );
			this.photonView.RPC("playerDiedRPC", PhotonTargets.AllViaServer, deathTypeValue, currentTile.name );
		}
	}

	[PunRPC]
	void playerDiedRPC( DeathType deathTypeValue, string tileWherePlayerDied )
	{
		this.tileWherePlayerDied = tileWherePlayerDied;
		changeColliderAxis( Axis.Z );
		ignorePlayerCollisions( true );

		if( deathTypeValue != DeathType.NO_MORE_HEALTH ) playerHealth.deductAllHealth();

		Debug.Log("playerDiedRPC : " + deathTypeValue + " " + gameObject.name );

		//For debugging
		if( PlayerStatsManager.Instance.getShowDebugInfoOnHUD() ) HUDMultiplayer.hudMultiplayer.activateUserMessage( "Died: " + deathTypeValue + " " + gameObject.name, 0, 10f );

		//Update the player statistics		
		if( this.photonView.isMine && GetComponent<PlayerAI>() == null )
		{
			GameManager.Instance.playerStatistics.incrementNumberOfDeathsLifetime();
		
			//Increment the number of times we died during this race
			numberOfTimesDiedDuringRace++;
		}

		//Remember how we died
		deathType = deathTypeValue;

		//If we were ziplining, detach from the zipline
		detachFromZipline();

		//Disable the player's shadow
		playerVisuals.enablePlayerShadow( false );

		anim.speed = 1f;

		//Change character state
		setCharacterState( PlayerCharacterState.Dying );

		//The PlayerSpell component needs to know that the player died
		GetComponent<PlayerSpell>().playerDied();

		//The PlayerIK component needs to know that the player died
		GetComponent<PlayerIK>().playerDied();

		//Reset the icon on the Minimap. The alpha might have been changed during Cloak.
		MiniMap.Instance.changeAlphaOfRadarObject( this, 1f );

		//Stop the particle systems. One might be playing if we died while sliding for example.
		playerVisuals.playDustPuff( false );
		playerVisuals.playWaterSplashWhileSliding( false );

		//Disable player controls when dying
		enablePlayerControl( false );
		
		//Deactivate any queued jump or slide
		queueJump = false;
		queueSlide = false;

		//Reset move direction and velocity. We keep the Y component so the player falls to the ground.
		moveDirection = new Vector3( 0,moveDirection.y,0 );
		capsuleCollider.attachedRigidbody.velocity = new Vector3( 0,moveDirection.y,0 );

		//Stop any currently playing sound
		playerSounds.stopAudioSource();

		//Also, when the player dies, we don't want him to continue talking.
		playerVoiceOvers.stopAudioSource();

		//Also tell PlayerRace. For example, we need to cancel the took the lead invoke that might be pending.
		GetComponent<PlayerRace>().playerDied();

		//Make adjustments depending on death type
	    switch (deathType)
		{
	        case DeathType.Cliff:
				StartCoroutine( waitBeforeResurrecting(2f) );	
				break;
	                
	        case DeathType.Enemy:
				StartCoroutine( waitBeforeResurrecting(2f) );	
				break;
	                
			case DeathType.Zombie:
				//Play collision sound
				playerSounds.playDyingSound();
				setAnimationTrigger(DeathWallTrigger);
				break;
		
			case DeathType.Flame:
				playerSounds.playFireDyingSound();
				setAnimationTrigger(DeathWallTrigger);
				break;
	                
	        case DeathType.Obstacle:
				//Play collision sound
				playerSounds.playDyingSound();
				playerCamera.Shake();
				setAnimationTrigger(DeathWallTrigger);
				break;

	        case DeathType.FallForward:
				//Play collision sound
				playerSounds.playDyingSound();
				playerCamera.Shake();
				setAnimationTrigger(FallForwardTrigger);
				break;

	        case DeathType.GreatFall:
				StartCoroutine( waitBeforeResurrecting(2.5f) );
				break;

			default:
				setAnimationTrigger(DeathWallTrigger);
				break;
		}
	}

	/// <summary>
	/// When set to true, the player will ignore collisions with other players.
	/// Typically used when the player is dead so that other players can go through his body.
	/// </summary>
	/// <param name="ignore">If set to <c>true</c> the player will ignore collisions with other players.</param>
	void ignorePlayerCollisions( bool ignore )
	{
		//We want a dead player to not collided with an alive player
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i] != GetComponent<PlayerRace>() )
			{
				//print("Changing collision with: " +  PlayerRace.players[i].name + " Collision is ignored: " + ignore );
				Physics.IgnoreCollision(capsuleCollider, PlayerRace.players[i].GetComponent<Collider>(), ignore );
			}
		}
	}

	public void death_completed ( AnimationEvent eve )
	{
		if( this.photonView.isMine && playerAI == null ) StartCoroutine( controlVignetting( 0.25f, 0.7f, 1f ) );
		StartCoroutine( waitBeforeResurrecting(2f) );
	}

	public void fall_forward_completed ( AnimationEvent eve )
	{
		if( this.photonView.isMine && playerAI == null ) StartCoroutine( controlVignetting( 0.25f, 0.7f, 1f ) );
		StartCoroutine( waitBeforeResurrecting(2f) );
	}

	IEnumerator waitBeforeResurrecting ( float duration )
	{
		yield return new WaitForSeconds(duration);
		LockstepManager.Instance.addActionToQueue( new LockstepManager.LockstepAction( LockstepActionType.RESURRECT, gameObject ) );
	}

	void changeColliderAxis( Axis axis )
	{
		if( axis == Axis.Y)
		{
			//Upright position
			capsuleCollider.direction = (int)Axis.Y;
			capsuleCollider.center = controllerOriginalCenter;
		}
		else if( axis == Axis.Z )
		{
			//Sliding or dead position
			capsuleCollider.direction = (int)Axis.Z;
			capsuleCollider.center = new Vector3( controllerOriginalCenter.x, PRONE_CAPSULE_CENTER_Y, controllerOriginalCenter.z );
		}
	}

	IEnumerator controlVignetting( float endVignettingFactor, float endBlurFactor, float duration )
	{
		VignetteAndChromaticAberration vAcA = Camera.main.GetComponent<VignetteAndChromaticAberration>();
 		vAcA.enabled = true;

		float elapsedTime = 0;
		
		float startVignettingFactor = vAcA.intensity;
		float startBlurFactor = vAcA.intensity;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			vAcA.intensity = Mathf.Lerp( startVignettingFactor, endVignettingFactor, elapsedTime/duration );
			vAcA.blur = Mathf.Lerp( startBlurFactor, endBlurFactor, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		
		vAcA.intensity = endVignettingFactor;
		vAcA.blur = endBlurFactor;
		if( vAcA.intensity == 0 ) vAcA.enabled = false;
	}

	//LockstepManager is in charge of calling this. Do not call directly.
	public void resurrectBegin()
	{
		playerCamera.setCameraParameters( 18f, PlayerCamera.DEFAULT_DISTANCE, PlayerCamera.DEFAULT_HEIGHT, PlayerCamera.DEFAULT_Y_ROTATION_OFFSET );
		playerCamera.activateMainCamera();
		playerCamera.positionCameraNow();
		playerCamera.resetCameraParameters();

		if( this.photonView.isMine && playerAI == null ) StartCoroutine( controlVignetting( 0f, 0f, 0f ) );

		//Only send an event if we are the local player and we are not a bot.
		if( this.photonView.isMine && playerAI == null ) GameManager.Instance.setGameState( GameState.Resurrect );

		//Reset data
		resetSharedLevelData(true);
		
		//Reposition dead body at the respawn location.
		//Don't use the currentTile which is local since it may be out of sync.
		//Use the name of the tile sent by the master client when the player died.
		GameObject tileWherePlayerDiedGameObject = GameObject.Find( tileWherePlayerDied );
		GameObject respawnLocationObject = tileWherePlayerDiedGameObject.transform.Find("respawnLocation").gameObject;

		if( respawnLocationObject != null )
		{
			Transform respawn = respawnLocationObject.transform;
			RaycastHit hit;
			float groundHeight = 0f;
	        if (Physics.Raycast(respawn.position, Vector3.down, out hit, 4.0F ))
			{
				groundHeight = hit.point.y;
			}
			//If the player died in a dead end trigger, the trigger will be activated when we move the player's body
			//to the respawn location. This in turn will cause isInDeadEnd to become true and when the player will try to change lanes,
			//he will turn instead and crash into a fence. To avoid that, disable the collider before moving the player and reenable it after.
			transform.GetComponent<Collider>().enabled = false;
			transform.position = new Vector3( respawn.position.x, groundHeight, respawn.position.z );
			//The respawnLocationObject is always point in the correct direction for the resurrected hero
			//temporarily set the tile has the parent of the hero and simply use the local rotation of the respawn location object
			transform.SetParent( currentTile.transform );
			transform.localRotation = Quaternion.Euler ( 0, respawn.localEulerAngles.y, 0 );
			transform.SetParent( null );
			tileRotationY = Mathf.Floor ( transform.eulerAngles.y );
			transform.GetComponent<Collider>().enabled = true;
			//Re-enable the player's blob shadow
			playerVisuals.enablePlayerShadow( true );
			playerCamera.positionCameraNow();
			playerCamera.lockCamera( true );
			transform.position = new Vector3( respawn.position.x, groundHeight + 9f, respawn.position.z );
			//By calling setCharacterState with StartRunning, the WorldSoundManager will know to resume the music.
			setCharacterState( PlayerCharacterState.StartRunning );
			changeColliderAxis( Axis.Y );
			ignorePlayerCollisions( false );
			//Mecanim Hack - we call rebind because the animation states are not reset properly when you die in the middle of an animation.
			//For example, if you die during a double jump, after you get resurrected and start running again, if you do another double jump, only part of the double jump animation will play, never the full animation.
			anim.Rebind();
			//Make player fall from sky, land and start running again
			fall( true );
		}
		else
		{
			Debug.LogError("PlayerControl-ResurrectBegin: Unable to find respawnLocation game object in tile : " + currentTile.name );
		}
	}

	void resetSharedLevelData( bool unlockCamera )
	{
		//Reset values
		//A shrink spell changes the scale value so we need to reset it
		transform.localScale = new Vector3( 1f, 1f, 1f );

		//Lanes
		currentLane = Lanes.Center;
		desiredLane = Lanes.Center;
		myLane = 0;

		jumping = false;
		queueSlide = false;
		queueJump = false;
		isInDeadEnd = false;
		deadEndTrigger = null;
		wantToTurn = false;
		deadEndTurnDone = false;
		if( unlockCamera ) playerCamera.lockCamera ( false );
		gravity = DEFAULT_GRAVITY;
		moveDirection = new Vector3(0,0,0);
		playerCamera.heightDamping = PlayerCamera.DEFAULT_HEIGHT_DAMPING;
		playerHealth.resetHealth();
	}

	//Called when a player lands after respawn.
	private void resurrectEnd()
	{		
		//Display a minimap message that this player or bot is back in the game.
		string heroName;
		if( playerAI == null )
		{
			//We're the player
			heroName = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex ).name.ToString();
		}
		else
		{
			//We're a bot
			heroName = 	GetComponent<PlayerAI>().botHero.userName;
		}
		this.photonView.RPC("playerResurrectedRPC", PhotonTargets.AllViaServer, heroName );


		//The GameState was Resurrect - change it back to Normal
		if( this.photonView.isMine && playerAI == null ) GameManager.Instance.setGameState(GameState.Normal);

		//Restore player controls
		enablePlayerControl( true );
		deathType = DeathType.Alive;
		//Display a Go! message
		if( this.photonView.isMine && playerAI == null ) HUDMultiplayer.hudMultiplayer.activateUserMessage( LocalizationManager.Instance.getText("GO"), 0f, 1.25f );
	}

	[PunRPC]
	public void playerResurrectedRPC( string heroName )
	{
		MiniMap.Instance.displayMessage( string.Format( backInTheGameString, heroName ) );
		playerVoiceOvers.playVoiceOver(VoiceOverType.VO_Resurrect);
	}

	public int getNumberOfTimesDiedDuringRace()
	{
		return numberOfTimesDiedDuringRace;
	}
	#endregion

	#region Stumble
	public void stumble()
	{
		//The OnControllerColliderHit function can send multiple collision events during a single
		//stumble, so ignore any new events while in the stumbling state.
		if ( playerCharacterState != PlayerCharacterState.Stumbling && playerCharacterState != PlayerCharacterState.Dying )
		{	
			Debug.Log ("Player stumbled");
			setCharacterState( PlayerCharacterState.Stumbling );
			//The player stumbles but recovers
			setAnimationTrigger(StumbleTrigger);
		}
	}

	public void stumble_completed ( AnimationEvent eve )
	{
		setCharacterState( PlayerCharacterState.Running );
	}
	#endregion

	public PlayerCharacterState getCharacterState()
	{
		return playerCharacterState;
	}

	public void setCharacterState( PlayerCharacterState newState )
	{
		playerCharacterState = newState;
		//Debug.Log("PlayerControl-setCharacterState to: " + newState + " for " + gameObject.name );
		//Send an event to interested classes if you are a human player
		if(multiplayerStateChanged != null && this.photonView.isMine && playerAI == null ) multiplayerStateChanged( playerCharacterState );
		
		playerRun.handlePlayerStateChange( playerCharacterState );
	}

	public void enablePlayerControl( bool enabled )
	{
		playerControlsEnabled = enabled;
	}

	public bool isPlayerControlEnabled()
	{
		return playerControlsEnabled;
	}

	/// <summary>
	/// Enables the player movement. If set to false, also sets the velocity of the rigid body to zero.
	/// </summary>
	/// <param name="enabled">If set to <c>true</c> enabled.</param>
	public void enablePlayerMovement( bool enabled )
	{
		playerMovementEnabled = enabled;
		if( !enabled ) capsuleCollider.attachedRigidbody.velocity = Vector3.zero;
	}

	public bool isPlayerMovementEnabled()
	{
		return playerMovementEnabled;
	}

	#region OnTriggerEnter, OnTriggerStay, OnTriggerExit
	void OnTriggerEnter(Collider other)
	{
		//Carefull, if you turn right inside a deadEnd OnTriggerEnter will be called a second time (but not if your turn left).
		//This is probably a Unity bug.
		if( other.CompareTag( "deadEnd" ) )
		{
			if( photonView.isMine )
			{
				//Do normal turn logic
				isInDeadEnd = true;
				wantToTurn = false;

				currentDeadEndType = other.GetComponent<deadEnd>().deadEndType;
				deadEndTrigger = other;
				//Force peers to be positioned at the beginning of the deadEnd trigger.
				photonView.RPC("deadEndEnterRPC", PhotonTargets.Others, transform.position, transform.rotation);
			}
		}
		//For the Great Fall trigger collider, don't forget to put in the ignoreRaycast layer or else the distanceToGround value will be incorrect.
		else if( other.CompareTag( "Great Fall" ) )
		{
			killPlayer( DeathType.GreatFall );
		}
		//For the Lock Camera trigger collider, don't forget to put in the ignoreRaycast layer or else the distanceToGround value will be incorrect.
		else if( other.CompareTag( "Lock Camera" ) )
		{
			playerCamera.lockCamera( true );
		}
		else if( other.CompareTag( "Entrance" ) )
		{
			SegmentInfo si = other.transform.parent.GetComponent<SegmentInfo>();
			if( si != null )
			{
				if( !si.entranceCrossed )
				{
					//We might recycle currentTile (the one prior to the one we just entered), this is why we are passing it as a parameter.
					generateLevel.tileEntranceCrossed( other.transform.parent );
					//This flag is set to avoid tileEntranceCrossed being called multiple time which can happen with onTriggerEnter.
					//This flag is set to false when a tile is added.
					si.entranceCrossed = true;
				}
				currentTilePos = si.transform.position;
				currentTile = si.gameObject;
				tileRotationY = Mathf.Floor ( currentTile.transform.eulerAngles.y );
				playerRace.tilesLeftBeforeReachingEnd--;
			}
			else
			{
				Debug.LogError("PlayerControl-OnTriggerEnter: " + other.transform.parent.name + " tile does not have a SegmentInfo component attached to it.");
			}
		}
		else if( other.CompareTag( "AttachZiplineTrigger" ) )
		{
			//Ignore if the player is Flying
			if( getCharacterState() == PlayerCharacterState.Flying ) return;

			//Cancel the speedboost if active before ziplining
			playerSpell.cancelRagingBull();
			isInZiplineTrigger = true;
		}
 		else if( other.CompareTag( "DetachZiplineTrigger" ) )
		{
			if( PhotonNetwork.isMasterClient ) this.photonView.RPC("detachFromZiplineRPC", PhotonTargets.All, transform.position, transform.eulerAngles.y, PhotonNetwork.time );
		}
  	}

	void OnTriggerStay(Collider other)
	{
		if( wantToTurn )
		{
			float currentZ = 0;
			float playerRotationY = Mathf.Floor( transform.eulerAngles.y );
			if( playerRotationY == 0 )
			{
				currentZ = deadEndTrigger.transform.position.z - transform.position.z;
			}
			else if( playerRotationY == 90f || playerRotationY == -270f )
			{
				currentZ = deadEndTrigger.transform.position.x - transform.position.x;
			}
			else if( playerRotationY == -90f || playerRotationY == 270f )
			{
				currentZ = transform.position.x - deadEndTrigger.transform.position.x;
			}
			else
			{
				Debug.LogWarning("PlayerControl-OnTriggerStay: returning 0");
				currentZ = 0;
			}
			
			//Lane 1 is the nearest to the player
			if ( myLane == 1 )
			{
				if ( currentZ <= laneLimit )
				{
					//We can turn now
					turnNow();					
				}
			}
			//Lane 2 is always the center lane
			else if ( myLane == 2 )
			{
				if ( currentZ <= 0 )
				{
					//We can turn now
					turnNow();
				}
			}
			//Lane 3 is the furthest from the player
			else if ( myLane == 3 )
			{
				if ( currentZ <= -laneLimit )
				{
					//We can turn now
					turnNow();
				}
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		if( getCharacterState() != PlayerCharacterState.Dying )
		{
			if( other.CompareTag( "deadEnd" ) )
			{
				if( photonView.isMine )
				{
					//Do normal turn logic
					if( !deadEndTurnDone && currentDeadEndType != DeadEndType.None && currentDeadEndType != DeadEndType.RightStraight && currentDeadEndType != DeadEndType.LeftStraight && getCharacterState() != PlayerCharacterState.Flying )
					{
						Debug.LogWarning("OnTriggerExit player exited dead end without turning " + other.name + " " + isInDeadEnd + " " + deadEndTurnDone + " " + currentDeadEndType );
						killPlayer ( DeathType.Exited_Without_Turning );
					}
					//Reset values
					isInDeadEnd = false;
					deadEndTurnDone = false;
					deadEndTrigger = null;
					wantToTurn = false;
					//Force peers to be positioned at the exit of the deadEnd trigger.
					photonView.RPC("deadEndExitRPC", PhotonTargets.Others, transform.position, transform.rotation);
				}
			}
			else if( other.name == "ZiplineTrigger" )
			{
				//Player is no longer in the zipline trigger
				isInZiplineTrigger = false;
			}
		}
	}
	#endregion


}
