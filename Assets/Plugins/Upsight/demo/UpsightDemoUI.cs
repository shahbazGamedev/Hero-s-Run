using UnityEngine;
using System.Collections.Generic;



public class UpsightDemoUI : MonoBehaviour
{
#if UNITY_IPHONE || UNITY_ANDROID
	public string androidAppToken;
	public string androidAppSecret;
	public string gcmProjectNumber;
	public string iosAppToken;
	public string iosAppSecret;

	private int _moreGamesBadgeCount = -1;


	void Start()
	{
#if UNITY_ANDROID
		Upsight.init( androidAppToken, androidAppSecret, gcmProjectNumber );
#else
		Upsight.init( iosAppToken, iosAppSecret );
#endif

		UpsightManager.badgeCountRequestSucceededEvent += badgeCount =>
		{
			_moreGamesBadgeCount = badgeCount;
			Debug.Log( "metadata request contained a badge with count: " + _moreGamesBadgeCount );
		};

		// we always want to register for push notifications at launch so that if there is a pending push notification to
		// display it gets shown
		Upsight.registerForPushNotifications();
		// perform an open request at every app luanch
		Upsight.requestAppOpen();
	}


	void OnApplicationPause( bool pauseStatus )
	{
		// session tracking requires that you call requestAppOpen every time your app is launched
		if( !pauseStatus )
			Upsight.requestAppOpen();
	}


	void OnGUI()
	{
		beginGuiColomn();

#if UNITY_ANDROID
		if( GUILayout.Button( "Enable Verbose Logs (Android only)" ) )
		{
			Upsight.setLogLevel( UpsightLogLevel.VERBOSE );
		}
#endif


		if( GUILayout.Button( "Request App Open" ) )
		{
			Upsight.requestAppOpen();
		}


		// if we have a more games badge count available use it for the button text
		var moreGamesButtonTitle = "Send Content Request (more_games_only)";
		if( _moreGamesBadgeCount >= 0 )
			moreGamesButtonTitle += string.Format( " ({0})", _moreGamesBadgeCount );

		if( GUILayout.Button( moreGamesButtonTitle ) )
		{
			Upsight.sendContentRequest( "more_games_only", true, true );
		}


		if( GUILayout.Button( "Send Content Request (interstitial)" ) )
		{
			Upsight.sendContentRequest( "interstitial", true, true );
		}


		if( GUILayout.Button( "Send Content Request (optin)" ) )
		{
			Upsight.sendContentRequest( "optin", true, true );
		}


		if( GUILayout.Button( "Send Content Request (rewarded)" ) )
		{
			Upsight.sendContentRequest( "rewarded", false, false );
		}


		if( GUILayout.Button( "Send Content Request (vg_test)" ) )
		{
			Upsight.sendContentRequest( "vg_test", true );
		}


		if( GUILayout.Button( "Send Content Request (vg_test) with dimensions" ) )
		{
			var dict = new Dictionary<string,object>();
			dict.Add( "ua_source", "PlayHaven" );
			dict.Add( "gold_balance", 2170 );
			dict.Add( "registered", true );

			Upsight.sendContentRequest( "vg_test", true, false, dict );
		}


		if( GUILayout.Button( "Toggle Opt Out Status" ) )
		{
			Upsight.setOptOutStatus( !Upsight.getOptOutStatus() );
		}


		endGuiColumn( true );


		if( GUILayout.Button( "Preload Content Request (announce)" ) )
		{
			Upsight.preloadContentRequest( "announce" );
		}


		if( GUILayout.Button( "Send Preloaded Content Request (announce)" ) )
		{
			Upsight.sendContentRequest( "announce", false );
		}


		if( GUILayout.Button( "Get Content Badge Number (more_games_only)" ) )
		{
			Upsight.getContentBadgeNumber( "more_games_only" );
		}


		if( GUILayout.Button( "Track In App Purchase" ) )
		{
			// we have to go platform depending here due to IAP differences between iOS and Android
#if UNITY_IPHONE
			Upsight.trackInAppPurchase( "com.playhaven.unityexample.plasmagun", 1, UpsightIosPurchaseResolution.Buy );
#else
			Upsight.trackInAppPurchase( "com.playhaven.unityexample.plasmagun", 1, UpsightAndroidPurchaseResolution.Bought, 45.55, "the-order-id", "Play" );
#endif
		}


		if( GUILayout.Button( "Report Custom Event" ) )
		{
			var dict = new Dictionary<string,object>();
			dict.Add( "first_key", "first_value" );
			dict.Add( "second_key", 38 );

			Upsight.reportCustomEvent( dict );
		}


		GUILayout.Label( "Push Notifications" );

		if( GUILayout.Button( "Register for Push Notifications" ) )
		{
			Upsight.registerForPushNotifications();
		}


		if( GUILayout.Button( "Deregister for Push Notifications" ) )
		{
			Upsight.deregisterForPushNotifications();
		}


		// iOS only push features
#if UNITY_IPHONE
		if( GUILayout.Button( "Turn off Automatic Opening of Content/Urls" ) )
		{
			Upsight.setShouldOpenContentRequestsFromPushNotifications( false );
			Upsight.setShouldOpenUrlsFromPushNotifications( false );
		}
#endif

		endGuiColumn();
	}


	#region Helpers to Tame GUI

	void beginGuiColomn()
	{
		var buttonHeight = ( Screen.width >= 960 || Screen.height >= 960 ) ? 70 : 30;

		GUI.skin.label.fixedHeight = buttonHeight;
		GUI.skin.label.margin = new RectOffset( 0, 0, 10, 0 );
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUI.skin.button.margin = new RectOffset( 0, 0, 10, 0 );
		GUI.skin.button.fixedHeight = buttonHeight;
		GUI.skin.button.fixedWidth = Screen.width / 2 - 20;
		GUI.skin.button.wordWrap = true;

		GUILayout.BeginArea( new Rect( 10, 10, Screen.width / 2, Screen.height ) );
		GUILayout.BeginVertical();
	}


	void endGuiColumn( bool hasSecondColumn = false )
	{
		GUILayout.EndVertical();
		GUILayout.EndArea();

		if( hasSecondColumn )
		{
			GUILayout.BeginArea( new Rect( Screen.width - Screen.width / 2, 10, Screen.width / 2, Screen.height ) );
			GUILayout.BeginVertical();
		}
	}

	#endregion

#endif
}
