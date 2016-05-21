using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
using System;

public class NotificationServicesHandler : MonoBehaviour {

	bool tokenSent;
	UnityEngine.iOS.LocalNotification localNotificationShortTerm;
	public int minutesBeforeShortTermNotification = 1440; //1 day

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	void Start ()
	{
		Debug.Log("NotificationServicesHandler - App Start. Cancel all notifications." );
		NotificationServices.CancelAllLocalNotifications();
		NotificationServices.ClearLocalNotifications();
		tokenSent = false;
 		NotificationServices.RegisterForNotifications( NotificationType.Alert | NotificationType.Badge | NotificationType.Sound, true);
		prepareLocalNotification();
	}
	
	void prepareLocalNotification()
	{
		//IMPORTANT: when you don't set the time zone, your notification is scheduled for GMT time.
		//Also, TimeZoneInfo.Local throws an exception in Unity. This is a known bug.
		//I was not able to change the alertlaunchImage
		localNotificationShortTerm = new UnityEngine.iOS.LocalNotification();
		localNotificationShortTerm.alertAction = LocalizationManager.Instance.getText("LOCAL_NOTIFICATION_SHORT_TERM_ALERT_ACTION");
		string alertBody = LocalizationManager.Instance.getText("LOCAL_NOTIFICATION_SHORT_TERM_ALERT_BODY");
		if( PlayerStatsManager.Instance.getAvatar() == Avatar.Hero )
		{
			alertBody = alertBody.Replace("<hero name>", LocalizationManager.Instance.getText("GALLERY_NAME_HERO") );
		}
		else
		{
			alertBody = alertBody.Replace("<hero name>", LocalizationManager.Instance.getText("GALLERY_NAME_HEROINE") );
		}
		localNotificationShortTerm.alertBody = alertBody;
		localNotificationShortTerm.applicationIconBadgeNumber = 1;
		localNotificationShortTerm.hasAction = true;
		localNotificationShortTerm.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
	}

	void Update ()
	{
		if (!tokenSent)
		{
			byte[] token = UnityEngine.iOS.NotificationServices.deviceToken;
			if (token != null)
			{
				// send token to a provider
				string hexToken = "%" + System.BitConverter.ToString(token).Replace('-', '%');
				//new WWW("http:/example.com?token=" + hexToken);
				Debug.Log("NotificationServicesHandler - token: " + hexToken );
				tokenSent = true;
			}
		}
		if (UnityEngine.iOS.NotificationServices.localNotificationCount > 0)
		{
			//Reset the badge number first
			NotificationServices.GetLocalNotification( 0 ).applicationIconBadgeNumber = -1;
			NotificationServices.ClearLocalNotifications();
		}
	}

	//If the device is paused by pressing the Home button, because of a low battery warning or a phone call, the game will automatically display the pause menu.
	void OnApplicationPause( bool pauseStatus )
	{

		if( pauseStatus )
		{
			//App is suspended. Schedule a local notification, but only if the player has not finished the game
			if( !LevelManager.Instance.getPlayerFinishedTheGame() )
			{
				localNotificationShortTerm.fireDate = DateTime.Now.AddMinutes(minutesBeforeShortTermNotification);
				NotificationServices.ScheduleLocalNotification(localNotificationShortTerm);
				Debug.Log("NotificationServicesHandler - OnApplicationPause-App is suspended. Schedule a local notification at this time: " + localNotificationShortTerm.fireDate  );
			}
		}
		else
		{
			//App is active. Cancel all notifications
			NotificationServices.CancelAllLocalNotifications();
			Debug.Log("NotificationServicesHandler - OnApplicationPause-App is active. Cancel all notifications.");
		}
	}

	public void sendTestLocalNotification()
	{
		// schedule notification to be delivered in 12 seconds
		localNotificationShortTerm.fireDate = DateTime.Now.AddSeconds(12);
		NotificationServices.ScheduleLocalNotification(localNotificationShortTerm);
	}

}
