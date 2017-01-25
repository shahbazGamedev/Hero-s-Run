using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
	public RaceStatus raceStatus = RaceStatus.NOT_STARTED;
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

	private void grantXPAward( XPAwardType awardType )
	{
		if ( raceAwardList.Contains( awardType ) )
		{
			Debug.LogWarning("PlayerRaceManager-grantXPAward: award type already exists in list: " + awardType );
		}
		else
		{
			Debug.LogError("PlayerRaceManager-grantXPAward: granting award: " + awardType );
			raceAwardList.Add( awardType );
		}
	}

	//Note: racePosition: 1 is the winner, 2 is second place, and so forth.
	public void playerCrossedFinishLine( int racePosition )
	{
		raceAwardList.Clear();
		this.racePosition = racePosition;
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
		}
		//Save the dates in the player profile
		GameManager.Instance.playerProfile.serializePlayerprofile();

	}

}
