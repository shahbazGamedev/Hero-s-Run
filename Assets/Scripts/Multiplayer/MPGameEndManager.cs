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
	[SerializeField] int timeBeforeNextRace = 30; //in seconds
	const float SLIDER_PROGRESS_DURATION = 2.5f;

	int totalXP = 0;

	void Start ()
	{
		//Static values - we just need to localise them
		nextRaceBegins.text = LocalizationManager.Instance.getText( "EOG_NEXT_RACE_BEGINS" ).Replace("\\n", System.Environment.NewLine );
		exitButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_EXIT" );

		startNextRace();

		configureRacePanel();

		totalXP = calculatedTotalXPAwarded();
		StartCoroutine( spinNumber( 0, totalXP, totalXPAwarded, null ) );

		configureXPPanel();
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

	void configureXPPanel()
	{
		int currentXP = GameManager.Instance.playerProfile.currentXP;
		int level = XPManager.Instance.getLevel( GameManager.Instance.playerProfile.currentXP );
		currentLevelText.text = level.ToString();
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
		currentAndNextXP.text = currentXP.ToString() + "/" + XPManager.Instance.getXPRequired( nextLevel ).ToString();
		int xpAwardedTest = 750;
		awardedXP.text = xpAwardedTest.ToString();
		int totalXP = currentXP + xpAwardedTest;
		totalXPAwarded.text = totalXP.ToString();
		sliderXP.value = totalXP/(float)XPManager.Instance.getXPRequired( nextLevel );
		Invoke("animateSlick", 10f);
	}

	void animateSlick()
	{
		sliderXP.value = 0;
		StartCoroutine( animateSlider( 0.75f ) );
	}

	public IEnumerator animateSlider( float newValue, System.Action onFinish = null  )
	{
		float startTime = Time.time;
		float elapsedTime = 0;
	
		float startValue = sliderXP.value;

		while ( elapsedTime <= SLIDER_PROGRESS_DURATION )
		{
			elapsedTime = Time.time - startTime;

			sliderXP.value =  Mathf.Lerp( startValue, newValue, elapsedTime/SLIDER_PROGRESS_DURATION );
			yield return new WaitForEndOfFrame();  
	    }
		if( onFinish != null ) onFinish.Invoke();
	}
	
	public IEnumerator spinNumber( float fromValue, float toValue, Text textField, System.Action onFinish = null  )
	{
		float startTime = Time.time;
		float elapsedTime = 0;
	
		float value = 0;
		int previousValue = -1;

		while ( elapsedTime <= SLIDER_PROGRESS_DURATION )
		{
			elapsedTime = Time.time - startTime;

			value =  Mathf.Lerp( fromValue, toValue, elapsedTime/SLIDER_PROGRESS_DURATION );
			if( (int)value != previousValue )
			{
				textField.text = ((int)value).ToString();
				previousValue = (int)value;
			}
			yield return new WaitForEndOfFrame();  
	    }
		if( onFinish != null ) onFinish.Invoke();
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
