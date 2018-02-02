using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using Cinemachine;

public enum DeathType {
		Alive = 0,
		Obstacle = 1,
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
	SideMove = 1, //No longer used. Kept because PlayerController references it.
	Sliding = 2,
	Running = 3,
	Jumping = 4,
	Dying = 5,
	Stumbling = 6,
	Winning = 7,
	StartRunning = 9,
	Falling = 10,
	Turning = 11,
	Turning_and_sliding = 12,
	Ziplining = 13,
	DoubleJumping = 14
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
	PlayerIK playerIK;	//This is an optional component
	Ragdoll ragdoll; 	//This is an optional component
	#endregion

	#region Hash IDs for player animations	
	//Hash IDs for player animations. These are used to improve performance.
	int RunTrigger = Animator.StringToHash("Run");
	int StumbleTrigger = Animator.StringToHash("Stumble");
	int JumpTrigger = Animator.StringToHash("Jump");
	int DoubleJumpTrigger = Animator.StringToHash("Double Jump");
	int FallTrigger = Animator.StringToHash("Fall");
	int LandTrigger = Animator.StringToHash("Land");
	int RespawnTrigger = Animator.StringToHash("Respawn");
	int LandAfterRespawnTrigger = Animator.StringToHash("Land After Respawn");
	int SlideDownTrigger = Animator.StringToHash("Slide Down");
	int SlideUpTrigger = Animator.StringToHash("Slide Up");
	int FallBackwardTrigger = Animator.StringToHash("Fall Backward");
	int FallForwardTrigger = Animator.StringToHash("Fall Forward");
	int WinTrigger = Animator.StringToHash("Win");
	int LoseTrigger = Animator.StringToHash("Lose");
	int AttachToZiplineTrigger = Animator.StringToHash("Zipline Attach");
	int OmniToolTrigger = Animator.StringToHash("OmniTool");
	int leaningBlendFactor = Animator.StringToHash("Leaning");

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
	const float COLLIDER_ABOVE_DISTANCE = 1f; //In meters.
	[Tooltip("If set to true, the player will continue sliding automatically as long as there is a collider just above his head. You can set it to false to save on performance.")]
	[SerializeField] bool checkForColliderAbove = false;
	#endregion

	#region General movement variables
	//Use to move the player along the X, Y and Z axis
	public Vector3 moveDirection;
	Vector3 forward;
	//We set the anim speed to 0 when we pause, so we need to remember the value when we unpause
	float animSpeedAtTimeOfPause;
	//Remember if the rigidbody was kinematic or not at the time of pause.
	bool wasKinematicAtTimeOfPause;
	//The state of the character i.e. running, jumping, sliding, etc.
	public PlayerCharacterState playerCharacterState;
	//True if the player is allowed to move, false otherwise. This flag is useful during camera cut-scenes to prevent the player from moving.
	bool playerMovementEnabled = false;
 	//Are inputs allowed?
	bool playerControlsEnabled = false;
	#endregion

	#region Powerup variables
	//When the player is using a slow time power up, this slows down time by a factor of 2.
	//However, in order for the game to not feel sluggish, we use a smaller value called SLOW_DOWN_FACTOR
	//for the accelerometer and when changing lanes.
	const float SLOW_DOWN_FACTOR = 1.5f;
	#endregion

	#region Zipline variables
	[SerializeField] Vector3 ziplinePlayerOffset = new Vector3( -0.28f, 2.67f, 0 );
	Transform ziplineAttachPoint;
	float ziplineExitAngle = 0;
	#endregion

	#region Lane variables
	public enum Lanes {
		Left = -1,
		Center = 0,
		Right = 1,
	}
	const float LANE_WIDTH = 2f; //In meters.
	const float LANE_LEFT_BOUNDARY  = -LANE_WIDTH * 0.5f;
	const float LANE_RIGHT_BOUNDARY = +LANE_WIDTH * 0.5f;
	//Due to rounding errors, the player may not reach exactly the lane limit. If there is less than 1% of the distance
	//remaining, assume that he did reach the lane limit which will allow us to finalize the side move.
	float adjustedLaneLimit = LANE_WIDTH * 0.99f;
	public Lanes currentLane = Lanes.Center;
	Lanes desiredLane = Lanes.Center;
	int myLane = 0; //0 is uninitialized, 1 is the nearest, 2 is in the center and 3 is the furthest
	public float sideMoveSpeed = 5.8f; //At what speed do you change lanes.
	public bool isChangingLanes = false;
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
	public GameObject currentTile;
	//This flag is used to avoid entrance crossed being called multiple times which can happen with OnTriggerEnter
	bool wasEntranceCrossed = false;
	public int tileIndex; //The index of the tile the player is currently on.
 	#endregion

	#region Death variables
	public DeathType deathType = DeathType.Alive;
	//When the player dies, we change the center and radius.
	//We need to be able to reset these values when the player is revived.
	Vector3 controllerOriginalCenter;
	const float PRONE_CAPSULE_CENTER_Y = 0.57f;
	const float PRONE_CAPSULE_CENTER_Z = -1.4f;
	string resurrectOnThisTile = string.Empty;
	int numberOfTimesDiedDuringRace = 0; //Used by PlayerStatistics to determine if the player had a perfect race, that is, he did not die a single time.
	const float DELAY_BEFORE_RESURRECTING = 1.75f;
	#endregion

	#region Other variables
	GenerateLevel generateLevel;
	CoopWaveGenerator coopWaveGenerator;
	Coroutine changeLeaningBlendFactorCoroutine;
	const float PACKET_EXPIRY = 0.375f; //Used to discard synchronization packets that are too old.
	#endregion

	#region Events
	//Event management used to notify other classes when the character state has changed
	public delegate void MultiplayerStateChanged( Transform player, PlayerCharacterState value );
	public static event MultiplayerStateChanged multiplayerStateChanged;
	#endregion

	#region Cached localized strings
	//Cache the string to avoid the runtime lookup
	string backInTheGameString;
	#endregion

	#region Related to distance remaining
	public float tileDistanceTraveled = 0;
	#endregion

	#region OmniTool
	TurnRibbonHandler turnRibbonHandler;
	CardName activeCardName; //Card that will be played when the OmniTool animation completes
	//Temporary - used to support old skins
	public bool hasOmniToolAnimation = true;
	#endregion

	#region Skill Bonuses
	SkillBonusEventCounter doubleKillEventCounter;
	#endregion

	#region For debugging
	public float animSpeed;
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
		ragdoll = GetComponent<Ragdoll>();
		if( ragdoll != null ) ragdoll.controlRagdoll( false );
		playerIK = GetComponent<PlayerIK>();
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
		turnRibbonHandler = GameObject.FindGameObjectWithTag("Turn-Ribbon").GetComponent<TurnRibbonHandler>();
		coopWaveGenerator = GameObject.FindObjectOfType<CoopWaveGenerator>();

		//Cache the string to avoid the runtime lookup
		backInTheGameString = LocalizationManager.Instance.getText( "MINIMAP_BACK_IN_GAME" );

		#region Skill Bonuses
		if( !GameManager.Instance.isCoopPlayMode() )
		{
			doubleKillEventCounter = gameObject.AddComponent<SkillBonusEventCounter>();
			doubleKillEventCounter.initialize( "SKILL_BONUS_DOUBLE_KILL", 50, 2, 5f );
		}
		#endregion

		//The character is in idle while waiting to run. 
		setCharacterState( PlayerCharacterState.Idle );

		//Calculate the ground height
		RaycastHit hit;

		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hit, 50f ))
		{
			transform.position = new Vector3( transform.position.x, hit.point.y, transform.position.z);
			//Debug.Log( "There is ground underneath the player on Start. The collider is: " + hit.collider.name + " at height: " + hit.point.y );
		}
		else
		{
			Debug.LogError( "There is no ground below the player during PlayerControl Start. Check that the Spawn Height property in CircuitInfo is correct." );
		}

		getFirstTileInfo();

		//We may not have been spawned in the center lane. Make sure the lane values are accurate.
		recalculateCurrentLane();

		//Tell the MasterClient that we are ready to go. Our level has been loaded and our player created.
		//The MasterClient will initiate the countdown
		this.photonView.RPC("readyToGo", PhotonTargets.MasterClient );	
	}

	//The player control needs info about the tile the player is on.
	void getFirstTileInfo()
	{	
		GameObject firstTile = generateLevel.getFirstTile();
		currentTile = firstTile;
		currentTilePos = firstTile.transform.position;
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
		//The player starts off running
		setAnimationTrigger(RunTrigger);
		setCharacterState( PlayerCharacterState.StartRunning );
		setCharacterState( PlayerCharacterState.Running );
	
		//When the GameState is NORMAL, we display the HUD
		if( this.photonView.isMine && playerAI == null ) GameManager.Instance.setGameState( GameState.Normal );

		enablePlayerMovement( true );
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
		this.photonView.RPC( "pauseRemotePlayers", PhotonTargets.All, transform.position, transform.eulerAngles.y, currentTile.name );
	}

	[PunRPC]
	public void pauseRemotePlayers( Vector3 positionAtTimeOfPause, float yRotationAtTimeOfpause, string tileName )
	{
		pausePlayer( true );
		transform.SetPositionAndRotation( positionAtTimeOfPause, Quaternion.Euler( transform.eulerAngles.x, yRotationAtTimeOfpause, transform.eulerAngles.z ) );
		GameObject tileGameObject = GameObject.Find( tileName );
		updateCurrentTileInfo( tileGameObject );
		recalculateCurrentLane();
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
			//Temporarily remove collider so as to not block the path for other players and
			//make the rigidbody kinetic so it does not fall through the ground because of gravity.
			wasKinematicAtTimeOfPause = capsuleCollider.attachedRigidbody.isKinematic;
			capsuleCollider.attachedRigidbody.isKinematic = true;		
			capsuleCollider.enabled = false;
		}
		else
		{
			capsuleCollider.enabled = true;
			capsuleCollider.attachedRigidbody.isKinematic = wasKinematicAtTimeOfPause;	//reset value
			enablePlayerControl( true );
			enablePlayerMovement( true );
			anim.speed = animSpeedAtTimeOfPause;
		}
		//Also pause various VFX
		playerVisuals.pausePlayer( isPaused );
	}
	#endregion

	void FixedUpdate()
	{
		//for debugging
		if( anim != null ) animSpeed = anim.speed;
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
						fall();
					}
				}
			}

			verifyIfDesiredLaneReached();
			stayWithinLanes();
		}
	}

	//Make sure the player stays within the lane limits (unless he is dying or falling or ziplining ).
	void stayWithinLanes()
	{
		if ( playerCharacterState != PlayerCharacterState.Dying && playerCharacterState != PlayerCharacterState.Falling && playerCharacterState != PlayerCharacterState.Ziplining )
		{
			//Player is following a corridor
			if( transform.eulerAngles.y > -0.01f && transform.eulerAngles.y < 0.01f )
			{
				transform.rotation = Quaternion.identity;
			}
			float playerRotationY = Mathf.Floor ( transform.eulerAngles.y );
			if( playerRotationY == 0 )
			{
				//Round to two decimal places to avoid small rounding errors
				float delta = Mathf.Round( (transform.position.x - currentTilePos.x) * 100.0f) / 100.0f;
				if( delta < -LANE_WIDTH )
				{
					//Debug.LogWarning ( "-LANE_WIDTH player x " + transform.position.x + " currentTilePos.x " + currentTilePos.x + " playerRotationY " + playerRotationY + " currentTile.name " + currentTile.name );
					transform.position = new Vector3 (  currentTilePos.x -LANE_WIDTH, transform.position.y, transform.position.z );
				}
				else if( delta > LANE_WIDTH )
				{
					//Debug.LogWarning ( "+LANE_WIDTH player x " + transform.position.x + " currentTilePos.x " + currentTilePos.x + " playerRotationY " + playerRotationY + " currentTile.name " + currentTile.name );
					transform.position = new Vector3 (  currentTilePos.x + LANE_WIDTH, transform.position.y, transform.position.z );
				}
			
			}
			else
			{
				//Round to two decimal places to avoid small rounding errors
				float delta = Mathf.Round( (transform.position.z - currentTilePos.z) * 100.0f) / 100.0f;
				if( delta < -LANE_WIDTH )
				{
					//Debug.LogWarning ( "+LANE_WIDTH player z " + transform.position.z + " currentTilePos.z " + currentTilePos.z + " playerRotationY " + playerRotationY + " currentTile.name " + currentTile.name );
					transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z - LANE_WIDTH );
				}
				else if( delta > LANE_WIDTH )
				{
					//Debug.LogWarning ( "+LANE_WIDTH player z " + transform.position.z + " currentTilePos.z " + currentTilePos.z + " playerRotationY " + playerRotationY + " currentTile.name " + currentTile.name );
					transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z + LANE_WIDTH );
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
				//Remove any leaning when you land
				lean( 0, 0.15f );
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
					jump();
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
			xVector =  Vector3.ClampMagnitude(xVector, Mathf.Abs(getMaxDist(moveDirection.x)));
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
				maxDist = currentTilePos.x + LANE_WIDTH - transform.position.x;
			}
			else
			{
				maxDist = currentTilePos.x - LANE_WIDTH - transform.position.x;
			}
			
		}
		//Player is facing right.
		else if( playerRotationY == 90f || playerRotationY == -270f )
		{
			if( totalX >=0 )
			{
				maxDist = currentTilePos.z - LANE_WIDTH - transform.position.z;
			}
			else
			{
				maxDist = currentTilePos.z + LANE_WIDTH - transform.position.z;
			}
		}
		//Player is facing left.
		else if( playerRotationY == -90f || playerRotationY == 270f )
		{
			if( totalX >=0 )
			{
				maxDist = currentTilePos.z + LANE_WIDTH - transform.position.z;
			}
			else
			{
				maxDist = currentTilePos.z - LANE_WIDTH - transform.position.z;
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
	/// Makes the player jump.
	/// </summary>
	public void jump()
	{
		if( jumping )
		{
			//Delay the second jump request until we are on the ground
			//Cancel any slide queue since we can only queue one movement at a time
			queueJump = true;
			queueSlide = false;
		}
		else
		{
			//Player is not jumping.
			//Allow jump if the player is either near the ground or above ground marked as Collapsing.
			//The Collapsing tag is used in the CollapsingBridge code.
			if ( distanceToGround < 0.5f || playerCollisions.getGroundType() == "Collapsing" )
			{
				//Hack - put moveDirection.x to zero in case finalizeSideMove was never called because of a collision
				moveDirection.x = 0;
				isChangingLanes = false;
				
				//Make sure the lane data is correct in case a collision forced us out of our lane
				recalculateCurrentLane();
	
				//We are allowed to jump from the slide state.
				playerSounds.stopAudioSource();							//stop the sliding sound if any
				playerVisuals.playDustPuff( false );					//stop the dust puff that loops while we are sliding
				playerVisuals.playWaterSplashWhileSliding( false );
				//We are jumping. Reset the collider to vertical.
				changeColliderAxis( Axis.Y );

				jumping = true;
				jumpStarted = true;
	
				setCharacterState( PlayerCharacterState.Jumping );
				moveDirection.y = jumpSpeed;
				setAnimationTrigger(JumpTrigger);
				//for debugging
				//remove jump sound for now because it is annoying
				//playSound( jumpingSound, false );
			}
		}
	}

	/// <summary>
	/// Makes the player do a higher double-jump.
	/// </summary>
	/// <param name="doubleJumpSpeed">Double jump speed.</param>
	public void doubleJump( float doubleJumpSpeed = 0 )
	{
		if( jumping )
		{
			//Delay the second jump request until we are on the ground
			//Cancel any slide queue since we can only queue one movement at a time
			queueJump = true;
			queueSlide = false;
		}
		else
		{
			//Player is not jumping.
			//Allow jump if the player is either near the ground or above ground marked as Collapsing.
			//The Collapsing tag is used in the CollapsingBridge code.
			if ( distanceToGround < 0.5f || playerCollisions.getGroundType() == "Collapsing" )
			{
				//Hack - put moveDirection.x to zero in case finalizeSideMove was never called because of a collision
				moveDirection.x = 0;
				isChangingLanes = false;

				//Make sure the lane data is correct in case a collision forced us out of our lane
				recalculateCurrentLane();
	
				//We are allowed to jump from the slide state.
				playerSounds.stopAudioSource();							//stop the sliding sound if any
				playerVisuals.playDustPuff( false );					//stop the dust puff that loops while we are sliding
				playerVisuals.playWaterSplashWhileSliding( false );
	
				jumping = true;
				jumpStarted = true;
	
				setCharacterState( PlayerCharacterState.DoubleJumping );
				moveDirection.y = doubleJumpSpeed;
				setAnimationTrigger(DoubleJumpTrigger);
				//for debugging
				//remove jump sound for now because it is annoying
				//playSound( jumpingSound, false );
			}
		}
	}
	#endregion

	#region Fall and Land
	public void fall( bool isRespawning = false )
	{
		if( playerCharacterState == PlayerCharacterState.Falling || playerCharacterState == PlayerCharacterState.Jumping || playerCharacterState == PlayerCharacterState.DoubleJumping ) return; //ignore, we are already falling or jumping

		//You can't queue a slide or jump when falling
		queueSlide = false;
		queueJump = false;

		//Give a little downward impetus
		moveDirection.y = -1f;
		//Remember at what height the player started to fall because this will help us calculate the fall distance.
		fallStartYPos = transform.position.y;
		gravity = DEFAULT_GRAVITY * 2f;
		playerCamera.heightDamping = PlayerCamera.DEFAULT_HEIGHT_DAMPING * 9f;
		setCharacterState(PlayerCharacterState.Falling);
		if( isRespawning )
		{
			setAnimationTrigger(RespawnTrigger);
		}
		else
		{
			setAnimationTrigger(FallTrigger);
		}
		//playSound( fallingSound, false );
		//print ( "fall started " + distanceToGround + " " + MIN_DISTANCE_FOR_FALL + " " + name );
	}

	public void land()
	{
		recalculateCurrentLane();
		//Reset values that we changed in the fall() method
		playerVisuals.playDustPuff( true, false );
		playerCamera.lockCamera( false );
		gravity = DEFAULT_GRAVITY;
		playerCamera.heightDamping = PlayerCamera.DEFAULT_HEIGHT_DAMPING;
		setCharacterState( PlayerCharacterState.Running );
		float fallDistance = fallStartYPos - transform.position.y;
		//Was I dead before?
		if( deathType != DeathType.Alive )
		{
			//The player was dead.
			//We have a different Land animation that plays when you respawn that is more heroic.
			setAnimationTrigger(LandAfterRespawnTrigger);
			resurrectEnd();
		}
		else
		{
			//The player was alive
			//If he fell a short distance, play the normal Land animation, else make the character Stumble.
			if( fallDistance < DISTANCE_FOR_LAND_ANIMATION )
			{
				setAnimationTrigger(LandTrigger);
			}
			else
			{
				setAnimationTrigger(StumbleTrigger);
			}
		}
		//print ( "player landed. Fall distance was: " + 	fallDistance );
	}

	void calculateFallDistance()
	{
		if( playerCharacterState == PlayerCharacterState.Falling )
		{
			//Calculate the fall distance
			float fallDistance = fallStartYPos - transform.position.y;
			if( fallDistance > FALL_TO_DEATH_DISTANCE )
			{	
				//Regardless of whether this player isMine or not (and this is why we don't call killPlayer instead), if he has fallen more than FALL_TO_DEATH_DISTANCE, 
				//kill him so that he can respawn.
				//Because of network latency, there is always a delay to receive an RPC.
				//In addition, multiple collision or trigger events can be sent by the physics engine.
				//In order to avoid sending the playerDiedRPC multiple times, set the player state immediately to DYING.
				setCharacterState( PlayerCharacterState.Dying );
				Debug.LogError( name + " fell a big distance so we're forcing the player to respawn-Fall distance: " + fallDistance + " Current tile: " + currentTile.name + " State: " + playerCharacterState );
				HUDMultiplayer.hudMultiplayer.activateUserMessage( "Your player fell through the ground on tile " + currentTile.name + ". Respawing in safe location.", 0, 3f );
				photonView.RPC( "playerDiedRPC", PhotonTargets.AllViaServer, DeathType.GreatFall, currentTile.name );
			}
		}
	}
	#endregion

	#region Sliding
	public void startSlide()
	{
		if( playerCharacterState != PlayerCharacterState.Falling )
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
				if( playerCharacterState != PlayerCharacterState.Sliding && playerCharacterState != PlayerCharacterState.Turning_and_sliding )
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

					setAnimationTrigger(SlideDownTrigger);
				}
			}
		}
	}

	void verifySlide()
	{
		if ( playerCharacterState == PlayerCharacterState.Sliding || playerCharacterState == PlayerCharacterState.Turning_and_sliding )
		{
			//For as long as we have a collider above the character's head, prolong the slide duration if this option is enabled.
			if ( checkForColliderAbove && checkAbove() )
			{
				slideStartTime = slideStartTime + Time.deltaTime;
				
			}
			if ( (Time.time - slideStartTime) > slideDuration )
			{
				//We are stopping sliding
				playerVisuals.playDustPuff( false );
				playerVisuals.playWaterSplashWhileSliding( false );
				setCharacterState( PlayerCharacterState.Running );
				setAnimationTrigger(SlideUpTrigger);
				//Reset capsule collider to upright position
				changeColliderAxis( Axis.Y );
			}
		}
	}	

	/// <summary>
	/// Returns true if there is a collider above the player's head within COLLIDER_ABOVE_DISTANCE.
	/// </summary>
	/// <returns><c>true</c>, if there is a collider, <c>false</c> otherwise.</returns>
	bool checkAbove()
	{
		//This method is used while sliding only. Therefore the axis collider will be Axis.Z.
		//Start the raycast in the center of the collider.
		//By starting from within the center of the collider, the raycast will not collide with this player.
		Vector3 rayCastStartPosition = new Vector3( transform.position.x,  transform.position.y + capsuleCollider.radius, transform.position.z );
		float rayCastLength = COLLIDER_ABOVE_DISTANCE + capsuleCollider.radius;
		//For debugging.
		//Debug.DrawLine(rayCastStartPosition, rayCastStartPosition + Vector3.up * rayCastLength, Color.magenta );
        return (Physics.Raycast(rayCastStartPosition, Vector3.up, rayCastLength ));
	}
	#endregion

	#region Turnining and changing lanes

	void turnCorner( bool isGoingRight )
	{
		if ( playerCharacterState == PlayerCharacterState.Running || playerCharacterState == PlayerCharacterState.Jumping || playerCharacterState == PlayerCharacterState.DoubleJumping || playerCharacterState == PlayerCharacterState.Sliding || playerCharacterState == PlayerCharacterState.Stumbling )
		{
			this.isGoingRight = isGoingRight;

			//Hack - put moveDirection.x to zero in case finalizeSideMove was never called because of a collision
			moveDirection.x = 0;
			isChangingLanes = false;

			//Make sure the lane data is correct in case a collision forced us out of our lane
			recalculateCurrentLane();

			//You can only turn once
			if (!deadEndTurnDone)
			{
				float sideMoveInitiatedZ = getTurnInitiatedPosition();

				//We have 3 cases:
				//Case 1: Player turned early or normally
				//What we want to do: wait until player reaches the next available lane (left, center of right) and then turn

				//Case 2: Player turned late
				//What we want to do: turn immediately

				//Case 3: Player turned too late
				//What we want to do: kill the player. He missed the turn.

				//Case 3: Player turned too late, we want to kill him
				if ( sideMoveInitiatedZ > 2f )
				{	
					Debug.LogWarning("turnCorner: game over - player turned too late." );
					killPlayer ( DeathType.Turned_Too_Late );
					return;
				}

				//Case 2: Player turned late, we want to turn now
				if ( sideMoveInitiatedZ > LANE_WIDTH && sideMoveInitiatedZ < 2f )
				{	
					if ( isGoingRight )
					{
						//Verify if the player is doing a side-move in an allowed direction
						if (currentDeadEndType == DeadEndType.Right )
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
						if (currentDeadEndType == DeadEndType.Left )
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
				if ( sideMoveInitiatedZ < LANE_WIDTH )
				{	
					if ( isGoingRight )
					{
						//Verify if the player is doing a side-move in an allowed direction
						if (currentDeadEndType == DeadEndType.Right )
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
						if (currentDeadEndType == DeadEndType.Left )
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

		playerSounds.playSideMoveSound();

		float playerRotY = transform.eulerAngles.y;
		
		if ( isGoingRight )
		{
			transform.rotation = Quaternion.Euler( 0,playerRotY + 90f,0 );
			lean( 0.55f, 0.3f );
		}
		else
		{
			transform.rotation = Quaternion.Euler( 0,playerRotY - 90f,0 );
			lean( -0.55f, 0.3f );
		}

		if( playerCharacterState == PlayerCharacterState.Turning_and_sliding )
		{
			setCharacterState( PlayerCharacterState.Sliding );
		}
		else
		{
			setCharacterState( PlayerCharacterState.Running );
		}
		Invoke( "cancelLeaningAfterTurn", 0.3f );
		//Debug.Log ("turnNow completed " + isGoingRight + " " + transform.eulerAngles.y + " " + playerCharacterState );

	}

	/// <summary>
	/// Lean left or right for the specified duration.
	/// leaningEndBlendFactor is a value between -1 (full lean left) and +1 (full lean right) with 0 meaning there is no leaning.
	/// </summary>
	/// <param name="leaningEndBlendFactor">Leaning end blend factor.</param>
	/// <param name="duration">Duration.</param>
	void lean( float leaningEndBlendFactor, float duration )
	{
		if( changeLeaningBlendFactorCoroutine != null ) StopCoroutine( changeLeaningBlendFactorCoroutine );
		changeLeaningBlendFactorCoroutine = StartCoroutine( changeLeaningBlendFactor( leaningEndBlendFactor, duration ) );
	}

	IEnumerator changeLeaningBlendFactor( float leaningEndBlendFactor, float duration )
	{
		float elapsedTime = 0;
		
		float leaningStartBlendFactor = anim.GetFloat( leaningBlendFactor );
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			anim.SetFloat( leaningBlendFactor, Mathf.Lerp( leaningStartBlendFactor, leaningEndBlendFactor, elapsedTime/duration ) );
			yield return new WaitForEndOfFrame();  
			
		} while ( elapsedTime < duration );
		anim.SetFloat( leaningBlendFactor, leaningEndBlendFactor );
	}

	void cancelLeaningAfterTurn()
	{
		lean( 0, 0.3f );
	}

	//move the player to the left lane if false,
	//and to the right lane if true
	void changeLane( bool isGoingRight )
	{
		//You can only change lanes while running.
		//You can also change your mind and change direction.
		if ( playerCharacterState == PlayerCharacterState.Running || ( isChangingLanes && this.isGoingRight != isGoingRight ) )
		{
			this.isGoingRight = isGoingRight;

			if( isGoingRight )
			{
				lean( 0.25f, 0.3f );
			}
			else
			{
				lean( -0.25f, 0.3f );
			}

			//Hack - put moveDirection.x to zero in case finalizeSideMove was never called because of a collision
			moveDirection.x = 0;
			isChangingLanes = false;

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
					isChangingLanes = true;
					moveDirection.x = currentSideMoveSpeed;
					playerSounds.playSideMoveSound();
					//Debug.Log ("changeLane completed " + isGoingRight + " to lane " + desiredLane + " current lane " + currentLane );

				}
				else
				{
					desiredLane = Lanes.Left;
					isChangingLanes = true;
					moveDirection.x = -currentSideMoveSpeed;
					playerSounds.playSideMoveSound();
					//Debug.LogError ("changeLane completed " + isGoingRight + " to lane " + desiredLane + " current lane " + currentLane );
				}
			}
			else if ( currentLane == Lanes.Right && !isGoingRight )
			{
				desiredLane = Lanes.Center;
				isChangingLanes = true;
				moveDirection.x = -currentSideMoveSpeed;
				playerSounds.playSideMoveSound();
				//Debug.Log ("changeLane completed " + isGoingRight + " to lane " + desiredLane + " current lane " + currentLane );
			}
			else if ( currentLane == Lanes.Left && isGoingRight )
			{
				desiredLane = Lanes.Center;
				isChangingLanes = true;
				moveDirection.x = currentSideMoveSpeed;
				playerSounds.playSideMoveSound();
				//Debug.Log ("changeLane completed " + isGoingRight + " to lane " + desiredLane + " current lane " + currentLane );
			}
		}
	}

	public void stopSideMove()
	{
		isChangingLanes = false;
		lean( 0, 0.3f );
		moveDirection.x = 0;
		recalculateCurrentLane();
	}

	void finalizeSideMove()
	{
		isChangingLanes = false;
		lean( 0, 0.3f );
		moveDirection.x = 0;
		currentLane = desiredLane;
	}

	//SetDesiredlane is only used while turning.
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

		if (sideMoveInitiatedZ < -LANE_WIDTH )
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
			changeLane ( isGoingRight );
		}
	}

	void verifyIfDesiredLaneReached()
	{
		if ( isChangingLanes )
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
							transform.position = new Vector3 (  currentTilePos.x + LANE_WIDTH, transform.position.y, transform.position.z );
							finalizeSideMove();
						}
					
					}
					//Facing right
					else if( playerRotationY == 90f ||  playerRotationY == -270f )
					{
						if( transform.position.z - currentTilePos.z <= -adjustedLaneLimit )
						{
							transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z - LANE_WIDTH );
							finalizeSideMove();
						}
					}
					//Facing left
					else
					{
						if( transform.position.z - currentTilePos.z >= adjustedLaneLimit )
						{
							transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z + LANE_WIDTH );
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
							transform.position = new Vector3 (  currentTilePos.x -LANE_WIDTH, transform.position.y, transform.position.z );
							finalizeSideMove();
						}
					
					}
					//Facing right
					else if( playerRotationY == 90f ||  playerRotationY == -270f )
					{
						if( transform.position.z - currentTilePos.z >= adjustedLaneLimit )
						{
							transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z + LANE_WIDTH );
							finalizeSideMove();
						}
					}
					//Facing left
					else
					{
						if( transform.position.z - currentTilePos.z <= -adjustedLaneLimit )
						{
							transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z - LANE_WIDTH );
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


	//Called every frame to ensure that the critical current lane value is always accurate
	public void recalculateCurrentLane()
	{
		float relativePos = 0;
		Lanes calculatedLane = currentLane;
		float playerYRotation = Mathf.Floor ( transform.eulerAngles.y );

		if( playerYRotation == 0 )
		{
			//X axis
			relativePos = transform.position.x - currentTilePos.x;	
		}
		else if( playerYRotation == 90f || playerYRotation == -270f )
		{
			//Z axis facing left
			relativePos = currentTilePos.z - transform.position.z;
		}
		else if( playerYRotation == -90f || playerYRotation == 270f )
		{
			//Z axis facing right
			relativePos = transform.position.z - currentTilePos.z;
		}

		if( relativePos > LANE_LEFT_BOUNDARY && relativePos < LANE_RIGHT_BOUNDARY )
		{
			calculatedLane = Lanes.Center;
		}
		else if( relativePos <= LANE_LEFT_BOUNDARY )
		{
			calculatedLane = Lanes.Left;
		}
		else if( relativePos >= LANE_RIGHT_BOUNDARY )
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
	public void attach_to_zipline_completed ( AnimationEvent eve )
	{
		//Have the hero say something on the zipline
		playerVoiceOvers.playVoiceOver( VoiceOverType.VO_Zipline );
	}

	public void attachToZipline()
	{
		//The player might die in the short time between jumping to catch the zipline and the zipline actually starting.
		//Remember, the zipline is activated by a lockstep.
		//If the player is dead, simply ignore the request to attach to the zipline.
		if( getCharacterState() == PlayerCharacterState.Dying ) return;

		SegmentInfo si = currentTile.GetComponent<SegmentInfo>();
		if( si != null )
		{
			//Store the ziplineExitAngle for later
			if( currentTile.GetComponent<Zipline>() == null )
			{
				Debug.LogError("PlayerControl-attachToZipline error-The current tile " + currentTile.name + " for " + name + " doesn't have a Zipline component." );
			}
			ziplineExitAngle = currentTile.GetComponent<Zipline>().ziplineExitAngle;

			float ziplineDuration = currentTile.GetComponent<Zipline>().ziplineDuration;

			List<SegmentInfo.BezierData>  curveList = si.curveList;
			SegmentInfo.BezierData bezierData = curveList[0];
			setCharacterState( PlayerCharacterState.Ziplining );
			enablePlayerControl( false );
			enablePlayerMovement( false );
			//It is possible for a change lane to be in progress when we attach to the zipline.
			//To avoid drifting along the X axis when we detach from the zipline, make sure we reset moveDirection.
			moveDirection = Vector3.zero;
			isChangingLanes = false;
			capsuleCollider.attachedRigidbody.isKinematic = true;
			setAnimationTrigger(AttachToZiplineTrigger);
			ziplineAttachPoint = transform.Find("Zipline Attach Point");
			ziplineAttachPoint.localPosition = ziplinePlayerOffset;
			ziplineAttachPoint.localEulerAngles = new Vector3( 0, 0, 0 );
			ziplineAttachPoint.SetParent(null);
			transform.SetParent( ziplineAttachPoint );
			transform.eulerAngles =  new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,6f);
			//A set of points that define one or many bezier paths (the paths should be passed in multiples of 4, which correspond to each individual bezier curve)
			//It goes in the order: startPoint,endControl,startControl,endPoint
			LTBezierPath ltBezier = new LTBezierPath( new Vector3[] { bezierData.bezierStart.position, bezierData.bezierControl2.position, bezierData.bezierControl1.position, bezierData.bezierEnd.position } );
			LeanTween.move(ziplineAttachPoint.gameObject, ltBezier.pts, ziplineDuration).setOrientToPath(true).setEase(LeanTweenType.easeOutQuad);
			playerCamera.playCutscene(CutsceneType.Ziplining);
			playerSounds.playZiplineSound();
		}
	}

	public void detachFromZipline()
	{
		if( getCharacterState() != PlayerCharacterState.Ziplining )
		{
			Debug.LogError( name + " PlayerControl-detachFromZipline: the player is NOT in the ziplining state. He is in this state: " + getCharacterState() );
			HUDMultiplayer.hudMultiplayer.activateUserMessage( "Hmmm, your player should be detaching from the zipline, but he isn't in the ziplining state. He is in this state: " + getCharacterState(), 0, 3.5f );
		}

		LeanTween.cancel( gameObject );
		transform.SetParent( null );
		if( ziplineAttachPoint == null ) Debug.LogError( "PlayerControl-detachFromZipline: the zipline attach point for " + name + " is null." );
		ziplineAttachPoint.SetParent( transform, false );
		ziplineAttachPoint.localScale = new Vector3( 1f, 1f, 1f ); 	//Just because of rounding when changing parent
		playerCamera.reactivateMaincamera();
		capsuleCollider.attachedRigidbody.isKinematic = false;
		transform.localScale = new Vector3( 1f, 1f, 1f ); 	//Just because of rounding when changing parent
		//The player might have died while ziplining.
		//We only do the last steps if the player is alive.
		if( getCharacterState() != PlayerCharacterState.Dying )
		{
			enablePlayerControl( true );
			enablePlayerMovement( true );
			transform.eulerAngles = new Vector3( 0, ziplineExitAngle, 0 );
			//We may have switched lanes because of the position change. Make sure the lane values are accurate.
			recalculateCurrentLane();
			fall();
		}
		else
		{
			//If the player is dead, it looks nicer with the camera locked.
			playerCamera.lockCamera( true );
		}
		playerSounds.stopAudioSource();
	}
	#endregion

	#region Animation
	public void playVictoryAnimation()
	{
		anim.applyRootMotion = true;
		setAnimationTrigger( WinTrigger );
	}

	public void playLoseAnimation()
	{
		anim.applyRootMotion = true;
		setAnimationTrigger( LoseTrigger );
	}

	public void playOmniToolAnimation( CardName cardName )
	{
		//Old skins (Hero_prefab and dHeroine_prefab) don't have the OmniTool animation. Therefore the OmniTool_completed event will never be received.
		//So for old skins, directly call OmniTool_completed.
		if( hasOmniToolAnimation )
		{
			print("playOmniToolAnimation for " + name + " Card " + cardName );
			//Store this value. We will use it when the OmniTool animation completes.
			activeCardName = cardName;
			setAnimationTrigger( OmniToolTrigger );
		}
		else
		{
			activeCardName = cardName;
			OmniTool_completed ( null );
		}
	}

	public void OmniTool_completed ( AnimationEvent eve )
	{
		print("OmniTool_completed-playing card: " + activeCardName + " for " + name );
		if( playerAI == null )
		{
			turnRibbonHandler.activateCard( activeCardName );
		}
		else
		{
			GetComponent<BotCardHandler>().activateCard( activeCardName );
		}
		activeCardName = CardName.None;
	}

	void setAnimationTrigger( int animationTrigger )
	{
		anim.SetTrigger( animationTrigger );
	}
	#endregion

	#region Player Death
	/// <summary>
	/// This is the official way to kill the player. Do not call playerDiedRPC directly.
	/// This method verifies that isMine is true before sending the playerDiedRPC.
	/// The playerDiedRPC call is sent to All.
	/// </summary>
	/// <param name="deathTypeValue">Death type value.</param>
	public void killPlayer( DeathType deathTypeValue )
	{
		//Only proceed if not dying already
		if ( photonView.isMine && playerCharacterState != PlayerCharacterState.Dying )
		{
			//Added test for debugging
			if ( playerCharacterState == PlayerCharacterState.Ziplining )
			{
				Debug.LogError( name + " PlayerControl-killPlayer was called while player was ziplining: " + deathTypeValue + ". Ignoring." );
			}
			else
			{
				//Because of network latency, there is always a delay to receive an RPC.
				//In addition, multiple collision or trigger events can be sent by the physics engine.
				//In order to avoid sending the playerDiedRPC multiple times, set the player state immediately to DYING.
				Debug.Log( name + " PlayerControl-killPlayer: " + deathTypeValue + " " + playerCharacterState );
				setCharacterState( PlayerCharacterState.Dying );
				photonView.RPC("playerDiedRPC", PhotonTargets.AllViaServer, deathTypeValue, currentTile.name );
			}
		}
	}

	//Skill bonus tracking
	public void incrementKillCounter()
	{
		if ( photonView.isMine && !GameManager.Instance.isCoopPlayMode() ) doubleKillEventCounter.incrementCounter();
	}

	[PunRPC]
	void playerDiedRPC( DeathType deathTypeValue, string resurrectOnThisTile )
	{

		this.resurrectOnThisTile = resurrectOnThisTile;
		
		changeColliderAxis( Axis.Z );
		ignorePlayerCollisions( true );

		Debug.Log("playerDiedRPC : " + deathTypeValue + " " + gameObject.name );

		if( deathTypeValue != DeathType.NO_MORE_HEALTH ) playerHealth.deductAllHealth();

		//Update the player statistics		
		if( this.photonView.isMine && playerAI == null )
		{
			GameManager.Instance.playerStatistics.incrementNumberOfDeathsLifetime();
		
			//Increment the number of times we died during this race
			numberOfTimesDiedDuringRace++;
		}

		//Remember how we died
		deathType = deathTypeValue;

		//Disable the player's shadow
		playerVisuals.enablePlayerShadow( false );

		anim.speed = 1f;

		//Change character state
		setCharacterState( PlayerCharacterState.Dying );

		//The PlayerSpell component needs to know that the player died
		playerSpell.playerDied();

		//The PlayerIK component needs to know that the player died
		if( playerIK !=null ) playerIK.playerDied();

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
		playerRace.playerDied();

		//Make adjustments depending on death type
	    switch (deathType)
		{	                
	        case DeathType.Enemy:
				StartCoroutine( waitBeforeResurrecting(DELAY_BEFORE_RESURRECTING) );	
				break;
	                
			case DeathType.Zombie:
				//Play collision sound
				playerSounds.playDyingSound();
				playerVisuals.playImpactVFX(); //The impact VFX has a sound associated with it
				setAnimationTrigger(FallBackwardTrigger);
				break;
		
			case DeathType.Flame:
				setAnimationTrigger(FallBackwardTrigger);
				break;
	                
	        case DeathType.Obstacle:
				//Play collision sound
				playerCamera.Shake();
				playerVisuals.playImpactVFX(); //The impact VFX has a sound associated with it
				setAnimationTrigger(FallBackwardTrigger);
				break;

	        case DeathType.FallForward:
				playerCamera.Shake();
				setAnimationTrigger(FallForwardTrigger);
				break;

	        case DeathType.GreatFall:
				StartCoroutine( waitBeforeResurrecting(DELAY_BEFORE_RESURRECTING) );
				break;

			default:
				setAnimationTrigger(FallBackwardTrigger);
				break;
		}
		if( GameManager.Instance.isCoopPlayMode() ) coopWaveGenerator.playerDied( transform );

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
			if( PlayerRace.players[i] != playerRace )
			{
				//print("Changing collision with: " +  PlayerRace.players[i].name + " Collision is ignored: " + ignore );
				Physics.IgnoreCollision(capsuleCollider, PlayerRace.players[i].GetComponent<Collider>(), ignore );
			}
		}
	}

	public void fall_backward_completed ( AnimationEvent eve )
	{
		if( this.photonView.isMine && playerAI == null && !GameManager.Instance.isCoopPlayMode() ) StartCoroutine( controlVignetting( 0.25f, 0.7f, 1f ) );
		if( ragdoll != null ) ragdoll.controlRagdoll( true );
		StartCoroutine( waitBeforeResurrecting(DELAY_BEFORE_RESURRECTING) );
		//The fall backward animation moves the  mesh of the body outside of the capsule collider.
		//This is why we change the capsule collider center. If we don't do this, zombies can walk through the player's body.
		capsuleCollider.center = new Vector3( controllerOriginalCenter.x, PRONE_CAPSULE_CENTER_Y, PRONE_CAPSULE_CENTER_Z );
	}

	public void fall_forward_completed ( AnimationEvent eve )
	{
		if( this.photonView.isMine && playerAI == null && !GameManager.Instance.isCoopPlayMode() ) StartCoroutine( controlVignetting( 0.25f, 0.7f, 1f ) );
		if( ragdoll != null ) ragdoll.controlRagdoll( true );
		StartCoroutine( waitBeforeResurrecting(DELAY_BEFORE_RESURRECTING) );
	}

	IEnumerator waitBeforeResurrecting ( float duration )
	{
		if( !GameManager.Instance.isCoopPlayMode() )
		{
			//Resurrect at the next lock step.
			yield return new WaitForSeconds(duration);
			LockstepManager.Instance.addActionToQueue( new LockstepManager.LockstepAction( LockstepActionType.RESURRECT, gameObject ) );
		}
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
			yield return new WaitForEndOfFrame();  
			
		} while ( elapsedTime < duration );
		
		vAcA.intensity = endVignettingFactor;
		vAcA.blur = endBlurFactor;
		if( vAcA.intensity == 0 ) vAcA.enabled = false;
	}

	public void coopResurrectBegin( string resurrectOnThisTile )
	{
		//Also stop the spectating.
		if( photonView.isMine && playerAI == null )
		{
			CinemachineVirtualCamera cmvc = GameObject.FindGameObjectWithTag("Main Virtual Camera").GetComponent<CinemachineVirtualCamera>();
			cmvc.m_Follow = transform;
			cmvc.m_LookAt = transform;
			HUDMultiplayer.hudMultiplayer.displayTopMessage( string.Empty );
		}
		this.resurrectOnThisTile = resurrectOnThisTile;
		resurrectBegin();
	}

	//LockstepManager is in charge of calling this. Do not call directly.
	public void resurrectBegin()
	{
		playerCamera.setCameraParameters( 18f, PlayerCamera.DEFAULT_DISTANCE, PlayerCamera.DEFAULT_HEIGHT, PlayerCamera.DEFAULT_Y_ROTATION_OFFSET );
		playerCamera.activateMainCamera();
		playerCamera.positionCameraNow();
		playerCamera.resetCameraParameters();

		if( this.photonView.isMine && playerAI == null && !GameManager.Instance.isCoopPlayMode() ) StartCoroutine( controlVignetting( 0f, 0f, 0f ) );

		//Only send an event if we are the local player and we are not a bot.
		if( this.photonView.isMine && playerAI == null ) GameManager.Instance.setGameState( GameState.Resurrect );

		//Reset data
		resetSharedLevelData(true);

		if( ragdoll != null ) ragdoll.controlRagdoll( false );
		
		//Reposition dead body at the respawn location.
		//Don't use the currentTile which is local since it may be out of sync.
		//Use the name of the tile sent when the player died.
		GameObject resurrectOnThisTileGameObject = GameObject.Find( resurrectOnThisTile );

		GameObject respawnLocationObject = resurrectOnThisTileGameObject.transform.Find("respawnLocation").gameObject;

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
			transform.SetParent( resurrectOnThisTileGameObject.transform );
			transform.localRotation = Quaternion.Euler ( 0, respawn.localEulerAngles.y, 0 );
			transform.SetParent( null );
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
			updateCurrentTileInfo ( resurrectOnThisTileGameObject );
			//Make player fall from sky, land and start running again
			enablePlayerMovement( true );
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
		anim.SetFloat( leaningBlendFactor, 0 );
		jumping = false;
		jumpStarted = false;
		queueSlide = false;
		queueJump = false;
		isInDeadEnd = false;
		deadEndTrigger = null;
		isChangingLanes = false;
		wantToTurn = false;
		deadEndTurnDone = false;
		if( unlockCamera ) playerCamera.lockCamera ( false );
		gravity = DEFAULT_GRAVITY;
		moveDirection = new Vector3(0,0,0);
		playerCamera.heightDamping = PlayerCamera.DEFAULT_HEIGHT_DAMPING;
		playerHealth.resetHealth();
		anim.Rebind(); //Not sure useful. Added back to see if it fixes the random bug where the player falls in T-position after respawning.

	}

	//Called when a player lands after respawn.
	private void resurrectEnd()
	{		
		//Display a minimap message that this player or bot is back in the game.
		string userName;
		if( playerAI == null )
		{
			//We're the player
			userName = GameManager.Instance.playerProfile.getUserName();
		}
		else
		{
			//We're a bot
			userName = 	playerAI.botHero.userName;
		}
		this.photonView.RPC("playerResurrectedRPC", PhotonTargets.AllViaServer, userName );


		//The GameState was Resurrect - change it back to Normal
		if( this.photonView.isMine && playerAI == null ) GameManager.Instance.setGameState(GameState.Normal);

		//Restore player controls
		enablePlayerControl( true );
		deathType = DeathType.Alive;
		//Display a Go! message
		if( this.photonView.isMine && playerAI == null ) HUDMultiplayer.hudMultiplayer.activateUserMessage( LocalizationManager.Instance.getText("GO"), 0f, 1.25f );
	}

	[PunRPC]
	public void playerResurrectedRPC( string userName )
	{
		MiniMap.Instance.displayMessage( string.Format( backInTheGameString, userName ) );
		playerVoiceOvers.playVoiceOver(VoiceOverType.VO_Resurrect);
	}

	public int getNumberOfTimesDiedDuringRace()
	{
		return numberOfTimesDiedDuringRace;
	}
	#endregion

	#region Stumble
	/// <summary>
	/// The player stumbles briefly.
	/// A player can only stumble if he is in the Running state.
	/// </summary>
	public void stumble()
	{
		if ( playerCharacterState == PlayerCharacterState.Running )
		{	
			if( playerSpell.isCardActive( CardName.Shrink ) )
			{
				//The player is shrunk. He will never get over the stumble, and worse, he may become stuck.
				//So kill him.
				killPlayer( DeathType.Obstacle );
			}
			else
			{
				//Player is normal size. Simply stumble.
				Debug.Log ("Player stumbled");
				setCharacterState( PlayerCharacterState.Stumbling );
				playerVoiceOvers.playVoiceOver(VoiceOverType.VO_Stumble);
				//The player stumbles but recovers
				setAnimationTrigger(StumbleTrigger);
			}
		}
	}

	public void stumble_completed ( AnimationEvent eve )
	{
		if( getCharacterState() != PlayerCharacterState.Dying ) setCharacterState( PlayerCharacterState.Running );
	}
	#endregion

	public PlayerCharacterState getCharacterState()
	{
		return playerCharacterState;
	}

	public void setCharacterState( PlayerCharacterState newState )
	{
		//if( playerAI == null ) Debug.Log( name + "PlayerControl-setCharacterState to: " + newState + " previous state was " + playerCharacterState );
		playerCharacterState = newState;
		//Send an event to interested classes if you are a human player
		if(multiplayerStateChanged != null && this.photonView.isMine ) multiplayerStateChanged( transform, playerCharacterState );
		
		playerRun.handlePlayerStateChange( playerCharacterState );
		playerVisuals.handlePlayerStateChange( playerCharacterState );
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
			isInDeadEnd = true;
			wantToTurn = false;

			currentDeadEndType = other.GetComponent<deadEnd>().deadEndType;
			deadEndTrigger = other;

			//Auto-turn
			bool autoTurn = true;
			if( autoTurn && playerAI == null && !photonView.isMine )
			{
				if ( currentDeadEndType == DeadEndType.Left )
				{
					sideSwipe( false );
				}
				else if ( currentDeadEndType == DeadEndType.Right )
				{
					sideSwipe( true );
				}
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
			if( !wasEntranceCrossed )
			{
				SegmentInfo si = other.transform.parent.GetComponent<SegmentInfo>();
				if( si != null )
				{
					//wasEntranceCrossed is used to prevent multiple OnTriggerEnter from occuring.
					wasEntranceCrossed = true;
					StartCoroutine( resetEntranceCrossed() );
					generateLevel.tileEntranceCrossed( other.transform.parent );
					tileIndex = si.tileIndex;
					tileDistanceTraveled = generateLevel.getLevelLength( tileIndex );
					//The distance remaining displayed on the HUD and stored in PlayerRace is equal to: The length of the level - the total distance traveled by the player.
					//The total distance traveled by the player is equal to the distance traveled for all previous tiles plus the distance traveled on the current tile.
					//The distance traveled for all previous tiles is maintained by PlayerControl because it gets updated each time an entrance is crossed.
					//The distance traveled on the current tile is maintained in PlayerRace.
					playerRace.distanceTravelledOnThisTile = 0;
					currentTilePos = si.transform.position;
					currentTile = si.gameObject;
					//Every time a new tile is entered, force synchronization.
					//However, we don't want to force synchronization when entering a turn.
					//Because of the RPC delay, the rotation could end up being wrong.
					//For turns, we will syncronize when the player has successfully exited the turn (deadEnd).
					if( si.tileType != TileType.Left && si.tileType != TileType.Right ) forcePositionSynchronization();
					//If you just entered the End tile reactivate some tiles since the camera does a 180.
					if( si.tileType == TileType.End )
					{
						generateLevel.activateTilesForCamera();
					}
					else
					{
						//If you are in the last position in the race, deactivates the tile with an index of (currentTileIndex - two) to help performance.
						//Race position is not yet reliable, so comment out next line for now.
						//if( playerRace.isInLastPosition() ) generateLevel.deactivatePreviousTile( si.tileIndex );
					}
				}
				else
				{
					Debug.LogError("PlayerControl-OnTriggerEnter: " + other.transform.parent.name + " tile does not have a SegmentInfo component attached to it.");
				}
			}
			else
			{
				Debug.LogError("PlayerControl-OnTriggerEnter: wasEntranceCrossed was enabled by another player. Player named " + name + " will NOT activate entrance crossed code." );
			}
		}
		else if( other.CompareTag( "AttachZiplineTrigger" ) )
		{
			//OnTriggerEnter can get called multiple times. Only attach to the zipline, if the player is not already ziplining and is alive.
			if( photonView.isMine && getCharacterState() != PlayerCharacterState.Ziplining && getCharacterState() != PlayerCharacterState.Dying )
			{
				//It takes a bit of time for the RPC to be received and for the lockstep action to take place.
				//This is why we set the state to Ziplining right away.
				setCharacterState(PlayerCharacterState.Ziplining);
				//Cancel the speedboost if active before ziplining
				playerSpell.cancelRagingBull();
				this.photonView.RPC("attachToZiplineRPC", PhotonTargets.All, transform.position, transform.eulerAngles.y );
			}
		}
 		else if( other.CompareTag( "DetachZiplineTrigger" ) )
		{
			//In Unity, OnTriggerEnter can get called multiple times. Only detach from the zipline, if the player is ziplining.
			if( photonView.isMine && getCharacterState() == PlayerCharacterState.Ziplining ) this.photonView.RPC("detachFromZiplineRPC", PhotonTargets.All, transform.position, transform.eulerAngles.y );
		}
  	}

	IEnumerator resetEntranceCrossed()
	{
		yield return new WaitForFixedUpdate();
		wasEntranceCrossed = false;
	}

	void forcePositionSynchronization()
	{
		if( photonView.isMine && playerAI == null && getCharacterState() != PlayerCharacterState.Dying ) this.photonView.RPC("positionSynchronizationRPC", PhotonTargets.Others, transform.position, transform.eulerAngles.y, capsuleCollider.attachedRigidbody.velocity, PhotonNetwork.time );
	}

	[PunRPC]
	void positionSynchronizationRPC( Vector3 syncPosition, float syncYRotation, Vector3 syncVelocity, double photonTime )
	{
		//Ignore if you died during the RPC transit.
		if( getCharacterState() == PlayerCharacterState.Dying ) return;

		//Discard old packets
		float syncDelay = (float) (PhotonNetwork.time - photonTime);
		if( syncDelay > PACKET_EXPIRY ) return;

		Vector3 newPosition = syncPosition + syncVelocity * syncDelay;

		//Note that MovePosition/MoveRotation are not immediate. They take effect at the next FixedUpdate() call.
		capsuleCollider.attachedRigidbody.MovePosition( newPosition );
		capsuleCollider.attachedRigidbody.MoveRotation( Quaternion.Euler( transform.eulerAngles.x, syncYRotation, transform.eulerAngles.z ) );

		//Determine on which tile we're on.
		//Given an average latency of 60 msec. and an average run speed of 20 m/s, the player has been positioned 1.2 meters further.
		//If we are positioned on a tile sloping upward, we want to make sure to start our raycast quite a bit higher than the feet, hence the 2f.
		//Also, the player can jump quite high with double jump, so let's make sure our raycast is long enough to hit the ground.
		//Ignore Players when raycasting as well as triggers.
		RaycastHit hit;
		if (Physics.Raycast( new Vector3( newPosition.x, newPosition.y + 2f, newPosition.z ), Vector3.down, out hit, 50f, ~(1 << MaskHandler.playerLayer ), QueryTriggerInteraction.Ignore ) )
		{
			GameObject objectUnderRaycast = hit.collider.transform.root.gameObject;

			if( objectUnderRaycast.GetComponent<SegmentInfo>() != null )
			{
				//Good! The object has a SegmentInfo component. That guarantees that this is a tile.
				updateCurrentTileInfo( objectUnderRaycast );
			}
			else
			{
				//The object that the raycast hit is not a tile.
				//Let's start another raycast but this time starting from the hit object. Hopefully, we will hit the tile underneath.
				if (Physics.Raycast( new Vector3( objectUnderRaycast.transform.position.x, objectUnderRaycast.transform.position.y + 0.05f, objectUnderRaycast.transform.position.z ), Vector3.down, out hit, 50f, ~(1 << MaskHandler.playerLayer ), QueryTriggerInteraction.Ignore ) )
				{
					GameObject objectUnderSecondRaycast = hit.collider.transform.root.gameObject;
		
					if( objectUnderSecondRaycast.GetComponent<SegmentInfo>() != null )
					{
						//Good! The object has a SegmentInfo component. That guarantees that this is a tile.
						updateCurrentTileInfo( objectUnderSecondRaycast );
					}
					else
					{
						Debug.LogError( "PlayerControl-positionSynchronizationRPC: The object that the second raycast hit " +  objectUnderSecondRaycast.name + " is not a tile. Unable to set current tile info for " + name + " at position " + newPosition );
					}
				}
				else
				{
					Debug.LogError( "PlayerControl-positionSynchronizationRPC: There is no ground below the player named " + name + " at position " + newPosition + " after second raycast." );
				}
			}
		}
		else
		{
			Debug.LogError( "PlayerControl-positionSynchronizationRPC: There is no ground below the player named " + name + " at position " + newPosition );
		}
		recalculateCurrentLane();
	}

	void updateCurrentTileInfo( GameObject tile )
	{
		currentTile = tile;
		currentTilePos = currentTile.transform.position;
		Debug.LogWarning( name + " updateCurrentTileInfo-Player rotation: " + transform.eulerAngles + " name of current tile: " + currentTile.name ); 

		tileIndex = currentTile.GetComponent<SegmentInfo>().tileIndex;
		tileDistanceTraveled = generateLevel.getLevelLength( tileIndex );

		//In coop, you get resurrected when your partner triggers the next wave which could be a couple of tiles further than where you died.
		//Also, in any multiplayer mode, a force position sync could make the player skip a tile if the lag is high enough.
		//Therefore, make sure subsequent tiles are active since a player may not have activated them by crossing the tile entrance trigger.
		generateLevel.activeNextTiles( tileIndex );

		//Recalculate the distance travelled on this tile. Ignore Y in the distance calculation.
		Transform tileEntrance = currentTile.transform.Find("Entrance");
		Vector3 tileEntrancePosition = new Vector3( tileEntrance.position.x, 0, tileEntrance.position.z );
		Vector3 playerPosition = new Vector3( transform.position.x, 0, transform.position.z );
		float distanceTravelledOnThisTile = Vector3.Distance( tileEntrancePosition, playerPosition );
		playerRace.distanceTravelledOnThisTile = distanceTravelledOnThisTile;
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
				if ( currentZ <= LANE_WIDTH )
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
				if ( currentZ <= -LANE_WIDTH )
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
				if( !deadEndTurnDone )
				{
					Debug.LogWarning("OnTriggerExit player exited dead end without turning " + other.name + " " + isInDeadEnd + " " + deadEndTurnDone + " " + currentDeadEndType );
					killPlayer ( DeathType.Exited_Without_Turning );
				}
				//Reset values
				isInDeadEnd = false;
				deadEndTurnDone = false;
				deadEndTrigger = null;
				wantToTurn = false;
				forcePositionSynchronization();
			}
		}
	}
	#endregion

}
