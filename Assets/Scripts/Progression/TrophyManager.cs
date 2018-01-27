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
	public const int BASE_TROPHIES = 30;
	public const int VARIABLE_TROPHIES = 15;
	public const int MAX_CHANGE_IN_TROPHIES = BASE_TROPHIES + VARIABLE_TROPHIES;

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

			print( "***" );
			print( "Sector 0 0 getTrophiesEarned " + getTrophiesEarned( 3, 0, 100, 100 ) );

			//Player won
			print( "won sector 1 same 30 getTrophiesEarned " + getTrophiesEarned( 1, 1, 100, 100 ) ); 	//Same
			print( "won sector 1 max 15 getTrophiesEarned " + getTrophiesEarned( 1, 1, 400, 1 ) );		//Player has more Max
			print( "won sector 1 min 45 getTrophiesEarned " + getTrophiesEarned( 1, 1, 1, 400 ) );		//Opponent has more max

			//Player lost
			print( "lost sector 1 same -30 getTrophiesEarned " + getTrophiesEarned( 2, 1, 100, 100 ) );	//Same
			print( "lost sector 1 max -45 getTrophiesEarned " + getTrophiesEarned( 2, 1, 400, 1 ) );	//Player has more Max
			print( "lost sector 1 min -15 getTrophiesEarned " + getTrophiesEarned( 2, 1, 1, 400 ) );	//Opponent has more max
			print( "---" );
			//Player won
			print( "won sector 2 same 30 getTrophiesEarned " + getTrophiesEarned( 1, 2, 500, 500 ) ); 	//Same
			print( "won sector 2 max 15 getTrophiesEarned " + getTrophiesEarned( 1, 2, 800, 401 ) );	//Player has more Max
			print( "won sector 2 min 45 getTrophiesEarned " + getTrophiesEarned( 1, 2, 401, 800 ) );	//Opponent has more max

			//Player lost
			print( "lost sector 2 same -30 getTrophiesEarned " + getTrophiesEarned( 2, 3, 600, 600 ) );	//Same
			print( "lost sector 2 max -45 getTrophiesEarned " + getTrophiesEarned( 2, 3, 800, 401 ) );	//Player has more Max
			print( "lost sector 2 min -15 getTrophiesEarned " + getTrophiesEarned( 2, 3, 401, 800 ) );	//Opponent has more max

		}
	}

	/// <summary>
	/// Returns true if trophies can be earned.
	/// You can earn trophies while playing online against one player.
	/// You don't earn trophies while playing with a friend, coop, or offline.
	/// However, trophies can always be earned when playing alone or against one bot in a Debug Build to facilitate testing.
	/// </summary>
	/// <returns><c>true</c>, if trophies can be earned, <c>false</c> otherwise.</returns>
	public bool canEarnTrophies()
	{
		PlayMode playMode = GameManager.Instance.getPlayMode();
		return Debug.isDebugBuild || playMode == PlayMode.PlayAgainstOnePlayer;
	}

	/// <summary>
	/// Returns the number of trophies earned. This number can be negative.
	/// The maximum number of trophies that can be earned or lost is MAX_CHANGE_IN_TROPHIES.
	/// The amount of trophies won or lost is based on the difference between you and your enemy's trophies.
	/// If you win against a player who has more trophies than you, you will be rewarded with more trophies.
	/// On the contrary, losing against an enemy with fewer trophies makes you lose more.
	/// </summary>
	/// <returns>The trophies earned.</returns>
	/// <param name="racePosition">Race position where one is the first place.</param>
	/// <param name="sector">Sector.</param>
	/// <param name="playersTrophies">Player trophies.</param>
	/// <param name="opponentTrophies">Opponent trophies.</param>
	public int getTrophiesEarned( int racePosition, int sector, int playerTrophies, int opponentTrophies )
	{
		if( sector == 0 ) return 0;
		int trophies = 0;
		PlayMode playMode = GameManager.Instance.getPlayMode();
		//PlayMode playMode = PlayMode.PlayAgainstOnePlayer;

		float trophyPercentageDifference = ( playerTrophies - opponentTrophies )/SectorManager.Instance.getTrophyRange( sector );

		if( playMode == PlayMode.PlayAgainstOnePlayer )
		{
			if( racePosition == 1 )
			{
				//Player won.
				if( playerTrophies == opponentTrophies )
				{
					trophies = BASE_TROPHIES;
				}
				else
				{
					trophies = (int)Math.Ceiling( BASE_TROPHIES - VARIABLE_TROPHIES * trophyPercentageDifference );
				}
			}
			else if( racePosition == 2 )
			{
				//Player lost. The player will lose trophies.
				if( playerTrophies == opponentTrophies )
				{
					trophies = -BASE_TROPHIES;
				}
				else
				{
					trophies = -(int)Math.Ceiling( BASE_TROPHIES + VARIABLE_TROPHIES * trophyPercentageDifference );
				}
			}
			else
			{
				Debug.LogError("TrophyManager-getTrophiesEarned: race position " + racePosition + " is invalid. It should be either one or two.");
			}
		}
		else if( Debug.isDebugBuild )
		{
			if( playMode == PlayMode.PlayAlone )
			{
				//Player won
				trophies = BASE_TROPHIES;
			}
			else if( playMode == PlayMode.PlayAgainstOneBot )
			{
				if( racePosition == 1 )
				{
					//Player won
					trophies = BASE_TROPHIES;
				}
				else
				{
					//Player lost
					trophies = -BASE_TROPHIES;
				}
			}
		}
		Debug.Log( "getTrophiesEarned-Mode: " + playMode + " Race Position: " + racePosition + " Player trophies: " + playerTrophies + " Opponent trophies: " + opponentTrophies + " Trophies earned: " + trophies + " for sector: " + sector );
		return trophies;
	}

}
