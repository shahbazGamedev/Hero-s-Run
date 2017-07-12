using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
	public const int MAX_CHANGE_IN_TROPHIES = 50;
	public const int BASE_TROPHIES = 30;
	public const int VARIABLE_TROPHIES = 20;
	public const float TROPHIES_TRACK_DELTA = 400f;

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
	/// Returns true if trophies can be earned.
	/// You can earn trophies while playing online in the 2-player or 3-player mode.
	/// You don't earn trophies while playing with a friend or if playing offline.
	/// However, trophies can always be earned regardless of the mode in a debug build to facilitate testing.
	/// </summary>
	/// <returns><c>true</c>, if trophies can be earned, <c>false</c> otherwise.</returns>
	public bool canEarnTrophies()
	{
		PlayMode playMode = GameManager.Instance.getPlayMode();
		return Debug.isDebugBuild || playMode == PlayMode.PlayTwoPlayers || playMode == PlayMode.PlayThreePlayers;
	}

	public int getTrophiesEarned( PlayMode playMode, int racePosition, int playersTrophies, int opponentTrophies )
	{
		int trophies = 0;
		if( playMode == PlayMode.PlayTwoPlayers || playMode == PlayMode.PlayThreePlayers )
		{
			if( playersTrophies == opponentTrophies )
			{
				trophies = BASE_TROPHIES;
			}
			if( playersTrophies > opponentTrophies )
			{
				trophies = (int)Math.Ceiling( BASE_TROPHIES + VARIABLE_TROPHIES * ( 1f - ( playersTrophies - opponentTrophies )/TROPHIES_TRACK_DELTA ) );
			}
			else
			{
				trophies = (int)Math.Ceiling( BASE_TROPHIES + VARIABLE_TROPHIES * ( opponentTrophies - playersTrophies )/TROPHIES_TRACK_DELTA );
			}
			if( racePosition >= 2 )
			{
				trophies = -Mathf.Abs( trophies );
			}
		}
		else if( Debug.isDebugBuild )
		{
			if( playMode == PlayMode.PlayAlone )
			{
				trophies = MAX_CHANGE_IN_TROPHIES;
			}
			else if( playMode == PlayMode.PlayAgainstEnemy || playMode == PlayMode.PlayAgainstTwoEnemies )
			{
				if( racePosition == 1 )
				{
					trophies = BASE_TROPHIES;
				}
				else
				{
					trophies = -BASE_TROPHIES;
				}
			}
			else if( playMode == PlayMode.PlayWithFriends )
			{
				if( racePosition == 1 )
				{
					trophies = BASE_TROPHIES;
				}
				else
				{
					trophies = -BASE_TROPHIES;
				}
			}
		}
		Debug.Log( "getTrophiesEarned-Mode: " + playMode + " Race Position: " + racePosition + " Players trophies: " + playersTrophies + " Opponent trophies: " + opponentTrophies + " Trophies earned: " + trophies );
		return trophies;
	}

}
