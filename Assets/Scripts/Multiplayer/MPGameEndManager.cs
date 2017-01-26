using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MPGameEndManager : MonoBehaviour {

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
	[SerializeField] MPCarouselManager mpCarouselManager;
	[SerializeField] MPLobbyMenu mpLobbyMenu;
	[SerializeField] Text exitButtonText;
	[SerializeField] int timeBeforeNextRace = 60; //in seconds
	const float ANIMATION_DURATION = 4f;

	void Start ()
	{
		//Static values - we just need to localise them
		nextRaceBegins.text = LocalizationManager.Instance.getText( "EOG_NEXT_RACE_BEGINS" ).Replace("\\n", System.Environment.NewLine );
		exitButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_EXIT" );

		startNextRace();
		configureRacePanel();

		//Individual XP Awards
		StartCoroutine( displayIndividualAwards() );

		StartCoroutine( configureXPPanel() );
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

	int calculatedTotalXPAwarded()
	{
		XPAwardType awardType;
		XPManager.XPAward xpAward;
		int total = 0;
		for( int i = 0; i < PlayerRaceManager.Instance.raceAwardList.Count; i++ )
		{
			awardType = PlayerRaceManager.Instance.raceAwardList[i];
			xpAward = XPManager.Instance.getXPAward( awardType );
			total = total + xpAward.xpAmount;
		}
		Debug.Log("MPGameEndManager-calculatedTotalXPAwarded: total: " + total );
		return total;
	}

	IEnumerator configureXPPanel()
	{
		//Get the amount of xp already earned towards the next level
		int xpAlreadyEarned = GameManager.Instance.playerProfile.xpProgressToNextLevel;
		
		//Calculate the number of XP won in this race
		int xpEarnedFromRace = calculatedTotalXPAwarded();
		GameManager.Instance.playerProfile.addToTotalXPEarned( xpEarnedFromRace, false );

		//Spin the total XP awarded from 0 to the amount earned in this race.
		StartCoroutine( spinNumber( 0, xpEarnedFromRace, totalXPAwarded  ) );

		//The player may level up multiple times
		int numberOfTimesLeveledUp = -1;
		int xpAlreadyEarnedForLevel = xpAlreadyEarned;
		while( xpEarnedFromRace > 0 )
		{
			numberOfTimesLeveledUp++;
			//Current level
			int level = GameManager.Instance.playerProfile.level + numberOfTimesLeveledUp;
			currentLevelText.text = level.ToString();
	
			//Next level
			int nextLevel = level + 1;
			if( nextLevel > XPManager.MAX_LEVEL )
			{
				nextLevel = XPManager.MAX_LEVEL;
				nextLevelText.text = LocalizationManager.Instance.getText( "EOG_MAX_LEVEL" );
			}
			else
			{
				nextLevelText.text = nextLevel.ToString();
			}
			//The additional XP required to reach the next level
			int xpNeededToReachNextLevel = XPManager.Instance.getXPRequired( level );

			//Amount we need to increase by
			int increaseAmount = Mathf.Min( xpEarnedFromRace, (xpNeededToReachNextLevel - xpAlreadyEarnedForLevel) );

			//Current XPs/XPs needed for next level
			//Spin the currentXP value from currentXP to increaseAmount
			StartCoroutine( spinNumber( xpAlreadyEarnedForLevel, xpAlreadyEarnedForLevel + increaseAmount, currentAndNextXP, "/" + XPManager.Instance.getXPRequired( level ).ToString() ) );
	
			//Animate the slider from the currentXP value to currentXP + totalXP 
			float fromValue = xpAlreadyEarnedForLevel/(float)XPManager.Instance.getXPRequired( nextLevel );
			float toValue = (xpAlreadyEarnedForLevel + increaseAmount)/(float)xpNeededToReachNextLevel;
			StartCoroutine( animateSlider( fromValue, toValue, sliderXP ) );
			
			xpEarnedFromRace = xpEarnedFromRace - increaseAmount;
			xpAlreadyEarnedForLevel = 0;
			yield return new WaitForSecondsRealtime( ANIMATION_DURATION + 8f );
		}

		GameManager.Instance.playerProfile.level = GameManager.Instance.playerProfile.level + numberOfTimesLeveledUp;
		GameManager.Instance.playerProfile.xpProgressToNextLevel = GameManager.Instance.playerProfile.totalXPEarned - XPManager.Instance.getTotalXPRequired( GameManager.Instance.playerProfile.level - 1 );
		GameManager.Instance.playerProfile.serializePlayerprofile();
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
		XPManager.XPAward xpAward;
		for( int i = 0; i < PlayerRaceManager.Instance.raceAwardList.Count; i++ )
		{
			awardType = PlayerRaceManager.Instance.raceAwardList[i];
			xpAward = XPManager.Instance.getXPAward( awardType );
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
		showMatchmakingScreen();
	}

	void showMatchmakingScreen()
	{
		CarouselEntry selected = mpCarouselManager.carouselEntryList[LevelManager.Instance.getCurrentMultiplayerLevel() ];
		mpLobbyMenu.configureCircuitData( selected.circuitImage.sprite, selected.circuitName.text, selected.entryFee.text );
		mpLobbyMenu.gameObject.SetActive( true );
		mpCarouselManager.gameObject.SetActive( false );
		gameObject.SetActive( false );
	}

	public void OnClickCloseGameEndCanvas()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		//Cancel the countdown
		StopAllCoroutines();
		gameObject.SetActive( false );
	}

}
