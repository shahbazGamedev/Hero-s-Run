using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RaceStatus {
	
	NOT_STARTED = 1,
	IN_PROGRESS = 2,
	COMPLETED = 3
}

public class PlayerRaceManager {

	[Header("General")]
	private static PlayerRaceManager playerRaceManager = null;
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

	public void grantXPAward( XPAwardType awardType )
	{
		if ( raceAwardList.Contains( awardType ) )
		{
			Debug.LogWarning("PlayerRaceManager-grantXPAward: award type already exists in list: " + awardType );
		}
		else
		{
			raceAwardList.Add( awardType );
		}
	}
}
