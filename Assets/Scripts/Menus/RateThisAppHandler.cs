﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RateThisAppHandler : MonoBehaviour {

	const int MAX_TIMES_DISPLAY_RATE_THIS_APP = 3;
	const int HOURS_TO_WAIT_BEFORE_SHOWING_AGAIN = 48;
	const int NUMBER_OF_CONSECUTIVE_WINS_NEEDED = 2;
	const string IOS_RATE_THIS_APP = "itms-apps://itunes.apple.com/app/id";
	const string BUNDLE_ID = "com.redlondongames.DragonRunSaga";
	const string TEST_BUNDLE_ID = "com.appsfresh.kolorklone";
	const string IOS_RATE_THIS_APP_URL = IOS_RATE_THIS_APP + TEST_BUNDLE_ID;
	const string ANDROID_RATE_THIS_APP = "market://details?id=";
	const string ANDROID_RATE_THIS_APP_URL = ANDROID_RATE_THIS_APP + TEST_BUNDLE_ID;

	// Use this for initialization
	void Start ()
 	{
		decideForRateThisApp();	
	}
	
	void decideForRateThisApp()
	{
		//If the player has already rated the app, return.
		if( GameManager.Instance.playerProfile.didPlayerRateApp ) return;

		//If we have shown it the maximum number of times, return.
		if( GameManager.Instance.playerProfile.timesRateThisAppDisplayed == MAX_TIMES_DISPLAY_RATE_THIS_APP ) return;

		//Wait HOURS_TO_WAIT_BEFORE_SHOWING_AGAIN hours before showing again. 
		if( DateTime.UtcNow < GameManager.Instance.playerProfile.getLastTimeRateThisAppWasShown().AddHours(HOURS_TO_WAIT_BEFORE_SHOWING_AGAIN) ) return;

		//Is this a good time to display the rate this app popup?
		//We assume it is a good time if the player returns to the main menu after winning NUMBER_OF_CONSECUTIVE_WINS_NEEDED or more races.
		if( LevelManager.Instance.consecutiveRacesWon < NUMBER_OF_CONSECUTIVE_WINS_NEEDED ) return;

		//If the player has no Internet connection, return.
		if( Application.internetReachability == NetworkReachability.NotReachable ) return;

		//Sweet! All the conditions are met, display the rate this app popup
		displayRateThisApp();
	}

	void displayRateThisApp()
	{
		gameObject.SetActive( true );
	}

	public void OnClickRateThisApp()
	{
		#if UNITY_IOS
		gameObject.SetActive( false );
		GameManager.Instance.playerProfile.didPlayerRateApp = true;
		GameManager.Instance.playerProfile.serializePlayerprofile();
		Application.OpenURL( IOS_RATE_THIS_APP_URL );
		#endif
	}

	public void OnClickRateThisAppLater()
	{
		GameManager.Instance.playerProfile.timesRateThisAppDisplayed++;
 		//This method also saves the profile, so do it last
		GameManager.Instance.playerProfile.setLastTimeRateThisAppWasShown( DateTime.UtcNow );
		gameObject.SetActive( false );
	}

}
