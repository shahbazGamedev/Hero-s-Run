using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Apple.ReplayKit;

public enum RaceStatus {
	
	NOT_STARTED = 1,
	IN_PROGRESS = 2,
	COMPLETED = 3
}

public class PlayerRaceManager {

	[Header("General")]
	private static PlayerRaceManager playerRaceManager = null;
	private TimeSpan MAX_TIME_FOR_CONSECUTIVE_RACE = new TimeSpan(0,2,0); //in minutes
	public float raceDuration;
	public int racePosition;

	public int trophiesOwnedByOpponent1;
	public int trophiesOwnedByOpponent2;

	RaceStatus raceStatus = RaceStatus.NOT_STARTED;
	public List<XPAwardType> raceAwardList = new List<XPAwardType>();
	public static PlayerRaceManager Instance
	{
        get
		{
            if (playerRaceManager == null)
			{

                playerRaceManager = new PlayerRaceManager();

            }
            return playerRaceManager;
        }
    }

	public void setRaceStatus( RaceStatus raceStatus )
	{
		this.raceStatus = raceStatus;
	}

	public RaceStatus getRaceStatus()
	{
		return raceStatus;
	}

	public void setTrophiesOwnedByOpponent( int opponentNumber, int trophiesOwned )
	{
		if( opponentNumber == 1 )
		{
			trophiesOwnedByOpponent1 = trophiesOwned;
			Debug.Log("setTrophiesOwnedByOpponent " + opponentNumber + " " + trophiesOwned );
		}
		else if( opponentNumber == 2 )
		{
			trophiesOwnedByOpponent2 = trophiesOwned;
			Debug.Log("setTrophiesOwnedByOpponent " + opponentNumber + " " + trophiesOwned );
		}
		else
		{
			Debug.LogError( "setTrophiesOwnedByOpponent-the opponent number specified " + opponentNumber + " is invalid. It should be 1 or 2.");
		}
	}

	private void grantXPAward( XPAwardType awardType )
	{
		if ( raceAwardList.Contains( awardType ) )
		{
			Debug.LogWarning("PlayerRaceManager-grantXPAward: award type already exists in list: " + awardType );
		}
		else
		{
			Debug.Log("PlayerRaceManager-grantXPAward: granting award: " + awardType );
			raceAwardList.Add( awardType );
		}
	}

	//Note: racePosition: 1 is the winner, 2 is second place, and so forth.
	public void playerCompletedRace( int racePosition, float raceDuration, float distanceTravelled, int numberOfTimesDiedDuringRace )
	{
		raceAwardList.Clear();
		this.racePosition = racePosition;
		this.raceDuration = raceDuration;
		raceStatus = RaceStatus.COMPLETED;
		grantXPAward(XPAwardType.FINISHED_RACE);
		if( racePosition == 1 ) grantXPAward(XPAwardType.WON);

		//Did the player complete this match in the allocated time to get the consecutive race XP award?
		TimeSpan utcNowTimeSpan = new TimeSpan( DateTime.UtcNow.Ticks );
		TimeSpan lastMatchPlayedTimestampTimeSpan = new TimeSpan( GameManager.Instance.playerProfile.getLastMatchPlayedTime().Ticks );

		if( utcNowTimeSpan <= (lastMatchPlayedTimestampTimeSpan + MAX_TIME_FOR_CONSECUTIVE_RACE) )
		{
			PlayerRaceManager.Instance.grantXPAward(XPAwardType.CONSECUTIVE_RACE);
			Debug.Log("PlayerRaceManager-playerCrossedFinishLine: GRANT CONSECUTIVE_RACE " + ((lastMatchPlayedTimestampTimeSpan + MAX_TIME_FOR_CONSECUTIVE_RACE) - utcNowTimeSpan ).Seconds.ToString() );
		}
		else
		{
			Debug.Log("PlayerRaceManager-playerCrossedFinishLine: DONT GRANT CONSECUTIVE_RACE " +  (utcNowTimeSpan - (lastMatchPlayedTimestampTimeSpan + MAX_TIME_FOR_CONSECUTIVE_RACE) ).Seconds.ToString() );
		}
		GameManager.Instance.playerProfile.setLastMatchPlayedTime( DateTime.UtcNow );

		//Verify if we should grant the first win of the day XP award
		if( racePosition == 1 ) 
		{
			//Is this the next day?
			Debug.Log("PlayerRaceManager-test for first win :" + DateTime.UtcNow.Date.ToString() + " vs " + GameManager.Instance.playerProfile.getLastMatchWonTime().ToString() );
			if( DateTime.UtcNow.Date > GameManager.Instance.playerProfile.getLastMatchWonTime() )
			{
				grantXPAward(XPAwardType.FIRST_WIN_OF_THE_DAY);
				GameManager.Instance.playerProfile.setLastMatchWonTime( DateTime.UtcNow );
			}
			//The victor is given coins but only in certain play modes.
			if( GameManager.Instance.getPlayMode() == PlayMode.PlayTwoPlayers || GameManager.Instance.getPlayMode() == PlayMode.PlayThreePlayers )
			{
				int coinsAwardedOnVictory = LevelManager.Instance.getSelectedCircuit().coinsAwardedOnVictory;
				GameManager.Instance.playerInventory.addCoins( coinsAwardedOnVictory );
			}
		}
		if( GameManager.Instance.canEarnTrophies() )
		{
			//Note: trophiesEarnedLastRace will be negative if the player lost.
			int trophiesEarnedLastRace = TrophyManager.Instance.getTrophiesEarned( GameManager.Instance.getPlayMode(), racePosition, GameManager.Instance.playerProfile.getTrophies(), trophiesOwnedByOpponent1 );
			//Store this value temporarily in player profile so the GameEndManager can retrieve it.
			GameManager.Instance.playerProfile.setTrophiesEarnedLastRace( trophiesEarnedLastRace );
			GameManager.Instance.playerProfile.changeTrophies( trophiesEarnedLastRace );
		}

		earnCrowns();

		//Update the player statistics
		GameManager.Instance.playerStatistics.updateRaceStatistics( racePosition, distanceTravelled, numberOfTimesDiedDuringRace );
		//Save the dates in the player profile
		GameManager.Instance.playerProfile.serializePlayerprofile();
		//Save the player deck because every time the players plays a card, we increment the timesUsed value in PlayerCardData.
		//We don't want to save every time the player plays a card while racing, so we do it once at the end of the race or if he quits.
		GameManager.Instance.playerDeck.serializePlayerDeck(true);

	}

	public void playerAbandonedRace()
	{
		Debug.Log("PlayerRaceManager-playerAbandonedRace" );
		GameManager.Instance.playerStatistics.incrementNumberRacesAbandoned();
		//Save the player deck because every time the players plays a card, we increment the timesUsed value in PlayerCardData.
		//We don't want to save every time the player plays a card while racing, so we do it once at the end of the race or if he quits.
		GameManager.Instance.playerDeck.serializePlayerDeck(true);
		//Remove trophies when you abandon a race
		if( GameManager.Instance.canEarnTrophies() )
		{
			// trophiesLost will be negative.
			// Assume you are in 3rd position if you abandon a race.
			int trophiesLost = TrophyManager.Instance.getTrophiesEarned( GameManager.Instance.getPlayMode(), 3, GameManager.Instance.playerProfile.getTrophies(), trophiesOwnedByOpponent1 );
			GameManager.Instance.playerProfile.changeTrophies( trophiesLost );
			GameManager.Instance.playerProfile.serializePlayerprofile();
			Debug.Log("PlayerRaceManager-playerAbandonedRace: trophies lost " + trophiesLost );
		}
		#if UNITY_IOS
		//When the player quits the race stop the recording and discard the video
		try
		{
			if( ReplayKit.isRecording )
			{
				ReplayKit.StopRecording();
				ReplayKit.Discard();
			}
		}
   		catch (Exception e)
		{
			Debug.LogError( "Replay exception: " +  e.ToString() + " ReplayKit.lastError: " + ReplayKit.lastError );
    	}
		#endif
	}

	void earnCrowns()
	{
		int crownsEarned = 0;
		switch ( GameManager.Instance.getPlayMode() )
		{
			case PlayMode.PlayTwoPlayers:
				if( racePosition == 1 ) 
				{
					crownsEarned = 3;
				}
				else if( racePosition == 2 ) 
				{
					crownsEarned = 0;
				}
			break;

			case PlayMode.PlayThreePlayers:
				if( racePosition == 1 ) 
				{
					crownsEarned = 3;
				}
				else if( racePosition == 2 ) 
				{
					crownsEarned = 1;
				}
				else if( racePosition == 3 ) 
				{
					crownsEarned = 0;
				}
			break;
			#if UNITY_EDITOR
			case PlayMode.PlayAlone:
				crownsEarned = 6;
			break;
			case PlayMode.PlayAgainstEnemy:
				if( racePosition == 1 ) 
				{
					crownsEarned = 3;
				}
				else if( racePosition == 2 ) 
				{
					crownsEarned = 0;
				}
			break;

			case PlayMode.PlayAgainstTwoEnemies:
				if( racePosition == 1 ) 
				{
					crownsEarned = 3;
				}
				else if( racePosition == 2 ) 
				{
					crownsEarned = 1;
				}
				else if( racePosition == 3 ) 
				{
					crownsEarned = 0;
				}
			break;
			#endif
		}
		GameManager.Instance.playerInventory.addCrowns( crownsEarned );
		GameManager.Instance.playerInventory.serializePlayerInventory( true );
	}
}
