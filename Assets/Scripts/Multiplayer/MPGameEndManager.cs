using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MPGameEndManager : MonoBehaviour {

	[Header("Testing Mode")]
	[SerializeField] bool xpTestingMode = false;
	[Header("Race Panel")]
	[SerializeField] Image circuitImage;
	[SerializeField] Text raceResult;
	[SerializeField] Text playerName;
	[SerializeField] Text raceTime;
	[SerializeField] Text nextRaceBegins;
	[SerializeField] Text nextRaceCountdown;
	
	[Header("XP Panel")]
	[SerializeField] Text currentLevelText;
	[SerializeField] Text nextLevelText;
	[SerializeField] Text currentAndNextXP;
	[SerializeField] Text awardedXP;
	[SerializeField] Text totalXPAwarded;
	[SerializeField] Slider sliderXP;

	[Header("Other")]
	[SerializeField] MatchmakingManager matchmakingManager;
	[SerializeField] Text exitButtonText;
	[SerializeField] int timeBeforeNextRace = 60; //in seconds
	const float ANIMATION_DURATION = 3f;

	void Start ()
	{
		//Static values - we just need to localise them
		nextRaceBegins.text = LocalizationManager.Instance.getText( "EOG_NEXT_RACE_BEGINS" ).Replace("\\n", System.Environment.NewLine );
		exitButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_EXIT" );

		if( !xpTestingMode ) startNextRace();
		configureRacePanel();

		//Individual XP Awards
		StartCoroutine( displayIndividualAwards() );
		int xpEarnedFromRace = calculatedTotalXPAwarded();

		if( !xpTestingMode ) StartCoroutine( configureXPPanel(xpEarnedFromRace) );
	}

	void configureRacePanel()
	{
		LevelData.MultiplayerInfo multiplayerInfo = LevelManager.Instance.getSelectedMultiplayerLevel();
		circuitImage.sprite = multiplayerInfo.circuitInfo.circuitImage;
		raceResult.text = getRacePositionString( PlayerRaceManager.Instance.racePosition );
		playerName.text = PlayerStatsManager.Instance.getUserName();
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

		GameManager.Instance.playerProfile.setLevel( GameManager.Instance.playerProfile.getLevel() + numberOfTimesLeveledUp );
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

	string getRacePositionString( int racePosition )
	{
		string racePositionString = string.Empty;
    	switch (racePosition)
		{
	        case 1:
				racePositionString = LocalizationManager.Instance.getText( "EOG_VICTORY" );
                break;
	                
	        case 2:
				racePositionString = LocalizationManager.Instance.getText( "EOG_2ND" );
                break;                
		}
		return racePositionString;
	}

	void startNextRace()
	{
		StartCoroutine( "nextRaceCountdownCoroutine" );
	}

	IEnumerator nextRaceCountdownCoroutine()
	{
		int countdown = timeBeforeNextRace;
		while (countdown >= 0)
		{
			nextRaceCountdown.text = countdown.ToString();
			yield return new WaitForSecondsRealtime( 1.0f );
			countdown --;
		}
		showMatchmaking();
	}

	void showMatchmaking()
	{
		CarouselEntry selected = LevelManager.Instance.selectedRaceDetails;
		matchmakingManager.configureCircuitData( selected.circuitImage.sprite, selected.circuitName.text, selected.entryFee.text );
		matchmakingManager.gameObject.SetActive( true );
		gameObject.SetActive( false );
	}

	public void OnClickShowMatchmaking()
	{
		//Cancel the countdown
		StopAllCoroutines();
		UISoundManager.uiSoundManager.playButtonClick();
		showMatchmaking();
	}

}
