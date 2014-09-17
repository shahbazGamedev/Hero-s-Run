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
	Vector3 lastSideTentaclePosition;
	public bool isTentacleSequenceActive = false;
	public bool isDarkQueenSequenceActive = false;

	
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
		print ("Starting tentacles sequence");
		isTentacleSequenceActive = true;
		Invoke( "startPierceUp", 1f );
		Invoke( "sideStartPierceUp", 2f );
	}

	public void stopTentaclesSequence()
	{
		print ("Stopping tentacles sequence");
		CancelInvoke( "startPierceUp" );
		CancelInvoke( "pierceUp" );
		CancelInvoke( "sideStartPierceUp" );
		CancelInvoke( "sidePierceUp" );
		isTentacleSequenceActive = false;
	}

	void startPierceUp()
	{
		float attackDistance = 1.1f * PlayerController.getPlayerSpeed();
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
		lastTentaclePosition = player.TransformPoint(new Vector3( xPos,0,attackDistance));

		//Calculate the ground height
		RaycastHit hit;
		int layermask = ~(1 << 8); //exclude player which is layer is 8
		float groundHeight = 0;
		if (Physics.Raycast(new Vector3( lastTentaclePosition.x, lastTentaclePosition.y + 10f, lastTentaclePosition.z ), Vector3.down, out hit, 25.0F, layermask ))
		{
			groundHeight = lastTentaclePosition.y + 10f - hit.distance;
			//groundHeight = 0;
			lastTentaclePosition = new Vector3(lastTentaclePosition.x, groundHeight - 2f, lastTentaclePosition.z);
			Invoke( "pierceUp", 0.33f );
		}
		else
		{
			//We did not find the ground. Abort.
			return;
		}

		//Display a sign that a tentacle is going to shoot up from the ground to warn the player
		ParticleSystem dust = (ParticleSystem)Instantiate(tentaclesSequence.tentacleAboutToAppearFx, Vector3.zero, Quaternion.identity );
		dust.transform.position = new Vector3( lastTentaclePosition.x, lastTentaclePosition.y + 2.1f, lastTentaclePosition.z );
		dust.Play();
		GameObject.Destroy( dust, 3f );
	}

	void pierceUp()
	{
		playerController.shakeCamera();
		GameObject go = (GameObject)Instantiate(tentaclesSequence.tentaclePrefab, Vector3.zero, Quaternion.identity );
		go.transform.position = lastTentaclePosition;
		go.transform.rotation = Quaternion.Euler( 0, Random.Range (-180f,180f), Random.Range (-6f,6f) );
		float randomScale = 1f + 0.4f * Random.value;
		go.transform.localScale = new Vector3( randomScale, randomScale, randomScale );
		go.name = "Fence";
		go.animation.Play("attack");
		go.animation.PlayQueued("wiggle", QueueMode.CompleteOthers);
		LeanTween.moveLocalY(go, go.transform.position.y + 2, 1.15f ).setEase(LeanTweenType.easeOutExpo).setOnComplete(pierceDown).setOnCompleteParam( go as Object );

		//Ground debris
		GameObject groundDebrisObject = (GameObject)Instantiate(tentaclesSequence.groundDebrisPrefab, Vector3.zero, Quaternion.identity );
		groundDebrisObject.transform.position = new Vector3( lastTentaclePosition.x, lastTentaclePosition.y + 2f, lastTentaclePosition.z );
		groundDebrisObject.transform.rotation = Quaternion.Euler( 0, Random.Range (-180f,180f), 0 );
		groundDebrisObject.transform.localScale = new Vector3( randomScale, randomScale, randomScale );
		LeanTween.moveLocalY(groundDebrisObject, groundDebrisObject.transform.position.y + 0.15f, 0.1f ).setEase(LeanTweenType.easeOutExpo);


		//LeanTween.moveLocalY(go, go.transform.position.y + 2, 1.15f ).setEase(LeanTweenType.easeOutExpo);
		go.audio.PlayDelayed(0.1f);
		GameObject flyingDebris = (GameObject)Instantiate(tentaclesSequence.debrisPrefab, Vector3.zero, Quaternion.identity );
		flyingDebris.transform.position = new Vector3( lastTentaclePosition.x, lastTentaclePosition.y + 4f, lastTentaclePosition.z );
		BreakableObject bo = flyingDebris.GetComponent<BreakableObject>();
		bo.triggerBreak( player.collider );
		//We only want to keep the tentacle for a few seconds
		GameObject.Destroy( go, 4f );
		Invoke( "startPierceUp", 1.2f + Random.value );

	}

	void pierceDown( object go )
	{
		GameObject tentacle = go as GameObject;
		tentacle.animation.CrossFade("attack", 0.4f);
		LeanTween.moveLocalY( tentacle, tentacle.transform.position.y - 18, 2f ).setEase(LeanTweenType.easeOutExpo);
	}

	void sideStartPierceUp()
	{
		float attackDistance = 1.1f * PlayerController.getPlayerSpeed() + Random.Range( -3,1 );
		//Pick random X location on either side of main path
		float xPos;
		int laneChoice = Random.Range(0, 4);
		if( laneChoice == 0 )
		{
			xPos = -2.6f;
		}
		else if( laneChoice == 1 )
		{
			xPos = -3.9f;
		}
		else if( laneChoice == 2 )
		{
			xPos = 2.6f;
		}
		else
		{
			xPos = 3.9f;
		}
		lastSideTentaclePosition = player.TransformPoint(new Vector3( xPos,0,attackDistance));
		
		//Calculate the ground height
		RaycastHit hit;
		int layermask = ~(1 << 8); //exclude player which is layer is 8
		float groundHeight = 0;
		if (Physics.Raycast(new Vector3( lastSideTentaclePosition.x, lastSideTentaclePosition.y + 10f, lastSideTentaclePosition.z ), Vector3.down, out hit, 25.0F, layermask ))
		{
			groundHeight = lastSideTentaclePosition.y + 10f - hit.distance;
			//groundHeight = 0;
			lastSideTentaclePosition = new Vector3(lastSideTentaclePosition.x, groundHeight - 2f, lastSideTentaclePosition.z);
			Invoke( "sidePierceUp", 0.33f );
		}
		else
		{
			//We did not find the ground. Abort.
			return;
		}

		//Display a sign that a tentacle is going to shoot up from the ground to warn the player
		ParticleSystem dust = (ParticleSystem)Instantiate(tentaclesSequence.tentacleAboutToAppearFx, Vector3.zero, Quaternion.identity );
		dust.transform.position = new Vector3( lastSideTentaclePosition.x, lastSideTentaclePosition.y + 2.1f, lastSideTentaclePosition.z );
		dust.Play();
		GameObject.Destroy( dust, 3f );
	}
	
	void sidePierceUp()
	{
		GameObject go = (GameObject)Instantiate(tentaclesSequence.tentaclePrefab, Vector3.zero, Quaternion.identity );
		go.transform.position = lastSideTentaclePosition;
		go.transform.rotation = Quaternion.Euler( 0, Random.Range (-180f,180f), Random.Range (-6f,6f) );
		float randomScale = 1f + 0.4f * Random.value;
		go.transform.localScale = new Vector3( randomScale, randomScale, randomScale );
		go.animation.Play("attack");
		go.animation.PlayQueued("wiggle", QueueMode.CompleteOthers);
		LeanTween.moveLocalY(go, go.transform.position.y + 2, 1.15f ).setEase(LeanTweenType.easeOutExpo);

		//Ground debris
		GameObject groundDebrisObject = (GameObject)Instantiate(tentaclesSequence.groundDebrisPrefab, Vector3.zero, Quaternion.identity );
		groundDebrisObject.transform.position = new Vector3( lastSideTentaclePosition.x, lastSideTentaclePosition.y + 2f, lastSideTentaclePosition.z );
		groundDebrisObject.transform.rotation = Quaternion.Euler( 0, Random.Range (-180f,180f), 0 );
		groundDebrisObject.transform.localScale = new Vector3( randomScale, randomScale, randomScale );
		LeanTween.moveLocalY(groundDebrisObject, groundDebrisObject.transform.position.y + 0.15f, 0.1f ).setEase(LeanTweenType.easeOutExpo);

		go.audio.PlayDelayed(0.1f);
		GameObject flyingDebris = (GameObject)Instantiate(tentaclesSequence.debrisPrefab, Vector3.zero, Quaternion.identity );
		flyingDebris.transform.position = new Vector3( lastSideTentaclePosition.x, lastSideTentaclePosition.y + 4f, lastSideTentaclePosition.z );
		BreakableObject bo = flyingDebris.GetComponent<BreakableObject>();
		bo.triggerBreak( player.collider );
		//We only want to keep the tentacle for a few seconds
		GameObject.Destroy( go, 3f );
		Invoke( "sideStartPierceUp", 0.8f + Random.value * 1.5f );
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
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
		GameManager.gameStateEvent += GameStateChange;
	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
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
				if( tentaclesSequence != null && isTentacleSequenceActive )
				{
					print ("Restart of tentacles sequence");
					playTentaclesSequence();
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
		else if( newState == CharacterState.StartRunning && isTentacleSequenceActive )
		{
			playTentaclesSequence();
		}
	}


	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Start_Kraken && !isTentacleSequenceActive )
		{
			playTentaclesSequence();
		}
		else if( eventType == GameEvent.Stop_Kraken && isTentacleSequenceActive )
		{
			stopTentaclesSequence();
		}
	}


}
