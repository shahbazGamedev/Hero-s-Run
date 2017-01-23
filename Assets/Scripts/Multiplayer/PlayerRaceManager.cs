using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRaceManager {

	[Header("General")]
	private static PlayerRaceManager playerRaceManager = null;
	public float raceDuration;

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
}
