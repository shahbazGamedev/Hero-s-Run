using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ClockTimeSetter : MonoBehaviour {

	public int hours;
	public int minutes;
	public Text display;
    string paddingChar="0";

	public RectTransform hourHand;
	public RectTransform minuteHand;

	void Awake()
	{
		//setClockTime ( hours, minutes );
		updateClockTime();
	}

	public void setClockTime ( int hourValue, int minutesValue )
 	{
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

	public void updateClockTime ()
 	{
		hours=(int)Mathf.Floor(GameManager.Instance.getGameClock()/60f);
		minutes=Mathf.RoundToInt(GameManager.Instance.getGameClock()%60);
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
