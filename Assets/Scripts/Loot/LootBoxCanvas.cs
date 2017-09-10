using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class LootBoxCanvas : MonoBehaviour {

	[Header("General")]
	//Data for every type of loot box. This data is static.
	[SerializeField] List<LootBoxData> lootBoxDataList = new List<LootBoxData>();
	//The transform holding the 3D models incuding the hero and the loot box.
	[SerializeField] Transform holder3D;

	//3D model of loot box. It can vary depending on the loot box types.
	[Header("Loot Box Model")]
	[SerializeField] Transform lootBoxSpawnLocation;
	GameObject lootBox;

	//The hero in the scene is the same as the one chosen in the Hero selection menu
	[Header("Hero")]
	[SerializeField] Transform heroSpawnLocation;
	GameObject hero;

	[Header("Loot Box UI")]
	//Shared
	[SerializeField] TextMeshProUGUI lootBoxTypeText;

	[Header("Loot box details like Base or Level")]
	[SerializeField] GameObject lootBoxDetails;
	[SerializeField] TextMeshProUGUI lootBoxDetailsText;
	
	[Header("Time remaining")]
	[SerializeField] GameObject timeRemaining;
	[SerializeField] TextMeshProUGUI timeRemainingText;
	[SerializeField] GameObject nextOneText;

	[Header("Unlock Information")]
	[SerializeField] GameObject unlockInformation;
	[SerializeField] TextMeshProUGUI unlockDetailText;
	[SerializeField] TextMeshProUGUI unlockCostText;

	[Header("Time to unlock information")]
	[SerializeField] GameObject timeToUnlockInformation;
	[SerializeField] TextMeshProUGUI timeToUnlockDetailText;
	[SerializeField] TextMeshProUGUI timeToUnlockText;

	[Header("Number of loot boxes ready to open")]
	[SerializeField] TextMeshProUGUI lootBoxOwnedText;

	[Header("Free loot box not ready to open")]
	[SerializeField] TextMeshProUGUI freeLootBoxExplanationText;

	[Header("Radial Button")]
	[SerializeField] RadialTimerButton radialTimerButton;
	[SerializeField] TextMeshProUGUI radialTimerText;

	[Header("Unlock Now Popup")]
	[SerializeField] GameObject unlockNowPopup;

	//Other variables
	public const int HOURS_BETWEEN_FREE_LOOT_BOX = 4;
	int currentIndex = -1;
	int lootBoxesOwned;
	LootBoxOwnedData selectedLootBoxData;

	void Start ()
	{
		Handheld.StopActivityIndicator();
		radialTimerButton.setOnInitialClickCallback( OnInitialClick );
		loadHero();
		addLootBoxesForTesting ();
		updateNumberOfLootBoxesReadyToOpen();
		StartCoroutine( updateNextFreeLootBoxTime() );
	}
	
	void loadHero()
	{
		int heroIndex = GameManager.Instance.playerProfile.selectedHeroIndex;
		HeroManager.HeroCharacter heroCharacter = HeroManager.Instance.getHeroCharacter( heroIndex );
		if( heroCharacter != null )
		{
			hero = GameObject.Instantiate( heroCharacter.skinPrefab, heroSpawnLocation.position, heroSpawnLocation.rotation );
			hero.transform.SetParent( holder3D );
			hero.transform.localScale = Vector3.one;
		}
	}

	void addLootBoxesForTesting ()
	{
		if( !Debug.isDebugBuild ) return;
		GameManager.Instance.playerInventory.addLootBox( new LootBoxOwnedData( LootBoxType.LEVEL_UP, 1, 1 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxOwnedData( LootBoxType.RACE_WON, 1, 1 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxOwnedData( LootBoxType.BASE_UNLOCKED, 1, 1 ) );
	}

	void updateNumberOfLootBoxesReadyToOpen()
	{
		//Remember that we ALWAYS have a Free loot box as part of our inventory so we can display when it will next open.
		lootBoxesOwned = GameManager.Instance.playerInventory.getNumberOfLootBoxesOwned();
		int lootBoxesReadyToBeOpened = lootBoxesOwned;

		//Is the free loot box ready to open?
		if( DateTime.UtcNow > getOpenTime() )
		{
			//The free loot box is ready.
		}
		else
		{
			//The free loot box is not ready. Substract one.
			lootBoxesReadyToBeOpened--;
		}
		lootBoxOwnedText.text = lootBoxesReadyToBeOpened.ToString();

		currentIndex++;
		if( currentIndex >= lootBoxesOwned )
		{
			currentIndex = 0;
		}
		configureLootBox();
	}

	/// <summary>
	/// Raises the click loot box event when the button has been pressed for the entire duration.
	/// </summary>
	public void OnClickLootBox()
	{
		UISoundManager.uiSoundManager.playButtonClick();

		switch( selectedLootBoxData.type )
		{
				case LootBoxType.FREE:
					openFreeLootBox();
				break;

				case LootBoxType.RACE_WON:
				case LootBoxType.BASE_UNLOCKED:
				case LootBoxType.LEVEL_UP:
					LootBoxClientManager.Instance.requestLootBox( selectedLootBoxData.type );
					GameManager.Instance.playerInventory.removeLootBoxAt( currentIndex );
				break;
		}
		updateNumberOfLootBoxesReadyToOpen();

	}

	public void OnInitialClick()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		print("OnInitialClick");
		if( selectedLootBoxData.type == LootBoxType.FREE )
		{
			CancelInvoke( "hideFreeLootBoxExplanationText" );
			//The free loot box is not ready. Display a message.
			freeLootBoxExplanationText.text = string.Format( LocalizationManager.Instance.getText("FREE_LOOT_BOX_EXPLANATION"), HOURS_BETWEEN_FREE_LOOT_BOX );
			freeLootBoxExplanationText.gameObject.SetActive( true );
			Invoke("hideFreeLootBoxExplanationText", 5f);
		}
	}

	void configureLootBox()
	{
		//Get the LootBoxOwned data
		selectedLootBoxData = GameManager.Instance.playerInventory.getLootBoxAt( currentIndex );
		//Localize the loot box name
		lootBoxTypeText.text = LocalizationManager.Instance.getText( "LOOT_BOX_NAME_" + selectedLootBoxData.type.ToString().ToUpper() );
		//Get the LootBoxData
		LootBoxData lootBoxData = getLootBoxData( selectedLootBoxData.type );
		//Destroy the previous loot box model
		GameObject.DestroyImmediate( lootBox );
		//Create a new loot box model
		lootBox = GameObject.Instantiate( lootBoxData.lootBoxPrefab, lootBoxSpawnLocation.position, lootBoxSpawnLocation.rotation );
		lootBox.transform.SetParent( holder3D );
		lootBox.transform.localScale = new Vector3( lootBoxData.lootBoxPrefab.transform.localScale.x, lootBoxData.lootBoxPrefab.transform.localScale.y, lootBoxData.lootBoxPrefab.transform.localScale.z );

		//Configure the UI
		switch( selectedLootBoxData.type )
		{
			case LootBoxType.FREE:
				configureFreeUI();
			break;

			case LootBoxType.LEVEL_UP:
				configureLevelUpUI();
			break;
			case LootBoxType.BASE_UNLOCKED:
				configureBaseUnlockedUI();
			break;
			case LootBoxType.RACE_WON:
				configureRaceWonUI();
			break;
		}
	}

	void configureFreeUI()
	{
		//freeLootBoxUI.SetActive( selectedLootBoxData.type == LootBoxType.FREE );	
		if( selectedLootBoxData.type == LootBoxType.FREE )
		{
			radialTimerButton.isActive = ( DateTime.UtcNow > getOpenTime() );
		}
		else
		{
			radialTimerButton.isActive = true;
		}
	}

	void configureLevelUpUI()
	{
	}

	void configureBaseUnlockedUI()
	{
	}

	void configureRaceWonUI()
	{
	}

	public void OnClickPrevious()
	{
		currentIndex--;
		if( currentIndex < 0 ) currentIndex = lootBoxesOwned  - 1;
		configureLootBox();
	}

	public void OnClickNext()
	{
		currentIndex++;
		if( currentIndex >= lootBoxesOwned )
		{
			currentIndex = 0;
		}
		configureLootBox();
	}
	
	#region Loot Box Data
	public LootBoxData getLootBoxData( LootBoxType type )
	{
		LootBoxData lootBoxData = lootBoxDataList.Find( lbd => lbd.type == type);
		if( lootBoxData != null )
		{
			return lootBoxData;
		}
		else
		{
			Debug.LogError("LootBoxCanvas-Could not find LootBoxData for type: " + type + ". Please configure this type in the editor." );
			return null;
		}
	}
	#endregion

	#region Free Loot Box
	IEnumerator updateNextFreeLootBoxTime()
	{
		nextOneText.SetActive( true );
		while( DateTime.UtcNow < getOpenTime() )
		{
			TimeSpan openTime = getOpenTime().Subtract( DateTime.UtcNow );
			string timeDisplayed = string.Format( LocalizationManager.Instance.getText( "FREE_LOOT_BOX_TIME_FORMAT" ), openTime.Hours, openTime.Minutes, openTime.Seconds );
			timeRemainingText.text = timeDisplayed;
			//Update every fifteen seconds
			yield return new WaitForSecondsRealtime( 15 );
		}
		nextOneText.SetActive( false );
		timeRemainingText.text = LocalizationManager.Instance.getText("FREE_LOOT_BOX_OPEN");
	}

	void openFreeLootBox()
	{
		//The free loot box is ready.
		LootBoxClientManager.Instance.requestLootBox( selectedLootBoxData.type );
		selectedLootBoxData.setLastFreeLootBoxOpenedTime( DateTime.UtcNow );
		//Schedule a local notification to remind the player of when his next free loot box will be available
		NotificationServicesHandler.Instance.scheduleFreeLootBoxNotification( HOURS_BETWEEN_FREE_LOOT_BOX * 60 );
		GameManager.Instance.playerInventory.serializePlayerInventory( true );
	}

	DateTime getOpenTime()
	{
		return GameManager.Instance.playerInventory.getFreeLootBoxOwnedData().getLastFreeLootBoxOpenedTime().AddHours(HOURS_BETWEEN_FREE_LOOT_BOX);
	}

	void hideFreeLootBoxExplanationText()
	{
		freeLootBoxExplanationText.gameObject.SetActive( false );
	}
	#endregion
}
