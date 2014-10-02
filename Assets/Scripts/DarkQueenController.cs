using UnityEngine;
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


	public ParticleSystem floatDownFx;

	Transform player;
	PlayerController playerController;

	DarkQueenState darkQueenState = DarkQueenState.None;

	public ParticleSystem krakenSpellFx;
	public AudioClip krakenSpellSound;

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


	public void arriveAndCastSpell()
	{
		transform.localScale = new Vector3( 1.2f, 1.2f, 1.2f );
		floatDownFx.Play ();
		float arriveSpeed = 0.3f;
		animation["DarkQueen_Arrive"].speed = arriveSpeed;
		animation.Play("DarkQueen_Arrive");
		Invoke("playLandAnimation", animation["DarkQueen_Arrive"].length/arriveSpeed );
		dimLights( animation["DarkQueen_Arrive"].length/arriveSpeed, 0.1f );

	}

	void playLandAnimation()
	{
		AchievementDisplay.activateDisplayDarkQueen( "You should never keep your Queen waiting.", 0.35f, 3.6f );
		audio.PlayOneShot( darkQueenVO );
		animation.CrossFade("DarkQueen_Land", 0.1f);
		Invoke("playIdleAnimation", animation["DarkQueen_Land"].length);
	}

	void playIdleAnimation()
	{
		floatDownFx.Stop ();
		animation.Play("DarkQueen_Idle");
		Invoke("castKrakenSpell", animation["DarkQueen_Idle"].length);
	}

	public void castKrakenSpell()
	{
		AchievementDisplay.activateDisplayDarkQueen( "Rise Sister, rise from the deep...", 0.35f, 3.8f );
		audio.PlayOneShot( darkQueenVO_riseFromTheDeep );
		animation.Play("DarkQueen_SpellCast");
		Invoke("playKrakenSpellFX", 0.3f);
		Invoke("leave", animation["DarkQueen_SpellCast"].length );
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
		animation["DarkQueen_Leave"].speed = 1.2f;
		animation.Play("DarkQueen_Leave");
		brightenLights( animation["DarkQueen_Leave"].length/1.2f );
		Invoke("playerStartsRunningAgain", animation["DarkQueen_Leave"].length/1.2f );
	}

	public void playerStartsRunningAgain()
	{
		dqks.step4();
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
	
	public void Disappear()
	{
		transform.localScale = new Vector3( 0.002f, 0.002f, 0.002f );
		gameObject.SetActive( false );
		darkQueenState = DarkQueenState.None;
	}
	
}
