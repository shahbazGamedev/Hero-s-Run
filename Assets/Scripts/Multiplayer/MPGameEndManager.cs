﻿using System.Collections;
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
	[SerializeField] Text currentLevel;
	[SerializeField] Text nextLevel;
	[SerializeField] Text currentAndNextXP;
	[SerializeField] Text awardedXP;
	[SerializeField] Text newAmountXP;
	[SerializeField] Slider sliderXP;

	[Header("Other")]
	[SerializeField] Text exitButtonText;
	[SerializeField] int timeBeforeNextRace = 30; //in seconds

	void Start ()
	{
		//Static values - we just need to localise them
		nextRaceBegins.text = LocalizationManager.Instance.getText( "EOG_NEXT_RACE_BEGINS" ).Replace("\\n", System.Environment.NewLine );
		exitButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_EXIT" );

		startNextRace();

		configureRacePanel();

		configureXPPanel();
	}

	void configureRacePanel()
	{
		LevelData.MultiplayerInfo multiplayerInfo = LevelManager.Instance.getSelectedMultiplayerLevel();
		circuitImage.sprite = multiplayerInfo.circuitInfo.circuitImage;
		//raceResult;
		playerName.text = PlayerStatsManager.Instance.getUserName();
		//Race time
		TimeSpan ts = TimeSpan.FromSeconds( PlayerRaceManager.Instance.raceDuration );
		DateTime dt = new DateTime(ts.Ticks);
		raceTime.text = LocalizationManager.Instance.getText( "EOG_RACE_TIME" ).Replace("<race duration>", dt.ToString("mm:ss") );
		//nextRaceCountdown;

	}

	void configureXPPanel()
	{
		//currentLevel;
		//nextLevel;
		//currentAndNextXP;
		//awardedXP;
		//newAmountXP;
		//sliderXP;
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
	}

	public void OnClickCloseGameEndCanvas()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		//Cancel the countdown
		StopCoroutine("nextRaceCountdownCoroutine");
		gameObject.SetActive( false );
	}


}
