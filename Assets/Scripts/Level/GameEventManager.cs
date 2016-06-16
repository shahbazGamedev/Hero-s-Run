using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameEventManager : MonoBehaviour {

	WeatherManager weatherManager;
	Lightning lightning;
	SimpleCamera simpleCamera;
	Transform player;
	PlayerController playerController;
	TrollController trollController;
	Transform fairy;
	FairyController fairyController;
	Transform darkQueen;
	DarkQueenController darkQueenController;
	ZombieManager zombieManager;

	GameState previousGameState = GameState.Unknown;

	//Island sequence
	OpeningSequence op;

	//Kraken sequence including Dark Queen
	TentaclesSequence tentaclesSequence;
	DarkQueenKrakenSequence darkQueenKrakenSequence;

	public bool isTentacleSequenceActive = false;

	const int TENTACLES_FACTORY_SIZE = 12;
	List<GameObject> tentaclesList = new List<GameObject>( TENTACLES_FACTORY_SIZE );
	int tentaclesListIndex = 0;
	List<GameObject> tentaclesGroundDebrisList = new List<GameObject>( TENTACLES_FACTORY_SIZE );
	int tentaclesGroundDebrisListIndex = 0;
	List<ParticleSystem> tentaclesDustList = new List<ParticleSystem>( TENTACLES_FACTORY_SIZE );
	int tentaclesDustListIndex = 0;

	//Zombie hands sequence including Dark Queen
	Vector3 lastZombieHandPosition;
	Vector3 lastSideZombieHandPosition;
	DarkQueenCemeterySequence darkQueenCemeterySequence;
	ZombieHandsSequence zombieHandsSequence;
	bool isZombieHandsSequenceActive = false;

	const int ZOMBIE_HANDS_FACTORY_SIZE = 1;
	List<GameObject> zombieHandsList = new List<GameObject>( ZOMBIE_HANDS_FACTORY_SIZE );
	int zombieHandsIndex = 0;
	List<ParticleSystem> zombieHandsBurtsOutFXList = new List<ParticleSystem>( ZOMBIE_HANDS_FACTORY_SIZE );
	int zombieHandsBurtsOutFXIndex = 0;
	List<ParticleSystem> zombieHandsDustList = new List<ParticleSystem>( ZOMBIE_HANDS_FACTORY_SIZE );
	int zombieHandsDustIndex = 0;


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

		fairy = GameObject.FindGameObjectWithTag("Fairy").transform;
		fairyController = fairy.GetComponent<FairyController>();

	}

	//Dark Queen sequence that plays before the Kraken tentacles sequence
	public void setOpeningSequence( DarkQueenKrakenSequence darkQueenKrakenSequence )
	{
		this.darkQueenKrakenSequence = darkQueenKrakenSequence;

		//Note that the Dark Queen is not in the Level scene. She is only in the tiles that use her.
		darkQueen = GameObject.FindGameObjectWithTag("DarkQueen").transform;
		darkQueenController = darkQueen.GetComponent<DarkQueenController>();
	}

	void startDarkQueenKrakenSequence()
	{
		print ("Start of dark queen Kraken sequence");
		isTentacleSequenceActive = true;
		GenerateLevel.enableSurroundingPlane( false ); //remove because it crosses the tomb ceiling and it does not look nice

		//Slowdown player and remove player control
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(19f, afterPlayerSlowdown ) );

		arriveAndCastSpell();
		AchievementDisplay.achievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("VO_FA_OH_NO"), 1.8f );
		playVoiceOver( fairy, darkQueenKrakenSequence.VO_FA_Oh_no );

	}

	void arriveAndCastSpell()
	{
		darkQueen.localScale = new Vector3( 1.2f, 1.2f, 1.2f );
		darkQueenController.floatDownFx.Play ();
		float arriveSpeed = 0.3f;
		darkQueen.GetComponent<Animation>()["DarkQueen_Arrive"].speed = arriveSpeed;
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Arrive");
		Invoke("playLandAnimation", darkQueen.GetComponent<Animation>()["DarkQueen_Arrive"].length/arriveSpeed );
		darkQueenController.dimLights( darkQueen.GetComponent<Animation>()["DarkQueen_Arrive"].length/arriveSpeed, 0.1f );
	}

	void afterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ( FairyEmotion.Worried );
		
	}

	void playLandAnimation()
	{
		AchievementDisplay.achievementDisplay.activateDisplayDarkQueen( LocalizationManager.Instance.getText("VO_DQ_NOT_KEEP_WAITING"), 3.6f );
		playVoiceOver( darkQueen, darkQueenKrakenSequence.VO_DQ_not_keep_waiting );
		darkQueen.GetComponent<Animation>().CrossFade("DarkQueen_Land", 0.1f);
		Invoke("playIdleAnimation", darkQueen.GetComponent<Animation>()["DarkQueen_Land"].length);
	}
	
	void playIdleAnimation()
	{
		darkQueenController.floatDownFx.Stop ();
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Idle");
		Invoke("castKrakenSpell", darkQueen.GetComponent<Animation>()["DarkQueen_Idle"].length);
	}
	
	void castKrakenSpell()
	{
		AchievementDisplay.achievementDisplay.activateDisplayDarkQueen( LocalizationManager.Instance.getText("VO_DQ_RISE_FROM_THE_DEEP"), 3.8f );
		playVoiceOver( darkQueen, darkQueenKrakenSequence.VO_DQ_rise_from_the_deep );
		darkQueen.GetComponent<Animation>().CrossFade("DarkQueen_SpellCast");
		Invoke("playKrakenSpellFX", 0.3f);
		Invoke("leave", darkQueen.GetComponent<Animation>()["DarkQueen_SpellCast"].length );
	}
	
	void playKrakenSpellFX()
	{
		darkQueen.GetComponent<AudioSource>().PlayOneShot( darkQueenController.spellSound );
		darkQueenController.spellFx.Play();
		darkQueenKrakenSequence.poisonMist.Play();
	}
	
	void leave()
	{
		darkQueenController.floatDownFx.Play ();
		darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].speed = 1.2f;
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Leave");
		darkQueenController.brightenLights( darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].length/1.2f );
		Invoke("playerStartsRunningAgain", darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].length/1.2f );
	}

	void playerStartsRunningAgain()
	{
		darkQueenController.Disappear();
		fairyController.Disappear ();
		playerController.allowRunSpeedToIncrease = true;
		playerController.startRunning(false);
		fairyController.resetYRotationOffset();
		Invoke ("activateTentacles", 2f );
	}

	public void activateTentacles()
	{
		playTentaclesSequence();
	}
	//End - Dark Queen sequence that play before kraken tentacles sequence

	//Kraken tentacles sequence
	public void setOpeningSequence( TentaclesSequence tentaclesSequence )
	{
		this.tentaclesSequence = tentaclesSequence;
		createTentaclesFactory();
	}

	void createTentaclesFactory()
	{
		tentaclesList.Clear();
		tentaclesListIndex = 0;
		tentaclesGroundDebrisList.Clear();
		tentaclesGroundDebrisListIndex = 0;
		tentaclesDustList.Clear();
		tentaclesDustListIndex = 0;

		ParticleSystem dust;
		GameObject go;
		for( int i =0; i < TENTACLES_FACTORY_SIZE; i++ )
		{
			go = (GameObject)Instantiate(tentaclesSequence.tentaclePrefab, Vector3.zero, Quaternion.identity );
			go.SetActive( false );
			tentaclesList.Add( go );

			go = (GameObject)Instantiate(tentaclesSequence.groundDebrisPrefab, Vector3.zero, Quaternion.identity );
			go.SetActive( false );
			tentaclesGroundDebrisList.Add( go );

			dust = (ParticleSystem)Instantiate(tentaclesSequence.tentacleAboutToAppearFx, Vector3.zero, Quaternion.identity );
			tentaclesDustList.Add( dust );
		}
	}

	void deactivateTentacleObjects()
	{
		GameObject go;
		for( int i =0; i < TENTACLES_FACTORY_SIZE; i++ )
		{
			go = tentaclesList[i];
			go.SetActive( false );
			go = tentaclesGroundDebrisList[i];
			go.SetActive( false );
		}
	}

	GameObject getFactoryTentacle()
	{
		GameObject tentacle = tentaclesList[tentaclesListIndex];
		tentacle.SetActive( true );
		tentaclesListIndex++;
		if( tentaclesListIndex == TENTACLES_FACTORY_SIZE ) tentaclesListIndex = 0;
		return tentacle;
	}

	GameObject getFactoryGroundDebris()
	{
		GameObject groundDebris = tentaclesGroundDebrisList[tentaclesGroundDebrisListIndex];
		groundDebris.SetActive( true );
		tentaclesGroundDebrisListIndex++;
		if( tentaclesGroundDebrisListIndex == TENTACLES_FACTORY_SIZE ) tentaclesGroundDebrisListIndex = 0;
		return groundDebris;
	}

	ParticleSystem getFactoryTentacleDust()
	{
		ParticleSystem dust = tentaclesDustList[tentaclesListIndex];
		tentaclesDustListIndex++;
		if( tentaclesDustListIndex == TENTACLES_FACTORY_SIZE ) tentaclesDustListIndex = 0;
		dust.Stop();
		return dust;
	}

	public void playTentaclesSequence()
	{
		print ("Starting tentacles sequence");
		isTentacleSequenceActive = true;
		Invoke( "startPierceUp", 1f );
		Invoke( "sideStartPierceUp", Random.Range( 1.5f, 2.1f ) );
	}

	public void stopTentaclesSequence()
	{
		print ("Stopping tentacles sequence");
		CancelInvoke( "startPierceUp" );
		StopCoroutine( "pierceUp" );
		CancelInvoke( "sideStartPierceUp" );
		StopCoroutine( "sidePierceUp" );
		deactivateTentacleObjects();
		isTentacleSequenceActive = false;
	}

	void startPierceUp()
	{
		float attackDistance = 1.2f * PlayerController.getPlayerSpeed();
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
		Vector3 tentaclePosition = player.TransformPoint(new Vector3( 0,0,attackDistance));
		tentaclePosition = new Vector3( playerController.currentTilePos.x + xPos,tentaclePosition.y,tentaclePosition.z);

		//Calculate the ground height
		RaycastHit hit;
		int layermask = ~(1 << 8); //exclude player which is layer is 8
		if (Physics.Raycast(new Vector3( tentaclePosition.x, tentaclePosition.y + 10f, tentaclePosition.z ), Vector3.down, out hit, 25.0F, layermask ))
		{
			tentaclePosition = new Vector3(tentaclePosition.x, hit.point.y - 2f, tentaclePosition.z);
			StartCoroutine( "pierceUp", tentaclePosition );
		}
		else
		{
			//We did not find the ground. Abort.
			return;
		}

		//Display a sign that a tentacle is going to shoot up from the ground to warn the player
		ParticleSystem dust = getFactoryTentacleDust();
		dust.transform.position = new Vector3( tentaclePosition.x, tentaclePosition.y + 2.12f, tentaclePosition.z );
		dust.Play();
	}

	IEnumerator pierceUp( Vector3 tentaclePosition )
	{
		yield return new WaitForSeconds(0.33f);

		playerController.shakeCamera();
		GameObject go = getFactoryTentacle();
		go.transform.position = tentaclePosition;
		go.transform.rotation = Quaternion.Euler( 0, Random.Range (-180f,180f), Random.Range (-6f,6f) );
		float randomScale = 1f + 0.3f * Random.value;
		go.transform.localScale = new Vector3( randomScale, randomScale, randomScale );
		go.name = "Fence";
		go.GetComponent<Animation>().Play("attack");
		go.GetComponent<Animation>().PlayQueued("wiggle", QueueMode.CompleteOthers);
		LeanTween.moveLocalY(go, go.transform.position.y + 2, 1.15f ).setEase(LeanTweenType.easeOutExpo).setOnComplete(pierceDown).setOnCompleteParam( go as Object );

		//Ground debris
		GameObject groundDebrisObject = getFactoryGroundDebris();
		groundDebrisObject.transform.position = new Vector3( tentaclePosition.x, tentaclePosition.y + 2f, tentaclePosition.z );
		groundDebrisObject.transform.rotation = Quaternion.Euler( 0, Random.Range (-180f,180f), 0 );
		groundDebrisObject.transform.localScale = new Vector3( randomScale, randomScale, randomScale );
		LeanTween.moveLocalY(groundDebrisObject, groundDebrisObject.transform.position.y + 0.15f, 0.1f ).setEase(LeanTweenType.easeOutExpo);

		go.GetComponent<AudioSource>().PlayDelayed(0.1f);

		GameObject flyingDebris = (GameObject)Instantiate(tentaclesSequence.debrisPrefab, new Vector3( tentaclePosition.x, tentaclePosition.y + 4f, tentaclePosition.z ), Quaternion.identity );
		BreakableObject bo = flyingDebris.GetComponent<BreakableObject>();
		bo.triggerBreak( player.GetComponent<Collider>() );
		Invoke( "startPierceUp", 2.1f );
	}

	void pierceDown( object go )
	{
		GameObject tentacle = go as GameObject;
		tentacle.GetComponent<Animation>().CrossFade("attack", 0.4f);
		LeanTween.moveLocalY( tentacle, tentacle.transform.position.y - 18, 2f ).setEase(LeanTweenType.easeOutExpo).setDelay(0.5f);
	}

	void sideStartPierceUp()
	{
		float attackDistance = 1.2f * PlayerController.getPlayerSpeed() + Random.Range( -3,1 );
		//Pick random X location on either side of main path
		float xPos;
		int laneChoice = Random.Range(0, 4);
		if( laneChoice == 0 )
		{
			xPos = -2.8f;
		}
		else if( laneChoice == 1 )
		{
			xPos = -4.1f;
		}
		else if( laneChoice == 2 )
		{
			xPos = 2.8f;
		}
		else
		{
			xPos = 4.1f;
		}
		Vector3 sideTentaclePosition = player.TransformPoint(new Vector3( 0,0,attackDistance));
		sideTentaclePosition = new Vector3( playerController.currentTilePos.x + xPos,sideTentaclePosition.y,sideTentaclePosition.z);

		//Calculate the ground height
		RaycastHit hit;
		int layermask = ~(1 << 8); //exclude player which is layer is 8
		if (Physics.Raycast(new Vector3( sideTentaclePosition.x, sideTentaclePosition.y + 10f, sideTentaclePosition.z ), Vector3.down, out hit, 25.0F, layermask ))
		{
			sideTentaclePosition = new Vector3(sideTentaclePosition.x, hit.point.y - 2f, sideTentaclePosition.z);
			StartCoroutine( "sidePierceUp", sideTentaclePosition );
		}
		else
		{
			//We did not find the ground. Abort.
			return;
		}

		//Display a sign that a tentacle is going to shoot up from the ground to warn the player
		ParticleSystem dust = getFactoryTentacleDust();
		dust.transform.position = new Vector3( sideTentaclePosition.x, sideTentaclePosition.y + 2.12f, sideTentaclePosition.z );
		dust.Play();
	}
	
	IEnumerator sidePierceUp( Vector3 sideTentaclePosition )
	{
		yield return new WaitForSeconds(0.33f);

		GameObject go = getFactoryTentacle();
		go.transform.position = sideTentaclePosition;
		go.transform.rotation = Quaternion.Euler( 0, Random.Range (-180f,180f), Random.Range (-6f,6f) );
		float randomScale = 1f + 0.3f * Random.value;
		go.transform.localScale = new Vector3( randomScale, randomScale, randomScale );
		go.GetComponent<Animation>().Play("attack");
		go.GetComponent<Animation>().PlayQueued("wiggle", QueueMode.CompleteOthers);
		LeanTween.moveLocalY(go, go.transform.position.y + 2, 1.15f ).setEase(LeanTweenType.easeOutExpo);

		//Ground debris
		GameObject groundDebrisObject = getFactoryGroundDebris();
		groundDebrisObject.transform.position = new Vector3( sideTentaclePosition.x, sideTentaclePosition.y + 2f, sideTentaclePosition.z );
		groundDebrisObject.transform.rotation = Quaternion.Euler( 0, Random.Range (-180f,180f), 0 );
		groundDebrisObject.transform.localScale = new Vector3( randomScale, randomScale, randomScale );
		LeanTween.moveLocalY(groundDebrisObject, groundDebrisObject.transform.position.y + 0.15f, 0.1f ).setEase(LeanTweenType.easeOutExpo);

		go.GetComponent<AudioSource>().PlayDelayed(0.1f);
		GameObject flyingDebris = (GameObject)Instantiate(tentaclesSequence.debrisPrefab, new Vector3( sideTentaclePosition.x, sideTentaclePosition.y + 4f, sideTentaclePosition.z ), Quaternion.identity );
		BreakableObject bo = flyingDebris.GetComponent<BreakableObject>();
		bo.triggerBreak( player.GetComponent<Collider>() );
		Invoke( "sideStartPierceUp", Random.Range( 1.5f, 2.1f ) );
	}

	//Dark Queen sequence that plays before the zombie hand sequence
	public void setOpeningSequence( DarkQueenCemeterySequence darkQueenCemeterySequence )
	{
		this.darkQueenCemeterySequence = darkQueenCemeterySequence;
		
		//Note that the Dark Queen is not in the Level scene. She is only in the tiles that use her.
		darkQueen = GameObject.FindGameObjectWithTag("DarkQueen").transform;
		darkQueenController = darkQueen.GetComponent<DarkQueenController>();
		
		GameObject zombieManagerObject = GameObject.FindGameObjectWithTag("CreatureManager");
		zombieManager = zombieManagerObject.GetComponent<ZombieManager>();
	}
	
	void startDarkQueenCemeterySequence()
	{
		print ("Start of Dark Queen cemetery sequence");
		isZombieHandsSequenceActive = true;

		trollController.stopPursuing();

		//Slowdown player and remove player control
		playerController.placePlayerInCenterLane();
		GameManager.Instance.setGameState(GameState.Checkpoint);
		StartCoroutine( playerController.slowDownPlayer(19f, cemeteryAfterPlayerSlowdown ) );
		cemeteryArriveAndCastSpell();

	}
	
	void cemeteryAfterPlayerSlowdown()
	{
		playerController.anim.SetTrigger("Idle_Look");
		//Call fairy
		fairyController.setYRotationOffset( -10f );
		fairyController.Appear ( FairyEmotion.Worried );
		AchievementDisplay.achievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("VO_FA_NOT_HER_AGAIN"), 2f );
		playVoiceOver( fairy, darkQueenCemeterySequence.VO_FA_NOT_HER_AGAIN );
	}

	void cemeteryArriveAndCastSpell()
	{
		darkQueen.localScale = new Vector3( 1.2f, 1.2f, 1.2f );
		darkQueenController.floatDownFx.Play ();
		float arriveSpeed = 0.3f;
		darkQueen.GetComponent<Animation>()["DarkQueen_Arrive"].speed = arriveSpeed;
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Arrive");
		Invoke("cemeteryPlayLandAnimation", darkQueen.GetComponent<Animation>()["DarkQueen_Arrive"].length/arriveSpeed );
		darkQueenController.dimLights( darkQueen.GetComponent<Animation>()["DarkQueen_Arrive"].length/arriveSpeed, 0.1f );
	}
	
	void cemeteryPlayLandAnimation()
	{
		darkQueen.GetComponent<Animation>().CrossFade("DarkQueen_Land", 0.1f);
		Invoke("cemeteryPlayIdleAnimation", darkQueen.GetComponent<Animation>()["DarkQueen_Land"].length);
	}
	
	void cemeteryPlayIdleAnimation()
	{
		AchievementDisplay.achievementDisplay.activateDisplayDarkQueen( LocalizationManager.Instance.getText("VO_DQ_STARTING_TO_ANNOY"), 3f );
		playVoiceOver( darkQueen, darkQueenCemeterySequence.VO_DQ_STARTING_TO_ANNOY );
		darkQueenController.floatDownFx.Stop ();
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Idle");
		Invoke("cemeteryCastKrakenSpell", darkQueen.GetComponent<Animation>()["DarkQueen_Idle"].length + 2.25f);
	}
	
	void cemeteryCastKrakenSpell()
	{
		AchievementDisplay.achievementDisplay.activateDisplayDarkQueen( LocalizationManager.Instance.getText("VO_DQ_BRING_BACK_BOOK"), 3.8f );
		playVoiceOver( darkQueen, darkQueenCemeterySequence.VO_DQ_BRING_BACK_BOOK );
		darkQueen.GetComponent<Animation>().CrossFade("DarkQueen_SpellCast");
		Invoke("cemeteryPlayKrakenSpellFX", 0.3f);
		Invoke("cemeteryLeave", darkQueen.GetComponent<Animation>()["DarkQueen_SpellCast"].length );
	}
	
	void cemeteryPlayKrakenSpellFX()
	{
		darkQueen.GetComponent<AudioSource>().PlayOneShot( darkQueenController.spellSound );
		darkQueenController.spellFx.Play();
		darkQueenCemeterySequence.poisonMist.Play();
		Invoke( "startZombieWave", 0.75f );
	}
	
	void startZombieWave()
	{
		startSingleZombieHandPierceUp();
		darkQueenCemeterySequence.zombieWaveObject.SetActive( true );
		ZombieWave activeZombieWave = darkQueenCemeterySequence.zombieWaveObject.GetComponent<ZombieWave>();
		zombieManager.triggerZombieWave( activeZombieWave.spawnLocations );
	}

	void cemeteryLeave()
	{
		darkQueenController.floatDownFx.Play ();
		darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].speed = 1.2f;
		darkQueen.GetComponent<Animation>().Play("DarkQueen_Leave");
		darkQueenController.brightenLights( darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].length/1.2f );
		Invoke("cemeteryPlayerStartsRunningAgain", darkQueen.GetComponent<Animation>()["DarkQueen_Leave"].length/1.2f );
	}
	
	void cemeteryPlayerStartsRunningAgain()
	{
		darkQueenController.Disappear();
		fairyController.Disappear ();
		playerController.allowRunSpeedToIncrease = true;
		//Give player control
		playerController.allowPlayerMovement(true );
		playerController.startRunning(false);
		fairyController.resetYRotationOffset();
	}

	//End - Dark Queen sequence that plays before zombie hands sequence

	//START ZOMBIE HANDS SEQUENCE
	public void setOpeningSequence( ZombieHandsSequence zombieHandsSequence )
	{
		this.zombieHandsSequence = zombieHandsSequence;
		createZombieHandsFactory();
	}
	
	void createZombieHandsFactory()
	{
		zombieHandsList.Clear();
		zombieHandsIndex = 0;
		zombieHandsBurtsOutFXList.Clear();
		zombieHandsBurtsOutFXIndex = 0;
		zombieHandsDustList.Clear();
		zombieHandsDustIndex = 0;
		
		ParticleSystem fx;
		ParticleSystem burstOutFx;
		GameObject go;
		for( int i =0; i < ZOMBIE_HANDS_FACTORY_SIZE; i++ )
		{
			go = (GameObject)Instantiate(zombieHandsSequence.zombieHandPrefab, Vector3.zero, Quaternion.identity );
			zombieHandsList.Add( go );
			
			burstOutFx = (ParticleSystem)Instantiate(zombieHandsSequence.burstOutFx, Vector3.zero, Quaternion.identity );
			zombieHandsBurtsOutFXList.Add( burstOutFx );

			fx = (ParticleSystem)Instantiate(zombieHandsSequence.zombieHandAboutToAppearFx, Vector3.zero, Quaternion.identity );
			zombieHandsDustList.Add( fx );
			
		}
	}
	
	GameObject getFactoryZombieHand()
	{
		GameObject zombieHand = zombieHandsList[zombieHandsIndex];
		zombieHandsIndex++;
		if( zombieHandsIndex == ZOMBIE_HANDS_FACTORY_SIZE ) zombieHandsIndex = 0;
		return zombieHand;
	}
	
	ParticleSystem getFactoryBurtsOutFX()
	{
		ParticleSystem burtsOutFX = zombieHandsBurtsOutFXList[zombieHandsBurtsOutFXIndex];
		zombieHandsBurtsOutFXIndex++;
		if( zombieHandsBurtsOutFXIndex == ZOMBIE_HANDS_FACTORY_SIZE ) zombieHandsBurtsOutFXIndex = 0;
		return burtsOutFX;
	}
	
	ParticleSystem getFactoryZombieHandDust()
	{
		ParticleSystem dust = zombieHandsDustList[zombieHandsDustIndex];
		zombieHandsDustIndex++;
		if( zombieHandsDustIndex == ZOMBIE_HANDS_FACTORY_SIZE ) zombieHandsDustIndex = 0;
		return dust;
	}

	public void playZombieHandsSequence()
	{
		print ("Starting zombie hands sequence");
		isZombieHandsSequenceActive = true;
		Invoke( "startZombieHandPierceUp", 1f );
		Invoke( "sideStartZombieHandPierceUp", 2f );
	}
	
	public void stopZombieHandsSequence()
	{
		print ("Stopping zombie hands sequence");
		CancelInvoke( "startZombieHandPierceUp" );
		CancelInvoke( "zombieHandPierceUp" );
		CancelInvoke( "sideStartZombieHandPierceUp" );
		CancelInvoke( "zombieHandSidePierceUp" );
		isZombieHandsSequenceActive = false;
	}
	
	void startZombieHandPierceUp()
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
		lastZombieHandPosition = player.TransformPoint(new Vector3( xPos,0,attackDistance));
		
		//Calculate the ground height
		RaycastHit hit;
		int layermask = ~(1 << 8); //exclude player which is layer is 8
		if (Physics.Raycast(new Vector3( lastZombieHandPosition.x, lastZombieHandPosition.y + 10f, lastZombieHandPosition.z ), Vector3.down, out hit, 25.0F, layermask ))
		{
			lastZombieHandPosition = new Vector3(lastZombieHandPosition.x, hit.point.y - 1f, lastZombieHandPosition.z);
			Invoke( "zombieHandPierceUp", 0.33f );
		}
		else
		{
			//We did not find the ground. Abort.
			return;
		}
		
		//Display a sign that a zombie hand is going to shoot up from the ground to warn the player
		ParticleSystem dust = getFactoryZombieHandDust();
		dust.transform.position = new Vector3( lastZombieHandPosition.x, lastZombieHandPosition.y + 1.23f, lastZombieHandPosition.z );
		dust.Play();

		//Send some debris (particles) flying up in the air as the hand bursts out of the ground
		ParticleSystem burstOutFx = getFactoryBurtsOutFX();
		burstOutFx.transform.position = new Vector3( lastZombieHandPosition.x, lastZombieHandPosition.y + 1.1f, lastZombieHandPosition.z );
		burstOutFx.Play();
	}
	
	void zombieHandPierceUp()
	{
		GameObject go = getFactoryZombieHand();
		go.transform.position = lastZombieHandPosition;
		go.transform.rotation = Quaternion.Euler( 0, player.eulerAngles.y + 180f + Random.Range (-6f,7f), Random.Range (-6f,6f) );
		float randomScale = 1f + 0.5f * Random.value;
		go.transform.localScale = new Vector3( randomScale, randomScale, randomScale );
		go.name = "Stumble";
		LeanTween.moveLocalY(go, go.transform.position.y + 1, 0.6f ).setEase(LeanTweenType.easeOutExpo).setOnComplete(zombieHandPierceDown).setOnCompleteParam( go as Object );

		go.GetComponent<AudioSource>().PlayDelayed(0.1f);

		Invoke( "startZombieHandPierceUp", 1.2f + Random.value );
	}
	
	void zombieHandPierceDown( object go )
	{
		GameObject zombieHand = go as GameObject;
		zombieHand.GetComponent<Animation>().Play("FistToSearch" );
		zombieHand.GetComponent<Animation>().PlayQueued("Search", QueueMode.CompleteOthers );
		LeanTween.moveLocalY( zombieHand, zombieHand.transform.position.y - 2, 2.5f ).setEase(LeanTweenType.easeOutExpo).setDelay( 3f );
	}
	
	void sideStartZombieHandPierceUp()
	{
		float attackDistance = 1.1f * PlayerController.getPlayerSpeed() + Random.Range( -3,1 );
		//Pick random X location on either side of main path
		float xPos;
		int laneChoice = Random.Range(0, 4);
		if( laneChoice == 0 )
		{
			xPos = -2.3f;
		}
		else if( laneChoice == 1 )
		{
			xPos = -3.3f;
		}
		else if( laneChoice == 2 )
		{
			xPos = 2.3f;
		}
		else
		{
			xPos = 3.3f;
		}
		lastSideZombieHandPosition = player.TransformPoint(new Vector3( xPos,0,attackDistance));
		
		//Calculate the ground height
		RaycastHit hit;
		int layermask = ~(1 << 8); //exclude player which is layer is 8
		if (Physics.Raycast(new Vector3( lastSideZombieHandPosition.x, lastSideZombieHandPosition.y + 10f, lastSideZombieHandPosition.z ), Vector3.down, out hit, 25.0F, layermask ))
		{
			lastSideZombieHandPosition = new Vector3(lastSideZombieHandPosition.x, hit.point.y - 1f, lastSideZombieHandPosition.z);
			Invoke( "zombieHandSidePierceUp", 0.33f );
		}
		else
		{
			//We did not find the ground. Abort.
			return;
		}
		
		//Display a sign that a zombie hand is going to shoot up from the ground to warn the player
		ParticleSystem dust = getFactoryZombieHandDust();
		dust.transform.position = new Vector3( lastSideZombieHandPosition.x, lastSideZombieHandPosition.y + 1.23f, lastSideZombieHandPosition.z );
		dust.Play();

		//Send some debris (particles) flying up in the air as the hand bursts out of the ground
		ParticleSystem burstOutFx = getFactoryBurtsOutFX();
		burstOutFx.transform.position = new Vector3( lastSideZombieHandPosition.x, lastSideZombieHandPosition.y + 1.1f, lastSideZombieHandPosition.z );
		burstOutFx.Play();
	}
	
	void zombieHandSidePierceUp()
	{
		GameObject go = getFactoryZombieHand();
		go.transform.position = lastSideZombieHandPosition;
		go.transform.rotation = Quaternion.Euler( 0, player.eulerAngles.y + 180f + Random.Range (-6f,7f), Random.Range (-6f,6f) );
		float randomScale = 1f + 0.5f * Random.value;
		go.transform.localScale = new Vector3( randomScale, randomScale, randomScale );
		LeanTween.moveLocalY(go, go.transform.position.y + 1, 0.8f ).setEase(LeanTweenType.easeOutExpo);

		go.GetComponent<AudioSource>().PlayDelayed(0.1f);

		Invoke( "sideStartZombieHandPierceUp", 0.8f + Random.value * 1.5f );
	}

	//Only one positioned hand version. Position is relative to the Dark Queen
	void startSingleZombieHandPierceUp()
	{
		lastZombieHandPosition = darkQueen.TransformPoint(new Vector3(1f,-1f,-0.8f));
		Invoke( "singleZombieHandPierceUp", 0.33f );

		//Display a sign that a zombie hand is going to shoot up from the ground to warn the player
		ParticleSystem dust = getFactoryZombieHandDust();
		dust.transform.position = new Vector3( lastZombieHandPosition.x, lastZombieHandPosition.y + 1.23f, lastZombieHandPosition.z );
		dust.Play();
		
		//Send some debris (particles) flying up in the air as the hand bursts out of the ground
		ParticleSystem burstOutFx = getFactoryBurtsOutFX();
		burstOutFx.transform.position = new Vector3( lastZombieHandPosition.x, lastZombieHandPosition.y + 1.1f, lastZombieHandPosition.z );
		burstOutFx.Play();
	}
	
	void singleZombieHandPierceUp()
	{
		GameObject go = getFactoryZombieHand();
		go.transform.position = lastZombieHandPosition;
		go.transform.rotation = Quaternion.Euler( 0, player.eulerAngles.y + 180f + Random.Range (-6f,7f), Random.Range (-6f,6f) );
		float randomScale = 1f + 0.5f * Random.value;
		go.transform.localScale = new Vector3( randomScale, randomScale, randomScale );
		go.name = "Stumble";
		LeanTween.moveLocalY(go, go.transform.position.y + 1.1f, 0.6f ).setEase(LeanTweenType.easeOutExpo).setOnComplete(singleZombieHandPierceDown).setOnCompleteParam( go as Object );
		go.GetComponent<AudioSource>().PlayDelayed(0.1f);
		
		GameObject flyingDebris = (GameObject)Instantiate(zombieHandsSequence.debrisPrefab, Vector3.zero, Quaternion.identity );
		flyingDebris.transform.position = new Vector3( lastZombieHandPosition.x, lastZombieHandPosition.y + 1.4f, lastZombieHandPosition.z );
		BreakableObject bo = flyingDebris.GetComponent<BreakableObject>();
		bo.triggerBreak( null );
		
	}
	
	void singleZombieHandPierceDown( object go )
	{
		GameObject zombieHand = go as GameObject;
		zombieHand.GetComponent<Animation>().Play("FistToSearch" );
		zombieHand.GetComponent<Animation>().PlayQueued("Search", QueueMode.CompleteOthers );
		//LeanTween.moveLocalY( zombieHand, zombieHand.transform.position.y - 2, 3.25f ).setEase(LeanTweenType.easeOutExpo).setDelay( 4f );
	}

	//END ZOMBIE HANDS SEQUENCE

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
		Invoke ( "enableTapToPlay", 1.2f );
	}

	void enableTapToPlay()
	{
		//The state change will cause Tap to play to become active
		GameManager.Instance.setGameState( GameState.Menu );
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
		bo.triggerBreak( player.GetComponent<Collider>() );
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
		AchievementDisplay.achievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("FAIRY_TROLL_WARNING"), 2.25f );
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
			StopCoroutine( "pierceUp" );
			StopCoroutine( "sidePierceUp" );
		}
		else if( newState == CharacterState.StartRunning && isTentacleSequenceActive )
		{
			playTentaclesSequence();
		}
		else if( newState == CharacterState.StartRunning && isZombieHandsSequenceActive )
		{
			//playZombieHandsSequence();
		}
	}


	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Start_Kraken && !isTentacleSequenceActive )
		{
			startDarkQueenKrakenSequence();
		}
		else if( eventType == GameEvent.Stop_Kraken && isTentacleSequenceActive )
		{
			stopTentaclesSequence();
		}
		else if( eventType == GameEvent.Start_Zombie_Hands && !isZombieHandsSequenceActive )
		{
			startDarkQueenCemeterySequence();
		}
		else if( eventType == GameEvent.Stop_Zombie_Hands && isZombieHandsSequenceActive )
		{
			stopZombieHandsSequence();
		}
	}

	void playVoiceOver( Transform speaker, AudioClip voiceOver )
	{
		//Currently, only English VOs are included in the game
		if( true || Application.systemLanguage == SystemLanguage.English )
		{
			speaker.GetComponent<AudioSource>().PlayOneShot( voiceOver );
		}
	}

}
