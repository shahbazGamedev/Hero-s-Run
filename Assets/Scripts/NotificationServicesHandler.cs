using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
using System;

public class NotificationServicesHandler : MonoBehaviour {

	public static NotificationServicesHandler Instance;
	bool tokenSent = false;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		Instance = this;
	}

	void Start ()
	{
 		NotificationServices.RegisterForNotifications( NotificationType.Alert | NotificationType.Badge | NotificationType.Sound, true);
		clearNotifications();
	}

	void clearNotifications()
	{
		for( int i = 0; i < UnityEngine.iOS.NotificationServices.localNotificationCount; i++ )
		{
			NotificationServices.GetLocalNotification( i ).applicationIconBadgeNumber = -1;
		}
		NotificationServices.ClearLocalNotifications();
		NotificationServices.ClearRemoteNotifications(); //Apparently you need to do both clears for the badge counter to reset
	}
	
	public void scheduleFreeLootBoxNotification( int inMinutes )
	{
		//Before rescheduling another notification, cancel the existing ones first
		NotificationServices.CancelAllLocalNotifications();

		//IMPORTANT: when you don't set the time zone, your notification is scheduled for GMT time.
		//Also, TimeZoneInfo.Local throws an exception in Unity. This is a known bug.
		//I was not able to change the alertlaunchImage or the sound
		UnityEngine.iOS.LocalNotification freeLootBoxNotification = new UnityEngine.iOS.LocalNotification();
		freeLootBoxNotification.alertBody = LocalizationManager.Instance.getText("LOCAL_NOTIFICATION_FREE_LOOT_BOX");
		freeLootBoxNotification.applicationIconBadgeNumber = 1;
		freeLootBoxNotification.hasAction = true;
		freeLootBoxNotification.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
		freeLootBoxNotification.fireDate = DateTime.Now.AddMinutes(inMinutes);
		NotificationServices.ScheduleLocalNotification(freeLootBoxNotification);
	}

	void Update ()
	{
		//This is for remote notifications
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
	}


}
