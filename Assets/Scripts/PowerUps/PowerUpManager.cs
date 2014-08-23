﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PowerUpType {
	//Note: Don't change these values without updating PlayerStatsManager
	None=0,
	Shield=1,
	Magnet=2,
	ZNuke = 3,
	MagicBoots = 4,
	SlowTime = 5,
	Life = 7
}

public class PowerUpManager : BaseClass {


	//The player can have multiple powerups active at the same time.
	//I.e. Magnet and Shield can be active at the same time, but not two magnets.
	static List<PowerUpType> activePowerUps = new List<PowerUpType>();

	GameObject player;
	PlayerController playerController;
	public ZombieManager zombieManager;
	PowerUpHUD powerUpHUD;

	//Power Up audio. They use 2D sound.
	public AudioClip pickUpSound;

	//For debugging
	//When forcePowerUpType is not set to NONE, only the power up type specified will be added to the level.
	PowerUpType forcePowerUpType = PowerUpType.None;

	//List of each powerup available.
	public List<PowerUpData> powerUpList = new List<PowerUpData>(6);
	static Dictionary<PowerUpType,PowerUpData> powerUpDictionary = new Dictionary<PowerUpType,PowerUpData>(6);

	//Duration is extended by the upgrade level * UPGRADE_DURATION_BOOST
	const float UPGRADE_DURATION_BOOST = 5; 	//in seconds
	//Base diameter is the value if upgrade level is 0. It is used by zNuke.
	const float BASE_DIAMETER = 14f; 	//in meters
	//Impact diameter is extended by the upgrade level * UPGRADE_DIAMETER_BOOST
	const float UPGRADE_DIAMETER_BOOST = 6f; 	//in meters

	void Awake()
	{
		fillDictionary();
		player = GameObject.FindGameObjectWithTag("Player");	
		playerController = (PlayerController) player.GetComponent("PlayerController");
		GameObject powerUpManagerObject = GameObject.FindGameObjectWithTag("PowerUpManager");
		powerUpHUD = powerUpManagerObject.GetComponent<PowerUpHUD>();
	}
	
	public void changeSelectedPowerUp(PowerUpType newPowerUpType )
	{
		PlayerStatsManager.Instance.setPowerUpSelected( newPowerUpType );
		PowerUpData pud = powerUpDictionary[newPowerUpType];
		powerUpHUD.activateDisplay(pud);
	}
	
	void fillDictionary()
	{
		//Copy all of the power up data into a dictionary for convenient access
		powerUpDictionary.Clear();
		foreach(PowerUpData powerUpData in powerUpList) 
		{
			powerUpDictionary.Add(powerUpData.powerUpType, powerUpData );
		}
		//We no longer need powerUpList
		powerUpList.Clear();
		powerUpList = null;
	}

	public static PowerUpType getRandomConsumablePowerUp()
	{
		int rd = Random.Range(0,3);
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
		}
		return randomConsumablePowerUp;
	}
	
	IEnumerator startTimerMagnet( PowerUpData pud )
	{
		float duration = getDuration(pud);
		do
		{
			duration = duration - Time.deltaTime;
			yield return _sync();
		} while ( duration > 0 );
		//Duration has expired. Deactivate power up.
		deactivatePowerUp( pud.powerUpType, false );
	}

	IEnumerator startTimerShield( PowerUpData pud )
	{
		float duration = getDuration(pud);
		do
		{
			duration = duration - Time.deltaTime;
			yield return _sync();
		} while ( duration > 0 );
		//Duration has expired. Deactivate power up.
		deactivatePowerUp( pud.powerUpType, false );
	}
	
	IEnumerator startTimerSlowTime( PowerUpData pud )
	{
		float duration = getDuration(pud);
		do
		{
			duration = duration - Time.deltaTime;
			yield return _sync();
		} while ( duration > 0 );
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
		audio.PlayOneShot( pickUpSound );
		//Play a pick-up particle effect if one has been specified
		if( pud.pickupEffect != null )
		{
			//Make the player the parent of this particle system
			pud.pickupEffect.transform.parent = player.transform;
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
			pud.activationEffect.transform.parent = player.transform;
			pud.activationEffect.transform.localPosition = new Vector3( 0, 1f, 0 );
			pud.activationEffect.Play();
		}

		//Verify if this type of power up is already active.
		if( activePowerUps.Contains( powerUpType ) )
		{
			//Yes, it is already active
			switch( powerUpType )
			{
			case PowerUpType.Magnet:
				
				audio.PlayOneShot( pickUpSound );
				StopCoroutine( "startTimerMagnet" );
				StartCoroutine( "startTimerMagnet", pud );
				Debug.Log("Magnet - prolonging duration." );
				break;

			case PowerUpType.Shield:
				
				audio.PlayOneShot( pickUpSound );
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
					audio.PlayOneShot( pickUpSound );
					activePowerUps.Add( pud.powerUpType );
					GameObject magnetSphere = player.transform.Find ("Magnet Sphere").gameObject;
					CoinAttractor coinAttractor = (CoinAttractor) magnetSphere.GetComponent(typeof(CoinAttractor));
					float radius =  (magnetSphere.collider as SphereCollider).radius;
					coinAttractor.attractCoinsWithinSphere( magnetSphere.transform.position, radius );
					StartCoroutine( "startTimerMagnet", pud );
					powerUpHUD.activateDisplay(pud);
					Debug.Log("Magnet - activatePowerUp with duration time of " + pud.duration );
				break;

				case PowerUpType.ZNuke:
				if( PlayerStatsManager.Instance.getPowerUpQuantity(PowerUpType.ZNuke) > 0 )
				{
					zombieManager.knockbackZombies( getImpactDiameter( pud ) );
					Debug.Log("PowerUpZNuke - activatePowerUp");
					PlayerStatsManager.Instance.decrementPowerUpInventory(pud.powerUpType);
				}
				break;

				case PowerUpType.MagicBoots:
				if( PlayerStatsManager.Instance.getPowerUpQuantity(PowerUpType.MagicBoots) > 0 )
				{
					playerController.doingDoubleJump = true;
					playerController.jump();
					Debug.Log("MagicBoots - activatePowerUp" );
					PlayerStatsManager.Instance.decrementPowerUpInventory(pud.powerUpType);
				}
				break;
			
				case PowerUpType.SlowTime:
				if( PlayerStatsManager.Instance.getPowerUpQuantity(PowerUpType.SlowTime) > 0 )
				{
					StopCoroutine( "startTimerSlowTime" );
					StartCoroutine( "startTimerSlowTime", pud );
					Debug.Log("SlowTime - activatePowerUp with duration time of " + pud.duration );
					//Slow down time
					Time.timeScale = 0.5f;
					PlayerStatsManager.Instance.decrementPowerUpInventory(pud.powerUpType);
				}
				break;

				case PowerUpType.Shield:
					audio.PlayOneShot( pickUpSound );
					activePowerUps.Add( pud.powerUpType );
					turnSomeCollidersIntoTriggers( true );
					StartCoroutine( "startTimerShield", pud );
					powerUpHUD.activateDisplay(pud);
					Debug.Log("Shield - activatePowerUp with duration time of " + pud.duration );
				break;
			}
		}
	}
	
	public void deactivatePowerUp(PowerUpType powerUpType, bool instantly )
	{
		PowerUpData pud = powerUpDictionary[powerUpType];

		//Play a deactivation sound if one has been set.
		if ( !instantly && pud.deactivationSound != null ) audio.PlayOneShot( pud.deactivationSound );

		switch( powerUpType )
		{
		case PowerUpType.Magnet:
			removeFromActiveList( PowerUpType.Magnet );
			StopCoroutine( "startTimerMagnet" );
			powerUpHUD.slideDisplayOut( powerUpType );
			break;

		case PowerUpType.Shield:
			removeFromActiveList( PowerUpType.Shield );
			StopCoroutine( "startTimerShield" );
			turnSomeCollidersIntoTriggers( false );
			powerUpHUD.slideDisplayOut( powerUpType );
			break;

		case PowerUpType.SlowTime:
			StopCoroutine( "startTimerSlowTime" );
			if( instantly )
			{
				//Restore normal time immediately
				StopCoroutine( "accelerateAfterSlowTime" );
				Time.timeScale = 1f;
			}
			else
			{
				//If the slow-time power-up expires normally, restore the time scale gradually back to 1.
				StartCoroutine( "accelerateAfterSlowTime" );
			}
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
				collider.isTrigger = isActive;
				//Since we are turning the collider into a trigger, we need to disable gravity for objects with a rigid body
				//as well or else the object will fall through the ground.
				if( collider.rigidbody != null )
				{
					collider.rigidbody.useGravity = !isActive;
				}
			}
		}
	}

	public void considerAddingPowerUp( GameObject newTile, int tileIndex )
	{
		LevelData.LevelInfo levelInfo = LevelManager.Instance.getLevelInfo();
		int powerUpDensity = levelInfo.powerUpDensity;
		
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
						int rdPowerUp = Random.Range( 1,7 );
						if( rdPowerUp == 1 )
						{
							//Create a Magnet power-up
							addPowerUp( PowerUpType.Magnet, placeholder, newTile );
						}
						else if( rdPowerUp == 2 )
						{
							//Create a Shield power-up
							addPowerUp( PowerUpType.Shield, placeholder, newTile );
						}
						else if( rdPowerUp == 3 )
						{
							//Create a Magic Boots power-up
							addPowerUp( PowerUpType.MagicBoots, placeholder, newTile );
						}
						else if( rdPowerUp == 4 )
						{
							//Create a Slow Time power-up
							addPowerUp( PowerUpType.SlowTime, placeholder, newTile );
						}
						else if( rdPowerUp == 5 )
						{
							//Create a Life power-up
							addPowerUp( PowerUpType.Life, placeholder, newTile );
						}
						else if( rdPowerUp >= 6 )
						{
							//Create a ZNuke power-up
							addPowerUp( PowerUpType.ZNuke, placeholder, newTile );
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
		GameObject go = (GameObject)Instantiate(powerUpPrefab, powerUpPlaceholder.position, Quaternion.Euler( powerUpPrefab.transform.eulerAngles.x, powerUpPlaceholder.eulerAngles.y, powerUpPrefab.transform.eulerAngles.z ) );
		go.transform.parent = newTile.transform;
	}

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
		GameManager.gameStateEvent += GameStateChange;
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}

	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
		GameManager.gameStateEvent -= GameStateChange;
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Give_Powerup )
		{
			//Player needs two power-ups for tutorial
			int currentNumberPowerUp =  PlayerStatsManager.Instance.getPowerUpQuantity( PowerUpType.MagicBoots );
			int delta = 2 - currentNumberPowerUp;
			if( delta > 0 )
			{
				//Player does not have enough power-ups. Add the missing delta.
				PlayerStatsManager.Instance.addToPowerUpInventory( PowerUpType.MagicBoots, delta );
			}
		}
	}

	void resetAllPowerUps()
	{
		foreach(KeyValuePair<PowerUpType, PowerUpData> pair in powerUpDictionary) 
		{
			deactivatePowerUp(pair.Key, true);
		}
		activePowerUps.Clear();
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			resetAllPowerUps();
		}
	}

	public void hideImmediatelyPowerUp()
	{
		powerUpHUD.hideImmediately(PlayerStatsManager.Instance.getPowerUpSelected());
	}

	public void slideDisplayOutPowerUp()
	{
		powerUpHUD.slideDisplayOut(PlayerStatsManager.Instance.getPowerUpSelected());
	}

		void GameStateChange( GameState newState )
	{
		if( newState == GameState.Checkpoint )
		{
			powerUpHUD.slideDisplayOut(PlayerStatsManager.Instance.getPowerUpSelected());
			resetAllPowerUps();
		}
		else if( newState == GameState.StatsScreen )
		{
			powerUpHUD.hideImmediately();
			resetAllPowerUps();
		}
	}

	float getDuration( PowerUpData pud )
	{
		//For example: 10 + 3 * 5 = 25 seconds
		return pud.duration + PlayerStatsManager.Instance.getPowerUpUpgradeLevel(pud.powerUpType) * UPGRADE_DURATION_BOOST;
	}

	float getImpactDiameter( PowerUpData pud )
	{
		//For example: 14 + 2 * 6 = 26 meters
		return BASE_DIAMETER + PlayerStatsManager.Instance.getPowerUpUpgradeLevel(pud.powerUpType) * UPGRADE_DIAMETER_BOOST;
	}
	
	[System.Serializable]
	public class PowerUpData
	{
		public PowerUpType powerUpType = PowerUpType.None;
		public float duration = 0f; //in seconds
		public ParticleSystem pickupEffect;
		public ParticleSystem activationEffect;
		public GameObject powerUpToSpawn;
		public AudioClip deactivationSound;
	}
}
