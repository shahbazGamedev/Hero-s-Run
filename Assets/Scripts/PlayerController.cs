using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;


public sealed class PlayerController : MonoBehaviour {
	
	private Transform mainCamera;

	//Variables for swipe
    float minSwipeDistancePixels;
    bool touchStarted = false;
    Vector2 touchStartPos;
	float minSwipeDistance = 0.022f; //About 17 pixels if width is 768 pixels

	//Audio
	public AudioClip 	slidingSound;
	public AudioClip 	jumpingSound;
	public AudioClip 	sideMoveSound;
	public AudioClip 	dyingSound;
	public AudioClip 	stumblingSound;
	public AudioClip 	flyingSound;
	public AudioClip 	fallingSound;
	public AudioClip 	footstepLeftSound;
	public AudioClip 	footstepRightSound;
	public AudioClip 	footstepWaterSound; //We do not need one for each foot.
	public AudioClip 	landGroundSound;
	public AudioClip 	landWaterSound;
	public AudioClip 	deathFireSound;
	public AudioClip 	snowFootstepLeftSound;
	public AudioClip 	snowFootstepRightSound;
	AudioClip leftFootstep;	//Footsteps sounds to use for current ground type
	AudioClip rightFootstep;

	//Particles
	public ParticleSystem dustPuff;
	public ParticleSystem appearFX; //Called by Teleport_leave_complete

	//Components
	public Animator anim;
	CharacterController controller;
	AudioSource audioSource;

	//When the player dies, we change the center and radius.
	//We need to be able to reset these values when the player is revived.
	Vector3 controllerOriginalCenter;
	float controllerOriginalRadius;


	//Lanes
	public enum Lanes {
		Left = -1,
		Center = 0,
		Right = 1,
	}
	public static float laneLimit = 1.3f;
	//Due to rounding errors, the player may not reach exactly the lane limit. If there is less than 1% of the distance
	//remaining, assume that he did reach the lane limit which will allow us to finalize the side move.
	float adjustedLaneLimit = laneLimit * 0.99f;
	public Lanes currentLane = Lanes.Center;
	public Lanes desiredLane = Lanes.Center;
	int myLane = 0; //0 is uninitialized, 1 is the nearest, 2 is in the center and 3 is the furthest

	//Use to move the player along the X, Y and Z axis
	public Vector3 moveDirection = new Vector3 ( 0,0,0 );
	public enum Swipe{Left,Right,Up,Down};

	//The gravity for the character
	const float DEFAULT_GRAVITY = 16f;
	public float gravity = DEFAULT_GRAVITY;
	public float gravityModifier = 1.8f;
	
	//Movement variables
	//By what percentage should we reduce the run speed during a jump
	const float JUMP_RUN_SPEED_MODIFIER = 0.75f;
	bool playerControlsEnabled = true;
	//True if the player is allowed to move, false otherwise. This flag is useful during camera cut-scenes to prevent the player from moving.
	bool playerMovementEnabled = true;
	public float jumpSpeed = 8.8f;
	public float doubleJumpSpeed = 12.4f;
	public float slopeJumpSpeed = 14f;
	public float sideMoveSpeed = 8f; 
	//when a new level starts or if the player dies and he is revived, he will
	//start running at runStartSpeed.
	float runStartSpeed = 0;
	//The running speed will increase with time to make it harder for the player,
	public static float runSpeed = 0; //NOT USED
	private float newRunSpeed = 0;
	//The run speed at time of death is needed because we want to start running again (in case of revive) at a 
	//percentage of this value.
	float runSpeedAtTimeOfDeath = 0;
	public float runSpeedAtTimeOfTurn;
	public float runSpeedTurnMultiplier;
	public float runSpeedAtTimeOfStumble;
	public float stumbleRunSpeedMultiplier = 0.9f; //If the player stumbles, his speed will be multiplied by this number.

	const float MAX_RUN_SPEED = 42f;
	const float SLOW_DOWN_END_SPEED = 5f;
	//Used to modify the blend amount between Run and Sprint animations based on the current run speed. Also used by the troll.
	public float blendFactor;
	//By what percentage to reduce the player's speed if he is revived.
	//We reduce the speed slightly to give a chance to the player.
	const float SPEED_REDUCTION_ON_REVIVE = 0.75f;
	//The run start speed specified in the level data.
	float levelRunStartSpeed = 0;
	//allowRunSpeedToIncrease is set to false while jumping
	public bool allowRunSpeedToIncrease = true;
	public float runSpeedBeforeJump = -1f;
	//Run acceleration is used to determine how fast the player's run speed
	//will increase. It is specified in the level data. A good value is 0.1f.
	float runAcceleration = 0;
	//When the player is using a slow time power up, this slows down time by a factor of 2.
	//However, in order for the game to not feel sluggish, we use a smaller value called SLOW_DOWN_FACTOR
	//for the accelerometer and when changing lanes.
	const float SLOW_DOWN_FACTOR = 1.5f;

	//Casts a circular shadow at the feet of the player
	Projector shadowProjector;

	PlayerCharacterState playerCharacterState;
	
	// Are we jumping? (Initiated with jump button and not grounded yet)
	public bool jumping = false;
	public bool doingDoubleJump = false;
	
	//Time slide started
	float slideStartTime;
	public float slideDuration = 0.6f; //in seconds
	//when in the air, you can queue a slide request so that you will slide
	//as soon as you touch the ground
	public bool queueSlide = false;

	//when in the air, you can queue a jump request so that you will jump again
	//as soon as you touch the ground
	//You can only queue one move at any given time
	public bool queueJump = false;

	//Used to indicate if the player is in a zone where a turn is required
	public bool isInDeadEnd = false;
	//What type of turn is supported in that deadEnd: Left only, Right only or Left and Right
	DeadEndType currentDeadEndType;
	//Used to indicate whether or not the player turned correctly in a dead end.
	bool deadEndTurnDone = false;
	//The dead end trigger that the player is in
	public Collider deadEndTrigger;
	//Used to indicate that the player wants to turn. The actual turn will be delayed until the player is aligned with a lane.
	public bool wantToTurn = false;
	//Used to indicate whether the player is making a side-move or turn to the right or to the left
	public bool isGoingRight = false;
	
	//This value is used to accelerate the run speed as time goes by
	float timeSessionStarted = 0;

	//This is used to calculate the distance travelled by the player
	Vector3 previousPlayerPosition;
	bool allowDistanceTravelledCalculations = true;

	public float distanceToGround = 0;
	const float MIN_DISTANCE_FOR_FALL = 7f;
	//When the player falls, we start calculating his fall distance. If the fall distance is greater than FALL_TO_DEATH_DISTANCE, then the player will die.
	float fallStartYPos = 0;
	const float FALL_TO_DEATH_DISTANCE = 60f;
	//If the fall distance is less than DISTANCE_FOR_LAND_ANIMATION meters, the hero plays the Land animation; above, he plays the Stumble animation.
	const float DISTANCE_FOR_LAND_ANIMATION = 34f;

	TrollController trollController;
	FairyController fairyController;
	PowerUpManager powerUpManager;

	public PlayerCamera sc;
	GenerateLevel generateLevel;
	
	public static DeathType deathType = DeathType.Alive;

	//For accelerometer
	bool usesAccelerometer = true;
	float accelerometerPreviousFrameX = 0;
	float accelerometerStrength = 22.5f; //Used to be 14
	
	//To move coins to the Coin icon at the top right of the screen
	Vector3 coinScreenPos;

	public Vector3 forward;
	public Vector3 currentTilePos = Vector3.zero;
	public GameObject currentTile;
	public float tileRotationY = 0; //Since we use this value often, we will store it.
	public TileType currentTileType;

	//For bezier curves
	bool usesBezierCurve = false;
	float bezierEndRotationY;
	GameObject dummyObject;
	Transform dummyTransform;
	//A tile can have multiple splines. They are stored in curveList when the player crosses an Entrance.
	List<SegmentInfo.BezierData> curveList;
	//A index pointing to the current curve that the player is travelling on.
	int curveListIndex = 0;
	//Two point along the bezier curve
	Vector3 point1;
	Vector3 point2;
	//The current curve the player is on
	Bezier currentBezierCurve;

	//Event management used to notify other classes when the Hero has been created
	public delegate void LocalPlayerCreated( Transform value, PlayerController playerController );
	public static event LocalPlayerCreated localPlayerCreated;

	//Event management used to notify other classes when the character state has changed
	public delegate void PlayerState( PlayerCharacterState value );
	public static event PlayerState playerStateChanged;
	
	//Event management used to notify other classes when the resurrection begins (for example to hide zombies)
	public delegate void ResurrectionBegin();
	public static event ResurrectionBegin resurrectionBegin;
	
	//Hash IDs for player animations. These are used to improve performance.
	int RunTrigger = Animator.StringToHash("Run");
	int FallTrigger = Animator.StringToHash("Fall");
	int LandTrigger = Animator.StringToHash("Land");
	int Double_JumpTrigger = Animator.StringToHash("Double Jump");
	int JumpTrigger = Animator.StringToHash("Jump");
	int Slide_DownTrigger = Animator.StringToHash("Slide Down");
	int Slide_UpTrigger = Animator.StringToHash("Slide Up");
	int VictoryTrigger = Animator.StringToHash("Victory");
	int DeathRiverTrigger = Animator.StringToHash("DeathRiver");
	int DeathWallTrigger = Animator.StringToHash("DeathWall");
	int StumbleTrigger = Animator.StringToHash("Stumble");
	int FallForwardTrigger = Animator.StringToHash("Fall_Forward");
	int speedBlendFactor = Animator.StringToHash("Speed");
	int lookbackBlendFactor = Animator.StringToHash("Look_Back");
	int Idle_LookTrigger = Animator.StringToHash("Idle_Look");
	int Finish_LineTrigger = Animator.StringToHash("Finish_Line");

	//For debugging swipes
	public string reasonDiedAtTurn;

	public ParticleSystem lavaSpurt; //Plays when player falls into lava
	public string groundType = "normal"; //Other choices are water and collapsing
	public string previousGroundType = "normal"; //Other choices are water and collapsing
	public ParticleSystem slideWaterSplash; //Plays when player slides in water.It loops.

	public GameObject Hero_Prefab;
	public GameObject Heroine_Prefab;

	bool isInZiplineTrigger;
	Transform ziplineAttachPoint;

	void Awake()
	{
		Transform blobShadowProjectorObject = transform.Find("Blob Shadow Projector");
		if( blobShadowProjectorObject == null )
		{
			Debug.LogError("PlayerController-error: Unable to find, Blob Shadow Projector." );
		}
		shadowProjector = blobShadowProjectorObject.GetComponent<Projector>();

		//Calculate the minimum swipe distance in pixels
        float screenDiagonalSize = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
        minSwipeDistancePixels = minSwipeDistance * screenDiagonalSize; 

		//Get a copy of the components
		anim = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();

		loadPlayerSkin();

		controller = GetComponent<CharacterController>();
		controllerOriginalCenter = controller.center;
		controllerOriginalRadius = controller.radius;

		sc = GetComponent<PlayerCamera>();

		//For coins
		Vector2 coinIconPos = HUDHandler.hudHandler.getCoinIconPos();
		coinScreenPos = new Vector3( coinIconPos.x, coinIconPos.y, 3f );
		mainCamera = Camera.main.transform;
		
		GameObject dummyObject = new GameObject();
		dummyObject.name = "Dummy";
		dummyTransform = dummyObject.transform;

		//For power ups
		GameObject powerUpManagerObject = GameObject.FindGameObjectWithTag("PowerUpManager");
		powerUpManager = (PowerUpManager) powerUpManagerObject.GetComponent("PowerUpManager");

		//Default footsteps
		leftFootstep = footstepLeftSound;
		rightFootstep = footstepRightSound;

		generateLevel = GameObject.FindObjectOfType<GenerateLevel>();
		if( localPlayerCreated != null ) localPlayerCreated( transform, this );
		Transform cutSceneCamera = transform.Find("CutsceneCamera");
		Skybox skyBox = (Skybox) cutSceneCamera.GetComponent("Skybox");
		//skyBox.material = LevelManager.Instance.getLevelData().skyBoxMaterial;
	}

	void loadPlayerSkin()
	{
		GameObject hero;
		if(PlayerStatsManager.Instance.getAvatar() == Avatar.Hero )
		{
			hero = (GameObject)Instantiate(Hero_Prefab, Vector3.zero, Quaternion.identity ) ;
		}
		else
		{
			hero = (GameObject)Instantiate(Heroine_Prefab, Vector3.zero, Quaternion.identity ) ;
		}
		hero.transform.parent = transform;
		hero.transform.localPosition = Vector3.zero;
		hero.transform.localRotation = Quaternion.identity;

		hero.name = "Hero";
		GetComponent<Animator>().avatar = hero.GetComponent<PlayerSkinInfo>().animatorAvatar;
		anim.Rebind();
		hero.SetActive( true );
	}

	void Start()
	{
		//The player controller needs info about the tile the player is on.
		generateLevel.setFirstTileInfoInPlayer( this );

		determineRunSpeed();

		//Place the character at the start position which is always 0,ground height,0
		//Calculate the ground height
		RaycastHit hit;
		if (Physics.Raycast(new Vector3( transform.position.x, 10f, transform.position.z ), Vector3.down, out hit, 12.0F ))
		{
			transform.position = new Vector3( transform.position.x, hit.point.y, transform.position.z);
			//Also adjust the camera height
			mainCamera.position = new Vector3(mainCamera.position.x, mainCamera.position.y + hit.point.y, mainCamera.transform.position.z); 
			sc.positionCameraNow();
		}

		//The character is in idle while waiting for the player to press the Run! button. 
		setCharacterState( PlayerCharacterState.Idle );
		if( currentTileType != TileType.Opening )
		{
			sc.playCutscene(CutsceneType.Checkpoint);
		}
	}

	void determineRunSpeed()
	{
		//Determine what the start run speed and run acceleration should be since they can vary
		//from episode to episode. Final episodes are more difficult than earlier levels.
		levelRunStartSpeed = LevelManager.Instance.getCurrentEpisodeInfo().RunStartSpeed;
		runAcceleration = LevelManager.Instance.getCurrentEpisodeInfo().RunAcceleration;
		//We need to set this here so that the troll can get the player's speed.
		newRunSpeed = levelRunStartSpeed;
		runSpeedTurnMultiplier = 0.9f;
	}
	
	public void startRunning()
	{
		startRunning( false );
	}

	public void startRunning( bool playCutscene )
	{	
		moveDirection = new Vector3(0,0,0); //Needed?
		Debug.Log("PlayerController - startRunning: deathType: " + deathType );


		previousPlayerPosition = transform.position;
		allowDistanceTravelledCalculations = true;
		usesAccelerometer = true;

		//Mecanim Hack - we call rebind because the animation states are not reset properly when you die in the middle of an animation.
		//For example, if you die during a double jump, after you get resurrected and start running again, if you do another double jump, only part of the double jump animation will play, never the full animation.
		anim.Rebind();

		//The player starts off running
		setAnimationTrigger(RunTrigger);
		setCharacterState( PlayerCharacterState.StartRunning );
		setCharacterState( PlayerCharacterState.Running );

		//This time is captured so we can slowly accelerate the run speed
		timeSessionStarted = Time.time;

		GameManager.Instance.setGameState( GameState.Normal );

		if( playCutscene )
		{
			sc.playCutscene( CutsceneType.Start );
			runStartSpeed = levelRunStartSpeed;
			newRunSpeed = runStartSpeed;
			controlTrollPursuit( true );
		}
		else
		{
			//When the player is revived and starts running again. There is no cutscene.
			//Lower the speed of the player by a small percentage if he is revived to make it a little easier for him.
			float newRunStartSpeed = runSpeedAtTimeOfDeath * SPEED_REDUCTION_ON_REVIVE;
			//The minimum speed is levelRunStartSpeed
			if( newRunStartSpeed < levelRunStartSpeed ) newRunStartSpeed = levelRunStartSpeed;
			runStartSpeed = newRunStartSpeed;
			newRunSpeed = runStartSpeed;
			//If we are in the opening cutscene, the OpeningSequence class will take care of enabling player controls at the right time.
			if( currentTileType != TileType.Opening )
			{
				enablePlayerControl( true );
			}
		}
	}
	

	void updateRunSpeed()
	{
		if( allowRunSpeedToIncrease && newRunSpeed <= MAX_RUN_SPEED )
		{
			newRunSpeed = (Time.time - timeSessionStarted) * runAcceleration + runStartSpeed; //in seconds
			//Update the blend amount between Run and Sprint animations based on the current run speed
			blendFactor = (newRunSpeed - runStartSpeed)/(MAX_RUN_SPEED-runStartSpeed);
			//If the blendFactor is set to one we will only play the Sprint animation
			//and the Run animation will stop playing. Because of that, we will no longer hear any footsteps.
			//For this reason, cap the blend factor to 0.98f so that we always blend in a little of the run animation and therefore
			//continue to get the run animation sound callbacks.
			if( blendFactor > 0.98f ) blendFactor = 0.98f;
			anim.SetFloat(speedBlendFactor, blendFactor);
		}
	}
	
	public static float getPlayerSpeed()
	{
		return runSpeed; //DON'T USE
	}

	public float getSpeed()
	{
		return newRunSpeed;
	}

	public PlayerCharacterState getCharacterState()
	{
		return playerCharacterState;
	}

	private void setCharacterState( PlayerCharacterState newState )
	{
		//print ("***setCharacterState from " + playerCharacterState + " to new " + newState );
		playerCharacterState = newState;
		//Send an event to interested classes
		if(playerStateChanged != null) playerStateChanged( playerCharacterState );

	}

	//In addition from preventing player movement, this method also disables
	//player controls.
	public void allowPlayerMovement( bool enabled )
	{
		playerMovementEnabled = enabled;
		enablePlayerControl( enabled );
	}

	public void enablePlayerControl( bool enabled )
	{
		playerControlsEnabled = enabled;
		if( enabled )
		{
			powerUpManager.changeSelectedPowerUp( PlayerStatsManager.Instance.getPowerUpSelected() );
		}
		else
		{
			//If the player is alive, it means we are leaving the game to go to a menu, so hide immediately
			if( deathType == DeathType.Alive )
			{
				powerUpManager.hideImmediatelyPowerUp();
			}
			else
			{
				//Player is dead. Since we are staying in the game, we have time to slide the power-up display out.
				powerUpManager.slideDisplayOutPowerUp();
			}
		}
	}
	
	public bool isPlayerControlEnabled()
	{
		return playerControlsEnabled;
	}
	
	public bool isPlayerOnBezierCurve()
	{
		return usesBezierCurve;
	}

	public TileType getCurrentTileType()
	{
		return currentTileType;
	}

	//Used to display debug information in the HUD.
	//It is useful to know on which tile the player is on.
	public string getCurrentTileName()
	{
		if( currentTile != null )
		{
			return currentTile.name;
		}
		else
		{
			return string.Empty;
		}
	}

	void Update()
	{
		if( GameManager.Instance.getGameState() == GameState.Normal )
		{
			updateDistanceTravelled();

			if( playerMovementEnabled )
			{
				checkBelow();
				updateRunSpeed();
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
						//There might be a small crack between the tiles. We dont want the player to fall if this is the case.
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
			}
		}
	}

	void fall()
	{
		if( playerCharacterState == PlayerCharacterState.Falling || playerCharacterState == PlayerCharacterState.Jumping ) return; //ignore, we are already falling or jumping

		//Reset moveDirection.y to 0 so we dont start falling very fast
		moveDirection.y = 0f;
		allowRunSpeedToIncrease = false;
		newRunSpeed = newRunSpeed * 0.65f;
		//Remember at what height the player started to fall because this will help us calculate the fall distance.
		fallStartYPos = transform.position.y;
		gravity = DEFAULT_GRAVITY * 2f;
		sc.heightDamping = PlayerCamera.DEFAULT_HEIGHT_DAMPING * 9f;
		allowDistanceTravelledCalculations = false;
		setCharacterState(PlayerCharacterState.Falling);
		setAnimationTrigger(FallTrigger);
		//playSound( fallingSound, false );
		print ( "fall started " + distanceToGround + " " + MIN_DISTANCE_FOR_FALL + " " + playerCharacterState );
	}

	void land()
	{
		//Reset values that we changed in the fall() method
		gravity = DEFAULT_GRAVITY;
		sc.heightDamping = PlayerCamera.DEFAULT_HEIGHT_DAMPING;
		allowRunSpeedToIncrease = true;
		allowDistanceTravelledCalculations = true;
		setCharacterState( PlayerCharacterState.Running );
		float fallDistance = fallStartYPos - transform.position.y;
		if( fallDistance < DISTANCE_FOR_LAND_ANIMATION )
		{
			anim.CrossFadeInFixedTime( "Land", 0.25f );
		}
		else
		{
			anim.CrossFadeInFixedTime( "Stumble", 0.25f );
			//The Land anim has a callback to play the Land sound, but not the Stumble anim
			audioSource.PlayOneShot( landGroundSound, 0.8f );
		}
		moveDirection.y = 0f;
		print ( "player landed. Fall distance was: " + 	fallDistance );
	}

	void updateDistanceTravelled()
	{
		if( playerCharacterState == PlayerCharacterState.Falling )
		{
			//Calculate the fall distance
			float fallDistance = fallStartYPos - transform.position.y;
			if( fallDistance > FALL_TO_DEATH_DISTANCE )
			{
				managePlayerDeath(DeathType.Cliff);
			}
		}

		//Update distance travelled if in Endless mode
		if( GameManager.Instance.getGameMode() == GameMode.Endless && allowDistanceTravelledCalculations )
		{
			//Do not take height into consideration for distance travelled
			Vector3 current = new Vector3(transform.position.x, 0, transform.position.z);
			Vector3 previous = new Vector3(previousPlayerPosition.x, 0, previousPlayerPosition.z);

			PlayerStatsManager.Instance.addToDistanceTravelled( Vector3.Distance( current, previous ) );

			previousPlayerPosition = transform.position;
		}
	}
	
	void LateUpdate()
	{
		if (Time.deltaTime == 0) return;

		//Make sure the character stays within the limits of the lanes (unless he is dying or falling or ziplining )
		if ( playerCharacterState != PlayerCharacterState.Dying && playerCharacterState != PlayerCharacterState.Falling && playerCharacterState != PlayerCharacterState.Ziplining )
		{
			if( usesBezierCurve )
			{
				//Player is following a bezier curve
				Vector3 closest = currentBezierCurve.ClosestPointOnBezier( transform.position );
				closest = new Vector3( closest.x, transform.position.y, closest.z );
				float dist =  Vector3.Distance( closest, transform.position );
				
				bool playerIsToTheRightOfTheCurve = Utilities.onWhichSide( point1, point2, transform.position );
	
				Vector3 relPos;
				if( dist > 1.3f )
				{
					if( playerIsToTheRightOfTheCurve )
					{
						relPos = new Vector3( laneLimit , 0 , 0 );
					}
					else
					{
						relPos = new Vector3( -laneLimit , 0 , 0 );
						
					}
					dummyTransform.position = new Vector3( closest.x, transform.position.y, closest.z );
					dummyTransform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
					Vector3 exactPos = dummyTransform.TransformPoint(relPos);
					transform.position = exactPos;
					Debug.Log( "+STAY ON PATH BEZIER: p pos " + transform.position + " p rot " + transform.eulerAngles + " dist " + dist  );
				}
			}
			else
			{
				//Player is following a corridor
				if( transform.eulerAngles.y > -0.01f && transform.eulerAngles.y < 0.01f )
				{
					transform.rotation = Quaternion.identity;
					//Debug.LogError("FIXING PLAYER ROTATION");
				}
				float playerRotationY = Mathf.Floor ( transform.eulerAngles.y );
				if( playerRotationY == 0 )
				{
					//Round to two decimal places to avoid small rounding errors
					float delta = Mathf.Round( (transform.position.x - currentTilePos.x) * 100.0f) / 100.0f;
					if( delta < -laneLimit )
					{
						Debug.LogWarning ( "-laneLimit player x " + transform.position.x + " currentTilePos.x " + currentTilePos.x + " playerRotationY " + playerRotationY + " currentTile.name " + currentTile.name );
						transform.position = new Vector3 (  currentTilePos.x -laneLimit, transform.position.y, transform.position.z );
					}
					else if( delta > laneLimit )
					{
						Debug.LogWarning ( "+laneLimit player x " + transform.position.x + " currentTilePos.x " + currentTilePos.x + " playerRotationY " + playerRotationY + " currentTile.name " + currentTile.name );
						transform.position = new Vector3 (  currentTilePos.x + laneLimit, transform.position.y, transform.position.z );
					}
				
				}
				else
				{
					//Round to two decimal places to avoid small rounding errors
					float delta = Mathf.Round( (transform.position.z - currentTilePos.z) * 100.0f) / 100.0f;
					if( delta < -laneLimit )
					{
						Debug.LogWarning ( "+laneLimit player z " + transform.position.z + " currentTilePos.z " + currentTilePos.z + " playerRotationY " + playerRotationY + " currentTile.name " + currentTile.name );
						transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z - laneLimit );
					}
					else if( delta > laneLimit )
					{
						Debug.LogWarning ( "+laneLimit player z " + transform.position.z + " currentTilePos.z " + currentTilePos.z + " playerRotationY " + playerRotationY + " currentTile.name " + currentTile.name );
						transform.position = new Vector3 (  transform.position.x, transform.position.y, currentTilePos.z + laneLimit );
					}
				}
			}
		}
	}
	
	void detectTaps()
	{
		if ( Input.touchCount > 0 )
		{
			Touch touch = Input.GetTouch(0);
			if( touch.tapCount == 2 )
			{
				if( touch.phase == TouchPhase.Ended  )
				{
					handlePowerUp();
				}
			}
		}
	}

	void handlePowerUp()
	{
		if( GameManager.Instance.getGameState() == GameState.Normal && playerCharacterState != PlayerCharacterState.Dying )
		{
			powerUpManager.activatePowerUp( PlayerStatsManager.Instance.getPowerUpSelected() );
		}
	}
	
	void playSound(AudioClip soundToPlay, bool isLooping )
    {
		audioSource.clip = soundToPlay;
		audioSource.loop = isLooping;
		audioSource.Play();
    }
	
	void handleSwipes()
	{
		//Verify if the player swiped across the screen
		if (Input.touchCount > 0)
		{
            Touch touch = Input.GetTouch( 0 );
            
            switch (touch.phase)
			{
		        case TouchPhase.Began:
		            touchStarted = true;
		            touchStartPos = touch.position;
	                break;
		                
		        case TouchPhase.Ended:
	                if (touchStarted)
					{
                        touchStarted = false;
	                }
	                break;
		                
		        case TouchPhase.Canceled:
	                touchStarted = false;
	                break;
		                
		        case TouchPhase.Stationary:
	                break;
		
		        case TouchPhase.Moved:
	                if (touchStarted)
					{
                        TestForSwipeGesture(touch);
	                }
	                break;
			}
		}	

	}
	
	void TestForSwipeGesture(Touch touch)
	{
		Vector2 lastPos = touch.position;
		float distance = Vector2.Distance(lastPos, touchStartPos);
		
		if (distance > minSwipeDistancePixels)
		{
			touchStarted = false;
	        float dy = lastPos.y - touchStartPos.y;
	        float dx = lastPos.x - touchStartPos.x;
	        
	        float angle = Mathf.Rad2Deg * Mathf.Atan2(dx, dy);
	        
	        angle = (360 + angle - 45) % 360;

			if (angle < 90)
			{
				//player swiped RIGHT
				sideSwipe( true );
	        }
			else if (angle < 180)
			{
				//player swiped DOWN
				startSlide ();
	        }
			else if (angle < 270)
			{
				//player swiped LEFT
				sideSwipe( false );
			}
			else
			{
				//player swiped UP
				if( isInZiplineTrigger && playerCharacterState != PlayerCharacterState.Ziplining )
				{
					attachToZipline();
				}
				else
				{
					jump();
				}
	        }
		}
	}
	
	void moveCharacter()
	{
		
		verifySlide();

		if (controller.isGrounded)
		{
			//If we we were falling and just landed,reset values and go back to running state.
			//However, before deciding to land, also check that the distance to the ground is less than 10 cm to avoid false positives (controller.isGrounded is not perfect).
			if( playerCharacterState == PlayerCharacterState.Falling && distanceToGround < 0.1f )
			{
				land();
			}

			//if we were jumping and just landed, reset values and go back to the running state
			if (jumping) 
			{
				if( groundType != "Water" )
				{
					dustPuff.loop = false;
					dustPuff.Play();
				}
				setAnimationTrigger(LandTrigger);
				moveDirection.y = 0f;
				jumping = false;
				doingDoubleJump = false;
				newRunSpeed = runSpeedBeforeJump;
				if( playerCharacterState != PlayerCharacterState.Dying )
				{
					//Don't allow the run speed to increase if the player is dead because this will cause
					//the player to continue to move forward even if he is dead because newRunSpeed won't stay at 0.
					allowRunSpeedToIncrease = true;
				}
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

		#if UNITY_EDITOR
		handleKeyboard();
		#endif

		//Handle mobile device swipes
		handleSwipes();
		detectTaps();

		// Move the controller
		
		// We are moving along a bezier curve
		if( usesBezierCurve )
		{
			float bezierTime = currentBezierCurve.ClosestTimeOnBezier( transform.position );
			//bezierTime can complete with a value around 0.98f, hence the +0.025f.
			if( bezierTime + 0.025f > 1f )
			{
				//We reached the end of this curve since the value is bigger than 1.
				curveListIndex++;
				if( curveListIndex == curveList.Count )
				{
					//No more bezier curves
					usesBezierCurve = false;
					
				}
				else
				{
					//Yes, there is another curve
					createBezier();
					bezierTime = currentBezierCurve.ClosestTimeOnBezier( transform.position );
					Debug.Log ("NEW CURVE BEZIER VALUE IS " + bezierTime );
				}
			}
				
			//1) Get two points along the curve
			point1 = currentBezierCurve.PointOnBezier( bezierTime );
			point2 = currentBezierCurve.PointOnBezier( bezierTime + 0.017f );
			
			//2) Set the rotation of the player to be tangentiel to the curve
			//Place our dummy object on the curve
			dummyTransform.position = new Vector3( point1.x, point1.y, point1.z );
			//Have the dummy object look at the next point on the curve
			dummyTransform.LookAt( point2 );
			//Use that Y rotation and give it to the player
			transform.rotation = Quaternion.Euler( 0, dummyTransform.eulerAngles.y, 0 );	
			
			//3) If there are no more bezier curves, the player is exiting the tile.
			//If this is the case, give the player an orthogonal rotation like 0, 90, 270 etc. based on the rotation
			//of the end point of the last curve.
			if( !usesBezierCurve )
			{	
				transform.rotation = Quaternion.Euler( 0, bezierEndRotationY, 0 );
			}
		}

		if( deathType == DeathType.Water )
		{
			playerSweptAwayByRiver();
		}
		else
		{
			if( playerCharacterState != PlayerCharacterState.Ziplining )
			{
				//1) Get the direction of the player
				forward = transform.TransformDirection(Vector3.forward);			
				//2) Scale vector based on run speed
				forward = forward * Time.deltaTime * newRunSpeed;
				//3) Add Y component for gravity. Both the x and y components are stored in moveDirection.
				forward.Set( forward.x, moveDirection.y * Time.deltaTime, forward.z );
				//4) Get a unit vector that is orthogonal to the direction of the player
				Vector3 relativePos = new Vector3(1 , 0 , 0 );
				Vector3 xPos = transform.TransformPoint(relativePos);
				Vector3 xVector = xPos - transform.position;
				//5) Scale the X component based on accelerometer and change lane values
				float accelerometerAverage = 0;
				if( playerControlsEnabled && usesAccelerometer )
				{
					//For accelerometer
					float accelerometerCurrentFrameX = Input.acceleration.x;
					if( Time.timeScale < 1f )
					{
						//Player is using a slow time power up.
						accelerometerCurrentFrameX = accelerometerCurrentFrameX * accelerometerStrength * SLOW_DOWN_FACTOR;
					}
					else
					{
						//Time is normal.
						accelerometerCurrentFrameX = accelerometerCurrentFrameX * accelerometerStrength;
					}
					accelerometerAverage = (accelerometerCurrentFrameX + accelerometerPreviousFrameX) * 0.5f;
					accelerometerPreviousFrameX = accelerometerCurrentFrameX;
				}
				xVector = xVector * Time.deltaTime * (moveDirection.x + accelerometerAverage);
	
				//6) If not on a bezier curve, clamp to the max distance we can travel perpendicularly without
				//exiting the left or right lanes.
				if( !usesBezierCurve )
				{
					xVector =  Vector3.ClampMagnitude(xVector, Mathf.Abs(getMaxDist(moveDirection.x + accelerometerAverage)));
				}
				//7) Add the X component to the forward direction
				forward = forward + xVector;
				//8) Move the controller
				#if UNITY_EDITOR
				if( controller.enabled ) controller.Move( forward );
				#else
				controller.Move( forward );
				#endif

			}
			else
			{
				//Player is ziplining. He is moved by a LeanTween function
			}
		}
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

	//Make the player be swept away by the current.
	//The River prefab when at 0 degrees has the water flowing from left to right.
	private void playerSweptAwayByRiver()
	{
		float sideCurrentStrength = 4f;
		float forwardCurrentStrength = 0.25f;

		//Player is facing straight. Current is +x.
		if( tileRotationY == 0 )
		{
			moveDirection = new Vector3( sideCurrentStrength, moveDirection.y, forwardCurrentStrength );
		}
		//Player is facing right. Current is down -z.
		else if( tileRotationY == 90f || tileRotationY == -270f )
		{
			moveDirection = new Vector3( forwardCurrentStrength, moveDirection.y, -sideCurrentStrength );
		}
		//Player is facing left. Current is down +z.
		else if( tileRotationY == -90f || tileRotationY == 270f )
		{
			moveDirection = new Vector3( -forwardCurrentStrength, moveDirection.y, sideCurrentStrength );
		}
		forward = moveDirection * Time.deltaTime;
				print("playerSweptAwayByRiver " + moveDirection + " tile Y " + currentTile.transform.eulerAngles.y);
		controller.Move( forward );
	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.LeftArrow) ) 
		{
			sideSwipe( false );
		}
		else if ( Input.GetKeyDown (KeyCode.RightArrow) ) 
		{
			sideSwipe( true );
		}
		else if ( Input.GetKeyDown (KeyCode.DownArrow) ) 
		{
			startSlide();
		}
		else if ( Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.Space)  ) 
		{
			if( isInZiplineTrigger && playerCharacterState != PlayerCharacterState.Ziplining )
			{
				attachToZipline();
			}
			else
			{
				jump();
			}
		}
		else if ( Input.GetKeyDown (KeyCode.P ) )
		{
			controller.enabled = !controller.enabled;
		}
		else if ( Input.GetKeyDown (KeyCode.K ) )
		{
			managePlayerDeath(DeathType.Obstacle);
		}
		else if ( Input.GetKeyDown (KeyCode.S ) )
		{
			//Slow down time
			if( Time.timeScale < 1f )
			{
				Time.timeScale = 1f;
			}
			else 
			{
				Time.timeScale = 0.5f;
			}
		}
		else if ( Input.GetKeyDown (KeyCode.D ) )
		{
			handlePowerUp();
		}
		else if ( Input.GetKeyDown (KeyCode.Q ) )
		{
			Debug.Log("Activating shield powerup " );
			powerUpManager.activatePowerUp(PowerUpType.Shield);
		}
		else if ( Input.GetKeyDown (KeyCode.T ) )
		{
			Debug.Log("Listing all textures: " );
			Utilities.printAllTexturesInScene();
		}
	}

	public void activateSpeedBoost( bool activate )
	{
		if( activate )
		{
			controlTrollPursuit( false );
			mainCamera.GetComponent<MotionBlur>().enabled = true;			
			allowRunSpeedToIncrease = false;
			newRunSpeed = newRunSpeed * PowerUpManager.SPEED_BOOST_MULTIPLIER;
		}
		else
		{
			mainCamera.GetComponent<MotionBlur>().enabled = false;			
			if( playerCharacterState != PlayerCharacterState.Dying ) allowRunSpeedToIncrease = true;
		}
	}

	private void createBezier()
	{
		SegmentInfo.BezierData bezierData = curveList[curveListIndex];
		currentBezierCurve = new Bezier( bezierData.bezierStart.position, bezierData.bezierControl1.position, bezierData.bezierControl2.position, bezierData.bezierEnd.position );
		bezierEndRotationY = bezierData.bezierEnd.eulerAngles.y;
	}
 
	public void jump()
	{
		if( playerControlsEnabled )
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
			if (playerCharacterState != PlayerCharacterState.Jumping && ( distanceToGround < 0.5f || groundType == "Collapsing" ) )
			{
				//Hack - put moveDirection.x to zero in case finalizeSideMove was never called because of a collision
				moveDirection.x = 0;
				
				//Make sure the lane data is correct in case a collision forced us out of our lane
				recalculateCurrentLane();

				//We are allowed to jump from the slide state.
				dustPuff.Stop();						//stop the dust puff that loops while we are sliding
				slideWaterSplash.Stop();
				deactivateOverheadObstacles( true );	//reactivate overhead obstacles since they would have been deactivated if we were sliding

				jumping = true;
				if( trollController != null ) trollController.jump();

				//Memorize the run speed
				runSpeedBeforeJump = newRunSpeed;
				//Lower the run speed during a jump
				newRunSpeed = newRunSpeed * JUMP_RUN_SPEED_MODIFIER;
				//Don't go lower then levelRunStartSpeed
				if( newRunSpeed < levelRunStartSpeed ) newRunSpeed = levelRunStartSpeed;
				//Don't accelerate during a jump (also it would reset the newRunSpeed variable).
				allowRunSpeedToIncrease = false;
				setCharacterState( PlayerCharacterState.Jumping );
				if( false )
				{
					//if you are on a steep slope, the normal jump speed is insufficient to make you feel you are jumping high.
					//So use a higher value instead.
					moveDirection.y = slopeJumpSpeed;
					setAnimationTrigger(JumpTrigger);
				}
				else if( doingDoubleJump )
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
	}
	
	void attachToZipline()
	{
		SegmentInfo si = currentTile.GetComponent<SegmentInfo>();
		if( si != null )
		{
			curveList = si.curveList;
			SegmentInfo.BezierData bezierData = curveList[0];
			setCharacterState( PlayerCharacterState.Ziplining );
			enablePlayerControl( false );
			setAnimationTrigger(Idle_LookTrigger);
			ziplineAttachPoint = transform.Find("Zipline Attach Point");
			ziplineAttachPoint.localPosition = new Vector3( 0, 2.15f, 0 );
			ziplineAttachPoint.localEulerAngles = new Vector3( 0, 0, 0 );
			ziplineAttachPoint.GetComponent<AudioSource>().Play();
			ziplineAttachPoint.SetParent(null);
			transform.SetParent( ziplineAttachPoint );
			transform.eulerAngles =  new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,6f);
			//A set of points that define one or many bezier paths (the paths should be passed in multiples of 4, which correspond to each individual bezier curve)
			//It goes in the order: startPoint,endControl,startControl,endPoint
			LTBezierPath ltBezier = new LTBezierPath( new Vector3[] { bezierData.bezierStart.position, bezierData.bezierControl2.position, bezierData.bezierControl1.position, bezierData.bezierEnd.position } );
			LeanTween.move(ziplineAttachPoint.gameObject, ltBezier.pts, 4f).setOrientToPath(true).setEase(LeanTweenType.easeOutQuad);
			sc.playCutscene(CutsceneType.Ziplining);
		}
	}

	void detachFromZipline()
	{
		LeanTween.cancel( gameObject );
		transform.SetParent( null );
		ziplineAttachPoint.SetParent( transform, false );
		ziplineAttachPoint.localScale = new Vector3( 1f, 1f, 1f ); 	//Just because of rounding when changing parent
		ziplineAttachPoint.GetComponent<AudioSource>().Stop();
		enablePlayerControl( true );
		sc.reactivateMaincamera();
		transform.eulerAngles = new Vector3(0,270f,0); //we turned left while ziplining
		fall();
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

	void sideSwipe( bool isGoingRight )
	{
		if( playerControlsEnabled && !usesBezierCurve )
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
	}

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
				if (sideMoveInitiatedZ > 1.9f )
				{	
					Debug.LogWarning("turnCorner: game over - player turned too late." );
					reasonDiedAtTurn = "TURNED TOO LATE";
					managePlayerDeath ( DeathType.Turned_Too_Late );
					return;
				}

				//Case 2: Player turned late, we want to turn now
				if ( sideMoveInitiatedZ > laneLimit && sideMoveInitiatedZ < 1.9f )
				{	
					if ( isGoingRight )
					{
						//Verify if the player is doing a side-move in an allowed direction
						if (currentDeadEndType == DeadEndType.Right || currentDeadEndType == DeadEndType.LeftRight )
						{
							//Turn is valid
							desiredLane = Lanes.Left;
							turnNow();
						}
						else
						{
							//Player turned the wrong way
							Debug.LogWarning("turnCorner: game over - player turned wrong way too late." );
							reasonDiedAtTurn = "LATE WRONG WAY 3";
							managePlayerDeath ( DeathType.Turned_Too_Late );
						}				
					}
					else
					{
						if (currentDeadEndType == DeadEndType.Left || currentDeadEndType == DeadEndType.LeftRight )
						{
							//Turn is valid
							desiredLane = Lanes.Right;
							turnNow();
						}
						else
						{
							//Player turned the wrong way
							Debug.LogWarning("turnCorner: game over - player turned wrong way too late." );
							reasonDiedAtTurn = "LATE WRONG WAY 4";
							managePlayerDeath ( DeathType.Turned_Too_Late );
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
						if (currentDeadEndType == DeadEndType.Right || currentDeadEndType == DeadEndType.LeftRight )
						{
							//Turn is valid
							setDesiredLane( sideMoveInitiatedZ );
						}
						else
						{
							//Player turned the wrong way
							Debug.LogWarning("turnCorner: game over - player turned wrong way." );
							reasonDiedAtTurn = "WRONG WAY 1";
							managePlayerDeath ( DeathType.Turned_Wrong_Way );
						}				
					}
					else
					{
						if (currentDeadEndType == DeadEndType.Left || currentDeadEndType == DeadEndType.LeftRight )
						{
							//Turn is valid
							setDesiredLane( sideMoveInitiatedZ );
						}
						else
						{
							//Player turned the wrong way
							Debug.LogWarning("turnCorner: game over - player turned wrong way." );
							reasonDiedAtTurn = "WRONG WAY 2";
							managePlayerDeath ( DeathType.Turned_Wrong_Way );
						}
					}
				}
			}
			else
			{
				reasonDiedAtTurn = "ALREADY TURNED";
				Debug.LogWarning("turnCorner: turn denied since player has already turned." );

			}
		}
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
					audioSource.PlayOneShot( sideMoveSound );
					//Debug.Log ("changeLane completed " + isGoingRight + " to lane " + desiredLane );

				}
				else
				{
					desiredLane = Lanes.Left;
					setCharacterState( PlayerCharacterState.SideMove );
					moveDirection.x = -currentSideMoveSpeed;
					audioSource.PlayOneShot( sideMoveSound );
					//Debug.Log ("changeLane completed " + isGoingRight + " to lane " + desiredLane );
				}
			}
			else if ( currentLane == Lanes.Right && !isGoingRight )
			{
				desiredLane = Lanes.Center;
				setCharacterState( PlayerCharacterState.SideMove );
				moveDirection.x = -currentSideMoveSpeed;
				audioSource.PlayOneShot( sideMoveSound );
				//Debug.Log ("changeLane completed " + isGoingRight + " to lane " + desiredLane );
			}
			else if ( currentLane == Lanes.Left && isGoingRight )
			{
				desiredLane = Lanes.Center;
				setCharacterState( PlayerCharacterState.SideMove );
				moveDirection.x = currentSideMoveSpeed;
				audioSource.PlayOneShot( sideMoveSound );
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

	void startSlide()
	{
		if( playerControlsEnabled )
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
						/*I used to change the controller capsule height,radius and position.
						  However, when sliding across the Entrance trigger, it would cause multiple
						  OnTriggerEnter events to be triggered causing in turn, the playerTileIndex
						  to be incremented incorrectly and ultimately, there was no more floor
						  under the player and he would fall. So instead, I disable and re-enable
						  overhead obstacles.*/

						deactivateOverheadObstacles( false );
						setCharacterState( PlayerCharacterState.Sliding );
						if( groundType == "Water" )
						{
							slideWaterSplash.Play();
						}
						else
						{
							dustPuff.loop = true;
							dustPuff.Play();
						}

						setAnimationTrigger(Slide_DownTrigger);

					}
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
				dustPuff.Stop();
				slideWaterSplash.Stop();

				if( playerCharacterState == PlayerCharacterState.Turning_and_sliding )
				{
					setCharacterState( PlayerCharacterState.Sliding );
				}
				else
				{
					setCharacterState( PlayerCharacterState.Running );
				}
				setAnimationTrigger(Slide_UpTrigger);
				deactivateOverheadObstacles( true );
			}
		}
	}	

	//This is related to sliding
	private void deactivateOverheadObstacles( bool deactivate )
	{
		Collider[] colliders = FindObjectsOfType(typeof(Collider)) as Collider[];
        foreach (Collider collider in colliders)
		{
			if ( collider.name == "DeadTree" )
			{
            	collider.enabled = deactivate;
			}
			else if ( collider.name == "Portcullis" )
			{
				collider.enabled = deactivate;
			}
			else if ( collider.name == "Flame" )
			{
				collider.GetComponent<TrapFlame>().isActive = deactivate;
			}
			else if ( collider.name == "Cerberus" )
			{
				collider.enabled = deactivate;
			}
		}
	}
	
	public IEnumerator moveCoin( Transform coin )
	{
   		Vector3 originalPos = coin.position;
		//Time in seconds to reach the coin counter
		float time = 0.75f;
   		float originalTime = time;
   		coin.localScale = new Vector3( 0.06f, 0.06f, 0.06f );
		Behaviour halo = (Behaviour)coin.gameObject.GetComponent("Halo");
		if( halo != null ) halo.enabled = false;

 
		while ( time > 0.0f )
		{
			time -= Time.deltaTime;
			Vector3 coinDestination = mainCamera.GetComponent<Camera>().ScreenToWorldPoint (coinScreenPos);

			if (coin != null )
			{
				coin.position = Vector3.Lerp( coinDestination, originalPos, time / originalTime );
				coin.rotation = mainCamera.rotation;
			}
			
			yield return new WaitForFixedUpdate();  
		}
		if (coin != null )
		{
			Destroy ( coin.gameObject );
		}

	}

	void groundTypeChanged()
	{
		//print("PlayerController-groundTypeChanged from " + previousGroundType + " to " + groundType );
		//Setup proper footsteps
		if( groundType == "Water" )
		{
			leftFootstep = footstepWaterSound;
			rightFootstep = footstepWaterSound;
		}
		else if( groundType == "Snow"  )
		{
			leftFootstep = snowFootstepLeftSound;
			rightFootstep = snowFootstepRightSound;
		}
		else
		{
			leftFootstep = footstepLeftSound;
			rightFootstep = footstepRightSound;
		}
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit )
	{
		if( playerCharacterState != PlayerCharacterState.Dying )
		{
			groundType = hit.gameObject.tag;
			if( groundType != previousGroundType )
			{
				groundTypeChanged();
			}
			previousGroundType = groundType;

			//The CharacterController is constantly colliding with the Quads making up the floor. Ignore those events.
			if (hit.collider.name == "Quad" || hit.collider.name == "Floor" ) return;
	
			//Debug.Log ("OnControllerColliderHit  " + hit.collider.name  );
			if (hit.collider.name == "DeadTree" )
			{
				if( hit.normal.y < 0.4f )
				{
					Debug.Log( "PLayer collided with: " + hit.collider.name + " Normal" + hit.normal );
					//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
					//Move the player back so he does not float in the air in case he hit the log while landing after a jump.
					//controller.Move( hit.normal );
					managePlayerDeath ( DeathType.Obstacle );
				}
			}
			else if (hit.collider.name == "GroundObstacle" )
			{
				if( hit.normal.y < 0.4f )
				{
					Debug.Log( "PLayer collided with: " + hit.collider.name + " Normal" + hit.normal );
					//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
					//Move the player back so he does not float in the air in case he hit the log while landing after a jump.
					//controller.Move( hit.normal );
					managePlayerDeath ( DeathType.Obstacle );
				}
			}
			else if( hit.gameObject.CompareTag("Chicken") )
			{
				Transform chickenTransform = hit.transform.Find("Chicken Trigger");
				if( chickenTransform == null )
				{
					Debug.LogError("PlayerController-OnControllerColliderHit: chicken collision error. Could not find chicken trigger.");
					return;
				}

				ChickenController chickenController = chickenTransform.gameObject.GetComponent<ChickenController>();

				if( playerCharacterState == PlayerCharacterState.Sliding )
				{
					//We can have multiple collisions. Only give stars on the first collision.
					//However, if the chicken falls on the road, we want the player to be able to collide with it a second time.
					if( Time.time - chickenController.timeWasHit > 0.3f )
					{
						//Save the fact that this chicken was hit so the player does not stumble a second time
						chickenController.wasHit = true;

						//Save the collision time to avoid multiple collision events on impact
						chickenController.timeWasHit = Time.time;

						//Give stars
						PlayerStatsManager.Instance.modifyCurrentCoins( 10, true, false );
						
						//Display coin total picked up icon
						HUDHandler.hudHandler.displayCoinPickup( 10 );

						//The faster the player runs, the further the chicken will fly
						float pushPower = newRunSpeed * 2.5f;

						//Make the chicken go flying
						Rigidbody body = hit.collider.attachedRigidbody;
						Vector3 force = new Vector3 (hit.controller.velocity.x, 7f, hit.controller.velocity.z) * pushPower;
						body.AddForceAtPosition(force, hit.point);
						hit.transform.eulerAngles = new Vector3( 0, transform.eulerAngles.y + Random.Range(-7,8),0);

						//Play the animations
						Animation chickenAnimation = hit.gameObject.GetComponent<Animation>();
						chickenAnimation.CrossFade("A_Chicken_Spawn");

						//Destroy the chicken after a while
						DestroyObject(hit.gameObject,10f);
					}
				}
				else
				{
					//Only make the player stumble the first time around
					if( !chickenController.wasHit )
					{
						Stumble();
					}
					else
					{
						//Allow the player to go right through the chicken
						hit.collider.attachedRigidbody.useGravity = false;
						hit.collider.enabled = false;
					}
				}
			}
			else if (hit.collider.name.StartsWith( "Stumble" ) )
			{
				Stumble();
			}
			else if (hit.gameObject.CompareTag( "Zombie" ) )
			{
				ZombieController zombieController = (ZombieController) hit.gameObject.GetComponent("ZombieController");
				//Ignore collision event if Zombie already dead.
				if( zombieController.getCreatureState() != CreatureState.Dying )
				{
					//You can't make a crawling zombie fall backwards
					if( ( playerCharacterState == PlayerCharacterState.Sliding || playerCharacterState == PlayerCharacterState.Turning_and_sliding || PowerUpManager.isThisPowerUpActive( PowerUpType.SpeedBoost ) ) && zombieController.getCreatureState() != CreatureState.Crawling )
					{
						//Give stars
						//PlayerStatsManager.Instance.modifyCurrentCoins( ZombieManager.NUMBER_STARS_PER_ZOMBIE, true, false );
						
						//Display coin total picked up icon
						//HUDHandler.hudHandler.displayCoinPickup( ZombieManager.NUMBER_STARS_PER_ZOMBIE );

						zombieController.knockback( transform );
						
					}
					else
					{
						Debug.Log( "Player collided with zombie: " + hit.collider.name + " Normal" + hit.normal.y + " but CANT TOPPLE HIM " + playerCharacterState + "  STATE Z "+ zombieController.getCreatureState());
						if( hit.normal.y < 0.4f )
						{
							//Player is running up Z axis
							if( Mathf.Floor( transform.eulerAngles.y ) == 0 )
							{
								if( Mathf.Abs( hit.normal.x ) > Mathf.Abs( hit.normal.z ) )
								{
									//Player collided with zombie on the side, just play an animation on the zombie
									zombieController.sideCollision();
								}
								else
								{
									//Player collided squarely with zombie. Kill the player.
									managePlayerDeath ( DeathType.Zombie );
									zombieController.victory( true );
								}
							}
							//Player is running along X axis
							else 
							{
								if( Mathf.Abs( hit.normal.z ) > Mathf.Abs( hit.normal.x ) )
								{
									//Player collided with zombie on the side, just play an animation on the zombie
									zombieController.sideCollision();
								}
								else
								{
									//Player collided squarely with zombie. Kill the player.
									managePlayerDeath ( DeathType.Zombie );
									zombieController.victory( true );
								}
							}
							Debug.Log( "PLayer collided with: " + hit.collider.name + " Normal" + hit.normal );
						}
						else
						{
							//We landed on the zombie's head
							land ();
						}
					}
				}
				else
				{
					Debug.LogError("Zombie already dead");
				}
			}
			else if (hit.collider.name.StartsWith( "Goblin" ) )
			{
				handleCreatureCollision( hit, hit.gameObject.GetComponent<ICreature>() );
			}
			else if (hit.collider.name.StartsWith( "Skeleton" ) )
			{
				handleCreatureCollision( hit, hit.gameObject.GetComponent<ICreature>() );
			}
			else if (hit.collider.name.StartsWith( "Firepit" ) )
			{
				Debug.Log( "Player collided with firepit: " + hit.collider.name + " Normal" + hit.normal.y );
				if( hit.normal.y < 0.4f )
				{
					//Player is running up Z axis
					if( Mathf.Floor( transform.eulerAngles.y ) == 0 )
					{
						if( Mathf.Abs( hit.normal.x ) > Mathf.Abs( hit.normal.z ) )
						{
							//Player collided with firepit on the side. Ignore.
						}
						else
						{
							//Player collided squarely with firepit. Kill the player.
							managePlayerDeath ( DeathType.Obstacle);
						}
					}
					//Player is running along X axis
					else 
					{
						if( Mathf.Abs( hit.normal.z ) > Mathf.Abs( hit.normal.x ) )
						{
							//Player collided with firepit on the side. Ignore.
						}
						else
						{
							//Player collided squarely with firepit. Kill the player.
							managePlayerDeath ( DeathType.Obstacle);
						}
					}
				}
			}
			else if (hit.collider.name.StartsWith( "Breakable Barrel" ) )
			{
				//Don't break if you land on top of the barrel
				if( hit.normal.y < 0.4f )
				{
					BreakableObject bo = (BreakableObject) hit.collider.GetComponent("BreakableObject");
					Debug.Log( "PLayer collided with breakable: " + hit.collider.name );
					//We pass the player collider to triggerBreak() because we do not want the barrel fragments to collide with the player.
					bo.triggerBreak( GetComponent<Collider>() );
					if( playerCharacterState == PlayerCharacterState.Sliding )
					{
						//Give stars
						PlayerStatsManager.Instance.modifyCurrentCoins( 10, true, false );

						//To do
						//Display coin total picked up icon
						HUDHandler.hudHandler.displayCoinPickup( 10 );

					}	
					else
					{
						Stumble();
					}
				}
			}
			else if (hit.collider.name.StartsWith( "Thrown Barrel" ) )
			{
				//Is the player protected by a Shield Power Up?
				if( PowerUpManager.isThisPowerUpActive( PowerUpType.Shield ) )
				{
					//This Power Up only works one time, so deactivate it
					powerUpManager.deactivatePowerUp( PowerUpType.Shield, false );
				}
				else
				{
					managePlayerDeath ( DeathType.Obstacle );
				}
			}
			else if (hit.collider.name.StartsWith( "Breakable Pumpkin" ) )
			{
				BreakableObject bo = (BreakableObject) hit.collider.GetComponent("BreakableObject");
				Debug.Log( "PLayer collided with breakable: " + hit.collider.name );
				//We pass the player collider to triggerBreak() because we do not want the barrel fragments to collide with the player.
				bo.triggerBreak( GetComponent<Collider>() );
				if( playerCharacterState == PlayerCharacterState.Sliding )
				{
					//Give stars
					PlayerStatsManager.Instance.modifyCurrentCoins( 10, true, false );
					
					//To do
					//Display coin total picked up icon
					HUDHandler.hudHandler.displayCoinPickup( 10 );
					
				}	
				else
				{
					Stumble();
				}
			}
			else if (hit.collider.name == "cart" )
			{
				if( hit.normal.y < 0.4f )
				{
					//Move the player back so he does not get stuck in the cart.
					//However, do not push him downwards as this may push the player through the floor in some rare cases (with the cart and coach notably).
					controller.Move( new Vector3( hit.normal.x, 1f, hit.normal.z ) );
					managePlayerDeath ( DeathType.Obstacle );
				}
			}
			else if (hit.collider.name == "Pendulum" )
			{
				//Move the player back so he does not get stuck in the pendulum.
				//controller.Move( hit.normal ); //disable test - seems to make Unity 5 crash
				managePlayerDeath ( DeathType.Obstacle );
			}
			else if (hit.collider.name.StartsWith( "Cow" ) )
			{
				Debug.Log( "Player collided with: " + hit.collider.name + " Normal" + hit.normal );
				SimpleController simpleController = (SimpleController) hit.collider.GetComponent("SimpleController");
				simpleController.playHitAnim();
				if( hit.normal.y < 0.4f )
				{
					//Player collided with cow squarely
					//Move the player back so he does not get stuck in the cow.
					//controller.Move( hit.normal );
					managePlayerDeath ( DeathType.Obstacle );
				}
			}
			else if (hit.collider.name.StartsWith("Fence") || hit.collider.name.StartsWith("Wall") || hit.collider.name.StartsWith("Portcullis") )
			{
				Debug.Log( "Player collided with: " + hit.collider.name + " Normal" + hit.normal );
				//Move the player back so he does not get stuck in the obstacle.
				//However, do not push him downwards as this may push the player through the floor in some rare cases (with the cart and coach notably).
				controller.Move( new Vector3( hit.normal.x, 1f, hit.normal.z ) );
				managePlayerDeath ( DeathType.Obstacle );
			}			
			else if (hit.collider.name.Equals("Weapon") )
			{
				//Skeleton footman or warlord, or goblin piker or wraith or demon
				if( !PowerUpManager.isThisPowerUpActive( PowerUpType.SpeedBoost ) )
				{
					managePlayerDeath ( DeathType.Obstacle );
				}
			}			
		}
	}
	
	void handleCreatureCollision( ControllerColliderHit hit, ICreature creature )
	{
		//Ignore collision event if the creature is already dead.
		if( creature != null && creature.getCreatureState() != CreatureState.Dying )
		{
			if( ( playerCharacterState == PlayerCharacterState.Sliding || playerCharacterState == PlayerCharacterState.Turning_and_sliding ) || PowerUpManager.isThisPowerUpActive( PowerUpType.SpeedBoost ) )
			{
				//Give stars
				//PlayerStatsManager.Instance.modifyCurrentCoins( CreatureManager.NUMBER_SOFT_CURRENCY_PER_CREATURE, true, false );
				
				//Display coin total picked up icon
				//HUDHandler.hudHandler.displayCoinPickup( CreatureManager.NUMBER_SOFT_CURRENCY_PER_CREATURE );

				creature.knockback( transform );
				
			}
			else
			{
				if( hit.normal.y < 0.4f )
				{
					//Player is running up Z axis
					if( Mathf.Floor( transform.eulerAngles.y ) == 0 )
					{
						if( Mathf.Abs( hit.normal.x ) > Mathf.Abs( hit.normal.z ) )
						{
							//Player collided with goblin on the side, just play an animation on the goblin
							creature.sideCollision();
						}
						else
						{
							//Player collided squarely with goblin. Kill the player.
							managePlayerDeath ( DeathType.Zombie );
							creature.victory( true );
						}
					}
					//Player is running along X axis
					else 
					{
						if( Mathf.Abs( hit.normal.z ) > Mathf.Abs( hit.normal.x ) )
						{
							//Player collided with zombie on the side, just play an animation on the zombie
							creature.sideCollision();
						}
						else
						{
							//Player collided squarely with zombie. Kill the player.
							managePlayerDeath ( DeathType.Zombie );
							creature.victory( true );
						}
					}
				}
				else
				{
					//We landed on the goblin's head
					land ();
				}
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		//Carefull, if you turn right inside a deadEnd OnTriggerEnter will be called a second time (but not if your turn left).
		//This is probably a Unity bug.
		if( other.CompareTag( "deadEnd" ) )
		{
			//Deactivate the speedboost if active because it is really hard to turn when you are going super fast
			if( PowerUpManager.isThisPowerUpActive( PowerUpType.SpeedBoost ) )
			{
				powerUpManager.deactivatePowerUp(PowerUpType.SpeedBoost, true );
			}
			isInDeadEnd = true;
			wantToTurn = false;

			currentDeadEndType = other.GetComponent<deadEnd>().deadEndType;
			deadEndTrigger = other;
			//Slow dow the player to make it easier to turn
			allowRunSpeedToIncrease = false;
			runSpeedAtTimeOfTurn = newRunSpeed;
			newRunSpeed = newRunSpeed * runSpeedTurnMultiplier;
		}
		else if ( other.name.StartsWith( "Coin" ) && !PowerUpManager.isThisPowerUpActive( PowerUpType.Magnet ) )
		{
			//Disable the collider
			other.enabled = false;
			PlayerStatsManager.Instance.modifyCoinCount( other.gameObject );
			//We don't want it to turn while flying up
			Destroy ( other.GetComponent ( "Rotator" ) );
			StartCoroutine( moveCoin( other.transform ) );
		}
		else if( other.name == "Lava" )
		{
			Debug.Log ("Player fell into lava.");
			playSound( deathFireSound, false );
			lavaSpurt.transform.position = new Vector3( transform.position.x, transform.position.y, transform.position.z );
			lavaSpurt.loop = false;
			lavaSpurt.Play();
			controlTrollPursuit( false );
			sc.lockCamera( true );
			managePlayerDeath( DeathType.Lava );
		}
		//For the Great Fall trigger collider, don't forget to put in the ignoreRaycast layer or else the distanceToGround value will be incorrect.
		else if( other.CompareTag( "Great Fall" ) )
		{
			Debug.Log ("Player is having a great fall.");
			controlTrollPursuit( false );
			managePlayerDeath( DeathType.GreatFall );
		}
		//For the Lock Camera trigger collider, don't forget to put in the ignoreRaycast layer or else the distanceToGround value will be incorrect.
		else if( other.CompareTag( "Lock Camera" ) )
		{
			Debug.Log ("Locking camera.");
			sc.lockCamera( true );
		}
		else if( other.name .StartsWith("BallOfFire") )
		{
			if( playerCharacterState != PlayerCharacterState.Sliding )
			{
				Debug.Log ("Player was hit by fireball.");
				managePlayerDeath ( DeathType.Fireball);
			}
		}
 		else if( other.name == "RiverCollider" )
		{
			if( playerCharacterState != PlayerCharacterState.Dying )
			{
				//Create a water splash
				Debug.Log ("Player fell into river.");
				other.GetComponent<ParticleSystem>().transform.position.Set ( transform.position.x, transform.position.y + 0.8f, transform.position.z );
				other.GetComponent<ParticleSystem>().Play();
				other.GetComponent<AudioSource>().PlayOneShot(other.GetComponent<AudioSource>().clip);
				//Also hide the coin pack since it gets in the way of the camera
				Transform riverCoinPack = currentTile.transform.Find("CoinPack8x1_1x10_river");
				if( riverCoinPack != null ) riverCoinPack.gameObject.SetActive(false);
				allowDistanceTravelledCalculations = false;
				managePlayerDeath ( DeathType.Water );
			}
		}
		else if( other.name == "Checkpoint" )
		{
			//Player has successfully completed the current level.
			Debug.Log ("Checkpoint triggered ");
			controlTrollPursuit( false );
			GameManager.Instance.setGameState(GameState.Checkpoint);
			StartCoroutine( slowDownPlayer( 16f, afterPlayerSlowdown, other.transform ) );
		}
		else if( other.CompareTag( "Entrance" ) )
		{
			SegmentInfo si = other.transform.parent.GetComponent<SegmentInfo>();
			if( si != null )
			{
				//We might recycle currentTile (the one prior to the one we just entered), this is why we are passing it as a parameter.
				generateLevel.tileEntranceCrossed( other.transform.parent );
				currentTilePos = si.transform.position;
				currentTile = si.gameObject;
				tileRotationY = Mathf.Floor ( currentTile.transform.eulerAngles.y );
				currentTileType = si.tileType;
				usesBezierCurve = si.usesBezierCurve;
				if( usesBezierCurve )
				{
					//reset
					curveListIndex = 0;
					curveList = si.curveList;
					createBezier();
				}
			}
			else
			{
				Debug.LogError("PlayerController-OnTriggerEnter: " + other.transform.parent.name + " tile does not have a SegmentInfo component attached to it.");
			}
		}
		else if( other.name == "disablePlayerControlTrigger" )
		{
			placePlayerInCenterLane();
		}
		else if( other.name == "ZiplineTrigger" )
		{
			//Deactivate the speedboost if active before ziplining
			if( PowerUpManager.isThisPowerUpActive( PowerUpType.SpeedBoost ) )
			{
				powerUpManager.deactivatePowerUp(PowerUpType.SpeedBoost, true );
			}
			isInZiplineTrigger = true;
		}
 		else if( other.name == "DetachZiplineTrigger" )
		{
			detachFromZipline();
		}
  	}
	
	//Make sure the player arrives in the center lane without sliding
	public void placePlayerInCenterLane()
	{
		print ("placePlayerInCenterLane called");
		controlTrollPursuit( false );
		//We do not want the player to be jumping or sliding as he reaches the end location.
		//Disable run acceleration as well.
		allowRunSpeedToIncrease = false;
		enablePlayerControl( false );
		queueJump = false;
		queueSlide = false;
		powerUpManager.deactivatePowerUp(PowerUpType.SlowTime, true );
		powerUpManager.deactivatePowerUp(PowerUpType.SpeedBoost, true );
		//If he was sliding, making him run again
		if ( playerCharacterState == PlayerCharacterState.Sliding )
		{
			//We are stopping sliding
			dustPuff.Stop();
			slideWaterSplash.Stop();
			setCharacterState( PlayerCharacterState.Running );
			setAnimationTrigger(Slide_UpTrigger);
		}

		//Clear move direction of any values. If we still have an x component for example, we will drift.
		//We do want to keep the Y value so we land on the ground if we were jumping.
		moveDirection = new Vector3( 0,moveDirection.y,0 );
		usesAccelerometer = false;
		accelerometerPreviousFrameX = 0;
		recalculateCurrentLane();  //make sure our currentLane info is valid
		//Side move speed is divided by 1.6 because it just looks better.
		if ( currentLane == Lanes.Left )
		{
			desiredLane = Lanes.Center;
			setCharacterState( PlayerCharacterState.SideMove );
			moveDirection.x = sideMoveSpeed/1.6f;
		}
		else if ( currentLane == Lanes.Right )
		{
			desiredLane = Lanes.Center;
			setCharacterState( PlayerCharacterState.SideMove );
			moveDirection.x = -sideMoveSpeed/1.6f;
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
				Debug.LogWarning("PlayerController-OnTriggerStay: returning 0");
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
				if( !deadEndTurnDone && currentDeadEndType != DeadEndType.None )
				{
					reasonDiedAtTurn = "EXITED DEAD END NO TURN";
					Debug.LogWarning("OnTriggerExit player exited dead end without turning " + other.name + " " + isInDeadEnd + " " + deadEndTurnDone + " " + currentDeadEndType );
					managePlayerDeath ( DeathType.Exited_Without_Turning );
				}
				//Reset values
				isInDeadEnd = false;
				deadEndTurnDone = false;
				deadEndTrigger = null;
				wantToTurn = false;
			}
			else if( other.name == "DeadTree" || other.name.StartsWith( "Stumble" ) || other.name == "cart" || other.name.StartsWith( "Breakable" ) || other.name == "Chicken" || other.name == "Pendulum" || other.name == "GroundObstacle" )
			{
				if( PowerUpManager.isThisPowerUpActive( PowerUpType.Shield ) )
				{
					//This Power Up only works one time, so deactivate it
					powerUpManager.deactivatePowerUp( PowerUpType.Shield, false );
				}
			}
			else if( other.name == "ZiplineTrigger" )
			{
				//Player is no longer in the zipline trigger
				isInZiplineTrigger = false;
			}
		}
	}

	public void Teleport_leave_complete( AnimationEvent eve )
	{
		appearFX.Play ();
		transform.localScale = new Vector3( 0.002f, 0.002f, 0.002f );
		appearFX.transform.position = new Vector3( transform.position.x, currentTilePos.y + 3.7f, transform.position.z );

	}
	
	//We pass the trigger value because we need its position. We cannot rely on the position of the player at the moment of trigger because it can fluctuate based on frame rate and such.
	//Therefore the final destination is based on the trigger's Z position plus the desired distance (and not the player's z position plus the desired distance, which is slightly inaccurate).
	public IEnumerator slowDownPlayer( float distance, System.Action onFinish, Transform trigger )
	{
		if( currentTile.transform.rotation.y != 0 ) Debug.LogError("PlayerController-slowDownPlayer error: slow down only works with tiles with a zero rotation for now.");
		float percentageComplete = 0;

		//Center player perfectly in the middle of the tile
		transform.SetParent( currentTile.transform );
		transform.localPosition = new Vector3( 0, transform.localPosition.y, transform.localPosition.z );
		transform.SetParent( null );

		Vector3 initialPlayerPosition = new Vector3( transform.position.x, transform.position.y, trigger.position.z );
		Vector3 finalPlayerPosition = initialPlayerPosition + (transform.TransformDirection(Vector3.forward) * distance);
		float distanceTravelled = 0;
		float brakeFactor = 0.7f; //brake the player before slowing him down
		float startSpeed = newRunSpeed * brakeFactor;
		float endSpeed = SLOW_DOWN_END_SPEED;

		float startBlendFactor = blendFactor;

		float startAnimationSpeed = anim.speed;
		float endAnimationSpeed = 1f;

		while ( distanceTravelled <= distance )
		{
			distanceTravelled = Vector3.Distance( transform.position, initialPlayerPosition );
			percentageComplete = distanceTravelled/distance;

			//Update run speed
			newRunSpeed =  Mathf.Lerp( startSpeed, endSpeed, percentageComplete );

			//Update the blend amount between Run and Sprint animations based on the current run speed
			blendFactor =  Mathf.Lerp( startBlendFactor, 0, percentageComplete );
			anim.SetFloat(speedBlendFactor, blendFactor);

			//update animation speed
			anim.speed = Mathf.Lerp( startAnimationSpeed, endAnimationSpeed, percentageComplete );

			//move player forward
			//1) Get the direction of the player
			forward = transform.TransformDirection(Vector3.forward);
			//2) Scale vector based on run speed
			forward = forward * Time.deltaTime * newRunSpeed;
			//3) Add Y component for gravity. Both the x and y components are stored in moveDirection.
			forward.Set( forward.x, moveDirection.y * Time.deltaTime, forward.z );
			//4) Get a unit vector that is orthogonal to the direction of the player
			Vector3 relativePos = new Vector3(1 , 0 , 0 );
			Vector3 xPos = transform.TransformPoint(relativePos);
			Vector3 xVector = xPos - transform.position;
			//5) Scale the X component based on accelerometer and change lane values
			xVector = xVector * Time.deltaTime * moveDirection.x;
			//6) If not on a bezier curve, clamp to the max distance we can travel perpendicularly without
			//exiting the left or right lanes.
			if( !usesBezierCurve )
			{
				xVector =  Vector3.ClampMagnitude(xVector, Mathf.Abs(getMaxDist(moveDirection.x)));
			}
			//7) Add the X component to the forward direction
			forward = forward + xVector;
			forward =  Vector3.ClampMagnitude(forward, Vector3.Distance( transform.position, finalPlayerPosition ) );

			//8) Move the controller
			controller.Move( forward );
			verifyIfDesiredLaneReached();
			yield return new WaitForFixedUpdate(); 
		}
		//Position the player exactly where he should be as we might have overshot the distance in the while loop
		transform.position = new Vector3( finalPlayerPosition.x, transform.position.y, finalPlayerPosition.z );
		onFinish.Invoke();
	}


	public void afterPlayerSlowdown()
	{
		setCharacterState( PlayerCharacterState.Winning );
		setAnimationTrigger(VictoryTrigger);
		//See Cullis Gate for next steps.
	}

	public IEnumerator walkForDistance( float distance, float walkSpeed, System.Action onFinish, bool playIdleLookAfterWalkCompleted )
	{
		Vector3 initialPlayerPosition = new Vector3( transform.position.x, transform.position.y, transform.position.z );
		Vector3 finalPlayerPosition = initialPlayerPosition + (transform.TransformDirection(Vector3.forward) * distance);
		float distanceTravelled = 0;
		anim.SetFloat(speedBlendFactor, 0 );
		setAnimationTrigger( RunTrigger );
		anim.speed = 1f;

		while ( distanceTravelled <= distance )
		{
			distanceTravelled = Vector3.Distance( transform.position, initialPlayerPosition );

			//move player forward
			//1) Get the direction of the player
			forward = transform.TransformDirection(Vector3.forward);
			//2) Scale vector based on walk speed
			forward = forward * Time.deltaTime * walkSpeed;
			//3) Add Y component for gravity. Both the x and y components are stored in moveDirection.
			forward.Set( forward.x, moveDirection.y * Time.deltaTime, forward.z );
			//8) Move the controller
			controller.Move( forward );
			yield return new WaitForFixedUpdate(); 
		}
		//Position the player exactly where he should be as we might have overshot the distance in the while loop
		transform.position = new Vector3( finalPlayerPosition.x, transform.position.y, finalPlayerPosition.z );
		if( playIdleLookAfterWalkCompleted ) setAnimationTrigger(Idle_LookTrigger);
		onFinish.Invoke();	
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

		audioSource.PlayOneShot( sideMoveSound );
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
		//Reset the run speed to what it was at the beginning of the turn.
		allowRunSpeedToIncrease = true;
		newRunSpeed = runSpeedAtTimeOfTurn;

		//Debug.Log ("turnNow completed " + isGoingRight + " " + transform.eulerAngles.y + " " + playerCharacterState );

	}

	public void managePlayerDeath( DeathType deathTypeValue )
	{
		//Only proceed if the player is not dying already
		if ( playerCharacterState != PlayerCharacterState.Dying )
		{
			//Remember how we died
			deathType = deathTypeValue;

			Debug.Log("managePlayerDeath : " + deathType );

			//If the player was looking over his shoulder, disable that
			disableLookOverShoulder();

			//Disable the player's shadow
			shadowProjector.enabled = false;

			//Do not change collider if player falls into river.
			if( deathType != DeathType.Water )
			{
				//Change his collider since he is now lying on the ground,
				//in particular so zombies will not be able to walk through his body
				controller.center = new Vector3( 0, controllerOriginalCenter.y ,-1f );
				controller.radius = 0.9f;
			}

			//Remember the run speed at time of death because we want to start running again (in case of revive) at a 
			//percentage of this value.
			//When we jump, the run speed is reduced.
			//If we died while jumping, we want to use runSpeedBeforeJump and not newRunSpeed.
			if( playerCharacterState == PlayerCharacterState.Jumping )
			{
				runSpeedAtTimeOfDeath = runSpeedBeforeJump;
			}
			else
			{
				runSpeedAtTimeOfDeath = newRunSpeed;
			}

			newRunSpeed = 0;
			runSpeedBeforeJump = 0;
			allowRunSpeedToIncrease = false;

			//Change character state
			setCharacterState( PlayerCharacterState.Dying );

			//Stop the dust particle system. It might be playing if we died while sliding.
			dustPuff.Stop();
			slideWaterSplash.Stop();

			//Disable player controls when dying
			enablePlayerControl( false );
			
			//Deactivate any queued jump or slide
			queueJump = false;
			queueSlide = false;

			//Reset move direction and forward
			moveDirection = new Vector3( 0,moveDirection.y,0 );

			//Stop any currently playing sound
			audioSource.Stop();

			//Make adjustments depending on death type
		    switch (deathType)
			{
		        case DeathType.Cliff:
					StartCoroutine( waitBeforeDisplayingSaveMeScreen(2f) );	
					break;
		                
		        case DeathType.Enemy:
					StartCoroutine( waitBeforeDisplayingSaveMeScreen(2f) );	
					break;
		                
				case DeathType.Zombie:
					//Play collision sound
					playSound( dyingSound, false );
					setAnimationTrigger(DeathWallTrigger);
					break;
			
				case DeathType.Flame:
					playSound( deathFireSound, false );
					setAnimationTrigger(DeathWallTrigger);
					break;
		                
		        case DeathType.Obstacle:
					//Play collision sound
					playSound( dyingSound, false );
					sc.Shake();
					setAnimationTrigger(DeathWallTrigger);
					break;

		        case DeathType.Water:
					sc.lockCamera ( true );
					anim.speed = 2.8f;
					setAnimationTrigger(DeathRiverTrigger);
					StartCoroutine( waitBeforeDisplayingSaveMeScreen(2.5f) );
					break;

		        case DeathType.VortexTrap:
					sc.lockCamera ( true );
					anim.speed = 3.8f;
					setAnimationTrigger(FallTrigger);
					LeanTween.moveLocalY( gameObject, transform.position.y - TrapVortex.distanceTravelledDown, TrapVortex.timeRequiredToGoDown ).setEase(LeanTweenType.easeOutExpo).setDelay(TrapVortex.delayBeforeBeingPulledDown);
					StartCoroutine( waitBeforeDisplayingSaveMeScreen(5f) );
					break;

		        case DeathType.GreatFall:
					StartCoroutine( waitBeforeDisplayingSaveMeScreen(2.5f) );
					break;

		        case DeathType.SpecialFall:
					sc.lockCamera ( true );
					anim.speed = 3.8f;
					setAnimationTrigger(FallTrigger);
					LeanTween.moveLocalY( gameObject, transform.position.y - TrapVortex.distanceTravelledDown, TrapVortex.timeRequiredToGoDown ).setEase(LeanTweenType.easeOutExpo).setDelay(0);
					StartCoroutine( waitBeforeDisplayingSaveMeScreen(2.5f) );
					break;

				default:
					setAnimationTrigger(DeathWallTrigger);
					break;
			}
		}
	}

	public void death_completed ( AnimationEvent eve )
	{
		StartCoroutine( waitBeforeDisplayingSaveMeScreen(1.5f) );
	}

	public void Footstep_left ( AnimationEvent eve )
	{
		audioSource.PlayOneShot( leftFootstep, 0.1f );
	}

	public void Footstep_right ( AnimationEvent eve )
	{
		audioSource.PlayOneShot( rightFootstep, 0.1f );
	}

	public void Land_sound ( AnimationEvent eve )
	{
		if( groundType != "Water" )
		{
			audioSource.PlayOneShot( landGroundSound, 0.28f );
		}
		else
		{
			audioSource.PlayOneShot( landWaterSound );
		}
	}

	public IEnumerator waitBeforeDisplayingSaveMeScreen ( float duration )
	{
		anim.speed = 1f;
		yield return new WaitForSeconds(duration);
		//Save the player stats before continuing
		PlayerStatsManager.Instance.savePlayerStats();
		GameManager.Instance.setGameState( GameState.SaveMe );
	}

	public IEnumerator waitBeforeResurrecting ( float duration )
	{
		//In this case, we have NO save screen. It is used by TrapMagicGate
		GameManager.Instance.setGameState( GameState.Resurrect );
		anim.speed = 1f;
		yield return new WaitForSeconds(duration);
		sc.setCameraParameters( 18f, PlayerCamera.DEFAULT_DISTANCE, PlayerCamera.DEFAULT_HEIGHT, PlayerCamera.DEFAULT_Y_ROTATION_OFFSET );
		sc.activateMainCamera();
		sc.positionCameraNow();
		sc.resetCameraParameters();
		resurrectBegin(true);
	}

	public void stumble_completed ( AnimationEvent eve )
	{
		setCharacterState( PlayerCharacterState.Running );
		runSpeed = runSpeedAtTimeOfStumble;
		allowRunSpeedToIncrease = true;
	}
	
	void Stumble()
	{
		//The OnControllerColliderHit function can send multiple collision events during a single
		//stumble, so ignore any new events while in the stumbling state.
		if ( playerCharacterState != PlayerCharacterState.Stumbling && playerCharacterState != PlayerCharacterState.Dying )
		{	
			Debug.Log ("Player stumbled");
			//Play player stumble animation by setting the state
			setCharacterState( PlayerCharacterState.Stumbling );
			//If the player stumbles, he loses a bit of speed and momentarily stops accelerating.
			allowRunSpeedToIncrease = false;
			runSpeedAtTimeOfStumble = runSpeed;
			runSpeed = stumbleRunSpeedMultiplier * runSpeed; //lower speed a bit
			//audio.PlayOneShot( stumblingSound );
			//Make troll appear right behind player
			//Note that "placeTrollBehindPlayer" may change the state of the character to Dying
			if( trollController.didPlayerStumblePreviously() )
			{
				//The player falls forward and dies (killed by the troll)
				setAnimationTrigger(FallForwardTrigger);
			}
			else
			{
				//The player stumbles but recovers
				setAnimationTrigger(StumbleTrigger);
			}
			trollController.placeTrollBehindPlayer();
		}
	}

	public void setTrollController( TrollController trollController )
	{
		this.trollController = trollController;
	}

	public void setFairyController( FairyController fairyController )
	{
		this.fairyController = fairyController;
	}

	void controlTrollPursuit( bool startPursuit )
	{
		if( trollController != null )
		{
			if( startPursuit )
			{
				trollController.startPursuing();
			}
			else
			{
				trollController.stopPursuing();
			}
		}
	}

	public void disableLookOverShoulder()
	{
		//StopCoroutine("lookOverShoulderAnim");
		anim.SetFloat( lookbackBlendFactor, 0f );
	}

	public void lookOverShoulder( float endBlendFactor, float stayDuration )
	{
		StartCoroutine(lookOverShoulderAnim(endBlendFactor, stayDuration) );
	}

	public IEnumerator lookOverShoulderAnim( float endBlendFactor, float stayDuration )
	{
		float elapsedTime = 0;
		
		float fadeInDuration = 0.4f;
		float fadeOutDuration = 0.5f;
		
		float startBlendFactor = 0;
		//Fade in
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			anim.SetFloat( lookbackBlendFactor, Mathf.Lerp( startBlendFactor, endBlendFactor, elapsedTime/fadeInDuration ) );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < fadeInDuration );
		
		anim.SetFloat( lookbackBlendFactor, endBlendFactor );
		
		//Stay
		yield return new WaitForSeconds(stayDuration);
		
		//Fade out
		elapsedTime = 0;
		
		startBlendFactor = endBlendFactor;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			anim.SetFloat( lookbackBlendFactor, Mathf.Lerp( startBlendFactor, 0, elapsedTime/fadeOutDuration ) );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < fadeOutDuration );
		
		anim.SetFloat( lookbackBlendFactor, 0 );
		
	}

	public void shakeCamera()
	{
		sc.Shake();
	}
	
	void checkBelow()
	{
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10.0F ))
		{
            distanceToGround = hit.distance;
			//print ("below " + hit.collider.name );
		}
		else
		{
			//Ground is further than 10 meters or possibly there is no collider below the player.
			//Just set an arbitrarely big value.
			distanceToGround = 1000f;
		}
        
	}
	
	//Returns true if there is a collider less than 1.5 meters above the player
	bool checkAbove()
	{
 		//The Player uses the Ignore Raycast layer
        if (Physics.Raycast(transform.position, Vector3.up, 1.5f ))
		{
            return true;
		}
		else
		{
			return false;
		}
        
	}
	
	public void resetSharedLevelData( bool unlockCamera )
	{
		Debug.Log("PlayerController - resetSharedLevelData: unlockCamera: " + unlockCamera );
		//Reset values
		//Teleport_leave_complete changes the scale value so we need to reset it
		transform.localScale = new Vector3( 1f, 1f, 1f );

		disableLookOverShoulder();

		//Variable for swipe
		touchStarted = false;

		//Character Controller
		controller.center = controllerOriginalCenter;
		controller.radius = controllerOriginalRadius;
		
		//Lanes
		currentLane = Lanes.Center;
		desiredLane = Lanes.Center;
		myLane = 0;

		deactivateOverheadObstacles( true );
		jumping = false;
		queueSlide = false;
		queueJump = false;
		isInDeadEnd = false;
		deadEndTrigger = null;
		wantToTurn = false;
		deadEndTurnDone = false;
		if( unlockCamera ) sc.lockCamera ( false );
		gravity = DEFAULT_GRAVITY;
		moveDirection = new Vector3(0,0,0);
		accelerometerPreviousFrameX = 0;
		allowRunSpeedToIncrease = true;
		deathType = DeathType.Alive;

		reasonDiedAtTurn = "";

		sc.heightDamping = PlayerCamera.DEFAULT_HEIGHT_DAMPING;

	}
	
	public void resurrectBegin( bool calledByMagicGate )
	{
		GameManager.Instance.setGameState(GameState.Resurrect);

		//0) Reset data

		resetSharedLevelData(true);
		
		//1) Stop pursuit
		controlTrollPursuit( false );

		//2) Hide and reset all zombies and goblins etc. by sending an event
		if(resurrectionBegin != null) resurrectionBegin();

		//2a) Reset the camera. If a cut-scene played when the player died, the camera parameters such as the FOV may have changed.
		sc.resetCameraParameters();
		
		//3) Reposition dead body at the respawn location.
		//Also, play the dead loop animation
		anim.Play("DeathWall_Loop");
		GameObject respawnLocationObject;

		if( getCurrentTileType() == TileType.T_Junction || getCurrentTileType() == TileType.T_Junction_2 )
		{
			//If the player's rotation is zero, this means he has not turned yet.
			//If this is the case, we will assume he turned right.
			float playerRotationY = Mathf.Floor ( transform.eulerAngles.y );
			if( playerRotationY == 0 )
			{
				respawnLocationObject = currentTile.transform.Find("respawnLocationRight").gameObject;
				generateLevel.playerTurnedAtTJunction( true, currentTile );
				
			}
			else
			{
				//Player has already turned at the T-Junction
				if( isGoingRight )
				{
					respawnLocationObject = currentTile.transform.Find("respawnLocationRight").gameObject;
				}
				else
				{
					respawnLocationObject = currentTile.transform.Find("respawnLocationLeft").gameObject;
				}
			}
		}
		else
		{
			respawnLocationObject = currentTile.transform.Find("respawnLocation").gameObject;
		}

		deathType = DeathType.Alive;

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
			//When he is on the last frame of the dead animation, the player is 0.0328f above the ground
			transform.position = new Vector3( respawn.position.x, groundHeight + 0.0328f, respawn.position.z );
			//The respawnLocationObject is always point in the correct direction for the resurrected hero
			//temporarily set the tile has the parent of the hero and simply use the local rotation of the respawn location object
			transform.SetParent( currentTile.transform );
			transform.localRotation = Quaternion.Euler ( 0, respawn.localEulerAngles.y, 0 );
			transform.SetParent( null );
			tileRotationY = Mathf.Floor ( transform.eulerAngles.y );
			transform.GetComponent<Collider>().enabled = true;
			sc.positionCameraNow();
		}
		else
		{
			Debug.LogError("PlayerController-ResurrectBegin: Unable to find respawnLocation game object in tile : " + currentTile.name );
		}

		if( calledByMagicGate )
		{
			//4a) Fly Fairy to body, have her speak to the player and then have her play a sprinkle animation
			fairyController.speakToPlayerPart1( );
		}
		else
		{
			//4b) Fly Fairy to body and have play a sprinkle animation
			fairyController.revivePlayer( );
		}	
	}

	public void resurrectMiddle()
	{
		//5) Play the revive animation
		anim.speed = 1.6f;
		anim.Play( "DeathWall_GetUp" );
	}

	public void get_up_completed ( AnimationEvent eve )
	{
		anim.speed = 1f;
		//Re-enable the player's blob shadow
		shadowProjector.enabled = true;
		resurrectEnd();
	}
	
	private void resurrectEnd()
	{
		//6) Disable colliders during grace period
		activateObstacleColliders( false );
		
		//7) Activate visual effect to indicate that you are in the grace period
		
		//8) Deduct appropriate amount of premium currency. The amount doubles each time you resurrect during a given run.
		
		//9) Start running or flying
		startRunning( false );

		//10) Restore player controls
		enablePlayerControl( true );

		//11) Give the player a grace period before the obstacles become active again
		StartCoroutine( waitForGracePeriod( 2 ) );
		
		//12) Display a Go! message
		HUDHandler.hudHandler.activateUserMessage( LocalizationManager.Instance.getText("GO"), 0f, 1.25f );
	}
	
	IEnumerator waitForGracePeriod( float duration )
	{
		//Give the player time to clear any immediate obstacles
		yield return new WaitForSeconds(duration);
		activateObstacleColliders( true );

	}

	//This is related to the grace period after player died
	private void activateObstacleColliders( bool isActive )
	{
		Collider[] colliders = FindObjectsOfType(typeof(Collider)) as Collider[];
        foreach (Collider collider in colliders)
		{
			if ( collider.name == "DeadTree" || collider.name.StartsWith( "Stumble" ) || collider.name == "cart" || collider.name.StartsWith( "Breakable" ) || collider.name == "Pendulum" || collider.name == "GroundObstacle" )
			{
				//Since we are disabling the collider, we need to disable gravity for objects with a rigid body
				//as well or else the object will fall through the ground.
				if( collider.GetComponent<Rigidbody>() != null )
				{
					collider.GetComponent<Rigidbody>().useGravity = isActive;
				}
            	collider.enabled = isActive;
				//Debug.Log (" activateObstacleColliders " + collider.name + " isActive " + isActive );
			}
			else if ( collider.name == "Flame" )
			{
				collider.GetComponent<TrapFlame>().isActive = isActive;
			}
        }
	}

	//Called every frame to ensure that the critical current lane value is always accurate
	private void recalculateCurrentLane()
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

	void OnEnable()
	{
		HUDMultiplayer.startRunningEvent += StartRunningEvent;
	}

	void OnDisable()
	{
		HUDMultiplayer.startRunningEvent -= StartRunningEvent;
	}

	void StartRunningEvent()
	{
		Debug.Log("Multiplayer - PlayerController: received StartRunningEvent");
		startRunning( false );
	}

	public void playVictoryAnimation()
	{
		setAnimationTrigger( Finish_LineTrigger );
	}

	void setAnimationTrigger( int animationTrigger )
	{
		anim.SetTrigger( animationTrigger );
	}

	public void Slide_sound_start ( AnimationEvent eve )
	{
		playSound( slidingSound, true );
	}

	public void Slide_sound_stop ( AnimationEvent eve )
	{
		audioSource.Stop();
	}

} 