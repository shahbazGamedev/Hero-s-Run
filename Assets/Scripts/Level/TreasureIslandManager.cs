using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TreasureIslandManager : MonoBehaviour {

	public enum ChestGiftType {
		None=0,
		Stars=1,	
		PowerUp = 2,
		Life = 3,
		Customization = 4	//Future implementation
	}

	[Header("Treasure Island")]
	public Text titleText; 		//e.g. Treasure island
	public Text locationText;	//e.g. Pirate's Cove
	public Text numberOfKeysText;	//Number of keys owned by player
	public GameObject chestContentPanel;
	public Text chestContentText;	//Description of the chest content
	public List<GameObject> availableChestModels = new List<GameObject>();
	public Transform chestSpawnLocation;
	public GameObject male;
	public GameObject female;
	Animation maleAnimation;
	Animation femaleAnimation;
	public AudioClip ambientSound;
	public Canvas fadeCanvas;


	bool levelLoading = false;

	ChestData MagicalChestData;

	//For debugging
	//When forceChestGiftType is not set to NONE, only the chest type specified will be added to the level. Not implemented yet.
	ChestGiftType forceChestGiftType = ChestGiftType.None;

	//List of each chest available.
	public List<ChestData> chestList = new List<ChestData>();
	Dictionary<ChestGiftType,ChestData> chestDictionary = new Dictionary<ChestGiftType,ChestData>();

	FCMain lastOpenChest;

	public GameObject propStar;
	public GameObject propLife;
	public GameObject propMagicBoots;
	public GameObject propSlowTime;
	public GameObject propZNuke;
	public GameObject propCustomization;
	PowerUpType giftPowerUp;

	public GameObject propStarParticle;
	public GameObject propLifeParticle;
	public GameObject propMagicBootsParticle;
	public GameObject propSlowTimeParticle;
	public GameObject propZNukeParticle;
	public GameObject propCustomizationParticle;

	void Awake()
 	{
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize();
		PlayerStatsManager.Instance.loadPlayerStats();
		#endif
		/* You can change the transform.position.z to control rendering priority.
 		This has to be done via script since it can't be changed in the inspector.
		(Not even in debug mode...it's editable in that case, but always just reverts to 0.)
		Canvases with a lower z position are rendered on top of canvases with a higher z position. */
		fadeCanvas.transform.position = new Vector3(fadeCanvas.transform.position.x,fadeCanvas.transform.position.y,-1);
		titleText.text = LocalizationManager.Instance.getText( "TREASURE_ISLAND_TITLE");
		locationText.text = LocalizationManager.Instance.getText( "TREASURE_ISLAND_LOCATION_1");
		numberOfKeysText.text = PlayerStatsManager.Instance.getTreasureKeysOwned().ToString();
		chestContentPanel.SetActive( false );

		fillDictionary();
	}

	void Start ()
 	{
		//The activity indicator has been started in WorldMapHandler
		Handheld.StopActivityIndicator();
		loadHero();
		spawnRandomChest();
		StartCoroutine( SoundManager.fadeInAmbience( ambientSound, 3f ) );

		if( PlayerStatsManager.Instance.getTreasureKeysOwned() > 0 )
		{
			InvokeRepeating("showTapToOpenChestMessage", 5f, 15f );
		}
		else
		{
			Invoke("showYouNeedKeysMessage", 3.5f );
		}

	}

	void Enable ()
 	{
		levelLoading = false;
		Animator fadeOutAnimator = fadeCanvas.GetComponent<Animator>();
		fadeOutAnimator.SetBool("FadeOut", true );
	}

	void loadHero()
	{
		Debug.Log("loadHero " + PlayerStatsManager.Instance.getAvatarName() );
		if( PlayerStatsManager.Instance.getAvatarName() == "Hero" )
		{
			female.SetActive( false );
			male.SetActive( true );
			maleAnimation = male.GetComponent<Animation>();
			maleAnimation.Play( "Hero_Idle_Loop" );
		}
		else
		{
			male.SetActive( false );
			female.SetActive( true );
			femaleAnimation = female.GetComponent<Animation>();
			femaleAnimation.Play( "Waiting_in_the_back" );
		}
	}

	void spawnRandomChest()
	{
		Debug.Log("spawnRandomChest " );
		int randomChestModel = Random.Range( 1,availableChestModels.Count );
		availableChestModels[randomChestModel].SetActive(true);
		//Position chest properly
		availableChestModels[randomChestModel].transform.position = chestSpawnLocation.position;
		availableChestModels[randomChestModel].transform.rotation = chestSpawnLocation.rotation;
		Debug.Log("spawnRandomChest " + availableChestModels[randomChestModel].name );
	}


	void fillDictionary()
	{
		//Copy all of the chest data into a dictionary for convenient access
		chestDictionary.Clear();
		foreach(ChestData chestData in chestList) 
		{
			chestDictionary.Add(chestData.chestGiftType, chestData );
		}
		//We no longer need chestList
		chestList.Clear();
		chestList = null;
	}

	void showMessage()
	{
		//Cancel an invoke that may have been called by opening another chest just before (like hideMessage)
		CancelInvoke();
		chestContentPanel.SetActive( true );
		Invoke ("hideMessage", 4f);
	}
	
	void hideMessage()
	{
		chestContentPanel.SetActive( false );
	}

	void showTapToOpenChestMessage()
	{
		AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("TRESURE_CHEST_TAP_TO_OPEN"), 0.35f, 3f );
	}

	void showYouNeedKeysMessage()
	{
		AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("TREASURE_CHEST_NEED_MORE_KEYS"), 0.35f, 4f );
	}

	public void closeMenu()
	{
		StartCoroutine( close() );
	}

	IEnumerator close()
	{
		Debug.Log("Close Treasure island " + levelLoading );
		if( !levelLoading )
		{
			SoundManager.playButtonClick();
			levelLoading = true;
			SoundManager.stopAmbience();
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.WorldMap );
		}
	}

	public void openChest( FCMain chest )
	{
		CancelInvoke();
		AchievementDisplay.enableShowDisplay( false );
		//Ignore if chest is already open
		if( chest.IsOpened()  ) return;

		//If the chest is locked (which means it was previously opened),
		//call open, but because it is locked, it will play the locked sound and a small animation without opening
		if( chest.IsLocked() )
		{
			chest.Open();
			return;
		}

		//Only continue if the player has a key or else, display a message on how he can get a key
		if( PlayerStatsManager.Instance.getTreasureKeysOwned() > 0 || PlayerStatsManager.Instance.getHasInfiniteTreasureIslandKeys() )
		{
			//If a chest was previously opened, close it and lock it before opening the new one
			if( lastOpenChest != null )
			{
				lastOpenChest.Close();
				lastOpenChest.Lock();
			}

			if( forceChestGiftType ==ChestGiftType.None )
			{
				int rdChest = Random.Range( 1,chestDictionary.Count );
				int i = 1;
				foreach(KeyValuePair<ChestGiftType,ChestData> pair in chestDictionary) 
				{
					if( i == rdChest )
					{
						//We found our entry
						MagicalChestData = pair.Value;
						FCProp fcProp = chest.gameObject.GetComponent<FCProp>();
						addPropToChest( fcProp, MagicalChestData.chestGiftType );
					}
					i++;
				}
			}
			else
			{
				MagicalChestData = chestDictionary[forceChestGiftType];
				FCProp fcProp = chest.gameObject.GetComponent<FCProp>();
				addPropToChest( fcProp, MagicalChestData.chestGiftType );
			}
			//This consumes one key
			PlayerStatsManager.Instance.decreaseTreasureKeysOwned(1);
			numberOfKeysText.text = PlayerStatsManager.Instance.getTreasureKeysOwned().ToString();

			chest.ToggleOpen();
			lastOpenChest = chest;
			Invoke ("giftPlayerWithTreasure", 0.5f);
		}
		else
		{
			chestContentText.text = LocalizationManager.Instance.getText("TREASURE_CHEST_NEED_MORE_KEYS");
			showMessage();
		}
	}

	void addPropToChest( FCProp fcProp, ChestGiftType chestGiftType)
	{
		FCPropParticle fcPropParticle = fcProp.gameObject.GetComponent<FCPropParticle>();

		switch (chestGiftType)
		{
			
		case ChestGiftType.Stars:
			fcProp.m_Prefab = propStar;
			//Position
			fcProp.m_PosBegin = new Vector3( 0, 0.4f, 0 );
			fcProp.m_PosEnd = new Vector3( 0, 0, 0 );
			fcProp.m_PosDelay = 0.25f;
			fcProp.m_PosDuration = 1f;
			//Rotation
			fcProp.m_RotationType = FCProp.eRotationType.Disable;
			fcProp.m_RotationEaseType = FCEaseType.eEaseType.InOutQuad;
			fcProp.m_Rotation = new Vector3( 0, -1, 0 );
			fcProp.m_MaxRotationRound = 5;
			fcProp.m_RotationDelay = 0;
			fcProp.m_RotationDurationPerRound = 1;
			//Scale
			fcProp.m_ScaleType = FCProp.eScaleType.Disable;
			fcProp.m_ScaleBegin = new Vector3( 1f, 1f, 1f );
			fcProp.m_ScaleEnd = new Vector3( 1f, 1f, 1f );
			fcProp.m_ScaleDelay = 0.5f;
			fcProp.m_ScaleDuration = 1f;
			//Prop particle
			fcPropParticle.m_Prefab = propStarParticle;
			break;
			
		case ChestGiftType.PowerUp:
			//Select a random power-up to give
			giftPowerUp = PowerUpManager.getRandomConsumablePowerUp();
			//giftPowerUp = PowerUpType.MagicBoots; //for debugging
			GameObject powerUpProp = null;
			switch (giftPowerUp)
			{
				case PowerUpType.MagicBoots:
					powerUpProp = propMagicBoots;
					fcProp.m_Prefab = powerUpProp;
					//Position
					fcProp.m_PosEaseType = FCEaseType.eEaseType.linear;
					fcProp.m_PosBegin = new Vector3( 0, 0.4f, 0 );
					fcProp.m_PosEnd = new Vector3( 0, 0.5f, 0 );
					fcProp.m_PosDelay = 0.25f;
					fcProp.m_PosDuration = 2f;
					//Rotation
					fcProp.m_RotationType = FCProp.eRotationType.Disable;
					fcProp.m_RotationEaseType = FCEaseType.eEaseType.InOutQuad;
					fcProp.m_Rotation = new Vector3( 1, -1, 0 );
					fcProp.m_MaxRotationRound = 5;
					fcProp.m_RotationDelay = 0;
					fcProp.m_RotationDurationPerRound = 1;
					//Scale
					fcProp.m_ScaleType = FCProp.eScaleType.Enable;
					fcProp.m_ScaleEaseType = FCEaseType.eEaseType.OutElastic;
					fcProp.m_ScaleBegin = new Vector3( 0.3f, 0.3f, 0.3f );
					fcProp.m_ScaleEnd = new Vector3( 0.6f, 0.6f, 0.6f );
					fcProp.m_ScaleDelay = 0.5f;
					fcProp.m_ScaleDuration = 1f;
					//Prop particle
					fcPropParticle.m_Prefab = propMagicBootsParticle;
					break;
				case PowerUpType.SlowTime:
					powerUpProp = propSlowTime;
					fcProp.m_Prefab = powerUpProp;
					//Position
					fcProp.m_PosEaseType = FCEaseType.eEaseType.linear;
					fcProp.m_PosBegin = new Vector3( 0, 0.4f, 0 );
					fcProp.m_PosEnd = new Vector3( 0, 0.5f, 0 );
					fcProp.m_PosDelay = 0.25f;
					fcProp.m_PosDuration = 2f;
					//Rotation
					fcProp.m_RotationType = FCProp.eRotationType.Disable;
					fcProp.m_RotationEaseType = FCEaseType.eEaseType.InOutQuad;
					fcProp.m_Rotation = new Vector3( 1, -1, 0 );
					fcProp.m_MaxRotationRound = 5;
					fcProp.m_RotationDelay = 0;
					fcProp.m_RotationDurationPerRound = 1;
					//Scale
					fcProp.m_ScaleType = FCProp.eScaleType.Enable;
					fcProp.m_ScaleEaseType = FCEaseType.eEaseType.OutElastic;
					fcProp.m_ScaleBegin = new Vector3( 0.2f, 0.2f, 0.2f );
					fcProp.m_ScaleEnd = new Vector3( 0.4f, 0.4f, 0.4f );
					fcProp.m_ScaleDelay = 0.5f;
					fcProp.m_ScaleDuration = 1f;
					//Prop particle
					fcPropParticle.m_Prefab = propSlowTimeParticle;
					break;
				case PowerUpType.ZNuke:
					powerUpProp = propZNuke;
					fcProp.m_Prefab = powerUpProp;
					//Position
					fcProp.m_PosEaseType = FCEaseType.eEaseType.linear;
					fcProp.m_PosBegin = new Vector3( 0, 0.4f, 0 );
					fcProp.m_PosEnd = new Vector3( 0, 0.5f, 0 );
					fcProp.m_PosDelay = 0.25f;
					fcProp.m_PosDuration = 2f;
					//Rotation
					fcProp.m_RotationType = FCProp.eRotationType.Disable;
					fcProp.m_RotationEaseType = FCEaseType.eEaseType.InOutQuad;
					fcProp.m_Rotation = new Vector3( 1, -1, 0 );
					fcProp.m_MaxRotationRound = 5;
					fcProp.m_RotationDelay = 0;
					fcProp.m_RotationDurationPerRound = 1;
					//Scale
					fcProp.m_ScaleType = FCProp.eScaleType.Enable;
					fcProp.m_ScaleEaseType = FCEaseType.eEaseType.OutElastic;
					fcProp.m_ScaleBegin = new Vector3( 0.2f, 0.2f, 0.2f );
					fcProp.m_ScaleEnd = new Vector3( 0.4f, 0.4f, 0.4f );
					fcProp.m_ScaleDelay = 0.5f;
					fcProp.m_ScaleDuration = 1f;
					//Prop particle
					fcPropParticle.m_Prefab = propZNukeParticle;
					break;
				}
			break;
			
		case ChestGiftType.Life:
			fcProp.m_Prefab = propLife;
			//Position
			fcProp.m_PosBegin = new Vector3( 0, 0.5f, 0 );
			fcProp.m_PosEnd = new Vector3( 0, 1f, 0 );
			fcProp.m_PosDelay = 0.25f;
			fcProp.m_PosDuration = 2f;
			//Rotation
			fcProp.m_RotationType = FCProp.eRotationType.Endless;
			fcProp.m_RotationEaseType = FCEaseType.eEaseType.InOutQuad;
			fcProp.m_Rotation = new Vector3( 1, -1, 0 );
			fcProp.m_MaxRotationRound = 5;
			fcProp.m_RotationDelay = 0;
			fcProp.m_RotationDurationPerRound = 1;
			//Scale
			fcProp.m_ScaleType = FCProp.eScaleType.Enable;
			fcProp.m_ScaleEaseType = FCEaseType.eEaseType.OutElastic;
			fcProp.m_ScaleBegin = new Vector3( 0.3f, 0.3f, 0.3f );
			fcProp.m_ScaleEnd = new Vector3( 0.6f, 0.6f, 0.6f );
			fcProp.m_ScaleDelay = 0.5f;
			fcProp.m_ScaleDuration = 1f;
			//Prop particle
			fcPropParticle.m_Prefab = propLifeParticle;
			break;
			
		case ChestGiftType.Customization:
			fcProp.m_Prefab = propCustomization;
			//Position
			fcProp.m_PosEaseType = FCEaseType.eEaseType.linear;
			fcProp.m_PosBegin = new Vector3( 0, 0.4f, 0 );
			fcProp.m_PosEnd = new Vector3( 0, 0.5f, 0 );
			fcProp.m_PosDelay = 0.25f;
			fcProp.m_PosDuration = 2f;
			//Rotation
			fcProp.m_RotationType = FCProp.eRotationType.Disable;
			fcProp.m_RotationEaseType = FCEaseType.eEaseType.InOutQuad;
			fcProp.m_Rotation = new Vector3( 1, -1, 0 );
			fcProp.m_MaxRotationRound = 5;
			fcProp.m_RotationDelay = 0;
			fcProp.m_RotationDurationPerRound = 1;
			//Scale
			fcProp.m_ScaleType = FCProp.eScaleType.Disable;
			fcProp.m_ScaleEaseType = FCEaseType.eEaseType.OutElastic;
			fcProp.m_ScaleBegin = new Vector3( 0.3f, 0.3f, 0.3f );
			fcProp.m_ScaleEnd = new Vector3( 0.6f, 0.6f, 0.6f );
			fcProp.m_ScaleDelay = 0.5f;
			fcProp.m_ScaleDuration = 1f;
			//Prop particle
			fcPropParticle.m_Prefab = propCustomizationParticle;
			break;
		}
	}

	void giftPlayerWithTreasure()
	{
		int quantityToGive = Random.Range( MagicalChestData.giftMinimum, MagicalChestData.giftMaximum + 1 );
		string entryText = "NOT SET";
		switch( MagicalChestData.chestGiftType )
		{
		case ChestGiftType.PowerUp:
			PlayerStatsManager.Instance.addToPowerUpInventory( giftPowerUp, quantityToGive );
			entryText = LocalizationManager.Instance.getText( "TREASURE_CHEST_POWER_UP" );
			entryText = entryText.Replace("<quantity>", quantityToGive.ToString() );
			entryText = entryText.Replace("<powerup name>", PowerUpDisplayData.getPowerUpName( giftPowerUp ) );
			chestContentText.text = entryText;
			Debug.Log("giftPlayerWithTreasure: gave " + quantityToGive + " power-ups of type " + giftPowerUp + " to Hero.");
			break;
			
		case ChestGiftType.Stars:
			PlayerStatsManager.Instance.modifyCurrentCoins( quantityToGive, false, false );
			entryText = LocalizationManager.Instance.getText( "TREASURE_CHEST_STARS" );
			entryText = entryText.Replace("<quantity>", quantityToGive.ToString("N0") );
			chestContentText.text = entryText;
			Debug.Log("giftPlayerWithTreasure: gave " + quantityToGive + " stars to Hero.");
			break;
			
		case ChestGiftType.Life:
			PlayerStatsManager.Instance.increaseLives(quantityToGive);
			PlayerStatsManager.Instance.savePlayerStats();
			entryText = LocalizationManager.Instance.getText( "TREASURE_CHEST_LIFE" );
			entryText = entryText.Replace("<quantity>", quantityToGive.ToString() );
			chestContentText.text = entryText;
			Debug.Log("giftPlayerWithTreasure: gave " + quantityToGive + " lives to Hero.");
			break;
			
		case ChestGiftType.Customization:
			entryText = LocalizationManager.Instance.getText( "TREASURE_CHEST_CUSTOMIZATION" );
			chestContentText.text = entryText;
			Debug.Log("giftPlayerWithTreasure: Customization - future implementation.");
			break;
		}
		PlayerStatsManager.Instance.savePlayerStats();
		Invoke ("showMessage", 0.5f);
	}
	
	// Update is called once per frame
	void Update () {

		#if UNITY_EDITOR
		// User pressed the left mouse up
		if (Input.GetMouseButtonUp(0))
		{
			MouseButtonUp(0);
		}
		#else
		detectTaps();
		#endif
	}

	void MouseButtonUp(int Button)
	{
		FCMain pMain = GetHitChest(Input.mousePosition);
		if( pMain != null )
		{
			openChest( pMain );
		}
	}

	void detectTaps()
	{
		if ( Input.touchCount > 0 )
		{
			Touch touch = Input.GetTouch(0);
			if( touch.tapCount == 1 )
			{
				if( touch.phase == TouchPhase.Ended  )
				{
					FCMain pMain = GetHitChest(Input.GetTouch(0).position);
					if( pMain != null )
					{
						openChest( pMain );
					}
				}
			}
		}
	}

	FCMain GetHitChest( Vector2 touchPosition )
	{
		// We need to actually hit an object
		RaycastHit hitt;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(touchPosition), out hitt, 1000))
		{
			if (hitt.collider)
			{
				return hitt.collider.gameObject.GetComponent<FCMain>();
			}
		}
		
		return null;
	}

	
	[System.Serializable]
	public class ChestData
	{
		public ChestGiftType chestGiftType = ChestGiftType.None;
		public int giftMinimum = 1;
		public int giftMaximum = 1;
		
	}

}
