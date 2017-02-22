using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
/// <summary>
/// Player statistics.
/// </summary>
public class PlayerStatistics {

	public int numberRacesRun = 0;				//Player at least started the race, may did not finish it.
	public int numberRacesWon = 0;				//Player completed race in first position.
	public int numberRacesAbandoned = 0;		//Player used the quit button during a race.

	public float distanceTravelledLifetime = 0;		//Sum of the distance travelled for all the completed races.
	public int numberOfDeathsLifetime = 0;			//Sum of all the times the player died during a race, whether it was completed or not.
	public int perfectRacesLifetime = 0;			//Sum of all the races where the player did not die once during a completed race.

	public int currentWinStreak = 0;			//The number of races won in a row. Resets as soon as you lose or abandon a race.
	public int bestWinStreakLifetime = 0;		//The player's best win streak ever.

	/// <summary>
	/// Gets the win/loss ratio, that is the number of races won divided by the number of races run.
	/// </summary>
	/// <returns>The win/loss ratio.</returns>
	public float getWinLoss()
	{
		return numberRacesWon/(float)numberRacesRun;
	}

	/// <summary>
	/// Increments the number of races run. Called as soon as the race starts. Saves the change immediately.
	/// </summary>
	public void incrementNumberRacesRun()
	{
		numberRacesRun++;
		serializePlayerStatistics( true );
	}

	/// <summary>
	/// Increments the number of races abandoned. Called if the player quits a race in progress. Resets the win streak to zero.
	/// </summary>
	public void incrementNumberRacesAbandoned()
	{
		numberRacesAbandoned++;
		currentWinStreak = 0;
		serializePlayerStatistics( true );
	}

	/// <summary>
	/// Increments the number of number of deaths, lifetime. Called each time the player dies.
	/// </summary>
	public void incrementNumberOfDeathsLifetime()
	{
		numberOfDeathsLifetime++;
		serializePlayerStatistics( true );
	}

	/// <summary>
	/// Called when the player has completed the race.
	/// </summary>
	public void updateRaceStatistics( int racePosition, float distanceTravelled, int numberOfTimesDiedDuringRace )
	{
		distanceTravelledLifetime = distanceTravelledLifetime + distanceTravelled;
		if( racePosition == 1 )
		{
			numberRacesWon++;
			currentWinStreak++;
			if( currentWinStreak > bestWinStreakLifetime ) bestWinStreakLifetime++;
		}
		else
		{
			currentWinStreak = 0;
		}
		if( numberOfTimesDiedDuringRace == 0 ) perfectRacesLifetime++;
		serializePlayerStatistics( false );
	}

	public void serializePlayerStatistics( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerStatistics( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}
}
