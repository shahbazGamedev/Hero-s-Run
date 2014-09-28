using UnityEngine;
using System.Collections;

public class GhostController : BaseClass {
	
	public enum GhostState {
		None = 0,
		Arrive = 1,
		Leave = 2,
		Hover = 3,
		Attack = 4
	}
	
	//Components

	public ParticleSystem appearFx;
	public AudioClip appearSound;
	
	Transform player;
	PlayerController playerController;
	
	public GhostState ghostState = GhostState.None;

	// The distance in the x-z plane to the target
	const float DEFAULT_DISTANCE = 2f;
	float distance = DEFAULT_DISTANCE;
	
	// the height we want the ghost to be above the player
	public const float DEFAULT_HEIGHT = 2.8f;
	float height = DEFAULT_HEIGHT;
	
	//Where to position the ghost relative to the player when appearing next to player
	Vector3 ghostRelativePos = new Vector3(-0.6f , DEFAULT_HEIGHT , DEFAULT_DISTANCE );
	
	// How much we 
	const float DEFAULT_HEIGHT_DAMPING = 6f;
	float heightDamping = DEFAULT_HEIGHT_DAMPING;
	
	const float DEFAULT_ROTATION_DAMPING = 3f;
	float rotationDamping = DEFAULT_ROTATION_DAMPING;
	
	const float DEFAULT_Y_ROTATION_OFFSET = 168f;
	float yRotationOffset = DEFAULT_Y_ROTATION_OFFSET;
	
	const float DEFAULT_X_ROTATION = 9f;
	float xRotation = DEFAULT_X_ROTATION;
	
	Vector3 xOffset = new Vector3( 0, 0, 0 );
	
	void Awake()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = player.GetComponent<PlayerController>();
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		if( ( GameManager.Instance.getGameState() == GameState.Normal || GameManager.Instance.getGameState() == GameState.Checkpoint ) && ghostState == GhostState.Hover && playerController.getCharacterState() != CharacterState.Dying )
		{
			positionGhost ();
		}
	}
	
	private void positionGhost ()
	{
		// Calculate the current rotation angles
		float wantedRotationAngle = player.eulerAngles.y + yRotationOffset;
		float wantedHeight = player.position.y + height;
		
		float currentRotationAngle = transform.eulerAngles.y;
		float currentHeight = transform.position.y;
		
		// Damp the rotation around the y-axis
		currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
		
		// Damp the height
		currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);
		
		// Convert the angle into a rotation
		Quaternion currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);
		
		//Order of rotations is ZXY
		
		// Set the position of the ghost on the x-z plane to:
		// distance meters behind the target
		transform.position = player.position;
		transform.position -= currentRotation * Vector3.forward * distance;
		
		// Set the height of the ghost
		transform.position = new Vector3( transform.position.x, currentHeight, transform.position.z );
		
		// Always look at the target
		transform.LookAt (player);
		
		//Tilt the camera down
		transform.rotation = Quaternion.Euler( xRotation, transform.eulerAngles.y, transform.eulerAngles.z );
		
		//More ghost slightly to the left
		Vector3 exactPos = transform.TransformPoint( xOffset );
		transform.position = exactPos;
	}

	public void setYRotationOffset( float offset )
	{
		yRotationOffset = offset;
	}
	
	public void resetYRotationOffset()
	{
		yRotationOffset = DEFAULT_Y_ROTATION_OFFSET;
	}

	public void Arrive( float timeToArrive )
	{
		ghostState = GhostState.Arrive;
		Vector3 arrivalStartPos = new Vector3( 0, 12f, PlayerController.getPlayerSpeed() * 2f );
		Vector3 exactPos = player.TransformPoint(arrivalStartPos);
		transform.position = exactPos;
		float wantedRotationAngle = player.eulerAngles.y + 180f;
		transform.rotation = Quaternion.Euler( 0, wantedRotationAngle, transform.eulerAngles.z );
		StartCoroutine("MoveToPosition", timeToArrive );
	}
	
	public void Appear()
	{
		transform.localScale = new Vector3( 1f, 1f, 1f );
		positionGhost ();
		//appearFx.Play();
		//audio.PlayOneShot( appearSound );
		ghostState = GhostState.Hover;
	}
	
	public void Disappear()
	{
		audio.PlayOneShot( appearSound );
		transform.localScale = new Vector3( 0.002f, 0.002f, 0.002f );
		appearFx.Play();
		Invoke("Disappear_part2", 2.3f);
	}
	
	public void Disappear_part2()
	{
		ghostState = GhostState.None;
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
			
			Vector3 exactPos = player.TransformPoint(ghostRelativePos);
			transform.position = Vector3.Lerp( startPosition, exactPos, fracJourney );
			
			//Tilt the fairy down
			transform.rotation = Quaternion.Euler( -8f, transform.eulerAngles.y, transform.eulerAngles.z );
			
			yield return _sync();  
			
		}
		ghostState = GhostState.Hover;
	}
	

	private void playAnimation( string animationName, WrapMode mode )
	{
		animation[ animationName ].wrapMode = mode;
		animation[ animationName ].speed = 1f;
		animation.CrossFade(animationName, 0.1f);
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
		if( eventType == GameEvent.Start_Ghost && ghostState == GhostState.None )
		{
			Arrive( 2.5f );
		}
		else if( eventType == GameEvent.Stop_Ghost && ghostState != GhostState.None )
		{
		} 
	}

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			print ("Player is touching ghost");
		}
	}
	
	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Paused )
		{
			animation.enabled = false;

		}
		else if( newState == GameState.Normal )
		{
			animation.enabled = true;
		}
	}
}
