using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ClockTimeSetter : MonoBehaviour {

	public Text display;
    string paddingChar="0";

	public RectTransform hourHand;
	public RectTransform minuteHand;

	void Start()
	{
		TimeSpan timeOfDay = GameManager.Instance.calculateTimeOfDay();
		setClockTime( timeOfDay.Hours, timeOfDay.Minutes );
	}

	void OnEnable()
	{
		GameManager.timeOfDayEvent += TimeOfDayEvent;
	}
	
	void OnDisable()
	{
		GameManager.timeOfDayEvent -= TimeOfDayEvent;
	}

	void TimeOfDayEvent( TimeSpan newTimeOfDay )
	{
		Debug.Log("ClockTimeSetter-TimeOfDayEvent: " + newTimeOfDay );
		setClockTime( newTimeOfDay.Hours, newTimeOfDay.Minutes );
	}

	void setClockTime ( int hourValue, int minutesValue )
 	{
		float hours =(float)hourValue;
		float minutes = (float)(minutesValue);
		if (minutes<10)
		{
			paddingChar="0";
		}
		else
		{
			paddingChar="";
		}
		display.text=hours.ToString ()+":"+paddingChar+minutes.ToString();
		hourHand.rotation=Quaternion.Euler(0,0,-(hours + minutes/60f) * 30f);
		minuteHand.rotation=Quaternion.Euler(0,0,-minutes*6f);
    }
	
}
