using UnityEngine;
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
	float flyingSpeed = 6.04f;
	CharacterController controller;
	public GameObject dragonFire;
	public GameObject dragonFireSpotlight;

	bool allowAttack = true;
	bool allowBreatheFire = false;

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
		allowBreatheFire = true;
		if( playerController.getCurrentTileType() == TileType.Opening ) Invoke ("Arrive", 10f );
	}

	// Update is called once per frame
	void LateUpdate ()
	{
		if( ( GameManager.Instance.getGameState() == GameState.Normal || GameManager.Instance.getGameState() == GameState.Checkpoint || GameManager.Instance.getGameState() == GameState.SaveMe ) && dragonState == DragonState.Fly )
		{
			if( allowAttack )
			{
				float distance = Vector3.Distance(player.position,transform.position);
				
				if( distance < 32f && allowBreatheFire )
				{
					breatheFire( "Attack_001");
				}
			}

			moveDragon();
		}
	}

	public void placeDragon( Transform tile, Vector3 localPosition, Vector3 localRotation, string initialAnimation, float flyingSpeed )
	{
		this.flyingSpeed = flyingSpeed;
		transform.parent = tile;
		transform.localPosition = localPosition;
		transform.localRotation = Quaternion.Euler( localRotation );
		dragonAnimation.Play(initialAnimation);
	}

	public void enableAttack( bool value )
	{
		allowAttack = value;
	}

	public void takeOff()
	{
		dragonAnimation[ "Run" ].speed = 2f;
		dragonAnimation.CrossFade("Run", 1f);		
		Vector3 exactPos = transform.TransformPoint(new Vector3( 0,30f,100f));
		transform.LookAt( exactPos );
		dragonState = DragonState.Fly;
	}

	public void roar()
	{
		GetComponent<AudioSource>().loop = false;
		GetComponent<AudioSource>().Play ();
	}

	void moveDragon()
	{

		//0) Target the player but we only want the Y rotation
		if( followsPlayer )
		{
			transform.LookAt( player );
			transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
		}

		//1) Get the direction of the dragon
		forward = transform.TransformDirection(Vector3.forward);			
		//2) Scale vector based on flying speed
		forward = forward * Time.deltaTime * flyingSpeed;
		//3) Move the controller
		controller.Move( forward );

	}


	public void Arrive( )
	{
		print ("*******Dragon arrive");
		Vector3 arrivalStartPos = new Vector3( -100f, 8.5f, PlayerController.getPlayerSpeed() * 16.1f );
		Vector3 exactPos = player.TransformPoint(arrivalStartPos);
		transform.position = exactPos;
		transform.rotation = Quaternion.Euler( 0, player.transform.eulerAngles.y + 90f, transform.eulerAngles.z );
		playAnimation("Run", WrapMode.Loop );
		dragonState = DragonState.Fly;
		//StartCoroutine("MoveToPosition", timeToArrive );
	}
	

	private void breatheFire( string attackAnim )
	{
		allowBreatheFire = false;
		dragonState = DragonState.FireBreathing;
		dragonAnimation[attackAnim].speed = 0.8f;
		dragonAnimation[attackAnim].wrapMode = WrapMode.Once;
		dragonAnimation.CrossFade(attackAnim);				
		Invoke( "breatheFireNowRoutine", dragonAnimation[attackAnim].length * 0.6f);
		Invoke( "resumeFlying", dragonAnimation[attackAnim].length);
		Invoke( "roar", 0.25f );

	}
	
	void breatheFireNowRoutine ()
	{
		StartCoroutine("breatheFireNow");
	}

	IEnumerator breatheFireNow ()
	{
		dragonFireSpotlight.GetComponent<Light>().enabled = true;
		dragonFire.GetComponent<ParticleSystem>().Play();
		float time = dragonAnimation["Attack_001"].length;
		while ( time > 0 )
		{
			time -= Time.deltaTime;
			transform.LookAt( player );
			transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y -10f, 0 );

			yield return _sync();
		}
	}

	private void resumeFlying ()
	{
		dragonFireSpotlight.GetComponent<Light>().enabled = false;
		dragonAnimation.CrossFade("Run");				
		dragonState = DragonState.Fly;
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
		if( eventType == GameEvent.Start_Moving )
		{
		}
		else if( eventType == GameEvent.Stop_Moving )
		{
		} 
	}
	

	void GameStateChange( GameState newState )
	{
		if( dragonAnimation != null )
		{
			if( newState == GameState.Paused )
			{
				dragonAnimation.enabled = false;

			}
			else if( newState == GameState.Normal )
			{
				dragonAnimation.enabled = true;
			}
		}
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
