using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum PowerUpType {
	//Note: Don't change these values without updating PlayerStatsManager
	None=0,
	Shield=1,
	Magnet=2,
	ZNuke = 3,
	MagicBoots = 4,
	SlowTime = 5,
	SpeedBoost = 6,
	Life = 7
}

public class PowerUpManager : BaseClass {

	const float SLIDE_DURATION = 0.3f;
	const float START_X_POSITION = -160f;
	const float FINAL_X_POSITION = 55f;

	public RectTransform equippedPowerUp;
	public Text equippedPowerUpQuantity;
	public RectTransform magnetPowerUp;
	public RectTransform shieldPowerUp;

	//The player can have multiple powerups active at the same time.
	//I.e. Magnet and Shield can be active at the same time, but not two magnets.
	static List<PowerUpType> activePowerUps = new List<PowerUpType>();

	Transform player;
	PlayerController playerController;
	AudioSource audioSource;

	[Tooltip("The sound to play when a power-up is picked up in the level. The same sound plays regardless of which power-up is picked up. Note that the AudioSource of the PowerUpManager is 2D, not 3D.")]
	public AudioClip pickUpSound;

	public ParticleSystem zNukeEffect;

	//For debugging
	//When forcePowerUpType is not set to NONE, only the power up type specified will be added to the level.
	PowerUpType forcePowerUpType = PowerUpType.None;
	const float DISTANCE_TO_GROUND = 2.4f;

	//List of each powerup available.
	public List<PowerUpData> powerUpList = new List<PowerUpData>(7);
	static Dictionary<PowerUpType,PowerUpData> powerUpDictionary = new Dictionary<PowerUpType,PowerUpData>(7);

	//Base diameter is the value if upgrade level is 0. It is used by zNuke.
	const float BASE_DIAMETER = 18f; 	//in meters
	const float PERCENTAGE_LIFE_SPAWNED = 0.5f;

	//Delegate used to communicate to other classes when the number of keys, lives, stars or star doubler changes
	public delegate void ZNukeExploded( float impactDiameter );
	public static event ZNukeExploded zNukeExploded;

	public const float SPEED_BOOST_MULTIPLIER = 1.5f;

	void Awake()
	{
		fillDictionary();
		player = GameObject.FindGameObjectWithTag("Player").transform;	
		playerController = player.GetComponent<PlayerController>();
		audioSource = GetComponent<AudioSource>();
	}
	
	public void changeSelectedPowerUp(PowerUpType newPowerUpType )
	{
		PlayerStatsManager.Instance.setPowerUpSelected( newPowerUpType );
		PowerUpData pud = powerUpDictionary[newPowerUpType];
		slideEquippedPowerUp(pud);
	}
	
	void fillDictionary()
	{
		//Copy all of the power up data into a dictionary for convenient access
		powerUpDictionary.Clear();
		for( int i = 0; i < powerUpList.Count; i++ )
		{
			powerUpDictionary.Add(powerUpList[i].powerUpType, powerUpList[i] );
		}
		//We no longer need powerUpList
		powerUpList.Clear();
		powerUpList = null;
	}

	public static PowerUpType getRandomConsumablePowerUp()
	{
		int rd = Random.Range(0,4);
		PowerUpType randomConsumablePowerUp = PowerUpType.None;
		switch( rd )
		{
		case 0:
			randomConsumablePowerUp = PowerUpType.ZNuke;
			break;
		case 1:
			randomConsumablePowerUp = PowerUpType.MagicBoots;
			break;
		case 2:
			randomConsumablePowerUp = PowerUpType.SlowTime;
			break;
		case 3:
			randomConsumablePowerUp = PowerUpType.SpeedBoost;
			break;
		}
		return randomConsumablePowerUp;
	}
	
	IEnumerator startTimerMagnet( PowerUpData pud )
	{
		float duration = getDuration(pud);
		pud.startTimer( duration );
		float elapsed = 0;
		
		do
		{
			elapsed = elapsed + Time.deltaTime;
			yield return _sync();
		} while ( elapsed < duration );
		//Duration has expired. Deactivate power up.
		deactivatePowerUp( pud.powerUpType, false );
	}

	IEnumerator startTimerShield( PowerUpData pud )
	{
		float duration = getDuration(pud);
		pud.startTimer( duration );
		float elapsed = 0;
		
		do
		{
			elapsed = elapsed + Time.deltaTime;
			yield return _sync();
		} while ( elapsed < duration );
		//Duration has expired. Deactivate power up.
		deactivatePowerUp( pud.powerUpType, false );
	}
	
	IEnumerator startTimerSlowTime( PowerUpData pud )
	{
		float duration = getDuration(pud);
		pud.startTimer( duration * Time.timeScale );
		float elapsed = 0;
		
		do
		{
			elapsed = elapsed + Time.deltaTime;
			yield return _sync();
		} while ( elapsed < duration );
		//Duration has expired. Deactivate power up.
		deactivatePowerUp( pud.powerUpType, false );
	}
	
	IEnumerator startTimerSpeedBoost( PowerUpData pud )
	{
		float duration = getDuration(pud);
		pud.startTimer( duration );
		float elapsed = 0;
		
		do
		{
			elapsed = elapsed + Time.deltaTime;
			yield return _sync();
		} while ( elapsed < duration );
		//Duration has expired. Deactivate power up.
		deactivatePowerUp( pud.powerUpType, false );
	}

	public static bool isThisPowerUpActive( PowerUpType powerUpType )
	{
		return activePowerUps.Contains( powerUpType );
	}

	public void pickUpPowerUp( PowerUpType powerUpType )
	{
		PowerUpData pud = powerUpDictionary[powerUpType];

		//Play pick-up sound
		audioSource.PlayOneShot( pickUpSound );
		//Play a pick-up particle effect if one has been specified
		if( pud.pickupEffect != null )
		{
			//Make the player the parent of this particle system
			pud.pickupEffect.transform.parent = player;
			pud.pickupEffect.transform.localPosition = new Vector3( 0, 1f, 0 );
			pud.pickupEffect.Play();
		}
		//Life is a special case. The inventory is maintained separately.
		if( powerUpType == PowerUpType.Life )
		{
			PlayerStatsManager.Instance.increaseLives(1);
			PlayerStatsManager.Instance.savePlayerStats();
			print("Life - pickUpPowerUp : adding 1 life. New total is " + PlayerStatsManager.Instance.getLives() );
		}
		else
		{
			PlayerStatsManager.Instance.incrementPowerUpInventory(powerUpType);
		}
	}

	//Called by the PowerUp prefab onTriggerEnter function
	public void activatePowerUp( PowerUpType powerUpType )
	{
		PowerUpData pud = powerUpDictionary[powerUpType];

		//Play an activation particle effect if one has been specified
		if( pud.activationEffect != null )
		{
			//Make the player the parent of this particle system
			pud.activationEffect.transform.parent = player;
			pud.activationEffect.transform.localPosition = new Vector3( 0, 1f, 0 );
			pud.activationEffect.Play();
		}

		//Play an activation sound if one has been specified
		if( pud.activationSound != null ) audioSource.PlayOneShot( pud.activationSound );

		//Verify if this type of power up is already active.
		if( activePowerUps.Contains( powerUpType ) )
		{
			//Yes, it is already active
			switch( powerUpType )
			{
			case PowerUpType.Magnet:
				
				StopCoroutine( "startTimerMagnet" );
				StartCoroutine( "startTimerMagnet", pud );
				Debug.Log("Magnet - prolonging duration." );
				break;

			case PowerUpType.Shield:
				
				StopCoroutine( "startTimerShield" );
				StartCoroutine( "startTimerShield", pud );
				Debug.Log("Shield - prolonging duration." );
				break;
			}
		}
		else
		{
			//No, this is a new power up
			switch( pud.powerUpType )
			{
				case PowerUpType.Magnet:
					activePowerUps.Add( pud.powerUpType );
					GameObject magnetSphere = player.Find ("Magnet Sphere").gameObject;
					CoinAttractor coinAttractor = (CoinAttractor) magnetSphere.GetComponent(typeof(CoinAttractor));
					float radius =  (magnetSphere.GetComponent<Collider>() as SphereCollider).radius;
					coinAttractor.attractCoinsWithinSphere( magnetSphere.transform.position, radius );
					StartCoroutine( "startTimerMagnet", pud );
					slideInPowerUp(magnetPowerUp);
					Debug.Log("Magnet - activatePowerUp with duration time of " + getDuration( pud ) );
				break;

				case PowerUpType.ZNuke:
				if( PlayerStatsManager.Instance.getPowerUpQuantity(PowerUpType.ZNuke) > 0 || Application.isEditor )
				{
					Debug.Log("PowerUpZNuke - activatePowerUp");
					PlayerStatsManager.Instance.decrementPowerUpInventory(pud.powerUpType);
					//Play a particle effect and a sound
					//Use a sphere that starts impactDiameter/2 meters in front of the player
					Vector3 relativePos = new Vector3(0f , 0f , getImpactDiameter( pud )/2f );
					Vector3 exactPos = player.TransformPoint(relativePos);
			
					zNukeEffect.transform.position = exactPos;
					zNukeEffect.GetComponent<AudioSource>().Play ();
					zNukeEffect.Play();

					//Send an event to interested classes
					if(zNukeExploded != null) zNukeExploded( getImpactDiameter( pud ) );
				}
				break;

				case PowerUpType.MagicBoots:
				if( PlayerStatsManager.Instance.getPowerUpQuantity(PowerUpType.MagicBoots) > 0 || Application.isEditor )
				{
					playerController.doingDoubleJump = true;
					playerController.jump();
					Debug.Log("MagicBoots - activatePowerUp" );
					PlayerStatsManager.Instance.decrementPowerUpInventory(pud.powerUpType);
				}
				break;
			
				case PowerUpType.SlowTime:
				if( PlayerStatsManager.Instance.getPowerUpQuantity(PowerUpType.SlowTime) > 0 || Application.isEditor )
				{
					StopCoroutine( "startTimerSlowTime" );
					StartCoroutine( "startTimerSlowTime", pud );
					Debug.Log("SlowTime - activatePowerUp with duration time of " + getDuration( pud ) );
					//Slow down time
					Time.timeScale = 0.5f;
					PlayerStatsManager.Instance.decrementPowerUpInventory(pud.powerUpType);
				}
				break;

				case PowerUpType.Shield:
					activePowerUps.Add( pud.powerUpType );
					turnSomeCollidersIntoTriggers( true );
					StartCoroutine( "startTimerShield", pud );
					slideInPowerUp(shieldPowerUp);
					Debug.Log("Shield - activatePowerUp with duration time of " + getDuration( pud ) );
				break;

				case PowerUpType.SpeedBoost:
				if( PlayerStatsManager.Instance.getPowerUpQuantity(PowerUpType.SpeedBoost) > 0 || Application.isEditor )
				{
					activePowerUps.Add( pud.powerUpType );
					playerController.activateSpeedBoost( true );
					StopCoroutine( "startTimerSpeedBoost" );
					StartCoroutine( "startTimerSpeedBoost", pud );
					PlayerStatsManager.Instance.decrementPowerUpInventory(pud.powerUpType);
					Debug.Log("SpeedBoost - activatePowerUp with duration time of " + getDuration( pud ) );
				}
				break;
			}
		}
	}
	
	public void deactivatePowerUp(PowerUpType powerUpType, bool instantly )
	{
		PowerUpData pud = powerUpDictionary[powerUpType];

		//Play a deactivation sound if one has been set.
		if ( !instantly && pud.deactivationSound != null ) audioSource.PlayOneShot( pud.deactivationSound );

		switch( powerUpType )
		{
		case PowerUpType.Magnet:
			if( activePowerUps.Contains( powerUpType ) )
			{
				slideOutPowerUp(magnetPowerUp);
			}
			removeFromActiveList( PowerUpType.Magnet );
			StopCoroutine( "startTimerMagnet" );
			pud.stopTimer();
			break;

		case PowerUpType.Shield:
			if( activePowerUps.Contains( powerUpType ) )
			{
				slideOutPowerUp(shieldPowerUp);
			}
			removeFromActiveList( PowerUpType.Shield );
			StopCoroutine( "startTimerShield" );
			pud.stopTimer();
			turnSomeCollidersIntoTriggers( false );
			break;

		case PowerUpType.SlowTime:
			StopCoroutine( "startTimerSlowTime" );
			if( instantly )
			{
				//Restore normal time immediately
				StopCoroutine( "accelerateAfterSlowTime" );
				pud.stopTimer();
				Time.timeScale = 1f;
			}
			else
			{
				//If the slow-time power-up expires normally, restore the time scale gradually back to 1.
				StartCoroutine( "accelerateAfterSlowTime" );
				pud.stopTimer();
			}
			break;

		case PowerUpType.SpeedBoost:
			removeFromActiveList( PowerUpType.SpeedBoost );
			StopCoroutine( "startTimerSpeedBoost" );
			playerController.activateSpeedBoost( false );
			pud.stopTimer();
			break;
		}
	}

	//Once the slow-time power-up expires normally, restore the time scale gradually back to 1. 
	IEnumerator accelerateAfterSlowTime()
	{
		float currentTimeScale = Time.timeScale;

		float duration = 2f;
		float elapsedTime = 0;

		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			Time.timeScale = Mathf.Lerp( currentTimeScale, 1f, elapsedTime/duration );
			yield return _sync();
		} while ( elapsedTime < duration );

		//Fix any left-overs
		Time.timeScale = 1f;
	}

	void removeFromActiveList(PowerUpType powerUpType )
	{
		if( activePowerUps.Contains( powerUpType ) )
		{
			activePowerUps.Remove( powerUpType );
		}
	}
	
	//When the player uses the SHIELD power up, we need to turn certain types of colliders into triggers (and remove the trigger
	//when the spell is finished).
	private void turnSomeCollidersIntoTriggers( bool isActive )
	{
		Collider[] colliders = FindObjectsOfType(typeof(Collider)) as Collider[];
		foreach (Collider collider in colliders)
		{
			if ( collider.name == "DeadTree" || collider.name.StartsWith("Stumble") || collider.name.StartsWith("Breakable")  || collider.name == "cart" || collider.name == "Chicken" || collider.name == "Pendulum" )
			{
				//Since we are turning the collider into a trigger, we need to disable gravity for objects with a rigid body
				//as well or else the object will fall through the ground.
				if( collider.GetComponent<Rigidbody>() != null )
				{
					collider.GetComponent<Rigidbody>().useGravity = !isActive;
				}
				collider.isTrigger = isActive;
			}
		}
	}

	public void considerAddingPowerUp( GameObject newTile, int tileIndex )
	{
		LevelData.EpisodeInfo episodeInfo = LevelManager.Instance.getCurrentEpisodeInfo();
		int powerUpDensity = episodeInfo.powerUpDensity;
		
		//Should we add a Power-up?
		if( powerUpDensity != 0 )
		{
			//Never add power ups on the first tile of a new road segment (tileIndex is zero).
			//Respect the power up density (number of tiles between power ups) specified for this level.
			if( tileIndex%powerUpDensity == 0 && tileIndex != 0 )
			{
				//Yes, we should.
				Transform placeholder = newTile.transform.Find("powerUpPlaceHolder");
				if( placeholder != null )
				{
					if( forcePowerUpType == PowerUpType.None )
					{
						int rdPowerUp = Random.Range( 1,8 );
						if( rdPowerUp == (int)PowerUpType.Shield )
						{
							addPowerUp( PowerUpType.Shield, placeholder, newTile );
						}
						else if( rdPowerUp == (int)PowerUpType.Magnet )
						{
							addPowerUp( PowerUpType.Magnet, placeholder, newTile );
						}
						else if( rdPowerUp == (int)PowerUpType.ZNuke )
						{
							addPowerUp( PowerUpType.ZNuke, placeholder, newTile );
						}
						else if( rdPowerUp == (int)PowerUpType.MagicBoots )
						{
							addPowerUp( PowerUpType.MagicBoots, placeholder, newTile );
						}
						else if( rdPowerUp == (int)PowerUpType.SlowTime )
						{
							addPowerUp( PowerUpType.SlowTime, placeholder, newTile );
						}
						else if( rdPowerUp == (int)PowerUpType.SpeedBoost )
						{
							addPowerUp( PowerUpType.SpeedBoost, placeholder, newTile );
						}
						else if( rdPowerUp == (int)PowerUpType.Life )
						{
							//Create a Life power-up
							//Life power-ups should be rare, so don't always spawn one
							if( Random.value > PERCENTAGE_LIFE_SPAWNED )
							{
								addPowerUp( PowerUpType.Life, placeholder, newTile );
							}
						}
					}
					else
					{
						addPowerUp( forcePowerUpType, placeholder, newTile );
					}
				}
			}
		}
	}
	
	private void addPowerUp ( PowerUpType type, Transform powerUpPlaceholder, GameObject newTile )
	{
		GameObject powerUpPrefab = powerUpDictionary[type].powerUpToSpawn;
		//Always place the power-up at the same height
		RaycastHit hit;
		if (Physics.Raycast(new Vector3(powerUpPlaceholder.position.x, powerUpPlaceholder.position.y + 3f, powerUpPlaceholder.position.z), Vector3.down, out hit, 10f ))
		{
			GameObject go = (GameObject)Instantiate(powerUpPrefab, new Vector3( powerUpPlaceholder.position.x, hit.point.y + DISTANCE_TO_GROUND, powerUpPlaceholder.position.z ), Quaternion.Euler( powerUpPrefab.transform.eulerAngles.x, powerUpPlaceholder.eulerAngles.y, powerUpPrefab.transform.eulerAngles.z ) );
			go.transform.parent = newTile.transform;
		}
		else
		{
			Debug.LogWarning("PowerupManager - there is no ground below the powerup of type, " + type.ToString() + ", in tile " + newTile.name );
		}
	}

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
		GameManager.gameStateEvent += GameStateChange;
		PlayerStatsManager.powerUpInventoryChanged += PowerUpInventoryChanged;
	}

	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
		GameManager.gameStateEvent -= GameStateChange;
		PlayerStatsManager.powerUpInventoryChanged -= PowerUpInventoryChanged;
	}

	void resetAllPowerUps()
	{
		foreach(KeyValuePair<PowerUpType, PowerUpData> pair in powerUpDictionary) 
		{
			deactivatePowerUp(pair.Key, true);
		}
		activePowerUps.Clear();
	}

	void PlayerStateChange( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			resetAllPowerUps();
		}
	}

	void PowerUpInventoryChanged()
	{
		equippedPowerUpQuantity.text = PlayerStatsManager.Instance.getPowerUpQuantity(PlayerStatsManager.Instance.getPowerUpSelected()).ToString();
	}

	public void hideImmediatelyPowerUp()
	{
		equippedPowerUp.anchoredPosition = new Vector2( START_X_POSITION, equippedPowerUp.anchoredPosition.y );
		magnetPowerUp.anchoredPosition = new Vector2( START_X_POSITION, magnetPowerUp.anchoredPosition.y );
		shieldPowerUp.anchoredPosition = new Vector2( START_X_POSITION, shieldPowerUp.anchoredPosition.y );
	}

	public void slideDisplayOutPowerUp()
	{
		slideOutPowerUp( equippedPowerUp );
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Checkpoint )
		{
			//slideDisplayOut(PlayerStatsManager.Instance.getPowerUpSelected());
			resetAllPowerUps();
		}
	}

	float getDuration( PowerUpData pud )
	{
		//For example: 10 + 3 * 5 = 25 seconds
		return pud.duration + PlayerStatsManager.Instance.getPowerUpUpgradeLevel(pud.powerUpType) * PowerUpDisplayData.getUpgradeBoostValue(pud.powerUpType);
	}

	float getImpactDiameter( PowerUpData pud )
	{
		//For example: 14 + 2 * 6 = 26 meters
		return BASE_DIAMETER + PlayerStatsManager.Instance.getPowerUpUpgradeLevel(pud.powerUpType) * PowerUpDisplayData.getUpgradeBoostValue(pud.powerUpType);
	}

	void slideEquippedPowerUp( PowerUpManager.PowerUpData pud )
	{
		Image[] images = equippedPowerUp.GetComponentsInChildren<Image>();
		images[0].sprite = PowerUpDisplayData.getPowerUpSprite( pud.powerUpType );
		equippedPowerUpQuantity.text = PlayerStatsManager.Instance.getPowerUpQuantity(pud.powerUpType).ToString();
		slideInPowerUp(equippedPowerUp);

	}

	void slideInPowerUp( RectTransform uiElement )
	{
		StartCoroutine( slideInPowerUpThread (uiElement ) );
	}

	IEnumerator slideInPowerUpThread (RectTransform uiElement )
	{
		float elapsed = 0;
		float initialPosX = uiElement.anchoredPosition.x;
		float distance = FINAL_X_POSITION - initialPosX;
		float posX;
		
		do
		{
			elapsed = elapsed + Time.deltaTime;
			posX = initialPosX + elapsed/SLIDE_DURATION * distance;
			uiElement.anchoredPosition = new Vector2( posX, uiElement.anchoredPosition.y );
			yield return _sync();
		} while ( elapsed < SLIDE_DURATION );
		uiElement.anchoredPosition = new Vector2( FINAL_X_POSITION, uiElement.anchoredPosition.y );
		
	}
	
	public void slideOutPowerUp( RectTransform uiElement )
	{
		StartCoroutine( slideOutPowerUpThread (uiElement ) );
	}

	IEnumerator slideOutPowerUpThread (RectTransform uiElement )
	{
		float elapsed = 0;
		float initialPosX = uiElement.anchoredPosition.x;
		float distance = initialPosX - START_X_POSITION;
		float posX;
		
		do
		{
			elapsed = elapsed + Time.deltaTime;
			posX = initialPosX - elapsed/SLIDE_DURATION * distance;
			uiElement.anchoredPosition = new Vector2( posX, uiElement.anchoredPosition.y );
			yield return _sync();
		} while ( elapsed < SLIDE_DURATION );
		uiElement.anchoredPosition = new Vector2( START_X_POSITION, uiElement.anchoredPosition.y );
	}

	[System.Serializable]
	public class PowerUpData
	{
		public PowerUpType powerUpType = PowerUpType.None;
		public float duration = 0f; //in seconds
		public ParticleSystem pickupEffect;
		public ParticleSystem activationEffect;
		public GameObject powerUpToSpawn;
		public AudioClip activationSound;
		public AudioClip deactivationSound;
		public PowerUpTimer powerUpTimer;

		public void startTimer(float timerDuration )
		{
			powerUpTimer.startTimer(timerDuration);
		}
		public void stopTimer()
		{
			powerUpTimer.stopTimer();
		}
	}
}
