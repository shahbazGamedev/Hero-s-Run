using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;

public class GameCenterManager : MonoBehaviour {

	//Valid achievements
	//Update achievementsDescriptionDict capacity if you add any.
	public static string NoviceRunner 	= "novice_runner";		//Ran 1000 meters
	public static string ZombieBowling 	= "zombie_bowling";		//Made 3 zombies fall in a row
	public static string BarrelCrusher 	= "barrel_crusher";		//Crushed X number of barrels
	public static string BootsOfJumping = "boots_of_jumping";	//First time used boots of jumping
	public static string WatchYourStep 	= "watch_your_step"; 	//Landed on the head of a zombie
	public static string CoinHoarder 	= "coin_hoarder"; 		//Accumulated 50000 coins
	public static string ChickenChaser 	= "chicken_chaser"; 	//Slid into a chicken and made it fly

	//Valid leaderboards
	public static string DistanceRunAllLevels = "distance_run_all_levels";

	static List<string> achievementsToReport = new List<string>();
	static List<string> achievementsCompleted = new List<string>();
	static Dictionary<string, IAchievementDescription> achievementsDescriptionDict = new Dictionary<string,IAchievementDescription>(8);
	public static bool isAuthenticated = false;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	void Start ()
	{
		// Authenticate and register a ProcessAuthentication callback
		// This call needs to be made before we can proceed to other calls in the Social API
		if( !isAuthenticated) Social.localUser.Authenticate (ProcessAuthentication);
	}
	
	// This function gets called when Authenticate completes
	// Note that if the operation is successful, Social.localUser will contain data from the server. 
	void ProcessAuthentication (bool success)
	{
		isAuthenticated = success;

		if (success)
		{
			// Request loaded achievements, and register a callback for processing them
			Social.LoadAchievements (achievements => {
				achievementsCompleted.Clear();
				if (achievements.Length > 0) {
					Debug.Log ("Got " + achievements.Length + " achievement instances");
					string myAchievements = "My achievements:\n";
					foreach (IAchievement achievement in achievements)
					{
						myAchievements += "\t" + achievement.id + " " + achievement.percentCompleted + " " + achievement.completed + "\n";
						if( achievement.completed )
						{
							//Copy it to our achievementsCompleted list
							achievementsCompleted.Add ( achievement.id );
						}
					}
					Debug.Log (myAchievements);
					Debug.Log ("GameCenterManager-ProcessAuthentication: Number of completed achievements is " + achievementsCompleted.Count + " out of " + achievements.Length );
				}
				else
				{
					Debug.Log ("No achievements returned");
				}
			});


			Social.LoadAchievementDescriptions (descriptions => {
				achievementsDescriptionDict.Clear();
				if (descriptions.Length > 0) {
					string achievementDescriptions = "Achievement Descriptions:\n";
					foreach (IAchievementDescription ad in descriptions)
					{
						achievementDescriptions += "\t" + ad.id + " " + ad.title + " " + ad.unachievedDescription + "\n";
						achievementsDescriptionDict.Add(ad.id, ad );
					}
				}
				else
					Debug.Log ("Failed to load achievement descriptions");
			});
		}
		else
			Debug.Log ("Failed to authenticate");
	}

	public static string getDescription( string achievementID )
	{
		if( achievementsDescriptionDict.ContainsKey( achievementID ) )
		{
			IAchievementDescription ad = achievementsDescriptionDict[achievementID];
			return ad.achievedDescription;
		}
		else
		{
			Debug.LogWarning("GameCenterManager-getDescription: could not find " + achievementID + " description in achievement dictionary.");
			return "DESCRIPTION NOT FOUND";
		}
	}

	public static Texture2D getImage( string achievementID )
	{
		if( achievementsDescriptionDict.ContainsKey( achievementID ) )
		{
			IAchievementDescription ad = achievementsDescriptionDict[achievementID];
			return ad.image;
		}
		else
		{
			Debug.LogWarning("GameCenterManager-getImage: could not find " + achievementID + " image in achievement dictionary.");
			return null;
		}
	}

	void Update()
	{
		//We can only send achievements from the main thread.
		foreach(string achievementID in achievementsToReport) 
		{
			reportAchievement( achievementID );
		}
		achievementsToReport.Clear();
	}

	public static bool isAchievementCompleted( string achievementID )
	{
		return achievementsCompleted.Contains( achievementID );
	}

	public static void resetAchievementsCompleted()
	{
		achievementsCompleted.Clear();
	}

	public static void addAchievement( string achievementID )
	{
		achievementsToReport.Add( achievementID );
	}

	//Reports an achievement to Game Center
	public static void reportAchievement( string achievementID )
	{
		Social.ReportProgress( achievementID, (double)100, result => {
			if (result)
			{
				Debug.Log ("Successfully reported achievement: " + achievementID );
				if( !achievementsCompleted.Contains( achievementID ) )
				{
					achievementsCompleted.Add( achievementID );
				}
			}
			else
			{
				Debug.Log ("Failed to report achievement: " + achievementID );
			}
		});
	}

	public static void updateLeaderboard()
	{
		//We only update the leaderboard if we are not in the editor.
		//We only update the leaderboard if we are in endless mode
 		#if !UNITY_EDITOR
		if( GameManager.Instance.getGameMode() == GameMode.Endless )
		{
			//The score is the sum of the Coins collected and the Distance run.
			int playerScore = LevelManager.Instance.getScore() + PlayerStatsManager.Instance.getDistanceTravelled();
			//For now, we only have a global leaderboard, which is used regardless of which episode you play
			GameCenterManager.reportLeaderboard( GameCenterManager.DistanceRunAllLevels, playerScore );
		}
		#endif
	}

	//Report leaderboard info (like the distance run) to Game Center
	public static void reportLeaderboard( string leaderboardID, int score )
	{
		Social.ReportScore( (long)score, leaderboardID,  result => {
		if (result)
				Debug.Log ("Successfully reported score to " + leaderboardID + " with a score of " + score);
		else
				Debug.Log ("Failed to report score to " + leaderboardID );
		});
	}

}
