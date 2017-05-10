using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trophy manager.
/// You only earn trophies when competing against random opponents in multiplayer:
/// - PlayOthers
/// - PlayThreePlayers
/// You do not earn trophies in the other play modes:
/// - PlayWithFriends
/// - PlayAlone
/// - PlayAgainstEnemy
///
/// For matches with trophies:
/// - If you win a race, you earn some trophies. If you lose a race, you lose some trophies.
/// - If the player abandons a match purposefully or loses connection, he is considered to have lost and will lose trophies.
/// - You earn more trophies in a 3-player race than in a 2-player race.
/// - The race track selected is based on the player's total number of trophies.
///
///	For matches without trophies:
/// - The player can choose the race track.
/// - Only unlocked race tracks can be selected.
/// - To unlock a race track, you must earn enough trophies.
/// - The tutorial race track is always unlocked.
///
/// </summary>
public class TrophyManager : MonoBehaviour {

	public static TrophyManager Instance;

	void Awake ()
	{
		if(Instance)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
	}

	/// <summary>
	/// Gets the number of trophies.
	/// </summary>
	/// <returns>The number of trophies.</returns>
	/// <param name="playMode">Play mode.</param>
	/// <param name="racePosition">Race position. Either 1,2 or 3 if the race was completed, -1 otherwise (player abandoned or lost connection during race).</param>
	public int getNumberOfTrophies( PlayMode playMode, int racePosition )
	{
		switch ( playMode )
		{
			case PlayMode.PlayAgainstEnemy:
			case PlayMode.PlayAlone:
			case PlayMode.PlayWithFriends:
				return 0;

			case PlayMode.PlayThreePlayers:
			case PlayMode.PlayOthers:
				return getTrophiesEarned( playMode, racePosition );

			default:
				return 0;
		}
	}

	int getTrophiesEarned( PlayMode playMode, int racePosition )
	{
		int trophies = 0;

		if( playMode == PlayMode.PlayOthers )
		{
			switch ( racePosition )
			{
				case 1:
					trophies = 30;
					break;
	
				case 2:
				case -1:
					trophies = -25;
					break;
			}
		}
		else if ( playMode == PlayMode.PlayThreePlayers )
		{
			switch ( racePosition )
			{
				case 1:
					trophies = 30;
					break;
	
				case 2:
					trophies = 20;
					break;
	
				case 3:
				case -1:
					trophies = -25;
					break;
			}
		}
		else
		{
			trophies = 0;
		}
		return trophies;
	}

}
