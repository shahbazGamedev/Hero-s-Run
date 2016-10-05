using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
public class ChallengeBoard {

	public List<Challenge> challengeList = new List<Challenge>();


	[System.Serializable]
	public class Challenge
	{
		public string challengerFirstName = ""; 	//First name of challenger i.e. Bob
		public string challengerID = "";			//Format example: ":"1378641986" i.e. Bob's Facebook ID
		public int score = 0; 						//Bob's score to beat
		public int episodeNumber = -1; 				//Episode number
		public int levelNumber = -1; 				//Level number
		public string created_time;					//date field are returned as ISO-8601 formatted strings from the App Request and are stored as a string since DateTime is not Serializable.

		public Challenge( string challengerFirstName, string challengerID, int score, int episodeNumber, DateTime created_time )
		{
			this.challengerFirstName = challengerFirstName;
			this.challengerID = challengerID;
			this.score = score;
			this.episodeNumber = episodeNumber;
			this.levelNumber = LevelManager.Instance.getLevelNumberFromEpisodeNumber( episodeNumber );
			this.created_time = created_time.ToString();
		}		

		public void printChallenge()
		{
			string printStr = challengerFirstName + " " + challengerID  + " " + score + " " + episodeNumber + " " + levelNumber + " " + created_time;
			Debug.Log( "Challenge: " + printStr );
		}
	}
	
	public void addChallenge( string challengerFirstName, string challengerID, int score, int episodeNumber, DateTime created_time )
	{
		Challenge challenge = new Challenge( challengerFirstName, challengerID, score, episodeNumber, created_time );
		challengeList.Add( challenge );
		serializeChallenges();
	}

	public List<Challenge> getChallenges( int episodeNumber )
	{
		List<Challenge> episodeChallengeList = new List<Challenge>();
		for( int i = 0; i < challengeList.Count; i++ )
		{
			if( challengeList[i].episodeNumber == episodeNumber ) episodeChallengeList.Add(challengeList[i]);
		}
		//Sorted with lowest score first
		List<Challenge> sortedChallengeListByScore = episodeChallengeList.OrderBy(challenge=>challenge.score).ToList();
		return sortedChallengeListByScore;
	}

	public void serializeChallenges()
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setChallenges( json );
		PlayerStatsManager.Instance.savePlayerStats();
	}

	
}
