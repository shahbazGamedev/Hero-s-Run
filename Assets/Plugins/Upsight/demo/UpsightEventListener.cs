using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// this script is part of the demo scene. It is designed so that you can just stick it on a GO in your scene then call any Upsight
/// methods. It will log every event that occurs making it very useful in learning how the plugin operates.
/// </summary>
public class UpsightEventListener : MonoBehaviour
{
#if UNITY_IPHONE || UNITY_ANDROID
	void OnEnable()
	{
		// Listen to all events for illustration purposes
		UpsightManager.openRequestSucceededEvent += openRequestSucceededEvent;
		UpsightManager.openRequestFailedEvent += openRequestFailedEvent;
		UpsightManager.contentWillDisplayEvent += contentWillDisplayEvent;
		UpsightManager.contentDidDisplayEvent += contentDidDisplayEvent;
		UpsightManager.contentRequestLoadedEvent += contentRequestLoadedEvent;
		UpsightManager.contentRequestFailedEvent += contentRequestFailedEvent;
		UpsightManager.contentPreloadSucceededEvent += contentPreloadSucceededEvent;
		UpsightManager.contentPreloadFailedEvent += contentPreloadFailedEvent;
		UpsightManager.badgeCountRequestSucceededEvent += badgeCountRequestSucceededEvent;
		UpsightManager.badgeCountRequestFailedEvent += badgeCountRequestFailedEvent;
		UpsightManager.trackInAppPurchaseSucceededEvent += trackInAppPurchaseSucceededEvent;
		UpsightManager.trackInAppPurchaseFailedEvent += trackInAppPurchaseFailedEvent;
		UpsightManager.reportCustomEventSucceededEvent += reportCustomEventSucceededEvent;
		UpsightManager.reportCustomEventFailedEvent += reportCustomEventFailedEvent;
		UpsightManager.contentDismissedEvent += contentDismissedEvent;
		UpsightManager.makePurchaseEvent += makePurchaseEvent;
		UpsightManager.dataOptInEvent += dataOptInEvent;
		UpsightManager.unlockedRewardEvent += unlockedRewardEvent;
		UpsightManager.pushNotificationWithContentReceivedEvent += pushNotificationWithContentReceivedEvent;
		UpsightManager.pushNotificationWithUrlReceivedEvent += pushNotificationWithUrlReceivedEvent;
	}


	void OnDisable()
	{
		// Remove all event handlers
		UpsightManager.openRequestSucceededEvent -= openRequestSucceededEvent;
		UpsightManager.openRequestFailedEvent -= openRequestFailedEvent;
		UpsightManager.contentWillDisplayEvent -= contentWillDisplayEvent;
		UpsightManager.contentDidDisplayEvent -= contentDidDisplayEvent;
		UpsightManager.contentRequestLoadedEvent -= contentRequestLoadedEvent;
		UpsightManager.contentRequestFailedEvent -= contentRequestFailedEvent;
		UpsightManager.contentPreloadSucceededEvent -= contentPreloadSucceededEvent;
		UpsightManager.contentPreloadFailedEvent -= contentPreloadFailedEvent;
		UpsightManager.badgeCountRequestSucceededEvent -= badgeCountRequestSucceededEvent;
		UpsightManager.badgeCountRequestFailedEvent -= badgeCountRequestFailedEvent;
		UpsightManager.trackInAppPurchaseSucceededEvent -= trackInAppPurchaseSucceededEvent;
		UpsightManager.trackInAppPurchaseFailedEvent -= trackInAppPurchaseFailedEvent;
		UpsightManager.reportCustomEventSucceededEvent -= reportCustomEventSucceededEvent;
		UpsightManager.reportCustomEventFailedEvent -= reportCustomEventFailedEvent;
		UpsightManager.contentDismissedEvent -= contentDismissedEvent;
		UpsightManager.makePurchaseEvent -= makePurchaseEvent;
		UpsightManager.dataOptInEvent -= dataOptInEvent;
		UpsightManager.unlockedRewardEvent -= unlockedRewardEvent;
		UpsightManager.pushNotificationWithContentReceivedEvent -= pushNotificationWithContentReceivedEvent;
		UpsightManager.pushNotificationWithUrlReceivedEvent -= pushNotificationWithUrlReceivedEvent;
	}



	void openRequestSucceededEvent( Dictionary<string,object> dict )
	{
		Debug.Log( "openRequestSucceededEvent: " + MiniJSON.Json.Serialize( dict ) );
	}


	void openRequestFailedEvent( string error )
	{
		Debug.Log( "openRequestFailedEvent: " + error );
	}


	void contentWillDisplayEvent( string placementID )
	{
		Debug.Log( "contentWillDisplayEvent: " + placementID );
	}


	void contentDidDisplayEvent( string placementID )
	{
		Debug.Log( "contentDidDisplay: " + placementID );
	}


	void contentRequestLoadedEvent( string placement )
	{
		Debug.Log( "contentRequestLoadedEvent: " + placement );
	}


	void contentRequestFailedEvent( string placement, string error )
	{
		Debug.Log( string.Format( "contentRequestFailedEvent. placement: {0}, error: {1}", placement, error ) );
	}


	void contentPreloadSucceededEvent( string placement )
	{
		Debug.Log( "contentPreloadSucceededEvent: " + placement );
	}


	void contentPreloadFailedEvent( string placement, string error )
	{
		Debug.Log( string.Format( "contentPreloadFailedEvent. placement: {0}, error: {1}", placement, error ) );
	}


	void badgeCountRequestSucceededEvent( int badgeCount )
	{
		Debug.Log( "badgeCountRequestSucceededEvent: " + badgeCount );
	}


	void badgeCountRequestFailedEvent( string error )
	{
		Debug.Log( "badgeCountRequestFailedEvent: " + error );
	}


	void trackInAppPurchaseSucceededEvent()
	{
		Debug.Log( "trackInAppPurchaseSucceededEvent" );
	}


	void trackInAppPurchaseFailedEvent( string error )
	{
		Debug.Log( "trackInAppPurchaseFailedEvent: " + error );
	}


	void reportCustomEventSucceededEvent()
	{
		Debug.Log( "reportCustomEventSucceededEvent" );
	}


	void reportCustomEventFailedEvent( string error )
	{
		Debug.Log( "reportCustomEventFailedEvent: " + error );
	}


	void contentDismissedEvent( string placement, string dismissType )
	{
		Debug.Log( string.Format( "contentDismissedEvent. placement: {0}, dismissType: {1}", placement, dismissType ) );
	}


	void makePurchaseEvent( UpsightPurchase purchase )
	{
		Debug.Log( "makePurchaseEvent: " + purchase );
	}


	void dataOptInEvent( Dictionary<string,object> dict )
	{
		Debug.Log( "dataOptInEvent: " + MiniJSON.Json.Serialize( dict ) );
	}


	void unlockedRewardEvent( UpsightReward reward )
	{
		Debug.Log( "unlockedRewardEvent: " + reward );
	}


	void pushNotificationWithContentReceivedEvent( string messageID, string contentUnitID )
	{
		Debug.Log( string.Format( "pushNotificationWithContentReceivedEvent. messageID: {0}, contentUnitID: {1}", messageID, contentUnitID ) );
	}


	void pushNotificationWithUrlReceivedEvent( string url )
	{
		Debug.Log( "pushNotificationWithUrlReceivedEvent: " + url );
	}

#endif
}


