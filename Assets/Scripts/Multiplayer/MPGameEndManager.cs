using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MPGameEndManager : MonoBehaviour {

	[Header("Testing Mode")]
	[SerializeField] bool xpTestingMode = false;
	[Header("Race Panel")]
	[SerializeField] Image backgroundImage;
	[SerializeField] Image circuitImage;
	[SerializeField] Text raceResult;
	[SerializeField] Text playerName;
	[SerializeField] Text raceTime;
	
	[Header("XP Panel")]
	[SerializeField] Text currentLevelText;
	[SerializeField] Text nextLevelText;
	[SerializeField] Text currentAndNextXP;
	[SerializeField] Text awardedXP;
	[SerializeField] Text totalXPAwarded;
	[SerializeField] Slider sliderXP;

	[SerializeField] Text numberOfTrophiesText;

	[Header("Sector Change Popup")]
	[SerializeField] GameObject sectorChangePopup;

	[Header("Other")]
	MatchmakingManager matchmakingManager;
	const float ANIMATION_DURATION = 3f;

	void Start ()
	{
		matchmakingManager = GameObject.FindGameObjectWithTag("Matchmaking").GetComponent<MatchmakingManager>();

		configureRacePanel();

		//Individual XP Awards
		int xpEarnedFromRace = calculatedTotalXPAwarded();
		StartCoroutine( displayIndividualAwards() );

		if( !xpTestingMode ) StartCoroutine( configureXPPanel(xpEarnedFromRace) );
	}

	void configureRacePanel()
	{
		LevelData.MultiplayerInfo multiplayerInfo = LevelManager.Instance.getSelectedCircuit();
		circuitImage.sprite = multiplayerInfo.circuitInfo.circuitImage;
		raceResult.text = LocalizationManager.Instance.getText( "EOG_" + PlayerRaceManager.Instance.racePosition.ToString() );
		playerName.text = GameManager.Instance.playerProfile.getUserName();
		backgroundImage.color = multiplayerInfo.circuitInfo.backgroundColor;

		if( CompetitionManager.Instance.canEarnCompetitivePoints() )
		{
			numberOfTrophiesText.gameObject.SetActive( true );
			int trophiesEarnedLastRace = GameManager.Instance.playerProfile.getTrophiesEarnedLastRace();
			if( trophiesEarnedLastRace > 0 )
			{
				//Add a plus sign to make it clearer
				numberOfTrophiesText.text = "+" + trophiesEarnedLastRace.ToString();
			}
			else
			{
				numberOfTrophiesText.text = trophiesEarnedLastRace.ToString();
			}
			//Reset value just to be safe
			GameManager.Instance.playerProfile.setTrophiesEarnedLastRace(0);
		}
		else
		{
			//In this play mode, no trophies can be earned, so hide the trophy details.
			numberOfTrophiesText.gameObject.SetActive( false );
		}

		//Race time
		TimeSpan ts = TimeSpan.FromSeconds( PlayerRaceManager.Instance.raceDuration );
		DateTime dt = new DateTime(ts.Ticks);
		raceTime.text = LocalizationManager.Instance.getText( "EOG_RACE_TIME" ).Replace("<race duration>", dt.ToString("mm:ss") );

	}

	public void grantTestXP( int testXPAmount )
	{
		StartCoroutine( configureXPPanel(testXPAmount) );
	}

	int calculatedTotalXPAwarded()
	{
		XPAwardType awardType;
		ProgressionManager.XPAward xpAward;
		int total = 0;
		for( int i = 0; i < PlayerRaceManager.Instance.raceAwardList.Count; i++ )
		{
			awardType = PlayerRaceManager.Instance.raceAwardList[i];
			xpAward = ProgressionManager.Instance.getXPAward( awardType );
			total = total + xpAward.xpAmount;
		}
		Debug.Log("MPGameEndManager-calculatedTotalXPAwarded: total: " + total );
		return total;
	}

	IEnumerator configureXPPanel( int xpEarnedFromRace )
	{
		//Get the amount of xp already earned towards the next level
		int xpAlreadyEarned = GameManager.Instance.playerProfile.xpProgressToNextLevel;
		
		//Calculate the number of XP won in this race
		GameManager.Instance.playerProfile.addToTotalXPEarned( xpEarnedFromRace, false );

		//Spin the total XP awarded from 0 to the amount earned in this race.
		StartCoroutine( spinNumber( 0, xpEarnedFromRace, totalXPAwarded, " " + LocalizationManager.Instance.getText( "EOG_XP" ) ) );

		//The player may level up multiple times
		int numberOfTimesLeveledUp = -1;
		int xpAlreadyEarnedForLevel = xpAlreadyEarned;
		while( xpEarnedFromRace > 0 )
		{
			numberOfTimesLeveledUp++;
			//Current level
			int level = GameManager.Instance.playerProfile.getLevel() + numberOfTimesLeveledUp;
			currentLevelText.text = level.ToString();
	
			//Next level
			int nextLevel = level + 1;
			if( nextLevel > ProgressionManager.MAX_LEVEL )
			{
				nextLevel = ProgressionManager.MAX_LEVEL;
				nextLevelText.text = LocalizationManager.Instance.getText( "EOG_MAX_LEVEL" );
			}
			else
			{
				nextLevelText.text = nextLevel.ToString();
			}
			//The additional XP required to reach the next level
			int xpNeededToReachNextLevel = ProgressionManager.Instance.getXPRequired( level );

			//Amount we need to increase by
			int increaseAmount = Mathf.Min( xpEarnedFromRace, (xpNeededToReachNextLevel - xpAlreadyEarnedForLevel) );		

			//If the increase amount is 0, we don't want to wait for the animation duration.
			if( increaseAmount > 0 )
			{
				//Current XPs/XPs needed for next level
				//Spin the currentXP value from currentXP to increaseAmount
				StartCoroutine( spinNumber( xpAlreadyEarnedForLevel, xpAlreadyEarnedForLevel + increaseAmount, currentAndNextXP, "/" + ProgressionManager.Instance.getXPRequired( level ).ToString() ) );
		
				//Animate the slider from the currentXP value to currentXP + totalXP 
				float fromValue = xpAlreadyEarnedForLevel/(float)ProgressionManager.Instance.getXPRequired( level );
				float toValue = (xpAlreadyEarnedForLevel + increaseAmount)/(float)xpNeededToReachNextLevel;
				StartCoroutine( animateSlider( fromValue, toValue, sliderXP ) );
			
				xpEarnedFromRace = xpEarnedFromRace - increaseAmount;
				yield return new WaitForSecondsRealtime( ANIMATION_DURATION );
			}
			xpAlreadyEarnedForLevel = 0;
			
		}

		if( numberOfTimesLeveledUp > 0 ) GameManager.Instance.playerProfile.incrementLevel();
		GameManager.Instance.playerProfile.xpProgressToNextLevel = GameManager.Instance.playerProfile.totalXPEarned - ProgressionManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.getLevel() - 1 );
		GameManager.Instance.playerProfile.serializePlayerprofile();
		//Also update the matchmaking screen if the player has leveled up so that the player frame gets updated
		if( numberOfTimesLeveledUp > 0 ) matchmakingManager.configureLocalPlayerData();
	}

	public IEnumerator animateSlider( float fromValue, float toValue, Slider slider, System.Action onFinish = null  )
	{
		float startTime = Time.time;
		float elapsedTime = 0;
	
		while ( elapsedTime <= ANIMATION_DURATION )
		{
			elapsedTime = Time.time - startTime;

			slider.value =  Mathf.Lerp( fromValue, toValue, elapsedTime/ANIMATION_DURATION );
			yield return new WaitForEndOfFrame();  
	    }
		if( onFinish != null ) onFinish.Invoke();
	}
	
	public IEnumerator spinNumber( float fromValue, float toValue, Text textField, string endString = null, System.Action onFinish = null  )
	{
		float startTime = Time.time;
		float elapsedTime = 0;
	
		float value = 0;
		int previousValue = -1;

		while ( elapsedTime <= ANIMATION_DURATION )
		{
			elapsedTime = Time.time - startTime;

			value =  Mathf.Lerp( fromValue, toValue, elapsedTime/ANIMATION_DURATION );
			if( (int)value != previousValue )
			{
				if( endString != null )
				{
					textField.text = ((int)value).ToString() + endString;
				}
				else
				{
					textField.text = ((int)value).ToString();
				}
				previousValue = (int)value;
			}
			yield return new WaitForEndOfFrame();  
	    }
		if( onFinish != null ) onFinish.Invoke();
	}

	IEnumerator displayIndividualAwards()
	{
		//Example: "CONSECUTIVE MATCH<color=orange>+200xp</color>"
		XPAwardType awardType;
		ProgressionManager.XPAward xpAward;
		for( int i = 0; i < PlayerRaceManager.Instance.raceAwardList.Count; i++ )
		{
			awardType = PlayerRaceManager.Instance.raceAwardList[i];
			xpAward = ProgressionManager.Instance.getXPAward( awardType );
			string awardText = LocalizationManager.Instance.getText( xpAward.awardTextID );
			awardedXP.text = awardText + "<color=orange>+" + xpAward.xpAmount.ToString() + "xp</color>";
			yield return new WaitForSecondsRealtime( 4.0f );
		}
	}

	void showMatchmaking()
	{
		GameManager.Instance.setGameState(GameState.Matchmaking);
		LevelData.CircuitInfo circuitInfo = LevelManager.Instance.getSelectedCircuit().circuitInfo;
		matchmakingManager.configureCircuitData( circuitInfo );
		matchmakingManager.gameObject.SetActive( true );
		gameObject.SetActive( false );
	}

	public void OnClickShowMatchmaking()
	{
		//Cancel the countdown
		StopAllCoroutines();
		UISoundManager.uiSoundManager.playButtonClick();
		verifyIfSectorChanged();
		showMatchmaking();
	}

	/// <summary>
	/// Verify if the player has changed sector. If he did, display the Sector Change popup.
	/// </summary>
	void verifyIfSectorChanged()
	{
		if( PlayerRaceManager.Instance.sectorStatus != SectorStatus.NO_CHANGE )
		{
			sectorChangePopup.SetActive( true );
		}
	}


}
