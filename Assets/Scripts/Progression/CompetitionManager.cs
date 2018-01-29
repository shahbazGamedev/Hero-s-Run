using System.Collections;
using UnityEngine;
using System;

public class CompetitionManager : MonoBehaviour {

	public static CompetitionManager Instance;
	public const int BASE_COMPETITIVE_POINTS = 30;
	public const int VARIABLE_COMPETITIVE_POINTS = 15;
	public const int MAX_CHANGE_IN_COMPETITIVE_POINTS = BASE_COMPETITIVE_POINTS + VARIABLE_COMPETITIVE_POINTS;

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

			/*print( "***" );
			print( "Sector 0 0 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.THIRD_PLACE, 0, 100, 100 ) );

			//Player won
			print( "won sector 1 same 30 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.FIRST_PLACE, 1, 100, 100 ) ); 	//Same
			print( "won sector 1 max 15 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.FIRST_PLACE, 1, 400, 1 ) );		//Player has more Max
			print( "won sector 1 min 45 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.FIRST_PLACE, 1, 1, 400 ) );		//Opponent has more max

			//Player lost
			print( "lost sector 1 same -30 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.SECOND_PLACE, 1, 100, 100 ) );	//Same
			print( "lost sector 1 max -45 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.SECOND_PLACE, 1, 400, 1 ) );	//Player has more Max
			print( "lost sector 1 min -15 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.SECOND_PLACE, 1, 1, 400 ) );	//Opponent has more max
			print( "---" );
			//Player won
			print( "won sector 2 same 30 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.FIRST_PLACE, 2, 500, 500 ) ); 	//Same
			print( "won sector 2 max 15 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.FIRST_PLACE, 2, 800, 401 ) );	//Player has more Max
			print( "won sector 2 min 45 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.FIRST_PLACE, 2, 401, 800 ) );	//Opponent has more max

			//Player lost
			print( "lost sector 2 same -30 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.SECOND_PLACE, 3, 600, 600 ) );	//Same
			print( "lost sector 2 max -45 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.SECOND_PLACE, 3, 800, 401 ) );	//Player has more Max
			print( "lost sector 2 min -15 getCompetitivePointsEarned " + getCompetitivePointsEarned( RacePosition.SECOND_PLACE, 3, 401, 800 ) );	//Opponent has more max*/

		}
	}

	/// <summary>
	/// Returns true if competitive points can be earned.
	/// You can only earn competitive points if you have completed the tutorial.
	/// You can earn competitive points while playing online against one player.
	/// You don't earn competitive points while playing with a friend, coop, or offline.
	/// However, competitive points can always be earned when playing alone or against one bot in a Debug Build to facilitate testing as long as you are not in coop.
	/// </summary>
	/// <returns><c>true</c>, if competitive points can be earned, <c>false</c> otherwise.</returns>
	public bool canEarnCompetitivePoints()
	{
		PlayMode playMode = GameManager.Instance.getPlayMode();
		if( !GameManager.Instance.playerProfile.hasCompletedTutorial() ) return false;
		return ( Debug.isDebugBuild && !GameManager.Instance.isCoopPlayMode() ) || playMode == PlayMode.PlayAgainstOnePlayer;
	}

	/// <summary>
	/// Returns the number of competitive points earned. This number can be negative.
	/// The maximum number of competitive points that can be earned or lost is MAX_CHANGE_IN_COMPETITIVE_POINTS.
	/// The amount of competitive points won or lost is based on the difference between you and your enemy's competitive points.
	/// If you win against a player who has more competitive points than you, you will be rewarded with more competitive points.
	/// On the contrary, losing against an enemy with fewer competitive points makes you lose more.
	/// </summary>
	/// <returns>The competitive points earned.</returns>
	/// <param name="racePosition">Race position where one is the first place.</param>
	/// <param name="sector">Sector.</param>
	/// <param name="playersCompetitivePoints">Player competitive points.</param>
	/// <param name="opponentCompetitivePoints">Opponent competitive points.</param>
	public int getCompetitivePointsEarned( RacePosition racePosition, int sector, int playerCompetitivePoints, int opponentCompetitivePoints )
	{
		if( sector == 0 ) return 0;
		int competitivePoints = 0;
		//PlayMode playMode = GameManager.Instance.getPlayMode();
		PlayMode playMode = PlayMode.PlayAgainstOnePlayer;

		float pointsPercentageDifference = ( playerCompetitivePoints - opponentCompetitivePoints )/SectorManager.Instance.getPointsRange( sector );

		if( playMode == PlayMode.PlayAgainstOnePlayer )
		{
			if( racePosition == RacePosition.FIRST_PLACE )
			{
				//Player won.
				if( playerCompetitivePoints == opponentCompetitivePoints )
				{
					competitivePoints = BASE_COMPETITIVE_POINTS;
				}
				else
				{
					competitivePoints = (int)Math.Ceiling( BASE_COMPETITIVE_POINTS - VARIABLE_COMPETITIVE_POINTS * pointsPercentageDifference );
				}
			}
			else if( racePosition == RacePosition.SECOND_PLACE )
			{
				//Player lost. The player will lose points.
				if( playerCompetitivePoints == opponentCompetitivePoints )
				{
					competitivePoints = -BASE_COMPETITIVE_POINTS;
				}
				else
				{
					competitivePoints = -(int)Math.Ceiling( BASE_COMPETITIVE_POINTS + VARIABLE_COMPETITIVE_POINTS * pointsPercentageDifference );
				}
			}
			else
			{
				Debug.LogError("CompetitionManager-getCompetitivePointsEarned: race position " + racePosition + " is invalid. It should be either first place or second place.");
			}
		}
		else if( Debug.isDebugBuild )
		{
			if( playMode == PlayMode.PlayAlone )
			{
				//Player won
				competitivePoints = BASE_COMPETITIVE_POINTS;
			}
			else if( playMode == PlayMode.PlayAgainstOneBot )
			{
				if( racePosition == RacePosition.FIRST_PLACE )
				{
					//Player won
					competitivePoints = MAX_CHANGE_IN_COMPETITIVE_POINTS;
				}
				else if( racePosition == RacePosition.SECOND_PLACE )
				{
					//Player lost
					competitivePoints = -MAX_CHANGE_IN_COMPETITIVE_POINTS;
				}
				else
				{
					Debug.LogError("CompetitionManager-getCompetitivePointsEarned: race position " + racePosition + " is invalid. It should be either first place or second place.");
				}
			}
		}
		Debug.Log( "getCompetitivePointsEarned-Mode: " + playMode + " Race Position: " + racePosition + " Player CompetitivePoints: " + playerCompetitivePoints + " Opponent CompetitivePoints: " + opponentCompetitivePoints + " CompetitivePoints earned: " + competitivePoints + " for sector: " + sector );
		return competitivePoints;
	}

}
