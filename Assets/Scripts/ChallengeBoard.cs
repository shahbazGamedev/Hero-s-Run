using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public enum ChallengeStatus {
	Not_started = 0,
	Completed = 2
}

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
		public string created_time;					//date field are returned as ISO-8601 formatted strings from the App Request and are stored as a string since DateTime is not Serializable.
		public ChallengeStatus status = ChallengeStatus.Not_started;

		public Challenge( string challengerFirstName, string challengerID, int score, int episodeNumber, DateTime created_time )
		{
			this.challengerFirstName = challengerFirstName;
			this.challengerID = challengerID;
			this.score = score;
			this.episodeNumber = episodeNumber;
			this.created_time = created_time.ToString();
		}		

		public void printChallenge()
		{
			string printStr = challengerFirstName + " " + challengerID  + " " + score + " " + episodeNumber + " " + created_time + " " + status;
			Debug.Log( "Challenge: " + printStr );
		}
	}
	
	public void addChallenge( string challengerFirstName, string challengerID, int score, int episodeNumber, DateTime created_time )
	{
		//If the same person has previously sent you a challenge for the same episode and you have accepted it, then only keep the challenge with the highest score.
		for( int i = 0; i < challengeList.Count; i++ )
		{
			if( challengeList[i].episodeNumber == episodeNumber && challengeList[i].challengerID == challengerID  )
			{
				if( score > challengeList[i].score )
				{
					challengeList[i].score = score; //update the score because it is higher
					serializeChallenges();
					return;
				}
				else
				{
					return; //we already have the entry with the highest score
				}
			}
		}

		//if we are here, it means it's a brand new entry :)
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

	public int getNumberOfActiveChallenges( int episodeNumber )
	{
		int numberOfActiveChallenges = 0;
		for( int i = 0; i < challengeList.Count; i++ )
		{
			if( challengeList[i].episodeNumber == episodeNumber ) numberOfActiveChallenges++;
		}
		return numberOfActiveChallenges;
	}

	public int getNumberOfUnstartedChallenges( int episodeNumber )
	{
		int numberOfUnstartedChallenges = 0;
		for( int i = 0; i < challengeList.Count; i++ )
		{
			if( challengeList[i].episodeNumber == episodeNumber && challengeList[i].status == ChallengeStatus.Not_started ) numberOfUnstartedChallenges++;
		}
		return numberOfUnstartedChallenges;
	}

	public void removeChallenge( Challenge challenge )
	{
		challengeList.Remove( challenge );
	}

	public List<Challenge> getCompletedChallenges( int episodeNumber )
	{
		List<Challenge> completedChallengesList = new List<Challenge>();
		for( int i = 0; i < challengeList.Count; i++ )
		{
			if( challengeList[i].episodeNumber == episodeNumber && challengeList[i].status == ChallengeStatus.Completed ) completedChallengesList.Add(challengeList[i]);
		}
		return completedChallengesList;
	}

	public void printAllChallenges()
	{
		for( int i = 0; i < challengeList.Count; i++ )
		{
			challengeList[i].printChallenge();
		}
	}

	public void serializeChallenges()
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setChallenges( json );
		PlayerStatsManager.Instance.savePlayerStats();
	}

	
}
