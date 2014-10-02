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

	public ParticleSystem floatDownFx;		//Bluish lights that play when she floats down from the sky
	public ParticleSystem spellFx;			//Electric fx that plays when she casts a spell
	public AudioClip spellSound;			//Sound fx that plays when she casts a spell

	PlayerController playerController;

	DarkQueenState darkQueenState = DarkQueenState.None;

	public Vector3 forward;
	float speed = 6f;
	CharacterController controller;

	//Use to dim and brighten lights surrounding the dark queen
	List<LightData> listOfLights = new List<LightData>(30);
	const float MAX_DISTANCE_LIGHT_AFFECTED = 180f;

	void Awake()
	{
		Transform player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = player.GetComponent<PlayerController>();
		controller = GetComponent<CharacterController>();
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
			//2) Scale vector based on move speed
			forward = forward * Time.deltaTime * speed;
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

	public void Disappear()
	{
		transform.localScale = new Vector3( 0.002f, 0.002f, 0.002f );
		gameObject.SetActive( false );
		darkQueenState = DarkQueenState.None;
	}

}
