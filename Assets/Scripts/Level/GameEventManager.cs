using UnityEngine;
using System.Collections;

public class GameEventManager : MonoBehaviour {

	WeatherManager weatherManager;
	Lightning lightning;
	SimpleCamera simpleCamera;
	Transform player;
	PlayerController playerController;
	TrollController trollController;
	FairyController fairyController;

	OpeningSequence op;

	TentaclesSequence tentaclesSequence;

	GameState previousGameState = GameState.Unknown;

	Vector3 lastTentaclePosition;
	
	// Use this for initialization
	void Awake () {

		GameObject weatherManagerObject = GameObject.FindGameObjectWithTag("WeatherManager");
		weatherManager = weatherManagerObject.GetComponent<WeatherManager>();

		GameObject sunlightObject = GameObject.FindGameObjectWithTag("Sunlight");
		lightning = sunlightObject.GetComponent<Lightning>();

		GameObject trollObject = GameObject.FindGameObjectWithTag("Troll");
		trollController = trollObject.GetComponent<TrollController>();

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		player = playerObject.transform;
		playerController = playerObject.GetComponent<PlayerController>();
		simpleCamera = player.GetComponent<SimpleCamera>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();

	}

	//Kraken tentacles sequence
	public void setOpeningSequence( TentaclesSequence tentaclesSequence )
	{
		this.tentaclesSequence = tentaclesSequence;

	}

	public void playTentaclesSequence()
	{
		print ("Start of tentacles sequence");
		InvokeRepeating( "startPierceUp", 0.2f, 2f );
	}

	public void stopTentaclesSequence()
	{
		print ("stop tentacles sequence");
		CancelInvoke( "startPierceUp" );
		CancelInvoke( "pierceUp" );
	}

	void startPierceUp()
	{
		Invoke( "pierceUp", 0.33f );
		float attackDistance = 0.81f * PlayerController.getPlayerSpeed();
		//Pick random X location
		float xPos;
		int laneChoice = Random.Range(0, 3);
		if( laneChoice == 0 )
		{
			xPos = -PlayerController.laneLimit;
		}
		else if( laneChoice == 1 )
		{
			xPos = 0;
		}
		else
		{
			xPos = PlayerController.laneLimit;
		}
		lastTentaclePosition = player.TransformPoint(new Vector3( xPos,-2.9f,attackDistance));
		//Display a sign that a tentacle is going to shoot up from the ground to warn the player
		ParticleSystem dust = (ParticleSystem)Instantiate(tentaclesSequence.tentacleAboutToAppearFx, Vector3.zero, Quaternion.identity );
		dust.transform.position = new Vector3( lastTentaclePosition.x, lastTentaclePosition.y + 3f, lastTentaclePosition.z );
		dust.Play();
		GameObject.Destroy( dust, 3f );
	}

	void pierceUp()
	{
		print ("Shooting up tentacle");
		playerController.shakeCamera();
		GameObject go = (GameObject)Instantiate(tentaclesSequence.tentaclePrefab, Vector3.zero, Quaternion.identity );
		go.transform.position = lastTentaclePosition;
		go.transform.rotation = player.rotation;
		go.name = "Fence";
		LeanTween.moveLocalY(go, go.transform.position.y + 2, 1.15f ).setEase(LeanTweenType.easeOutExpo);
		go.audio.Play ( (ulong)1.15 );
		GameObject flyingDebris = (GameObject)Instantiate(tentaclesSequence.debrisPrefab, Vector3.zero, Quaternion.identity );
		flyingDebris.transform.position = new Vector3( lastTentaclePosition.x, lastTentaclePosition.y + 4f, lastTentaclePosition.z );
		BreakableObject bo = flyingDebris.GetComponent<BreakableObject>();
		bo.triggerBreak( player.collider );
		//We only want to keep the tentacle for a few seconds
		GameObject.Destroy( go, 3f );
	}


	//ISLAND TOWER OPENING SEQUENCE START
	public void setOpeningSequence( OpeningSequence openingSequence )
	{
		op = openingSequence;

		weatherManager.setFogTarget( simpleCamera.cutsceneCamera, 30f );
		weatherManager.setWeatherTarget( op.rainLocation, 0 );
		weatherManager.activateRain( true );
		lightning.controlLightning(GameEvent.Start_Lightning);
		//Position player near the top of the tower
		player.position = new Vector3( 0, 79.34f, -2f );
		simpleCamera.playCutscene( CutsceneType.OpeningSequence );
		trollController.stopPursuing();
		op.InvokeRepeating("playCrowSound", 3.5f, 10f );
	}
	

	public void playOpeningSequence()
	{
		Invoke ("step1", 0.2f );
	}

	//Explosion starts
	void step1()
	{
		//Explosion sound
		op.playExplosionSound();
		//Explosion effect
		op.explosionFx.Play ();
		//Hide tower door
		//Unfortunately, the tower door has a mesh that is not centered properly; it is centered at the botoom of the tower.
		//This is why we also have a breakable door located in the proper location.
		//If the mesh was centered correctly, we could have a single breakable door.
		op.door.SetActive(false);
		//Shatter breakable door
		BreakableObject bo = op.breakableDoor.GetComponent<BreakableObject>();
		bo.triggerBreak( player.collider );
		//Stop crows 
		op.CancelInvoke("playCrowSound");
		Invoke ("step2", 0.1f );
	}

	//Fire starts
	void step2()
	{
		op.fireFx.Play();
		Invoke ("step3", 0.1f );
	}

	//Player starts running
	void step3()
	{
		playerController.startRunning( false );
		Invoke ("step4", 2.5f );

	}

	//Camera is positioned in back of player but at a different X rotation angle
	void step4()
	{
		simpleCamera.setCameraParameters( 18f, SimpleCamera.DEFAULT_DISTANCE, SimpleCamera.DEFAULT_HEIGHT, SimpleCamera.DEFAULT_Y_ROTATION_OFFSET );
		simpleCamera.activateMainCamera();
		simpleCamera.positionCameraNow();
		weatherManager.setWeatherTarget( player, 8f );
		weatherManager.setFogTarget( player, 30f );
		Invoke ("step5", 2.1f );
	}

	void step5()
	{
		//Camera default X rotation angle is restored
		simpleCamera.resetCameraParameters();
		//Give player control
		playerController.allowPlayerMovement(true );
		//Stop the lightning
		lightning.controlLightning(GameEvent.Stop_Lightning);
		Invoke ("step6",4f );
	}

	//Call fairy
	void step6()
	{
		fairyController.Appear ( FairyEmotion.Worried );
		Invoke ("step7", 3f );
	}

	//Fairy warns player of troll
	void step7()
	{
		AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("FAIRY_TROLL_WARNING"), 0.35f, 2.25f );
		Invoke ("step8", 1.5f );
	}

	//Player looks behind his shoulder
	void step8()
	{
		playerController.lookOverShoulder( 1f, 0.4f );
		Invoke ("step9", 1f );
	}

	//Call troll
	void step9()
	{
		trollController.runBehindPlayer();
		Invoke ("step10", 1f );
	}
			
	//Troll smashes ground
	void step10()
	{
		trollController.smashNow();
		Invoke ("step11", 4f );
	}

	//Make the fairy disappear
	void step11()
	{
		fairyController.Disappear ();
	}
	//ISLAND TOWER OPENING SEQUENCE END

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
		GameManager.gameStateEvent += GameStateChange;
	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
		GameManager.gameStateEvent -= GameStateChange;
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Paused )
		{
			CancelInvoke();
			LeanTween.pause( gameObject );
		}
		else if (newState == GameState.Normal )
		{
			if( previousGameState == GameState.Countdown )
			{
				print ("gros caca " + tentaclesSequence.isSequenceActive );
				if( tentaclesSequence != null && tentaclesSequence.isSequenceActive )
				{
					print ("Restart of tentacles sequence");
					InvokeRepeating( "startPierceUp", 0.2f, 2f );
					LeanTween.resume( gameObject );
				}
			}
		}
		print ("GEM GameStateChange " +previousGameState + " " +  newState);
		previousGameState = newState;
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			CancelInvoke();
		}
	}


}
