using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreasureIslandManager : MonoBehaviour {

	public enum ChestGiftType {
		None=0,
		Key=1,				//Treasure island key
		Stars=2,	
		PowerUp = 3,
		Life = 4,
		Customization = 5	//Future implementation
	}

	bool levelLoading = false;

	ChestData MagicalChestData;

	//For debugging
	//When forceChestGiftType is not set to NONE, only the chest type specified will be added to the level. Not implemented yet.
	ChestGiftType forceChestGiftType = ChestGiftType.None;

	//List of each chest available.
	public List<ChestData> chestList = new List<ChestData>();
	Dictionary<ChestGiftType,ChestData> chestDictionary = new Dictionary<ChestGiftType,ChestData>();

	//GUI related
	bool showDisplay = false;
	Vector2 saveMePopupSize = new Vector2( Screen.width * 0.8f, Screen.height * 0.3f );
	Rect saveMeModalRect;
	Rect saveMePopupRect;
	public GUIStyle chestTextStyle;
	GUIContent chestTextContent = new GUIContent( System.String.Empty );
	GUIContent numberOfKeysTextContent = new GUIContent( System.String.Empty );

	FCMain lastOpenChest;

	public GameObject propKey;
	public GameObject propStar;
	public GameObject propLife;
	public GameObject propMagicBoots;
	public GameObject propSlowTime;
	public GameObject propZNuke;
	public GameObject propCustomization;
	PowerUpType giftPowerUp;

	public GameObject propKeyParticle;
	public GameObject propStarParticle;
	public GameObject propLifeParticle;
	public GameObject propMagicBootsParticle;
	public GameObject propSlowTimeParticle;
	public GameObject propZNukeParticle;
	public GameObject propCustomizationParticle;

	void Awake() {

		fillDictionary();
		float posX = (Screen.width - saveMePopupSize.x)/2;
		float posY = Screen.height * 0.22f;
		saveMeModalRect = new Rect( posX, posY, saveMePopupSize.x, saveMePopupSize.y );
		saveMePopupRect = new Rect( 0,0, saveMePopupSize.x, saveMePopupSize.y );
		chestTextStyle.fixedWidth = saveMePopupRect.width * 0.9f;
		chestTextContent = new GUIContent( System.String.Empty );

	}

	// Use this for initialization
	void Start () {
		levelLoading = false;
		//The activity indicator has been started in WorldMapHandler
		Handheld.StopActivityIndicator();
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
	
	void OnGUI ()
	{
		if( showDisplay )
		{
			displayMessage();
		}
		displayNumberOfKeys();
		drawQuitButton();
	}
	
	void displayMessage()
	{
		GUI.BeginGroup(saveMeModalRect);
		Rect textRect = GUILayoutUtility.GetRect( chestTextContent, chestTextStyle );
		float textCenterX = (saveMePopupRect.width-textRect.width)/2f;
		float titleHeight = 0.3f * saveMePopupRect.height;
		Rect titleTextRect = new Rect( textCenterX, titleHeight, textRect.width, textRect.height );
		Utilities.drawLabelWithDropShadow( titleTextRect, chestTextContent, chestTextStyle );
		GUI.EndGroup();
		
	}

	void displayNumberOfKeys()
	{
		numberOfKeysTextContent.text = PlayerStatsManager.Instance.getTreasureIslandKeys().ToString();

		GUI.BeginGroup(saveMeModalRect);
		Rect textRect = GUILayoutUtility.GetRect( numberOfKeysTextContent, chestTextStyle );
		float textCenterX = (saveMePopupRect.width-textRect.width)/2f;
		float titleHeight = 0;
		Rect titleTextRect = new Rect( textCenterX, titleHeight, textRect.width, textRect.height );
		Utilities.drawLabelWithDropShadow( titleTextRect, numberOfKeysTextContent, chestTextStyle );
		GUI.EndGroup();
		
	}

	void showMessage()
	{
		//Cancel an invoke that may have been called by opening another chest just before (like hideMessage)
		CancelInvoke();
		showDisplay = true;
		print ("showMessage " + chestTextContent.text );
		Invoke ("hideMessage", 5f);
	}
	
	void hideMessage()
	{
		showDisplay = false;
	}

	void drawQuitButton()
	{
		//Draw button
		float marginX = Screen.width * 0.1f;
		float marginY = Screen.height - Screen.width * 0.2f;
		float buttonSize = Screen.width * 0.075f;
		Rect buttonRect = new Rect( marginX, marginY, 50, 50 );
		if( GUI.Button( buttonRect, "Quit" )) 
		{
			SoundManager.playButtonClick();
			StartCoroutine( quit() );
		}
	}

	IEnumerator quit()
	{
		if( !levelLoading )
		{
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			Application.LoadLevel( 3 );
		}
		
	}

	public void openChest( FCMain chest )
	{
		//Ignore if chest is already open
		if( chest.IsOpened()  ) return;

		//If the chest is locked (which means it was previously opened),
		//call open, but because it locked, it will play the locked sound and a small animation without opening
		if( chest.IsLocked() )
		{
			chest.Open();
			return;
		}

		//Only continue if the player has a key or else, display a message on how he can get a key
		if( PlayerStatsManager.Instance.getTreasureIslandKeys() > 0 || PlayerStatsManager.Instance.getHasInfiniteTreasureIslandKeys() )
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
			PlayerStatsManager.Instance.decreaseTreasureIslandKeys(1);
			chest.ToggleOpen();
			lastOpenChest = chest;
			Invoke ("giftPlayerWithTreasure", 0.5f);
		}
		else
		{
			chestTextContent.text = LocalizationManager.Instance.getText("TREASURE_CHEST_NEED_MORE_KEYS");
			showMessage();
		}
	}

	void addPropToChest( FCProp fcProp, ChestGiftType chestGiftType)
	{
		FCPropParticle fcPropParticle = fcProp.gameObject.GetComponent<FCPropParticle>();

		switch (chestGiftType)
		{
		case ChestGiftType.Key:
			fcProp.m_Prefab = propKey;
			//Position
			fcProp.m_PosBegin = new Vector3( 0, 0.4f, 0 );
			fcProp.m_PosEnd = new Vector3( 0, 1, 0 );
			fcProp.m_PosDelay = 0.25f;
			fcProp.m_PosDuration = 2f;
			//Rotation
			fcProp.m_RotationType = FCProp.eRotationType.Endless;
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
			fcPropParticle.m_Prefab = propKeyParticle;

			break;
			
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
			chestTextContent.text = entryText;
			Debug.Log("giftPlayerWithTreasure: gave " + quantityToGive + " power-ups of type " + giftPowerUp + " to Hero.");
			break;
			
		case ChestGiftType.Stars:
			PlayerStatsManager.Instance.modifyCoinCount( quantityToGive );
			entryText = LocalizationManager.Instance.getText( "TREASURE_CHEST_STARS" );
			entryText = entryText.Replace("<quantity>", quantityToGive.ToString("N0") );
			chestTextContent.text = entryText;
			Debug.Log("giftPlayerWithTreasure: gave " + quantityToGive + " stars to Hero.");
			break;

		case ChestGiftType.Key:
			PlayerStatsManager.Instance.increaseTreasureIslandKeys( quantityToGive );
			entryText = LocalizationManager.Instance.getText( "TREASURE_CHEST_KEY" );
			entryText = entryText.Replace("<quantity>", quantityToGive.ToString("N0") );
			chestTextContent.text = entryText;
			Debug.Log("giftPlayerWithTreasure: gave " + quantityToGive + " mystery chest keys to Hero.");
			break;
			
		case ChestGiftType.Life:
			PlayerStatsManager.Instance.increaseLives(quantityToGive);
			PlayerStatsManager.Instance.savePlayerStats();
			entryText = LocalizationManager.Instance.getText( "TREASURE_CHEST_LIFE" );
			entryText = entryText.Replace("<quantity>", quantityToGive.ToString() );
			chestTextContent.text = entryText;
			Debug.Log("giftPlayerWithTreasure: gave " + quantityToGive + " lives to Hero.");
			break;
			
		case ChestGiftType.Customization:
			entryText = LocalizationManager.Instance.getText( "TREASURE_CHEST_CUSTOMIZATION" );
			chestTextContent.text = entryText;
			Debug.Log("giftPlayerWithTreasure: Customization - future implementation.");
			break;
		}
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
			if( touch.tapCount == 2 )
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
