﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DarkQueenController : BaseClass {
	
	enum  DarkQueenState {
		None = 0,
		Arrive = 1,
		Leave = 2,
		Hover = 3,
		Walk = 4
	}

	public enum LightStatus {
		isNormal = 0,
		isFadingOut = 1,
		isFadingIn = 2,
		isFaded = 3
	}

	//Components
	Animation fairyAnimation;

	public ParticleSystem appearFx;
	public AudioClip appearSound;
	
	public ParticleSystem fairySpellFx;
	public AudioClip fairySpellSound;

	public ParticleSystem floatDownFx;

	Transform player;
	PlayerController playerController;

	DarkQueenState darkQueenState = DarkQueenState.None;

	public ParticleSystem krakenSpellFx;
	public AudioClip krakenSpellSound;

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
	float flyingSpeed = 6f;
	CharacterController controller;

	List<LightData> listOfLights = new List<LightData>(30);
	const float MAX_DISTANCE_LIGHT_AFFECTED = 180f;
	public AudioClip darkQueenVO;
	public AudioClip darkQueenVO_riseFromTheDeep;
	public DarkQueenKrakenSequence dqks;
	public ParticleSystem poisonMist;


	void Awake()
	{
		//Get a copy of the components
		fairyAnimation = (Animation) GetComponent("Animation");
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = (PlayerController) player.gameObject.GetComponent(typeof(PlayerController));
		controller = GetComponent<CharacterController>();
		dqks = GetComponent<DarkQueenKrakenSequence>();
	}

	void Start()
	{
		prepareListOfLights();
	}


	// Update is called once per frame
	void LateUpdate ()
	{
		if( ( GameManager.Instance.getGameState() == GameState.Normal || GameManager.Instance.getGameState() == GameState.Checkpoint ) && darkQueenState == DarkQueenState.Walk && playerController.getCharacterState() != CharacterState.Dying )
		{

			//1) Get the direction of the dark queen
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on flying speed
			forward = forward * Time.deltaTime * flyingSpeed;
			//3) Move the controller
			controller.Move( forward );
		}
	}

	public class LightData
	{
		public Light aLight;
		public float originalIntensity;
		public LightStatus lightStatus = LightStatus.isNormal;
	}

	void prepareListOfLights()
	{
		//Get a list of all lights so we can fade them in and out as the dark queen moves
		LightData ld;
		listOfLights.Clear();
		Light[] lightsArray = FindObjectsOfType(typeof(Light)) as Light[];
		foreach(Light li in lightsArray )
		{
			if( Vector3.Distance(transform.position,li.transform.position) < MAX_DISTANCE_LIGHT_AFFECTED )
			{
				if( li.name != "Staff Light" )
				{
					ld = new LightData();
					ld.aLight = li;
					ld.originalIntensity = li.intensity;
					listOfLights.Add ( ld );
				}
			}
		}
		//Also get the sunlight object
		GameObject Sun = GameObject.FindGameObjectWithTag("Sunlight");
		ld = new LightData();
		ld.aLight = Sun.light;
		ld.originalIntensity = Sun.light.intensity;
		listOfLights.Add ( ld );
		
		
	}

	public void dimLights( float duration, float finalIntensity )
	{
		foreach(LightData ld in listOfLights )
		{
			if( ld.lightStatus == LightStatus.isNormal )
			{
				StartCoroutine(fadeOutLight( ld, duration, finalIntensity ) );
			}
		}
	}

	public void brightenLights( float duration )
	{
		foreach(LightData ld in listOfLights )
		{
			if( ld.lightStatus == LightStatus.isFaded )
			{
				StartCoroutine(fadeInLight( ld, duration ) );
			}
		}
	}

	public IEnumerator fadeOutLight( LightData ld, float duration, float endIntensity )
	{
		float elapsedTime = 0;
		
		float startIntensity = ld.originalIntensity;
		ld.lightStatus = LightStatus.isFadingOut;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			ld.aLight.intensity =  Mathf.Lerp( startIntensity, endIntensity, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		
		ld.aLight.intensity = endIntensity;
		ld.lightStatus = LightStatus.isFaded;

		print ("DarkQueenController-Finished fading out light named " + ld.aLight.name + " to intensity: " + endIntensity );
	}

	public IEnumerator fadeInLight( LightData ld, float duration )
	{
		float elapsedTime = 0;
		
		float startIntensity = ld.aLight.intensity;
		ld.lightStatus = LightStatus.isFadingIn;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			ld.aLight.intensity =  Mathf.Lerp( startIntensity, ld.originalIntensity, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		ld.aLight.intensity = ld.originalIntensity;
		ld.lightStatus = LightStatus.isFaded;
		print ("DarkQueenController-Finished fading in light named " + ld.aLight.name + " to intensity: " + ld.originalIntensity );
	}

	public void setYRotationOffset( float offset )
	{
		yRotationOffset = offset;
	}

	public void resetYRotationOffset()
	{
		yRotationOffset = DEFAULT_Y_ROTATION_OFFSET;
	}

	public void walk( bool enable )
	{
		if( enable )
		{
			darkQueenState = DarkQueenState.Walk;
		}
		else
		{
			darkQueenState = DarkQueenState.None;
		}
	}

	public void floatDown( float height, System.Action callback )
	{
		transform.localScale = new Vector3( 1.2f, 1.2f, 1.2f );
		floatDownFx.Play ();
		LeanTween.moveLocalY(gameObject, gameObject.transform.localPosition.y - height,4f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(callback);
	}
	
	private void positionFairy ()
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

		// Set the position of the fairy on the x-z plane to:
		// distance meters behind the target
		transform.position = player.position;
		transform.position -= currentRotation * Vector3.forward * distance;
		
		// Set the height of the fairy
		transform.position = new Vector3( transform.position.x, currentHeight, transform.position.z );
		
		// Always look at the target
		transform.LookAt (player);
		
		//Tilt the camera down
		transform.rotation = Quaternion.Euler( xRotation, transform.eulerAngles.y, transform.eulerAngles.z );

		//More fairy slightly to the left
		Vector3 exactPos = transform.TransformPoint( xOffset );
		transform.position = exactPos;
	}

	public void arriveAndCastSpell()
	{
		transform.localScale = new Vector3( 1.2f, 1.2f, 1.2f );
		floatDownFx.Play ();
		float arriveSpeed = 0.3f;
		fairyAnimation["DarkQueen_Arrive"].speed = arriveSpeed;
		fairyAnimation.Play("DarkQueen_Arrive");
		Invoke("playLandAnimation", fairyAnimation["DarkQueen_Arrive"].length/arriveSpeed );
		dimLights( fairyAnimation["DarkQueen_Arrive"].length/arriveSpeed, 0.1f );

	}

	void playLandAnimation()
	{
		AchievementDisplay.activateDisplayDarkQueen( "You should never keep your Queen waiting.", 0.35f, 3.6f );
		audio.PlayOneShot( darkQueenVO );
		fairyAnimation.CrossFade("DarkQueen_Land", 0.1f);
		Invoke("playIdleAnimation", fairyAnimation["DarkQueen_Land"].length);
	}

	void playIdleAnimation()
	{
		floatDownFx.Stop ();
		fairyAnimation.Play("DarkQueen_Idle");
		Invoke("castKrakenSpell", fairyAnimation["DarkQueen_Idle"].length);
	}

	public void castKrakenSpell()
	{
		AchievementDisplay.activateDisplayDarkQueen( "Rise Sister, rise from the deep...", 0.35f, 3.8f );
		audio.PlayOneShot( darkQueenVO_riseFromTheDeep );
		fairyAnimation.Play("DarkQueen_SpellCast");
		Invoke("playKrakenSpellFX", 0.3f);
		Invoke("leave", fairyAnimation["DarkQueen_SpellCast"].length );
	}

	void playKrakenSpellFX()
	{
		audio.PlayOneShot( krakenSpellSound );
		krakenSpellFx.Play();
		poisonMist.Play();
	}

	public void leave()
	{
		floatDownFx.Play ();
		fairyAnimation["DarkQueen_Leave"].speed = 1.2f;
		fairyAnimation.Play("DarkQueen_Leave");
		brightenLights( fairyAnimation["DarkQueen_Leave"].length/1.2f );
		Invoke("playerStartsRunningAgain", fairyAnimation["DarkQueen_Leave"].length/1.2f );
	}

	public void playerStartsRunningAgain()
	{
		dqks.step4();
	}

	public void stopFloatDownFx()
	{
		floatDownFx.Stop ();

	}

	public void Arrive( float timeToArrive )
	{
		darkQueenState = DarkQueenState.Arrive;
		Vector3 arrivalStartPos = new Vector3( -18f, 12f, PlayerController.getPlayerSpeed() * 2f );
		Vector3 exactPos = player.TransformPoint(arrivalStartPos);
		transform.position = exactPos;
		transform.rotation = Quaternion.Euler( 0, player.transform.eulerAngles.y + 90f, transform.eulerAngles.z );
		StartCoroutine("MoveToPosition", timeToArrive );
	}

	public void Appear()
	{
		transform.localScale = new Vector3( 1f, 1f, 1f );
		positionFairy ();
		fairyAnimation.Play("Hover_Worried");
		gameObject.SetActive( true );
		appearFx.Play();
		audio.PlayOneShot( appearSound );
		darkQueenState = DarkQueenState.Hover;
	}

	public void Disappear()
	{
		audio.PlayOneShot( appearSound );
		transform.localScale = new Vector3( 0.002f, 0.002f, 0.002f );
		appearFx.Play();
		Invoke("Disappear_part2", 2.3f);
		floatDownFx.Stop ();

	}

	public void Disappear_part2()
	{
		gameObject.SetActive( false );
		darkQueenState = DarkQueenState.None;
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
		darkQueenState = DarkQueenState.Hover;
	}
	
}
