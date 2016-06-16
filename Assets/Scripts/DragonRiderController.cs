using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DragonRiderController : BaseClass {
	
	private Transform mainCamera;

	public ZombieManager zombieManager;
	
	//Variables for swipe
    float minSwipeDistancePixels;
    bool touchStarted = false;
    Vector2 touchStartPos;
	float minSwipeDistance = 0.025f; //About 30 pixels on an iPhone 5

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
	public AudioClip 	landSound;
	public AudioClip 	deathFireSound;

	//Particles
	public ParticleSystem dustPuff;
	public ParticleSystem appearFX; //Called by teleportLeaveComplete

	//Components
	public Animator anim;
	CharacterController controller;
	HUDHandler hudHandler;

	//Dragon or flying related
	public AnimationClip dragonFlyAnimation;
	public AnimationClip dragonTakeOffAnimation;
	public AnimationClip dragonNoseDive;
	public AnimationClip dragonBreatheFire;
	public AnimationClip dragonEjectPlayer;
	public Transform Dragon;
	Animation dragonAnimation;
	Transform dragonNeckBone;
	private GameObject dragonFire;
	Transform BASE_Master_Root;

	private float yawStrength = 4.5f;
	private float pitchStrength = 0.8f;
	private float rollStrength = 0.8f;
	private float verticalStrength = 10f;
	private float rotationDamping = 5f;
	//Used to apply a temporary speed boost diving down
	private static float speedBoost = 0f;
	private float pitchSpeedBoostStrength = 12f;

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
	public float sideMoveSpeed = 6f; 
	//when a new level starts or if the player dies and he is revived, he will
	//start running at runStartSpeed.
	float runStartSpeed = 0;
	//The running speed will increase with time to make it harder for the player,
	public static float runSpeed = 0;
	//The run speed at time of death is needed because we want to start running again (in case of revive) at a 
	//percentage of this value.
	float runSpeedAtTimeOfDeath = 0;
	const float MAX_RUN_SPEED = 42f;
	const float SLOW_DOWN_END_SPEED = 5f;
	//Used to modify the blend amount between Run and Sprint animations based on the current run speed. Also used by the troll.
	public float blendFactor;
	//By what percentage to reduce the player's speed if he is revived.
	//We reduce the speed slightly to give a chance to the player.
	const float SPEED_REDUCTION_ON_REVIVE = 0.85f;
	//The run start speed specified in the level data.
	float levelRunStartSpeed = 0;
	//allowRunSpeedToIncrease is set to false while jumping
	bool allowRunSpeedToIncrease = true;
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

	public static CharacterState _characterState;

	public enum DragonState {
		None = -1,
		Flying = 0,
		FireBreathing = 1,
		NoseDive = 2,
		TakeOff = 3,
		EjectPlayer = 4
	}
	public DragonState dragonState;
	private bool dragonHasControl = false;
	
	// Are we jumping? (Initiated with jump button and not grounded yet)
	public bool jumping = false;
	public bool doingDoubleJump = false;
	bool jumpingReachedApex = false;
	
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
	const float FALL_TO_DEATH_DISTANCE = 50f;
	//If the fall distance is less than DISTANCE_FOR_LAND_ANIMATION meters, the hero plays the Land animation; above, he plays the Stumble animation.
	const float DISTANCE_FOR_LAND_ANIMATION = 34f;

	TrollController trollController;
	GameObject fairy;
	FairyController fairyController;
	PowerUpManager powerUpManager;

	SimpleCamera sc;
	GenerateLevel gl;
	
	public static DeathType deathType = DeathType.Alive;

	//For accelerometer
	bool usesAccelerometer = true;
	float accelerometerPreviousFrameX = 0;
	float accelerometerPreviousFrameY = 0;
	float accelerometerStrength = 14f;
	
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
	public GameObject sphere1;
	public GameObject sphere2;
	//The current curve the player is on
	Bezier currentBezierCurve;

	//Event management used to notify other classes when the character state has changed
	public delegate void PlayerState( CharacterState value );
	public static event PlayerState playerStateChanged;
	
	//For stats and achievements
	static EventCounter boots_of_jumping 	= new EventCounter( GameCenterManager.BootsOfJumping, 1, CounterType.Total_any_level );
	static EventCounter dragon_rider 		= new EventCounter( GameCenterManager.DragonRider, 1, CounterType.Total_any_level );
	static EventCounter watch_your_step 	= new EventCounter( GameCenterManager.WatchYourStep, 1, CounterType.Total_any_level );
	static EventCounter chicken_chaser 		= new EventCounter( GameCenterManager.ChickenChaser, 1, CounterType.Total_any_level );
	
	//Hash IDs for player animations. These are used to improve performance.
	int RunTrigger = Animator.StringToHash("Run");
	int Dragon_HappyTrigger = Animator.StringToHash("Dragon_Happy");
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

	//For debugging swipes
	public string lastSwipe;
	public string reasonDiedAtTurn;

	void Awake()
	{
		//Instantiate the avatar selected by the player and attach it to the Hero game object
		GameObject avatar = Resources.Load( "Avatar/" + PlayerStatsManager.Instance.getAvatarName() ) as GameObject;
		GameObject Hero_Prefab = null;
		if( avatar != null )
		{
			Hero_Prefab = (GameObject)Instantiate(avatar, Vector3.zero, Quaternion.identity ) ;
			Transform blobShadowProjectorObject = Hero_Prefab.transform.FindChild("Blob Shadow Projector");
			if( blobShadowProjectorObject == null )
			{
				Debug.LogError("PlayerController-error: Unable to find, Blob Shadow Projector, in the " + Hero_Prefab.name + " prefab." );
			}
			shadowProjector = blobShadowProjectorObject.GetComponent<Projector>();

			Hero_Prefab.transform.parent = transform;
		}
		else
		{
			Debug.LogError("PlayerController-error: Unable to find avatar " + PlayerStatsManager.Instance.getAvatarName() + " in Resources/Avatar folder." );
		}

		//Calculate the minimum swipe distance in pixels
        float screenDiagonalSize = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
        minSwipeDistancePixels = minSwipeDistance * screenDiagonalSize; 

		//Get a copy of the components
		anim = Hero_Prefab.GetComponent<Animator>();

		//For dragon riding
		BASE_Master_Root = Hero_Prefab.transform.FindChild("BASE_Master_Root");

		controller = GetComponent<CharacterController>();
		hudHandler = GetComponent<HUDHandler>();

		GameObject Troll = GameObject.FindGameObjectWithTag("Troll");
		trollController = Troll.GetComponent<TrollController>();
		sc = GetComponent<SimpleCamera>();
		gl = GetComponent<GenerateLevel>();

		//For coins
		Vector2 coinIconPos = HUDHandler.getCoinIconPos();
		coinScreenPos = new Vector3( coinIconPos.x, coinIconPos.y, 3f );
		mainCamera = Camera.main.transform;
		
		GameObject dummyObject = new GameObject();
		dummyObject.name = "Dummy";
		dummyTransform = dummyObject.transform;

		//for debugging bezier
		sphere1.SetActive( false );
		sphere2.SetActive( false );

		//For power ups
		GameObject powerUpManagerObject = GameObject.FindGameObjectWithTag("PowerUpManager");
		powerUpManager = (PowerUpManager) powerUpManagerObject.GetComponent("PowerUpManager");


	}

	void Start()
	{

		determineRunSpeed();

		if( currentTileType != TileType.Opening )
		{
			GameManager.Instance.setGameState(GameState.Menu);

			//Place the character at the start position which is always 0,0,0
			transform.position = Vector3.zero;
			
			//The character is in idle while waiting for the player to press the Run! button. 
			setCharacterState( CharacterState.Idle );		

			if( currentTileType != TileType.Start || LevelManager.Instance.isTutorialActive() )
			{
				//The troll is only visible when the player starts off on a Start tile.
				//The troll is not visible during the tutorial.
				//For all other tiles, the troll is inactive at first and will only become visible if the player stumbles.
				trollController.stopPursuing();
			}
		}
	}

	void determineRunSpeed()
	{
		//Use the level data to determine what the start run speed and run acceleration should be since they can vary
		//from level to level. Final levels are more difficult than earlier levels.
		LevelData.LevelInfo levelInfo = LevelManager.Instance.getLevelInfo();
		//Note: getRunStartSpeed & getRunAcceleration return the values adjusted according to the difficulty level of the game.
		//The RunStartSpeed is higher in Heroic mode than in normal mode for example.
		levelRunStartSpeed = levelInfo.getRunStartSpeed();
		runAcceleration = levelInfo.getRunAcceleration();
		//We need to set this here so that the troll can get the player's speed.
		runSpeed = levelRunStartSpeed;
	}
	
	public void startRunning()
	{
		startRunning( currentTileType == TileType.Start );
	}

	public void startRunning( bool playCutscene )
	{	
		previousPlayerPosition = transform.position;
		allowDistanceTravelledCalculations = true;

		//The player starts off running
		anim.SetTrigger(RunTrigger);
		setCharacterState( CharacterState.StartRunning );
		setCharacterState( CharacterState.Running );

		//This time is captured so we can slowly accelerate the run speed
		timeSessionStarted = Time.time;

		StartCoroutine( SoundManager.fadeInMusic(2f) );
		GameManager.Instance.setGameState( GameState.Normal );

		if( playCutscene )
		{
			sc.playCutscene( CutsceneType.Start );
			runStartSpeed = levelRunStartSpeed;
			runSpeed = runStartSpeed;
			if( !LevelManager.Instance.isTutorialActive() )
			{
				trollController.startPursuing();
			}
		}
		else
		{
			//When the player is revived and starts running again. There is no cutscene.
			//Lower the speed of the player by a small percentage if he is revived to make it a little easier for him.
			float newRunStartSpeed = runSpeedAtTimeOfDeath * SPEED_REDUCTION_ON_REVIVE;
			//The minimum speed is levelRunStartSpeed
			if( newRunStartSpeed < levelRunStartSpeed ) newRunStartSpeed = levelRunStartSpeed;
			runStartSpeed = newRunStartSpeed;
			runSpeed = runStartSpeed;
			//If we are in the opening cutscene, the OpeningSequence class will take care of enabling player controls at the right time.
			if( currentTileType != TileType.Opening )
			{
				enablePlayerControl( true );
			}
		}
	}
	
	void startFlying()
	{		
		setCharacterState( CharacterState.Flying );

		trollController.stopPursuing();

		//The player starts flying
		//Dragon related
		dragonState = DragonState.Flying;
		dragonAnimation = (Animation) Dragon.GetComponent("Animation");
		dragonNeckBone = Dragon.transform.Find("RedD/RedD Pelvis/RedD Spine/RedD Spine1/RedD Neck/RedD Neck1");
		dragonAnimation[dragonFlyAnimation.name].wrapMode = WrapMode.Loop;
		dragonAnimation[dragonFlyAnimation.name].speed = 1.6f;
		dragonAnimation.Play(dragonFlyAnimation.name);
		//Get the particle system
		dragonFire = Dragon.transform.Find("RedD/RedD Pelvis/RedD Spine/RedD Spine1/RedD Neck/RedD Neck1/RedD Head/RedD Ponytail1/RedD Ponytail1Nub/Dragon Fire").gameObject;
		
		//Player related
		anim.SetTrigger(Dragon_HappyTrigger);
		GetComponent<AudioSource>().Stop();
		
		//This time is captured so we can slowly accelerate the run speed
		timeSessionStarted = Time.time;
		runSpeed = runStartSpeed;
		
		StartCoroutine( SoundManager.fadeOutMusic(2f) );
		GameManager.Instance.setGameState( GameState.Normal );
		
		//Fade out the ambience sound of the level since we will be flying in the air
		StartCoroutine( SoundManager.fadeOutAmbience( 7f ) );
		
		playSound( flyingSound, true );
		//Clear move direction of any values. If we still have an x component for example, we will drift.
		moveDirection = new Vector3( 0,0,0 );
		accelerometerPreviousFrameX = 0;
		accelerometerPreviousFrameY = 0;
		sc.setCameraParameters( 30f, 4f, 5.5f,0 );
		gl.setNumberVisibleTiles( 4 );
		
		//Disable screen dimming while flying since the only input is the accelerometer
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		enablePlayerControl( true );
		Debug.Log("PlayerController-startFlying complete " );

		//Achievement
		dragon_rider.incrementCounter();

	}
	
	void stopFlying()
	{		
		//Dragon related
		dragonState = DragonState.None;
		
		//Player related
		anim.SetTrigger(RunTrigger);
		setCharacterState( CharacterState.Running );

		//This time is captured so we can slowly accelerate the run speed
		timeSessionStarted = Time.time;
		runSpeed = runStartSpeed;
		
		//Fade the music back in
		StartCoroutine( SoundManager.fadeInMusic(2f) );

		//Clear move direction of any values. If we still have an x component for example, we will drift.
		moveDirection = new Vector3( 0,0,0 );
		accelerometerPreviousFrameX = 0;
		accelerometerPreviousFrameY = 0;
		
		//Orient the player in the same direction as the current tile
		transform.rotation = Quaternion.Euler ( 0, tileRotationY, 0 );
		
		//Fade back in ambience sound
		AudioClip audioClip = LevelManager.Instance.getLevelInfo().AmbienceSound;
		StartCoroutine( SoundManager.fadeInAmbience( audioClip, 6f ) );

		sc.resetCameraParameters();
		gl.resetNumberVisibleTiles();
		
		//Reenable screen dimming since we are no longer flying to preserve battery life.
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
		enablePlayerControl( true );
		dragonHasControl = false;
		trollController.startPursuing();
		Debug.Log("PlayerController-stopFlying complete moveDirection: " + moveDirection + " run speed " + runSpeed + " forward " + forward + " rot " + transform.eulerAngles + " cl " + currentLane + " dl " + desiredLane + " wantToTurn " + wantToTurn + " deadEndTurnDone " + deadEndTurnDone);

	}
	
	private void positionDragon()
	{
		Vector3 relativePos = new Vector3(0 , -1.6f , -0.5f );
		Vector3 exactPos = transform.TransformPoint(relativePos);
		Dragon.transform.position = exactPos;
		Dragon.transform.rotation = transform.rotation;
		
	}

	private void breatheFire()
	{
		dragonState = DragonState.FireBreathing;
		dragonAnimation[dragonBreatheFire.name].speed = 0.8f;
		dragonAnimation[dragonBreatheFire.name].wrapMode = WrapMode.Once;
		dragonAnimation.CrossFade(dragonBreatheFire.name);				
		StartCoroutine( "WaitForBreatheFire" );
	}

	private IEnumerator WaitForBreatheFire ()
	{
		bool breathedFire = false;
		float duration = dragonAnimation[dragonBreatheFire.name].length;
   		do
	    {
			if( dragonAnimation[dragonBreatheFire.name].time > 0.6f && !breathedFire )
			{
				dragonFire.GetComponent<ParticleSystem>().Play();
				Dragon.GetComponent<AudioSource>().loop = false;
				Dragon.GetComponent<AudioSource>().Play();
				breathedFire = true;
			}
			duration = duration - Time.deltaTime;
	        yield return _sync();
	    } while ( duration > 0 );
		
		dragonState = DragonState.Flying;
	}

	private void ejectPlayerFromDragon()
	{
		dragonState = DragonState.EjectPlayer;
		dragonAnimation[dragonEjectPlayer.name].speed = 0.8f;
		dragonAnimation[dragonEjectPlayer.name].wrapMode = WrapMode.Once;
		dragonAnimation.CrossFade(dragonEjectPlayer.name);				
		StartCoroutine( "WaitForEjectPlayer" );
	}

	private IEnumerator WaitForEjectPlayer ()
	{
		Debug.Log ("WaitForEjectPlayer-animation started.");
		bool playerEjected = false;
		float duration = dragonAnimation[dragonEjectPlayer.name].length;
   		do
	    {
			float pitch = Mathf.LerpAngle (Dragon.eulerAngles.x, 10f, rotationDamping * Time.deltaTime);
			Dragon.rotation = Quaternion.Euler ( pitch, Dragon.eulerAngles.y, Dragon.eulerAngles.z );
			if( dragonAnimation[dragonEjectPlayer.name].time > 0.6f && !playerEjected )
			{
				stopFlying ();
				jump();
				Debug.Log ("WaitForEjectPlayer-jump.");
				Dragon.GetComponent<AudioSource>().loop = false;
				Dragon.GetComponent<AudioSource>().Play();
				playerEjected = true;
			}
			duration = duration - Time.deltaTime;
	        yield return _sync();
	    } while ( duration > 0 );
		//Destroy the dragon after a few seconds
		Destroy ( Dragon.gameObject, 2f );
		//reset the player's rotation
		transform.rotation = Quaternion.Euler ( 0, tileRotationY, 0 );
		
		Debug.Log ("WaitForEjectPlayer-animation finished.");
	}

	void updateRunSpeed()
	{
		if( allowRunSpeedToIncrease && runSpeed <= MAX_RUN_SPEED )
		{
			runSpeed = (Time.time - timeSessionStarted) * runAcceleration + runStartSpeed; //in seconds
			//Update the blend amount between Run and Sprint animations based on the current run speed
			blendFactor = (runSpeed - runStartSpeed)/(MAX_RUN_SPEED-runStartSpeed);
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
		return runSpeed;
	}
	
	public static float getPlayerSpeedBoost()
	{
		return speedBoost;
	}

	public CharacterState getCharacterState()
	{
		return _characterState;
	}

	private void setCharacterState( CharacterState newState )
	{
		print ("***setCharacterState from " + _characterState + " to new " + newState );
		_characterState = newState;
		//Send an event to interested classes
		if(playerStateChanged != null) playerStateChanged( _characterState );

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
		print ("PlayerController-enablePlayerControl: " + enabled );
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


	void Update()
	{
		if( GameManager.Instance.getGameState() == GameState.Normal )
		{
			updateDistanceTravelled();

			if( _characterState == CharacterState.Flying )
			{
				if( playerControlsEnabled && usesAccelerometer )
				{

					//Reset value
					speedBoost = 0;
					//Section A - adjust accelerometer values
					float accelerometerCurrentFrameX = Input.acceleration.x;
					//Notes:
					//if the device is horizontal, accelerometerCurrentFrameY = 0
					//if the device is vertical with the home button at the bottom, accelerometerCurrentFrameY = 1
					//if the device is vertical with the home button at the top, accelerometerCurrentFrameY = -1
					float accelerometerCurrentFrameY = -Input.acceleration.y;
					//If the player holds the phone tilted up at an angle of 45 degrees (accelerometerCurrentFrameY = 0.5f) we
					//want to stay flat; if the phone is tilted more, we want to go up; if the phone is tilted at less, we want to go down.
					//If the phone is really tilted down (i.e. accelerometerCurrentFrameY is less then zero), we cap the value.
					if( accelerometerCurrentFrameY < 0 ) accelerometerCurrentFrameY = 0;
					
					//Should we add a speed boost due to pitch?
					if( accelerometerCurrentFrameY < 0.65f )
					{
						//We are doing a nose dive, boost the speed.
						if( dragonState == DragonState.Flying )
						{
							//Only change the state if we are simply flying.
							//We do not want to interrupt a fire breath.
							dragonState = DragonState.NoseDive;
						}
						speedBoost = ( 0.65f - accelerometerCurrentFrameY ) * pitchSpeedBoostStrength;
					}
					else
					{
						if( dragonState != DragonState.FireBreathing )
						{
							dragonState = DragonState.Flying;
						}
					}
					//verticalStrength controls how fast we move up and down as well as pitch
					accelerometerCurrentFrameY = -( 0.65f - accelerometerCurrentFrameY ) * verticalStrength;
					
					//Section B - rotations
					//Note: transform.eulerAngles always returns a value between 0 and 360 degrees.
					
					//1 - Calculate Roll - Z axis
					//Z axis is counter-clockwise, so +10 degrees tilts left and 350 degrees tilts right.
					float roll;
					if( Mathf.Abs ( accelerometerCurrentFrameX ) > 0.2f )
					{	
						//The player is tilting the device sufficiently, so tilt
						roll = transform.eulerAngles.z - accelerometerCurrentFrameX * rollStrength;
						roll = Utilities.clampRotation ( roll, 10f );
					}
					else
					{
						//The player is not tilting the device a lot so slowly go back to 0 degrees.
						roll = Mathf.LerpAngle (transform.eulerAngles.z, 0, rotationDamping * Time.deltaTime);
					}
						
					//2 - Calculate Pitch - X axis				
					float pitch = transform.eulerAngles.x - accelerometerCurrentFrameY * pitchStrength;
					//X axis is counter-clockwise, so +10 degrees tilts forward and 350 degrees tilts backward.
					pitch = Utilities.clampRotation ( pitch, 10f );
					
					//3 - Calculate Yaw - Y axis				
					float yaw = transform.eulerAngles.y + accelerometerCurrentFrameX * yawStrength;
					//Do not allow player to fly back to where he started. He must always move forward.
			//yaw = Utilities.clampRotation ( yaw, 80f );

					//4 - Rotate
					transform.rotation = Quaternion.Euler ( pitch, yaw, roll );
					
					//Section C - translations
					accelerometerCurrentFrameY = accelerometerCurrentFrameY * 1.5f;
					moveDirection.Set(  moveDirection.x , moveDirection.y + accelerometerCurrentFrameY - accelerometerPreviousFrameY, moveDirection.z );
					accelerometerPreviousFrameY = accelerometerCurrentFrameY;
				}
				if( playerControlsEnabled || dragonHasControl )
				{
					updateRunSpeed();
					moveCharacter();
					if( dragonState != DragonState.EjectPlayer )
					{
						positionDragon();
					}
					
				}
				 
			}
			else
			{
				
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
					moveDirection.Set(  moveDirection.x + accelerometerCurrentFrameX - accelerometerPreviousFrameX, moveDirection.y, moveDirection.z );
					accelerometerPreviousFrameX = accelerometerCurrentFrameX;
				}
				if( playerMovementEnabled )
				{
					checkBelow();
					updateRunSpeed();
					moveCharacter();

					//Verify if the player is falling, but only if he is not flying on a dragon.
					//Also ignore if we are already falling or dying.
					if( _characterState != CharacterState.Flying && _characterState != CharacterState.Falling && _characterState != CharacterState.Dying )
					{
						//Verify how far is the ground
						if( distanceToGround > MIN_DISTANCE_FOR_FALL )
						{
							//There might be a small crack between the tiles. We dont want the player to fall if this is the case.
							//So also check 15cm in front of the player to see how far the ground is before deciding to fall.
							Vector3 exactPos = transform.TransformPoint(new Vector3( 0,0,0.15f));

							if ( !Physics.Raycast(exactPos, Vector3.down, MIN_DISTANCE_FOR_FALL ))
							{
								//Ground is further than MIN_DISTANCE_FOR_FALL meters.
								fall ();
							}
						}
					}

					verifyIfDesiredLaneReached();
				}
			}
		}

	}

	void fall()
	{
		if( _characterState == CharacterState.Falling ) return; //ignore, we are already falling

		//Remember at what height the player started to fall because this will help us calculate the fall distance.
		fallStartYPos = transform.position.y;
		gravity = DEFAULT_GRAVITY * 1.5f;
		sc.heightDamping = SimpleCamera.DEFAULT_HEIGHT_DAMPING * 9f;
		allowRunSpeedToIncrease = false;
		allowDistanceTravelledCalculations = false;
		setCharacterState(CharacterState.Falling);
		anim.SetTrigger(FallTrigger);
		//playSound( fallingSound, false );
		print ( "fall started " + distanceToGround + " " + MIN_DISTANCE_FOR_FALL + " " + _characterState );
	}

	void land()
	{
		//Reset values that we changed in the fall() method
		gravity = DEFAULT_GRAVITY;
		sc.heightDamping = SimpleCamera.DEFAULT_HEIGHT_DAMPING;
		allowRunSpeedToIncrease = true;
		allowDistanceTravelledCalculations = true;
		setCharacterState( CharacterState.Running );
		float fallDistance = fallStartYPos - transform.position.y;
		if( fallDistance < DISTANCE_FOR_LAND_ANIMATION )
		{
			anim.SetTrigger(LandTrigger);
		}
		else
		{
			anim.SetTrigger(StumbleTrigger);
			//The Land anim has a callback to play the Land sound, but not the Stumble anim
			GetComponent<AudioSource>().PlayOneShot( landSound, 0.8f );
		}
		moveDirection.y = 0f;
		print ( "player landed. Fall distance was: " + 	fallDistance );
	}

	void updateDistanceTravelled()
	{
		if( _characterState == CharacterState.Falling )
		{
			//Calculate the fall distance
			float fallDistance = fallStartYPos - transform.position.y;
			if( fallDistance > FALL_TO_DEATH_DISTANCE )
			{
				managePlayerDeath(DeathType.Cliff);
			}
		}

		//Update distance travelled
		if( allowDistanceTravelledCalculations )
		{
			PlayerStatsManager.Instance.addToDistanceTravelled( Vector3.Distance( transform.position, previousPlayerPosition ) );

			previousPlayerPosition = transform.position;
		}
	}

	void LateUpdate()
	{
		if( _characterState != CharacterState.Flying )
		{

			if (Time.deltaTime == 0) return;

			//Make sure the character stays within the limits of the lanes (unless he is dying )
			if ( _characterState != CharacterState.Dying )
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
		else
		{
			//Make sure the dragon keeps his neck above the water
			if( transform.position.y < -0.6f )
			{
				transform.position = new Vector3 (  transform.position.x, -0.6f, transform.position.z );
			}
			else if( transform.position.y > 60f )
			{
				transform.position = new Vector3 (  transform.position.x, 60f, transform.position.z );
			}

			//Make the Hero follow the up and down movement of the dragon's neck
			if( dragonState != DragonState.EjectPlayer )
			{
				BASE_Master_Root.position = new Vector3( dragonNeckBone.position.x, dragonNeckBone.position.y - 0.15f, dragonNeckBone.position.z );
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
					if( _characterState == CharacterState.Flying )
					{
						//We are on the dragon, breathe fire
						breatheFire();
					}
					else
					{
						handlePowerUp();
					}
				}
			}
		}
	}

	void handlePowerUp()
	{
		if( _characterState != CharacterState.Dying )
		{
			powerUpManager.activatePowerUp( PlayerStatsManager.Instance.getPowerUpSelected() );
		}
	}
	
	void playSound(AudioClip soundToPlay, bool isLooping )
    {
		GetComponent<AudioSource>().clip = soundToPlay;
		GetComponent<AudioSource>().loop = isLooping;
		GetComponent<AudioSource>().Play();
    }
	
	void handleSwipes()
	{
		//Verify if the player swiped across the screen
		if (Input.touchCount > 0)
		{
            var touch = Input.touches[0];
            
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
				if( _characterState != CharacterState.Flying )
				{
					moveToSide( true );
					lastSwipe = "RIGHT";
				}
	        }
			else if (angle < 180)
			{
				//player swiped DOWN
				if( _characterState != CharacterState.Flying )
				{
					startSlide ();
					lastSwipe = "DOWN";
				}
	        }
			else if (angle < 270)
			{
				//player swiped LEFT
				if( _characterState != CharacterState.Flying )
				{
					moveToSide( false );
					lastSwipe = "LEFT";
				}
			}
			else
			{
				//player swiped UP
				if( _characterState != CharacterState.Flying )
				{
					jump();
					lastSwipe = "UP";
				}
	        }
		}
	}
	
	void moveCharacter()
	{
		
		verifySlide();
		recalculateCurrentLane();
			
		// When we reach the apex of the jump
		if (jumping && !jumpingReachedApex && moveDirection.y <= 0.0)
		{
			jumpingReachedApex = true;
		}

		// Apply gravity
		if (controller.isGrounded)
		{
			//If we we were falling and just landed,reset values and go back to running state
			if( _characterState == CharacterState.Falling )
			{
				land();
			}

			//if we were jumping and just landed, reset values and go back to the running state
			if (jumping) 
			{
				dustPuff.loop = false;
				dustPuff.Play();
				anim.SetTrigger(LandTrigger);
				moveDirection.y = 0f;
				jumping = false;
				doingDoubleJump = false;
				runSpeed = runSpeedBeforeJump;
				if( _characterState != CharacterState.Dying )
				{
					//Don't allow the run speed to increase if the player is dead because this will cause
					//the player to continue to move forward even if he is dead because runSpeed won't stay at 0.
					allowRunSpeedToIncrease = true;
				}
				jumpingReachedApex = false;
				if ( queueSlide )
				{
					queueSlide = false;
					gravity = DEFAULT_GRAVITY;
					startSlide();
				}
				else if ( queueJump )
				{
					setCharacterState( CharacterState.Running );
					queueJump = false;
					jump();
				}
				else
				{
					//Resume running, but only if we are not dying
					if( _characterState != CharacterState.Dying )
					{
						setCharacterState( CharacterState.Running );
					}
				}
			}
		}
		else
		{
			if( _characterState != CharacterState.Flying )
			{
				moveDirection.y -= gravity * Time.deltaTime;
			}
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
			
			//For debugging
			//sphere1.transform.position = point1;
			//sphere2.transform.position = point2;
			
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
			//1) Get the direction of the player
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			float totalSpeed = runSpeed + speedBoost;
			forward = forward * Time.deltaTime * totalSpeed;
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
				xVector =  Vector3.ClampMagnitude(xVector, Mathf.Abs(getMaxDist()));
			}
			//7) Add the X component to the forward direction
			forward = forward + xVector;
			//8) Move the controller
			controller.Move( forward );
		}


		// ANIMATION sector
		if( _characterState != CharacterState.Flying )
		{
			//We are on the ground
		}
		else
		{	

			//We are flying
			if( dragonAnimation ) {
				if( dragonState == DragonState.Flying ) 
				{
					dragonAnimation[dragonFlyAnimation.name].wrapMode = WrapMode.Loop;
					dragonAnimation.CrossFade(dragonFlyAnimation.name, 0.6f);				
				} 
				else if (dragonState == DragonState.FireBreathing ) 
				{
					dragonAnimation.CrossFade(dragonBreatheFire.name);				
				} 
				else if( dragonState == DragonState.NoseDive ) 
				{
					dragonAnimation[dragonNoseDive.name].speed = 1f;
					dragonAnimation[dragonNoseDive.name].wrapMode = WrapMode.Loop;
					dragonAnimation.CrossFade(dragonNoseDive.name, 0.6f );				
				} 
				else if( dragonState == DragonState.TakeOff ) 
				{
					dragonAnimation.CrossFade(dragonTakeOffAnimation.name, 0.6f);				
				} 
				else if( dragonState == DragonState.EjectPlayer ) 
				{
					dragonAnimation.CrossFade(dragonEjectPlayer.name, 0.6f);				
				} 
			}
		}
	}

	//Returns the maximum distance the player can travel perpendicularly without
	//exiting the left or right lanes.
	private float getMaxDist()
	{
		float maxDist = 0;
		float playerRotationY = Mathf.Floor ( transform.eulerAngles.y );
		//Player is facing straight.
		if( playerRotationY == 0 )
		{
			if( moveDirection.x >=0 )
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
			if( moveDirection.x >=0 )
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
			if( moveDirection.x >=0 )
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
		float sideCurrentStrength = 5f;
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
		controller.Move( forward );
	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.LeftArrow) ) 
		{
			if( _characterState != CharacterState.Flying )
			{
				moveToSide( false );
				lastSwipe = "LEFT";

			}
			else
			{
				transform.rotation = Quaternion.Euler ( 0, transform.eulerAngles.y - 15f, 0 );
				
			}
		}
		else if ( Input.GetKeyDown (KeyCode.RightArrow) ) 
		{
			if( _characterState != CharacterState.Flying )
			{
				moveToSide( true );
				lastSwipe = "RIGHT";
			}
			else
			{	
				transform.rotation = Quaternion.Euler ( 0, transform.eulerAngles.y + 15f, 0 );
			}
		}
		else if ( Input.GetKeyDown (KeyCode.DownArrow) ) 
		{
			if( _characterState != CharacterState.Flying )
			{
				startSlide();
				lastSwipe = "DOWN";
			}
			else
			{
				moveDirection.Set(  moveDirection.x, moveDirection.y - 2f, moveDirection.z );
			}
		}
		else if ( Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.Space)  ) 
		{
			if( _characterState != CharacterState.Flying )
			{
				jump();
				lastSwipe = "UP";
			}
			else
			{
				moveDirection.Set(  moveDirection.x, moveDirection.y + 2f, moveDirection.z );
			}
		}
		else if ( Input.GetKeyDown (KeyCode.F) ) 
		{
			startFlying();
		}
		else if ( Input.GetKeyDown (KeyCode.A ) )
		{
			breatheFire();
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
		else if ( Input.GetKeyDown (KeyCode.R ) )
		{
			Debug.LogWarning("Resetting tile with keyboard " + currentTile.name );
			TileReset tr = currentTile.GetComponent<TileReset>();
			tr.resetTile();
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
	
			//Only allow a jump if we are not already jumping and if we are on the ground
			if (_characterState != CharacterState.Jumping && _characterState != CharacterState.SideMove && distanceToGround < 0.5f )
			{
				jumping = true;
				trollController.jump();

				//Memorize the run speed
				runSpeedBeforeJump = runSpeed;
				//Lower the run speed during a jump
				runSpeed = runSpeed * JUMP_RUN_SPEED_MODIFIER;
				//Don't go lower then levelRunStartSpeed
				if( runSpeed < levelRunStartSpeed ) runSpeed = levelRunStartSpeed;
				//Don't accelerate during a jump (also it would reset the runSpeed variable).
				allowRunSpeedToIncrease = false;
				setCharacterState( CharacterState.Jumping );
				if( doingDoubleJump )
				{
					moveDirection.y = doubleJumpSpeed;
					anim.SetTrigger(Double_JumpTrigger);
					boots_of_jumping.incrementCounter();
				}
				else
				{
					moveDirection.y = jumpSpeed;
					anim.SetTrigger(JumpTrigger);
				}
				//for debugging
				//remove jump sound for now because it is annoying
				//playSound( jumpingSound, false );
			}
		}
	}
	
	void setDesiredLane( float sideMoveInitiatedZ )
	{
		if( _characterState == CharacterState.Sliding )
		{
			setCharacterState( CharacterState.Turning_and_sliding );
		}
		else
		{
			setCharacterState( CharacterState.Turning );
		}

		if (sideMoveInitiatedZ > laneLimit )
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
		else if (sideMoveInitiatedZ < laneLimit && sideMoveInitiatedZ >= 0)
		{
			//Lane 2 is always the center lane
			myLane = 2;
			desiredLane = Lanes.Center;
		}
		else if (sideMoveInitiatedZ < 0 && sideMoveInitiatedZ >= -laneLimit)
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
		else
		{
			Debug.LogWarning("PlayerController-setDesiredLane: did not find a valid lane: " + sideMoveInitiatedZ );
		}
		wantToTurn = true;

	}
	float getSideMoveInitiatedPosition()
	{
		//return deadEndTrigger.transform.position.z;
		float playerRotationY = Mathf.Floor( transform.eulerAngles.y );
		if( playerRotationY == 0 )
		{
			return deadEndTrigger.transform.position.z - transform.position.z;
		}
		else if( playerRotationY == 90f || playerRotationY == -270f )
		{
			return deadEndTrigger.transform.position.x - transform.position.x;
		}
		else if( playerRotationY == -90f || playerRotationY == 270f )
		{
			return transform.position.x - deadEndTrigger.transform.position.x;
		}
		else
		{
			Debug.LogWarning("getSideMoveInitiatedPosition: returning 0");
			return 0;
		}
	}
	
	
	//move the player to the left lane if false,
	//and to the right lane if true
	void moveToSide( bool isGoingRight )
	{
		if( playerControlsEnabled && !usesBezierCurve )
		{
			//Only allow side-moves if we are running or jumping or sliding
	 		if ( _characterState == CharacterState.Running || _characterState == CharacterState.Jumping || _characterState == CharacterState.Sliding )
			{
				this.isGoingRight = isGoingRight;
	
				//We want to turn the corner
				if (isInDeadEnd )
				{		
					//You can only turn once
					if (!deadEndTurnDone)
					{
						//Remember the Z value of the active trigger when the player decided to do a side-move
						float sideMoveInitiatedZ = getSideMoveInitiatedPosition();
						
						//Only allow the side-move if the player did it before the center of the left lane was passed.
						if (sideMoveInitiatedZ > -laneLimit )
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
									Debug.LogWarning("moveToSide: game over - player turned wrong way." );
									reasonDiedAtTurn = "WRONG WAY 1";
									managePlayerDeath ( DeathType.Turn );
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
									Debug.LogWarning("moveToSide: game over - player turned wrong way." );
									reasonDiedAtTurn = "WRONG WAY 2";
									managePlayerDeath ( DeathType.Turn );
								}
							}
						}
						else
						{
							//Give a chance to the player even if he turned late
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
									Debug.LogWarning("moveToSide: game over - player turned wrong way too late." );
									reasonDiedAtTurn = "LATE WRONG WAY 3";
									managePlayerDeath ( DeathType.Turn );
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
									Debug.LogWarning("moveToSide: game over - player turned wrong way too late." );
									reasonDiedAtTurn = "LATE WRONG WAY 4";
									managePlayerDeath ( DeathType.Turn );
								}
							}
						}
					}
					else
					{
						reasonDiedAtTurn = "ALREADY TURNED";
						Debug.LogWarning("moveToSide: turn denied since player has already turned." );
	
					}
				}
				else
				{
					//We are not in a dead end
					//You can only change lanes while running
	 				if ( _characterState == CharacterState.Running )
					{
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
								setCharacterState( CharacterState.SideMove );
								moveDirection.x = currentSideMoveSpeed;
								GetComponent<AudioSource>().PlayOneShot( sideMoveSound );
								Debug.Log ("moveToSide completed " + isGoingRight + " to lane " + desiredLane );
		
							}
							else
							{
								desiredLane = Lanes.Left;
								setCharacterState( CharacterState.SideMove );
								moveDirection.x = -currentSideMoveSpeed;
								GetComponent<AudioSource>().PlayOneShot( sideMoveSound );
								Debug.Log ("moveToSide completed " + isGoingRight + " to lane " + desiredLane );
							}
						}
						else if ( currentLane == Lanes.Right && !isGoingRight )
						{
							desiredLane = Lanes.Center;
							setCharacterState( CharacterState.SideMove );
							moveDirection.x = -currentSideMoveSpeed;
							GetComponent<AudioSource>().PlayOneShot( sideMoveSound );
							Debug.Log ("moveToSide completed " + isGoingRight + " to lane " + desiredLane );
						}
						else if ( currentLane == Lanes.Left && isGoingRight )
						{
							desiredLane = Lanes.Center;
							setCharacterState( CharacterState.SideMove );
							moveDirection.x = currentSideMoveSpeed;
							GetComponent<AudioSource>().PlayOneShot( sideMoveSound );
							Debug.Log ("moveToSide completed " + isGoingRight + " to lane " + desiredLane );
						}
					}
				}
			}
		}
	}
	
	void finalizeSideMove()
	{
		if (jumping)
		{
			setCharacterState( CharacterState.Jumping );
		}
		else
		{
			setCharacterState( CharacterState.Running );					
		}
		moveDirection.x = 0;
		currentLane = desiredLane;
	}

	void verifyIfDesiredLaneReached()
	{
		if ( _characterState == CharacterState.SideMove || _characterState == CharacterState.Stumbling )
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
			if( _characterState != CharacterState.SideMove )
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
					if( _characterState != CharacterState.Sliding )
					{
						/*I used to change the controller capsule height,radius and position.
						  However, when sliding across the Entrance trigger, it would cause multiple
						  OnTriggerEnter events to be triggered causing in turn, the playerTileIndex
						  to be incremented incorrectly and ultimately, there was no more floor
						  under the player and he would fall. So instead, I disable and re-enable
						  overhead obstacles.*/
						deactivateOverheadObstacles( true );
						setCharacterState( CharacterState.Sliding );
						dustPuff.loop = true;
						dustPuff.Play();

						anim.SetTrigger(Slide_DownTrigger);

						playSound( slidingSound, true );
					}
				}
			}
		}
	}

	void verifySlide()
	{
		if ( _characterState == CharacterState.Sliding || _characterState == CharacterState.Turning_and_sliding )
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
				if( _characterState == CharacterState.Turning_and_sliding )
				{
					setCharacterState( CharacterState.Sliding );
				}
				else
				{
					setCharacterState( CharacterState.Running );
				}
				anim.SetTrigger(Slide_UpTrigger);
				GetComponent<AudioSource>().Stop();
				deactivateOverheadObstacles( false );
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
				//If a collider is a trigger, it no longer behaves like a solid object
            	collider.isTrigger = deactivate;
			}
			if ( collider.name == "Portcullis" )
			{
				//If a collider is a trigger, it no longer behaves like a solid object
				collider.isTrigger = deactivate;
			}
			else if ( collider.name.StartsWith("Flame") )
			{
            	collider.enabled = !deactivate;
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

			if (coin.gameObject != null )
			{
				coin.position = Vector3.Lerp( coinDestination, originalPos, time / originalTime );
				coin.rotation = mainCamera.rotation;
			}
			
			yield return _sync();
		}
		if (coin.gameObject != null )
		{
			Destroy ( coin.gameObject );
		}

	}
	
	void OnControllerColliderHit (ControllerColliderHit hit )
	{
		if( _characterState != CharacterState.Dying )
		{
			//The CharacterController is constantly colliding with the Quads making up the floor. Ignore those events.
			if (hit.collider.name == "Quad" ) return;
	
			//Debug.Log ("OnControllerColliderHit  " + hit.collider.name  );
			if (hit.collider.name == "DeadTree" )
			{
				if( hit.normal.y < 0.4f )
				{
					Debug.Log( "PLayer collided with: " + hit.collider.name + " Normal" + hit.normal );
					//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
					//Move the player back so he does not float in the air in case he hit the log while landing after a jump.
					controller.Move( hit.normal );
					managePlayerDeath ( DeathType.Obstacle );
				}
			}
			else if( hit.collider.name.StartsWith("Chicken") && !CheatManager.Instance.getIsInvincible() )
			{
				Transform chickenTransform = hit.transform.FindChild("Chicken Trigger");
				if( chickenTransform == null )
				{
					Debug.LogError("PlayerController-OnControllerColliderHit: chicken collision error. Could not find chicken trigger.");
					return;
				}

				ChickenController chickenController = chickenTransform.gameObject.GetComponent<ChickenController>();

				if( _characterState == CharacterState.Sliding )
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
						HUDHandler.displayCoinTotal( 10, Color.yellow, false );

						//Give the Chicken Chaser achievement
						chicken_chaser.incrementCounter();

						//The faster the player runs, the further the chicken will fly
						float pushPower = runSpeed * 4f;

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
						controller.Move( hit.normal );
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
			else if (hit.collider.name.StartsWith( "Stumble" ) && !CheatManager.Instance.getIsInvincible() )
			{
				Stumble();
			}
			else if (hit.collider.name.StartsWith( "Zombie" ) && !CheatManager.Instance.getIsInvincible() )
			{
				ZombieController zombieController = (ZombieController) hit.gameObject.GetComponent("ZombieController");
				//Ignore collision event if Zombie already dead.
				if( zombieController.getZombieState() != ZombieController.ZombieState.Dying )
				{
					//You can't make a crawling zombie fall backwards
					if( _characterState == CharacterState.Sliding && zombieController.getZombieState() != ZombieController.ZombieState.Crawling )
					{
						//Give stars
						PlayerStatsManager.Instance.modifyCurrentCoins( 10, true, false );
						
						//Display coin total picked up icon
						HUDHandler.displayCoinTotal( 10, Color.yellow, false );

						zombieController.fallToBack();
						
					}
					else
					{
						//Debug.Log( "Player collided with zombie: " + hit.collider.name + " Normal" + hit.normal.y );
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
							watch_your_step.incrementCounter();
						}
					}
				}
			}
			else if (hit.collider.name.StartsWith( "Firepit" ) && !CheatManager.Instance.getIsInvincible() )
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
					if( _characterState == CharacterState.Sliding )
					{
						//Give stars
						PlayerStatsManager.Instance.modifyCurrentCoins( 10, true, false );

						//To do
						//Display coin total picked up icon
						HUDHandler.displayCoinTotal( 10, Color.yellow, false );

					}	
					else
					{
						Stumble();
					}
				}
			}
			else if (hit.collider.name.StartsWith( "Breakable Pumpkin" ) )
			{
				BreakableObject bo = (BreakableObject) hit.collider.GetComponent("BreakableObject");
				Debug.Log( "PLayer collided with breakable: " + hit.collider.name );
				//We pass the player collider to triggerBreak() because we do not want the barrel fragments to collide with the player.
				bo.triggerBreak( GetComponent<Collider>() );
				if( _characterState == CharacterState.Sliding )
				{
					//Give stars
					PlayerStatsManager.Instance.modifyCurrentCoins( 10, true, false );
					
					//To do
					//Display coin total picked up icon
					HUDHandler.displayCoinTotal( 10, Color.yellow, false );
					
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
					controller.Move( hit.normal );
					managePlayerDeath ( DeathType.Obstacle );
				}
			}
			else if (hit.collider.name == "Pendulum" && !CheatManager.Instance.getIsInvincible() )
			{
				//Move the player back so he does not get stuck in the pendulum.
				controller.Move( hit.normal );
				managePlayerDeath ( DeathType.Obstacle );
			}
			else if (hit.collider.name.StartsWith( "Cow" ) && !CheatManager.Instance.getIsInvincible() )
			{
				Debug.Log( "Player collided with: " + hit.collider.name + " Normal" + hit.normal );
				SimpleController simpleController = (SimpleController) hit.collider.GetComponent("SimpleController");
				simpleController.playHitAnim();
				if( hit.normal.y < 0.4f )
				{
					//Player collided with cow squarely
					//Move the player back so he does not get stuck in the cow.
					controller.Move( hit.normal );
					managePlayerDeath ( DeathType.Obstacle );
				}
			}
			else if (hit.collider.name.StartsWith("Fence") || hit.collider.name.StartsWith("Wall") || hit.collider.name.StartsWith("Portcullis") )
			{
				Debug.Log( "Player collided with: " + hit.collider.name + " Normal" + hit.normal );
				//Move the player back so he does not get stuck in the fence.
				controller.Move( hit.normal );
				managePlayerDeath ( DeathType.Obstacle );
			}

			//If the player collided with an object (like a zombie) while doing a side move, the character controller
			//may have shifted. Because of that, maybe the conditions to finalize the side move will never occur and we will be stuck 
			//with a non-zero moveDirection.x component which will cause the player to always drift in one direction on mobile.
			//So force reset in this case.
			//if( _characterState == CharacterState.SideMove )
			//{
				//Debug.LogError ("Resetting lane info because of collision with " + hit.collider.name ); 
				//resetLaneInfo();
				//finalizeSideMove();
			//}
			
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		//Carefull, if you turn right inside a deadEnd OnTriggerEnter will be called a second time (but not if your turn left).
		//This is probably a Unity bug.
		if( other.name == "deadEnd" )
		{
			isInDeadEnd = true;
			wantToTurn = false;

			deadEnd deadEndComponent = (deadEnd) other.GetComponent(typeof(deadEnd));
			currentDeadEndType = deadEndComponent.deadEndType;
			deadEndTrigger = other;
		}
		else if ( other.name.StartsWith( "Coin" ) && !PowerUpManager.isThisPowerUpActive( PowerUpType.Magnet ) )
		{
			//Disable the collider
			other.enabled = false;
			PlayerStatsManager.Instance.modifyCoinCount( other.gameObject );
			CoinManager.playCoinPickupSound();
			//We don't want it to turn while flying up
			Destroy ( other.GetComponent ( "Rotator" ) );
			StartCoroutine( moveCoin( other.transform ) );
		}
		else if( other.name == "Dragon Trigger" )
		{
			//Disable the Dragon Trigger collider sphere as we dont need it for the time being
			other.enabled = false;
			//The dragon is the parent object of the collider sphere
			Dragon = other.transform.parent;
			//We no longer want the Dragon's Lair tile to be the parent of the Dragon
			Dragon.parent = null;
			positionDragon();
			startFlying();			
		}
		else if( other.name.StartsWith("Flame") && !CheatManager.Instance.getIsInvincible() )
		{
			//Is the player protected by a Shield Power Up?
			if( PowerUpManager.isThisPowerUpActive( PowerUpType.Shield ) )
			{
				//This Power Up only works one time, so deactivate it
				powerUpManager.deactivatePowerUp( PowerUpType.Shield, false );
			}
			else
			{
				managePlayerDeath ( DeathType.Flame );
			}
		}
		else if( other.name == "Lava" )
		{
			Debug.Log ("Player fell into lava.");
			trollController.stopPursuing();
			sc.lockCamera( true );
			managePlayerDeath( DeathType.Lava );
		}
		else if( other.name .StartsWith("BallOfFire") )
		{
			if( _characterState != CharacterState.Sliding )
			{
				Debug.Log ("Player was hit by fireball.");
				managePlayerDeath ( DeathType.Fireball);
			}
		}
 		else if( other.name == "RiverCollider" )
		{
			if( _characterState != CharacterState.Dying )
			{
				//Create a water splash
				Debug.Log ("Player fell into river.");
				other.GetComponent<ParticleSystem>().transform.position.Set ( transform.position.x, transform.position.y + 0.8f, transform.position.z );
				other.GetComponent<ParticleSystem>().Play();
				other.GetComponent<AudioSource>().PlayOneShot(other.GetComponent<AudioSource>().clip);
				//Also hide the coin pack since it gets in the way of the camera
				Transform riverCoinPack = currentTile.transform.FindChild("CoinPack8x1_1x10_river");
				if( riverCoinPack != null ) riverCoinPack.gameObject.SetActive(false);
				allowDistanceTravelledCalculations = false;
				managePlayerDeath ( DeathType.Water );
			}
		}
		else if( other.name == "Checkpoint" )
		{
			//Player has successfully completed the current level.
			Debug.Log ("Checkpoint triggered ");
			allowDistanceTravelledCalculations = false;
			//Report score to Game Center if not in the Unity Editor
			#if !UNITY_EDITOR
			GameCenterManager.reportLeaderboard( GameCenterManager.DistanceRunAllLevels, PlayerStatsManager.Instance.getDistanceTravelled() );
			#endif
			trollController.stopPursuing ();
			StartCoroutine( SoundManager.fadeOutMusic( 6.3f ) );
			StartCoroutine( SoundManager.fadeOutAmbience( 6.3f ) );
			GameManager.Instance.setGameState(GameState.Checkpoint);
			StartCoroutine( slowDownPlayer( 16f ) );
		}
		else if( other.name == "Entrance" )
		{
			SegmentInfo si = other.transform.parent.GetComponent<SegmentInfo>();
			if( si != null )
			{
				if( !si.entranceCrossed )
				{
					//We might recycle currentTile (the one prior to the one we just entered), this is why we are passing it as a parameter.
					gl.tileEntranceCrossed( other.transform.parent );
					//This flag is set to avoid tileEntranceCrossed being called multiple time which can happen with onTriggerEnter.
					//This flag is set to false when a tile is added.
					si.entranceCrossed = true;
				}
				currentTilePos = si.tile.transform.position;
				currentTile = si.tile;
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
			//We do not want the player to be jumping or sliding as he reaches the finish line,
			//so disable controls as soon he enters the last tile.
			//Disable run acceleration as well.
			allowRunSpeedToIncrease = false;
			enablePlayerControl( false );
			//If he was sliding, making him run again
			if ( _characterState == CharacterState.Sliding )
			{
				//We are stopping sliding
				dustPuff.Stop();
				setCharacterState( CharacterState.Running );
				anim.SetTrigger(Slide_UpTrigger);
				GetComponent<AudioSource>().Stop();
			}
			placePlayerInCenterLane();
		}
		else if( other.name == "DragonTileEntrance" )
		{
			//We do not want the player to be jumping or sliding as he reaches the dragon.
			enablePlayerControl( false );
			placePlayerInCenterLane();
			//We do not want the player to collide with the dragon
			GameObject go = GameObject.Find("Dragon");
			Physics.IgnoreCollision( transform.GetComponent<Collider>(), go.GetComponent<Collider>());
		}
		else if( other.name == "StopFlyingTileEntrance" )
		{
			enablePlayerControl( false );
			moveDirection = new Vector3( 0,0,0 );
			accelerometerPreviousFrameX = 0;
			accelerometerPreviousFrameY = 0;
			//The dragon might have passed through a dead end, so put value to false so we don't die when we do our next move to side.
			isInDeadEnd = false;
			wantToTurn = false;

			dragonHasControl = true;
			//Center the player before dropping him off of the dragon
			float playerRotY = Mathf.Floor ( transform.eulerAngles.y );
			if( playerRotY == 0 )
			{
				//Center on x axis
				transform.position = new Vector3( currentTilePos.x, transform.position.y, transform.position.z );
			}
			else
			{
				//Center on z axis
				transform.position = new Vector3( transform.position.x, transform.position.y, currentTilePos.z );
			}
		}
		else if( other.name == "Stop Flying Trigger" )
		{
			ejectPlayerFromDragon();
		}

   	}
	
	//Make sure the player arrives to the dragon or castle in the center lane
	private void placePlayerInCenterLane()
	{
		//Clear move direction of any values. If we still have an x component for example, we will drift.
		moveDirection = new Vector3( 0,0,0 );
		accelerometerPreviousFrameX = 0;
		//Side move speed is divided by 2 because it just looks better.
		if ( currentLane == Lanes.Left )
		{
			desiredLane = Lanes.Center;
			setCharacterState( CharacterState.SideMove );
			moveDirection.x = sideMoveSpeed/2f;
		}
		else if ( currentLane == Lanes.Right )
		{
			desiredLane = Lanes.Center;
			setCharacterState( CharacterState.SideMove );
			moveDirection.x = -sideMoveSpeed/2f;
		}
	}
	


	void OnTriggerStay(Collider other)
	{
		if( wantToTurn && _characterState != CharacterState.Flying )
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
		if( GameManager.Instance.getGameState() != GameState.Resurrect )
		{
			if( other.name == "deadEnd" && _characterState != CharacterState.Flying )
			{
				if( !deadEndTurnDone && currentDeadEndType != DeadEndType.None && currentDeadEndType != DeadEndType.RightStraight)
				{
					reasonDiedAtTurn = "EXITED DEAD END NO TURN";
					Debug.LogWarning("OnTriggerExit player exited dead end without turning " + other.name + " " + isInDeadEnd + " " + deadEndTurnDone + " " + currentDeadEndType );
					managePlayerDeath ( DeathType.Turn );
				}
				//Reset values
				isInDeadEnd = false;
				deadEndTurnDone = false;
				deadEndTrigger = null;
				wantToTurn = false;
			}
			else if( other.name == "DeadTree" || other.name.StartsWith( "Stumble" ) || other.name == "cart" || other.name.StartsWith( "Breakable" ) || other.name == "Chicken" || other.name == "Pendulum" )
			{
				if( PowerUpManager.isThisPowerUpActive( PowerUpType.Shield ) )
				{
					//This Power Up only works one time, so deactivate it
					powerUpManager.deactivatePowerUp( PowerUpType.Shield, false );
				}
			}
			
			//Debug.Log ("OnTriggerExit " + other.name);
		}
	}

	public void teleportLeaveComplete()
	{
		appearFX.Play ();
		transform.localScale = new Vector3( 0.002f, 0.002f, 0.002f );
		appearFX.transform.position = new Vector3( transform.position.x, currentTilePos.y + 3.7f, transform.position.z );

	}
	
	public IEnumerator slowDownPlayer( float distance )
	{

		float percentageComplete = 0;

		Vector3 initialPlayerPosition = new Vector3( transform.position.x, transform.position.y, transform.position.z );
		float distanceTravelled = 0;

		float startSpeed = getPlayerSpeed();
		float endSpeed = SLOW_DOWN_END_SPEED;

		float startBlendFactor = blendFactor;

		float startAnimationSpeed = anim.speed;
		float endAnimationSpeed = 1f;

		do
		{
			distanceTravelled = Vector3.Distance( transform.position, initialPlayerPosition );
			percentageComplete = distanceTravelled/distance;

			//Update run speed
			runSpeed =  Mathf.Lerp( startSpeed, endSpeed, percentageComplete );

			//Update the blend amount between Run and Sprint animations based on the current run speed
			blendFactor =  Mathf.Lerp( startBlendFactor, 0, percentageComplete );
			anim.SetFloat(speedBlendFactor, blendFactor);

			//update animation speed
			anim.speed = Mathf.Lerp( startAnimationSpeed, endAnimationSpeed, percentageComplete );

			//move player forward
			//1) Get the direction of the player
			forward = transform.TransformDirection(Vector3.forward);
			//2) Scale vector based on run speed
			forward = forward * Time.deltaTime * runSpeed;
			//3) Add Y component for gravity. Both the x and y components are stored in moveDirection.
			forward.Set( forward.x, moveDirection.y * Time.deltaTime, forward.z );
			//4) Move the controller
			controller.Move( forward );

			yield return new WaitForFixedUpdate(); 
		}
		while ( distanceTravelled <= distance );
		
		setCharacterState( CharacterState.Winning );
		anim.SetTrigger(VictoryTrigger);
		//See Cullis Gate for next steps.
	}
	
	void turnNow()
	{
		wantToTurn = false;
		deadEndTurnDone = true;
		currentLane = desiredLane;
		//Hack - put moveDirection.x to zero in case finalizeSideMove was never called because of a collision
		moveDirection.x = 0;
		if( currentDeadEndType == DeadEndType.LeftRight )
		{
			Debug.Log("PlayerController-turnNow: player turned " + isGoingRight + " at T-Junction." + currentTile.name );
			//Now that we know which way the player is turning, tell the level generator right away
			//so he can activate the proper tiles and, if needed, move the tiles on the Right of the T-Junction to 
			//the Left assuming the player decided to turn that way.
			//We want to do this early to avoid the tiles popping into view.
			gl.playerTurnedAtTJunction( isGoingRight, currentTile );
		}

		GetComponent<AudioSource>().PlayOneShot( sideMoveSound );
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

		if( _characterState == CharacterState.Turning_and_sliding )
		{
			setCharacterState( CharacterState.Sliding );
		}
		else
		{
			setCharacterState( CharacterState.Running );
		}

		//Debug.Log ("turnNow completed " + isGoingRight + " " + transform.eulerAngles.y + " " + _characterState );

	}

	public void managePlayerDeath( DeathType deathTypeValue )
	{
		//Only proceed if the player is not dying already
		if ( _characterState != CharacterState.Dying )
		{
			Debug.Log("managePlayerDeath : " + deathTypeValue );

			if( _characterState == CharacterState.Flying )
			{
				stopFlying();
			}

			//If the player was looking over his shoulder, disable that
			disableLookOverShoulder();

			//Disable the player's shadow
			shadowProjector.enabled = false;

			//Change his collider since he is now lying on the ground,
			//in particular so zombies will not be able to walk through his body
			controller.center = new Vector3( 0, 0.9f ,-1f );
			controller.radius = 0.9f;

			//Remember the run speed at time of death because we want to start running again (in case of revive) at a 
			//percentage of this value.
			//When we jump, the run speed is reduced.
			//If we died while jumping, we want to use runSpeedBeforeJump and not runSpeed.
			if( _characterState == CharacterState.Jumping )
			{
				runSpeedAtTimeOfDeath = runSpeedBeforeJump;
			}
			else
			{
				runSpeedAtTimeOfDeath = runSpeed;
			}

			runSpeed = 0;
			runSpeedBeforeJump = 0;
			speedBoost = 0;
			allowRunSpeedToIncrease = false;

			//Change character state
			setCharacterState( CharacterState.Dying );

			//Stop the dust particle system. It might be playing if we died while sliding.
			dustPuff.Stop();
	
			//Remember how we died
			deathType = deathTypeValue;
			
			//Disable player controls when dying
			enablePlayerControl( false );
			
			//Deactivate any queued jump or slide
			queueJump = false;
			queueSlide = false;

			//Reset move direction and forward
			moveDirection = new Vector3( 0,moveDirection.y,0 );

			//Fade out the music
			StartCoroutine( SoundManager.fadeOutMusic(1f, 0.25f) );

			//Stop any currently playing sound
			GetComponent<AudioSource>().Stop();

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
					anim.SetTrigger(DeathWallTrigger);
					break;
			
				case DeathType.Flame:
					playSound( deathFireSound, false );
					anim.SetTrigger(DeathWallTrigger);
					break;
		                
		        case DeathType.Obstacle:
					//Play collision sound
					playSound( dyingSound, false );
					sc.Shake();
					anim.SetTrigger(DeathWallTrigger);
					break;

		        case DeathType.Water:
					sc.lockCamera ( true );
					anim.speed = 2.8f;
					anim.SetTrigger(DeathRiverTrigger);
					StartCoroutine( waitBeforeDisplayingSaveMeScreen(2.5f) );
					break;

				default:
					anim.SetTrigger(DeathWallTrigger);
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
		GetComponent<AudioSource>().PlayOneShot( footstepLeftSound, 0.2f );
	}

	public void Footstep_right ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( footstepRightSound, 0.2f );
	}

	public void Land_sound ( AnimationEvent eve )
	{
		GetComponent<AudioSource>().PlayOneShot( landSound, 0.28f );
	}

	public IEnumerator waitBeforeDisplayingSaveMeScreen ( float duration )
	{
		anim.speed = 1f;
		yield return new WaitForSeconds(duration);
		//Save the player stats before continuing
		PlayerStatsManager.Instance.savePlayerStats();
		GameManager.Instance.setGameState( GameState.SaveMe );
	}

	public void stumble_completed ( AnimationEvent eve )
	{
		setCharacterState( CharacterState.Running );
	}
	
	public void Stumble()
	{
		//The OnControllerColliderHit function can send multiple collision events during a single
		//stumble, so ignore any new events while in the stumbling state.
		if ( _characterState != CharacterState.Stumbling && _characterState != CharacterState.Dying )
		{	
			Debug.Log ("Player stumbled");
			//sc.Shake();
			//Play player stumble animation by setting the state
			setCharacterState( CharacterState.Stumbling );
			//audio.PlayOneShot( stumblingSound );
			//Possibly vibrate device
			//Possibly shake the camera
			//Make enemy appear right behind player
			//Note that "placeEnemyBehindPlayer" may change the state of the character to Dying
			if( trollController.didPlayerStumblePreviously() )
			{
				//The player falls forward and dies (killed by the troll)
				anim.SetTrigger(FallForwardTrigger);
			}
			else
			{
				//The player stumbles but recovers
				anim.SetTrigger(StumbleTrigger);
			}
			trollController.placeTrollBehindPlayer();
		}
	}

	public void disableLookOverShoulder()
	{
		StopCoroutine("lookOverShoulderAnim");
		anim.SetFloat( lookbackBlendFactor, 0f );
	}

	public void lookOverShoulder()
	{
		StartCoroutine("lookOverShoulderAnim");
	}

	public IEnumerator lookOverShoulderAnim()
	{
		float elapsedTime = 0;
		
		float fadeInDuration = 0.4f;
		float stayDuration = 0.4f;
		float fadeOutDuration = 0.5f;
		
		float startBlendFactor = 0;
		float endBlendFactor = 1f;
		anim.SetFloat( lookbackBlendFactor, 1f );
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
	
	private void resetSharedLevelData()
	{
		//Reset values
		//teleportLeaveComplete changes the scale value so we need to reset it
		transform.localScale = new Vector3( 1f, 1f, 1f );

		disableLookOverShoulder();

		//Re-enable the player's blob shadow
		shadowProjector.enabled = true;

		//Variable for swipe
		touchStarted = false;

		//Character Controller
		controller.center = new Vector3( 0, 0.9f ,0f );
		controller.radius = 0.5f;
		
		//Lanes
		currentLane = Lanes.Center;
		desiredLane = Lanes.Center;
		myLane = 0;

		deactivateOverheadObstacles( false );
		jumping = false;
		jumpingReachedApex = false;
		queueSlide = false;
		queueJump = false;
		isInDeadEnd = false;
		deadEndTrigger = null;
		wantToTurn = false;
		deadEndTurnDone = false;
		sc.lockCamera ( false );
		gravity = DEFAULT_GRAVITY;
		moveDirection = new Vector3(0,0,0);
		accelerometerPreviousFrameX = 0;
		accelerometerPreviousFrameY = 0;
		allowRunSpeedToIncrease = true;
		speedBoost = 0;
		deathType = DeathType.Alive;

		lastSwipe = "";
		reasonDiedAtTurn = "";

	}

	public void resurrectBegin()
	{
		//0) Reset data
		resetSharedLevelData();
		
		//1) Stop pursuit
		trollController.stopPursuing ();

		//2) Hide and reset all zombies
		zombieManager.resetAllZombies();

		//2a) Reset the camera. If a cut-scene played when the player died, the camera parameters such as the FOV may have changed.
		sc.resetCameraParameters();
		
		//3) Reposition dead body at the respawn location.
		//Also, play the dead loop animation
		anim.Play("DeathWall_Loop");
		GameObject respawnLocationObject;

		if( LevelManager.Instance.isTutorialActive() )
		{
			GameObject firstTileOfActiveTutorial = gl.getFirstTileOfActiveTutorial();
			respawnLocationObject = firstTileOfActiveTutorial.transform.Find("respawnLocation").gameObject;
			SegmentInfo si = firstTileOfActiveTutorial.GetComponent<SegmentInfo>();
			if( si != null )
			{
				currentTilePos = si.tile.transform.position;
				currentTile = si.tile;
				tileRotationY = Mathf.Floor ( currentTile.transform.eulerAngles.y );
				currentTileType = si.tileType;
				usesBezierCurve = si.usesBezierCurve;
				if( usesBezierCurve )
				{
					//reset
					curveListIndex = 0;
					curveList = si.curveList;
					createBezier();
					finalizeSideMove();
				}
			}
			else
			{
				Debug.LogError("PlayerController-resurrectBegin: firstTileOfActiveTutorial game object " + firstTileOfActiveTutorial.name +  " does not have a SegmentInfo component attached to it.");
			}
		}
		else
		{
			if( getCurrentTileType() == TileType.T_Junction || getCurrentTileType() == TileType.T_Junction_Landmark_Cemetery )
			{
				//If the player's rotation is zero, this means he has not turned yet.
				//If this is the case, we will assume he turned right.
				float playerRotationY = Mathf.Floor ( transform.eulerAngles.y );
				if( playerRotationY == 0 )
				{
					respawnLocationObject = currentTile.transform.Find("respawnLocationRight").gameObject;
					gl.playerTurnedAtTJunction( true, currentTile );
					
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
		}

		deathType = DeathType.Alive;

		if( respawnLocationObject != null )
		{
			Transform respawn = respawnLocationObject.transform;
			RaycastHit hit;
			float groundHeight = 0f;
	        if (Physics.Raycast(respawn.position, Vector3.down, out hit, 2.0F ))
			{
				groundHeight = hit.point.y;
			}
			//If the player died in a dead end trigger, the trigger will be activated when we move the player's body
			//to the respawn location. This in turn will cause isInDeadEnd to become true and when the player will try to change lanes,
			//he will turn instead and crash into a fence. To avoid that, disable the collider before moving the player and reenable it after.
			transform.GetComponent<Collider>().enabled = false;
			//When he is on the last frame of the dead animation, the player is 0.0328f above the ground
			transform.position = new Vector3( respawn.position.x, groundHeight + 0.0328f, respawn.position.z );
			transform.rotation = Quaternion.Euler ( 0, respawn.localEulerAngles.y + tileRotationY, 0 );
			tileRotationY = Mathf.Floor ( transform.eulerAngles.y );
			transform.GetComponent<Collider>().enabled = true;
		}
		else
		{
			Debug.LogError("PlayerController-ResurrectBegin: Unable to find respawnLocation game object in tile : " + currentTile.name );
		}

		//4) Fly Tinker Bell to body and have play a sprinkle animation
		GameObject fairyPrefab = Resources.Load( "Fairy") as GameObject;
		if( fairyPrefab != null )
		{
			float fairyRotY = transform.eulerAngles.y + 205f;
			Vector3 relativePos = new Vector3(0.3f , 0.4f , 1f );
			Vector3 exactPos = transform.TransformPoint(relativePos);
			fairy = (GameObject)Instantiate(fairyPrefab, new Vector3( exactPos.x, exactPos.y, exactPos.z ), Quaternion.Euler( 0, fairyRotY, 0 ) ) ;
			fairyController = (FairyController) fairy.GetComponent(typeof(FairyController));
			//fairyController.revivePlayer( this );
		}
		else
		{
			Debug.LogError("PlayerController-ResurrectBegin: Unable to find Fairy prefab in Resources folder." );
		}
	
	}

	public void resurrectMiddle()
	{
		//4) Play a sparkling particle effect
		GameObject prefab = Resources.Load( "Particles/Resurrect Lights - Blue") as GameObject;
		GameObject go = (GameObject)Instantiate(prefab, new Vector3( transform.position.x, transform.position.y, transform.position.z ), Quaternion.identity );
		go.GetComponent<ParticleSystem>().Play();

		//5) Play the revive animation
		anim.speed = 1.5f;
		anim.Play( "DeathWall_GetUp" );

		//Destroy fairy and particle effect as we dont need them anymore
		Destroy ( fairy, 1.1f );
		Destroy ( go, 3f );

	}

	public void get_up_completed ( AnimationEvent eve )
	{
		anim.speed = 1f;
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
			if ( collider.name == "DeadTree" || collider.name.StartsWith( "Stumble" ) || collider.name.StartsWith("Flame") || collider.name == "cart" || collider.name.StartsWith( "Breakable" ) || collider.name == "Pendulum" )
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
        }
	}

	//Called every frame to ensure that the critical current lane value is always accurate
	private void recalculateCurrentLane()
	{
		if( !usesBezierCurve && _characterState != CharacterState.Turning && _characterState != CharacterState.Turning_and_sliding && _characterState != CharacterState.SideMove )
		{
			float min = -laneLimit + controller.radius;
			float max = laneLimit - controller.radius;
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
				Debug.LogWarning("recalculateCurrentLane changed current lane from: " + currentLane + " to: " + calculatedLane );
				currentLane = calculatedLane;
				desiredLane = currentLane;
			}
		}
	}
	
	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
	}
	
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
	}

	void GameStateChange( GameState newState )
	{
		//Debug.LogWarning("PlayerController: GameStateChange: " + newState );
		if( newState == GameState.Paused )
		{
			anim.enabled = false;	
		}
		else if( newState == GameState.Countdown )
		{
			
		}
		else if( newState == GameState.Normal )
		{
			anim.enabled = true;	
		}
	}

	void OnApplicationPause( bool pauseStatus )
	{
		Debug.Log( "PlayerController-OnApplicationPause: pauseStatus is: " + pauseStatus );
	}


} 