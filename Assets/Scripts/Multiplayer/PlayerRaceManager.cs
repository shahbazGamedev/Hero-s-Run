using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Apple.ReplayKit;

//This is a global status (i.e. for all players).
public enum RaceStatus {
	
	NOT_STARTED = 1,	//Initial value.
	IN_PROGRESS = 2,	//Set when the start countdown has finished.
	COMPLETED = 3 		//Set when all players have crossed the finish line.
}

public enum RacePosition {
	
	NOT_SET_1 = -1,
	NOT_SET_2 = -2,
	FIRST_PLACE = 0,
	SECOND_PLACE = 1,
	THIRD_PLACE = 2
}

public class PlayerRaceManager {

	[Header("General")]
	private static PlayerRaceManager playerRaceManager = null;
	private TimeSpan MAX_TIME_FOR_CONSECUTIVE_RACE = new TimeSpan(0,2,0); //in minutes
	public float raceDuration;
	public RacePosition racePosition;

	public int trophiesOwnedByOpponent1;
	public int trophiesOwnedByOpponent2;

	RaceStatus raceStatus = RaceStatus.NOT_STARTED;
	public List<XPAwardType> raceAwardList = new List<XPAwardType>();
	public SectorStatus sectorStatus = SectorStatus.NO_CHANGE;
	bool initialized = false;

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

	void initialize()
	{
		PlayerProfile.sectorChanged += SectorChanged;		
		initialized = true;
	}

	public void setRaceStatus( RaceStatus raceStatus )
	{
		if ( !initialized )
		{
			initialize();
		}
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

	public void playerCompletedRace( RacePosition racePosition, float raceDuration, int numberOfTimesDiedDuringRace )
	{
		raceAwardList.Clear();
		this.racePosition = racePosition;
		this.raceDuration = raceDuration;
		grantXPAward(XPAwardType.FINISHED_RACE);
		if( GameManager.Instance.playerProfile.getSkillBonus() > 0 ) grantXPAward(XPAwardType.SKILL_BONUS);
		if( racePosition == RacePosition.FIRST_PLACE && GameManager.Instance.isOnlinePlayMode() ) grantXPAward(XPAwardType.WON);

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
		if( racePosition == RacePosition.FIRST_PLACE ) 
		{
			//Is this the next day?
			Debug.Log("PlayerRaceManager-test for first win :" + DateTime.UtcNow.Date.ToString() + " vs " + GameManager.Instance.playerProfile.getLastMatchWonTime().ToString() );
			if( GameManager.Instance.isOnlinePlayMode() && DateTime.UtcNow.Date > GameManager.Instance.playerProfile.getLastMatchWonTime() )
			{
				grantXPAward(XPAwardType.FIRST_WIN_OF_THE_DAY);
				GameManager.Instance.playerProfile.setLastMatchWonTime( DateTime.UtcNow );
			}
			//The victor is given coins but only in certain play modes.
			if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstOnePlayer || GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstTwoPlayers )
			{
				int softCurrencyAwardedOnVictory = SectorManager.Instance.getSectorVictorySoftCurrency( GameManager.Instance.playerProfile.getCurrentSector() );
				GameManager.Instance.playerInventory.addCoins( softCurrencyAwardedOnVictory );
			}
			//For rate this app
			GameManager.Instance.playerProfile.incrementConsecutiveWins();

			//Grant victor a loot box
			GameManager.Instance.playerInventory.addLootBox( new LootBoxOwnedData( LootBoxType.RACE_WON, GameManager.Instance.playerProfile.getCurrentSector(), LootBoxState.READY_TO_UNLOCK ) );
		}
		else
		{
			//For rate this app
			GameManager.Instance.playerProfile.resetConsecutiveWins();
		}

		if( CompetitionManager.Instance.canEarnCompetitivePoints() )
		{
			//Note: trophiesEarnedLastRace will be negative if the player lost.
			int trophiesEarnedLastRace = CompetitionManager.Instance.getCompetitivePointsEarned( racePosition, GameManager.Instance.playerProfile.getCurrentSector(), GameManager.Instance.playerProfile.getCompetitivePoints(), trophiesOwnedByOpponent1 );
			//Store this value temporarily in player profile so the GameEndManager can retrieve it.
			GameManager.Instance.playerProfile.setCompetitivePointsEarnedLastRace( trophiesEarnedLastRace );
			GameManager.Instance.playerProfile.changeCompetitivePoints( trophiesEarnedLastRace );
		}

		//Update the player statistics
		GenerateLevel generateLevel = GameObject.FindObjectOfType<GenerateLevel>();
		float distanceTraveled = generateLevel.levelLengthInMeters;
		GameManager.Instance.playerStatistics.updateRaceStatistics( racePosition, distanceTraveled, numberOfTimesDiedDuringRace, GameManager.Instance.playerProfile.getSkillBonus() );
		//Save the dates in the player profile
		GameManager.Instance.playerProfile.serializePlayerprofile();
		//Save the player deck because every time the players plays a card, we increment the timesUsed value in PlayerCardData.
		//We don't want to save every time the player plays a card while racing, so we do it once at the end of the race or if he quits.
		GameManager.Instance.playerDeck.serializePlayerDeck(true);

	}

	/// <summary>
	/// Called when a player abandons the race.
	/// A race is considered abandoned if the player leaves before crossing the finish line.
	/// This method does nothing when in coop mode.
	/// However in competition, the player will lose trophies and his races abandoned count will increase.
	/// </summary>
	public void playerAbandonedRace()
	{
		if( GameManager.Instance.isCoopPlayMode() ) return;

		Debug.Log("PlayerRaceManager-playerAbandonedRace" );
		GameManager.Instance.playerStatistics.incrementNumberRacesAbandoned();
		//Save the player deck because every time the players plays a card, we increment the timesUsed value in PlayerCardData.
		//We don't want to save every time the player plays a card while racing, so we do it once at the end of the race or if he quits.
		GameManager.Instance.playerDeck.serializePlayerDeck(true);
		//Remove trophies when you abandon a race
		if( CompetitionManager.Instance.canEarnCompetitivePoints() )
		{
			// trophiesLost will be negative.
			// Assume you are in 2nd position if you abandon a race.
			int trophiesLost = CompetitionManager.Instance.getCompetitivePointsEarned( RacePosition.SECOND_PLACE, GameManager.Instance.playerProfile.getCurrentSector(), GameManager.Instance.playerProfile.getCompetitivePoints(), trophiesOwnedByOpponent1 );
			GameManager.Instance.playerProfile.changeCompetitivePoints( trophiesLost );
			GameManager.Instance.playerProfile.serializePlayerprofile();
			Debug.Log("PlayerRaceManager-playerAbandonedRace: Trophies lost " + trophiesLost );
		}

		//For rate this app
		GameManager.Instance.playerProfile.resetConsecutiveWins();

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

	void SectorChanged( SectorStatus sectorStatus, int previousSector, int newSector )
	{
		//The ultimate user of sectorStatus is MPGameEndManager.
		//However, MPGameEndManager resides in the Matchmaking scene and not the Level scene.
		//This is why we save sectorStatus in PlayerRaceManager.
		this.sectorStatus = sectorStatus;
		Debug.Log("PlayerRaceManager- Sector changed event: Sector status is: " + sectorStatus + " Previous sector was: " + previousSector + " New sector is: " +  newSector );
	}

}
