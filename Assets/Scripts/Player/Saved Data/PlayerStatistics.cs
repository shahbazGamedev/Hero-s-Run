using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum StatisticDataType
 {
	RACES_RUN = 0,						//Player at least started the race, maybe did not finish it.
	RACES_WON = 1,						//Player completed race in first position.
	RACES_ABANDONED = 2,				//Player used the quit button during a race.
	RACES_COMPLETED = 12,				//Player completed the race. He might have won or lost.

	DISTANCE_TRAVELED_LIFETIME = 3,		//Sum of the distance travelled for all the completed races.
	DEATHS_LIFETIME = 4,				//Sum of all the times the player died during a race, whether it was completed or not.
	PERFECT_RACES_LIFETIME = 5,			//Sum of all the races where the player did not die once during a completed race.

	CURRENT_WIN_STREAK = 6,				//The number of races won in a row. Resets as soon as you lose or abandon a race.
	BEST_WIN_STREAK_LIFETIME = 7,		//The player's best win streak ever.
	WIN_LOSS_RATIO = 8,					//Races won divided by races run.

	HIGHEST_COMPETITIVE_POINTS = 9,		//Highest number of competitive points ever reached.
	
	FAVORITE_CARD = 10,					//The card most frequently played.
	AVERAGE_SKILL_BONUS = 11,			//The player's average skill bonus.

	//Coop
	COOP_HIGH_SCORE_WAVES = 100			//The highest wave completed in coop mode.
}

[System.Serializable]
/// <summary>
/// Player statistics.
/// </summary>
public class PlayerStatistics {

	[SerializeField] List<StatisticData> statisticEntriesList = new List <StatisticData>();

	/// <summary>
	/// Populate the entries list for first time user.
	/// </summary>
	public void initialize()
	{		
		statisticEntriesList.Add( new StatisticData( StatisticDataType.CURRENT_WIN_STREAK, 0 ) );
		statisticEntriesList.Add( new StatisticData( StatisticDataType.BEST_WIN_STREAK_LIFETIME, 0 ) );

		statisticEntriesList.Add( new StatisticData( StatisticDataType.WIN_LOSS_RATIO, 0 ) );
		statisticEntriesList.Add( new StatisticData( StatisticDataType.PERFECT_RACES_LIFETIME, 0 ) );

		statisticEntriesList.Add( new StatisticData( StatisticDataType.HIGHEST_COMPETITIVE_POINTS, 0 ) );
		statisticEntriesList.Add( new StatisticData( StatisticDataType.FAVORITE_CARD, 0 ) );

		statisticEntriesList.Add( new StatisticData( StatisticDataType.RACES_WON, 0 ) );
		statisticEntriesList.Add( new StatisticData( StatisticDataType.RACES_RUN, 0 ) );
		statisticEntriesList.Add( new StatisticData( StatisticDataType.RACES_COMPLETED, 0 ) );

		statisticEntriesList.Add( new StatisticData( StatisticDataType.RACES_ABANDONED, 0 ) );
		statisticEntriesList.Add( new StatisticData( StatisticDataType.DEATHS_LIFETIME, 0 ) );

		statisticEntriesList.Add( new StatisticData( StatisticDataType.DISTANCE_TRAVELED_LIFETIME, 0 ) );

		statisticEntriesList.Add( new StatisticData( StatisticDataType.AVERAGE_SKILL_BONUS, 0 ) );

		//Coop related
		statisticEntriesList.Add( new StatisticData( StatisticDataType.COOP_HIGH_SCORE_WAVES, 1 ) );

	}

	public List<StatisticData> getStatisticEntriesList()
	{
		return statisticEntriesList;
	}

	public int getStatisticData( StatisticDataType type )
	{
		return statisticEntriesList.Find( data => data.type == type).value;
	}

	public void setStatisticData( StatisticDataType type, int value )
	{
		statisticEntriesList.Find( data => data.type == type).value = value;
	}

	public void setHighestNumberOfCompetitivePoints( int value )
	{
		if( value > getStatisticData(StatisticDataType.HIGHEST_COMPETITIVE_POINTS) )
		{
			setStatisticData( StatisticDataType.HIGHEST_COMPETITIVE_POINTS, value );
			serializePlayerStatistics( true );
		}
	}

	/// <summary>
	/// Gets the win/loss ratio, that is the number of races won divided by the number of races run.
	/// Returns zero if the the number of races run is zero.
	/// </summary>
	/// <returns>The win/loss ratio.</returns>
	public float getWinLoss()
	{
		//Don't divide by zero.
		if( getStatisticData(StatisticDataType.RACES_RUN) > 0 )
		{
			return getStatisticData(StatisticDataType.RACES_WON)/(float)getStatisticData(StatisticDataType.RACES_RUN);
		}
		else
		{
			return 0;
		}
	}

	/// <summary>
	/// Increments the number of races run. Called as soon as the race starts. Saves the change immediately.
	/// </summary>
	public void incrementNumberRacesRun()
	{
		int numberRacesRun = getStatisticData(StatisticDataType.RACES_RUN);
		setStatisticData( StatisticDataType.RACES_RUN, numberRacesRun + 1 );
		serializePlayerStatistics( true );
	}

	/// <summary>
	/// Increments the number of races abandoned. Called if the player quits a race in progress. Resets the win streak to zero.
	/// </summary>
	public void incrementNumberRacesAbandoned()
	{
		int numberRacesAbandoned = getStatisticData(StatisticDataType.RACES_ABANDONED);
		setStatisticData( StatisticDataType.RACES_ABANDONED, numberRacesAbandoned + 1 );

		setStatisticData( StatisticDataType.CURRENT_WIN_STREAK, 0 );

		serializePlayerStatistics( true );
	}

	/// <summary>
	/// Increments the number of deaths, lifetime. Called each time the player dies.
	/// </summary>
	public void incrementNumberOfDeathsLifetime()
	{
		int numberOfDeathsLifetime = getStatisticData(StatisticDataType.DEATHS_LIFETIME);
		setStatisticData( StatisticDataType.DEATHS_LIFETIME, numberOfDeathsLifetime + 1 );
		serializePlayerStatistics( true );
	}

	/// <summary>
	/// Increments the number of races completed.
	/// </summary>
	public void incrementNumberOfRacesCompleted()
	{
		int numberOfRacesCompleted = getStatisticData(StatisticDataType.RACES_COMPLETED);
		setStatisticData( StatisticDataType.RACES_COMPLETED, numberOfRacesCompleted + 1 );
	}

	/// <summary>
	/// Called when the player has completed the race.
	/// </summary>
	public void updateRaceStatistics( RacePosition racePosition, float distanceTravelled, int numberOfTimesDiedDuringRace, int skillBonusEarned )
	{
		int distanceTravelledLifetime = getStatisticData(StatisticDataType.DISTANCE_TRAVELED_LIFETIME);
		setStatisticData( StatisticDataType.DISTANCE_TRAVELED_LIFETIME, (int) (distanceTravelledLifetime + distanceTravelled) );

		if( racePosition == RacePosition.FIRST_PLACE )
		{
			int numberRacesWon = getStatisticData(StatisticDataType.RACES_WON);
			setStatisticData( StatisticDataType.RACES_WON, numberRacesWon + 1 );

			int currentWinStreak = getStatisticData(StatisticDataType.CURRENT_WIN_STREAK);
			setStatisticData( StatisticDataType.CURRENT_WIN_STREAK, currentWinStreak + 1 );

			if( getStatisticData(StatisticDataType.CURRENT_WIN_STREAK) > getStatisticData(StatisticDataType.BEST_WIN_STREAK_LIFETIME) )
			{
				int bestWinStreakLifetime = getStatisticData(StatisticDataType.BEST_WIN_STREAK_LIFETIME);
				setStatisticData( StatisticDataType.BEST_WIN_STREAK_LIFETIME, bestWinStreakLifetime + 1 );
			}
		}
		else
		{
			setStatisticData( StatisticDataType.CURRENT_WIN_STREAK, 0 );
		}
		if( numberOfTimesDiedDuringRace == 0 )
		{
			int perfectRacesLifetime = getStatisticData(StatisticDataType.PERFECT_RACES_LIFETIME);
			setStatisticData( StatisticDataType.PERFECT_RACES_LIFETIME, perfectRacesLifetime + 1 );
		}

		incrementNumberOfRacesCompleted();
		updateAverageSkillBonus( skillBonusEarned );

		serializePlayerStatistics( false );
	}

	/// <summary>
	/// Updates the average skill bonus. Called when the player has completed the race.
	/// </summary>
	/// <param name="skillBonusEarned">Skill bonus earned.</param>
	void updateAverageSkillBonus( int skillBonusEarned )
	{
		int currentAverageSkillBonus = getStatisticData(StatisticDataType.AVERAGE_SKILL_BONUS);
		setStatisticData( StatisticDataType.AVERAGE_SKILL_BONUS, (int) ( ( currentAverageSkillBonus + skillBonusEarned ) / (float) getStatisticData(StatisticDataType.RACES_COMPLETED) ) );
	}

	#region Coop
	public void updateCoopHighScoreWaves( int waveReached )
	{
		int currentHighScoreWaves = getStatisticData(StatisticDataType.COOP_HIGH_SCORE_WAVES);
		int waveCompleted = waveReached - 1;
		if( waveCompleted > currentHighScoreWaves )
		{
			setStatisticData( StatisticDataType.COOP_HIGH_SCORE_WAVES, waveCompleted );
			serializePlayerStatistics( true );
		}
	}
	#endregion

	public void serializePlayerStatistics( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerStatistics( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}

	[System.Serializable]
	public class StatisticData
	{
		public StatisticDataType type;
		public int value;
		public StatisticData( StatisticDataType type, int value )
		{
			this.type = type;
			this.value = value;
		}
	}

}
