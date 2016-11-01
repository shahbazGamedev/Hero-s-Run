using UnityEngine;
using System.Collections;
using System;

public class ClockSetter3D : MonoBehaviour {

	[Header("3D Clock")]
	//This script sets the hour and minute hands of a clock.
	public Transform minuteHand;
	public Transform hourHand;
	[Tooltip("Used to add a small random (from X to Y) number of minutes to make it feel real (and not always 6PM sharp for example).")]
	public Vector2 additionalMinutesRange = Vector2.zero;

	// Use this for initialization
	void OnEnable ()
	{
		updateTime ();
	}

	void updateTime ()
 	{
		int episodeNumber = LevelManager.Instance.getCurrentEpisodeNumber();

		//Get time of day for current episode
		Vector2 levelTimeOfDay = LevelManager.Instance.getCurrentEpisodeInfo().timeOfDay;

		//Calculate time penalty
		int penaltyInMinutes = PlayerStatsManager.Instance.getNumberDeathLeadingToEpisode( episodeNumber ) * GameManager.TIME_PENALTY_IN_MINUTES;

		//Add a small random number of minutes to make it feel real (and not always 6PM sharp for example)
		int additionalMinutes = UnityEngine.Random.Range( (int)additionalMinutesRange.x, (int)additionalMinutesRange.y );

		TimeSpan timeOfDay = new TimeSpan((int)levelTimeOfDay.x, (int)levelTimeOfDay.y, 0 );
		TimeSpan span = TimeSpan.FromMinutes( penaltyInMinutes + additionalMinutes );
		timeOfDay = timeOfDay.Add(span);
		setTime( timeOfDay );
    }

	void setTime( TimeSpan timeOfDay )
	{
		float hours =(float)timeOfDay.Hours;
		float minutes = (float)timeOfDay.Minutes;
		hourHand.localRotation = Quaternion.Euler(-(hours + minutes/60f) * 30f,0 ,0 );
		//The -90 in the calculation is simply because the minute hand model pivot has a 90 degree offset
		minuteHand.localRotation = Quaternion.Euler(-minutes*6f-90f,0 ,0 );

	}
}
