using UnityEngine;
using System.Collections;

public class MyUpsightManager : MonoBehaviour {

	public bool useQAVersion = false;
	private static bool upsightOptOutOption = true;
	const int MAXIMUM_DAILY_STARS = 2000; //Use to prevent fraud by capping the maximum number of stars you can get per reward

	#if UNITY_IPHONE || UNITY_ANDROID
	// These values are from the demo scene.
	// Replace them with your own.
	string androidAppToken = "a0ae2f5cd6dc465aaeaf4b05692ac3be";
	string androidAppSecret = "0e6deaf6ff0040c0963848d534bf1cb3";
	string iosAppToken = "39da36bd4be24e18a54b367dc5407647";
	string iosAppSecret = "8c52361b93664202a783487944cb7477";
	
	void Start()
	{
		if( useQAVersion )
		{
			iosAppToken = "e28e38b5daec4738b5c3acfaa4b0f12a";
			iosAppSecret = "4d89c8e832824cff9a596d7f09642f28";
		}
		else
		{
			iosAppToken = "75bcd67459a1475f8e99e88239481101";
			iosAppSecret = "b6cc4824e5d64ea785d9e092788a0245";
		}

		#if UNITY_ANDROID
		Upsight.init( androidAppToken, androidAppSecret );
		#else
		Upsight.init( iosAppToken, iosAppSecret );
		#endif
		
		// Make an open request at every app launch
		Upsight.requestAppOpen();
		print("MyUpsightManager: requestAppOpen sent. QA mode is: " + useQAVersion );

		//Very important - COPPA related
		//By default, we will use the safer, opt-out option in order to be COPPA compliant.
		//If the user successfully connects to Facebook, we will assume the player is 13 years old or older
		//and set the opt-out to false.
		// Set the opt-out status to the safe version
		setUpsightOptOutOption( true );

		// Get the opt-out status
		print( "MyUpsightManager: COPPA opt-out status is set to: " + Upsight.getOptOutStatus() );

	}
	
	void OnApplicationPause( bool pauseStatus )
	{
		// Make an open request whenever app is resumed
		if( !pauseStatus )
			Upsight.requestAppOpen();
	}
	
	#endif

	public static void setUpsightOptOutOption( bool value )
	{
		//For debugging
		upsightOptOutOption = false;
		Upsight.setOptOutStatus( false );
	}
	
	public static bool getUpsightOptOutOption()
	{
		return upsightOptOutOption;
	}

	void OnEnable()
	{
		// Listen for the notification badge count event.
		UpsightManager.badgeCountRequestSucceededEvent += myShowNotificationBadgeCountMethod;
		// Listen for the unlocked reward event.
		UpsightManager.unlockedRewardEvent += myUnlockedRewardMethod;
	}
	
	void OnDisable()
	{
		// Listen for the notification badge count event.
		UpsightManager.badgeCountRequestSucceededEvent -= myShowNotificationBadgeCountMethod;
		// Listen for the unlocked reward event.
		UpsightManager.unlockedRewardEvent -= myUnlockedRewardMethod;
	}

	void myShowNotificationBadgeCountMethod( int badgeCount )
	{
		// Draw a notification badge on top of your More Games button
		// using the badgeCount
		// Get the notification badge count for the placement “more_games”.
		// This placement must have a More Games Widget associated with it!
		//Upsight.getContentBadgeNumber( "more_games" );

	}

	// Give the user the UpsightReward that was unlocked
	//
	// reward.name - The name of your reward configured in the dashboard
	// reward.quantity - Integer quantity of the reward configured in the dashboard
	// reward.receipt - Unique identifier to detect duplicate reward unlocks. Your
	//                  app should ensure that each receipt is only unlocked once.
	void myUnlockedRewardMethod( UpsightReward reward )
	{
		if( reward.name.StartsWith("daily_bonus") && reward.quantity <= MAXIMUM_DAILY_STARS )
		{
			//Grant a bonus of X stars to the player
			PlayerStatsManager.Instance.modifyCoinCount( reward.quantity );
			Debug.Log("MyUpsightManager: The player received a daily bonus of " + reward.quantity + " stars.");
		}
	}

	/*This method is used to send a content request to Upsight that requires localization.
	The placements including all language variations need to be created in the Upsight dashboard.
	For the placement name, do not include the language suffix.
	For example, as a parameter, pass "daily_bonus" and not "daily_bonus_en" to get the daily bonus screen in English.
	The suffix will be appended automatically.
	Valid language suffix are:
	en	English
	fr	French
	sp	Spanish
	it	Italian
	de	German

	Currently, supported placement names are:
	1) daily_bonus (gives a daily star reward to the player)
	2) announcement_01 (displays an announcement to the players like "Get the update. 50 new levels added!")
	*/
	public static void requestLocalizedContent( string placementName )
	{
		SystemLanguage language = Application.systemLanguage;
		string suffix;
		switch (language)
		{
		case SystemLanguage.English:
			suffix = "_en";
			break;
			
		case SystemLanguage.French:
			suffix = "_fr";
			break;
			
		case SystemLanguage.Italian:
			suffix = "_it";
			break;
			
		case SystemLanguage.German:
			suffix = "_de";
			break;
			
		case SystemLanguage.Spanish:
			suffix = "_sp";
			break;
			
		default:
			//The language is not supported. Default to English.
			suffix = "_en";
			Debug.LogWarning("MyUpsightManager-requestLocalizedContent: the device language " + language + " is not supported. Defaulting to English."  );
			break;
		}
		Debug.Log("MyUpsightManager-sending content request: " + placementName + suffix );

		//We can now send the request
		Upsight.sendContentRequest( placementName + suffix, true );
	}

}