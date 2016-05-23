using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ClockTimeSetter : MonoBehaviour {

	public Text timeLeftTitle;
	public Text timePenaltyTitle;
	public Text timeLeft;
	public Text timePenalty;
	public RectTransform hourHand;
	public RectTransform minuteHand;

    string paddingChar;

	void Start()
	{
		timeLeftTitle.text = LocalizationManager.Instance.getText("CLOCK_TIME_LEFT");
		timePenaltyTitle.text = LocalizationManager.Instance.getText("CLOCK_TIME_PENALTY");
	}

	public void updateTime (  int episodeNumber, int levelNumber, Level_Progress levelProgress  )
 	{
		Debug.Log("ClockTimeSetter-updateTime: levelProgress " + levelProgress );
        switch (levelProgress)
		{
	        case Level_Progress.LEVEL_START:
				updateTime( episodeNumber, levelNumber );
                break;
	                
	        case Level_Progress.LEVEL_END_WITH_NO_PROGRESS:
				updateTime( episodeNumber, levelNumber );
                break;
	                
	        case Level_Progress.LEVEL_END_WITH_PROGRESS:
				updateTime2( episodeNumber );
                break;
	                
	        case Level_Progress.LEVEL_END_WITH_GAME_COMPLETED:
				updateTime2( episodeNumber );
                break;
		}
    }

	//Pocket watch shows current time
	//But the text shows the time left before midnight
	void updateTime ( int episodeNumber, int levelNumber )
 	{
		Debug.Log("ClockTimeSetter-updateTime episode: " + episodeNumber + " level: " + levelNumber );

		//Get time of day for current level
		Vector2 levelTimeOfDay = LevelManager.Instance.getLevelInfo(levelNumber).timeOfDay;

		//Calculate time penalty
		int penaltyInMinutes = PlayerStatsManager.Instance.getNumberDeathLeadingToEpisode( episodeNumber ) * GameManager.TIME_PENALTY_IN_MINUTES;
		timePenalty.text = penaltyInMinutes.ToString();

		Debug.Log("ClockTimeSetter-penalty: " + penaltyInMinutes );

		TimeSpan timeOfDay = new TimeSpan((int)levelTimeOfDay.x, (int)levelTimeOfDay.y, 0 );
		TimeSpan span = TimeSpan.FromMinutes(penaltyInMinutes);
		timeOfDay = timeOfDay.Add(span);
		setTime( timeOfDay );
    }

	void updateTime2 ( int episodeNumber )
 	{
		//Get time of day of last checkpoint
		Vector2 levelTimeOfDayAtStart = LevelManager.Instance.getLevelInfo(LevelManager.Instance.getLevelNumberOfLastCheckpoint()).timeOfDay;
		TimeSpan timeOfDayAtStart = new TimeSpan((int)levelTimeOfDayAtStart.x, (int)levelTimeOfDayAtStart.y, 0 );

		//Get time of day for the next level to complete
		Vector2 levelTimeOfDayAtEnd = LevelManager.Instance.getLevelInfo(LevelManager.Instance.getNextLevelToComplete() ).timeOfDay;
		TimeSpan timeOfDayAtEnd = new TimeSpan((int)levelTimeOfDayAtEnd.x, (int)levelTimeOfDayAtEnd.y, 0 );

		//Calculate the elapsed time in minutes
		TimeSpan elapsedTime =  timeOfDayAtEnd.Subtract( timeOfDayAtStart );

		//Calculate time penalty
		int penaltyInMinutes = PlayerStatsManager.Instance.getNumberDeathLeadingToEpisode( episodeNumber ) * GameManager.TIME_PENALTY_IN_MINUTES;
		timePenalty.text = penaltyInMinutes.ToString();

		Debug.Log("ClockTimeSetter-updateTime2 timeOfDayAtStart: " + timeOfDayAtStart + " timeOfDayAtEnd: " + timeOfDayAtEnd + " elapsed in min. " + elapsedTime.TotalMinutes );
		
		StartCoroutine( spinLevelTime( 1f, 3f, timeOfDayAtStart, (int) elapsedTime.TotalMinutes ) );

    }

	void setTime( TimeSpan timeOfDay )
	{
		//Pocket Watch
		float hours =(float)timeOfDay.Hours;
		float minutes = (float)timeOfDay.Minutes;
		hourHand.rotation=Quaternion.Euler(0,0,-(hours + minutes/60f) * 30f);
		minuteHand.rotation=Quaternion.Euler(0,0,-minutes*6f);

		//Time Left
		TimeSpan midnight = new TimeSpan(24, 0, 0 );
		TimeSpan timeLeftSpan = midnight.Subtract( timeOfDay );
		hours = timeLeftSpan.Hours;
		minutes = timeLeftSpan.Minutes;
		if (minutes < 10)
		{
			paddingChar = "0";
		}
		else
		{
			paddingChar = "";
		}
		timeLeft.text = hours.ToString() + ":" + paddingChar + minutes.ToString();
	}
	
	IEnumerator spinLevelTime( float waitDelay, float duration, TimeSpan timeOfDay, int numberOfMinutes )
	{
		yield return new WaitForSeconds(waitDelay);
		Debug.Log("spinLevelTime " + numberOfMinutes );
		float startTime = Time.time;
		float elapsedTime = 0;
		float currentNumber = 0;
		TimeSpan newTime = new TimeSpan(0, 0, 0 );
		int startValue = 0;
		TimeSpan span;

		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			currentNumber =  Mathf.Lerp( startValue, numberOfMinutes, elapsedTime/duration );
			span = TimeSpan.FromMinutes(currentNumber);
			newTime = timeOfDay.Add( span );
			setTime( newTime );
			yield return new WaitForFixedUpdate();  
	    }
		//Calculate time penalty
		int penaltyInMinutes = PlayerStatsManager.Instance.getNumberDeathLeadingToEpisode( LevelManager.Instance.getCurrentEpisodeNumber() ) * GameManager.TIME_PENALTY_IN_MINUTES;
		Debug.Log("spinLevelTime LevelManager.Instance.getCurrentEpisodeNumber: " + LevelManager.Instance.getCurrentEpisodeNumber() + " penalty " + penaltyInMinutes );
		StartCoroutine( spinPenaltyTime( 1f, 2f, newTime, penaltyInMinutes ) );	
	}

	IEnumerator spinPenaltyTime( float waitDelay, float duration, TimeSpan timeOfDay, int numberOfMinutes )
	{
		yield return new WaitForSeconds(waitDelay);
		Debug.Log("spinPenaltyTime " + numberOfMinutes );
		float startTime = Time.time;
		float elapsedTime = 0;
		float currentNumber = 0;
		TimeSpan newTime;
		int startValue = 0;
		TimeSpan span;

		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			currentNumber =  Mathf.Lerp( startValue, numberOfMinutes, elapsedTime/duration );
			span = TimeSpan.FromMinutes(currentNumber);
			newTime = timeOfDay.Add( span );
			setTime( newTime );
			yield return new WaitForFixedUpdate();  
	    }		
	}

}
