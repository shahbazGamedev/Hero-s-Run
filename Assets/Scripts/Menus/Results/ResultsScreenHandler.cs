﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ResultsScreenHandler : MonoBehaviour {

	[SerializeField] RectTransform resultsHolder;
	[SerializeField] GameObject resultPrefab;

	public void showResults()
	{
		adjustSizeOfResultsScreen( PlayerRace.players.Count );
		for(int i=0; i<PlayerRace.players.Count;i++)
		{
			//For each player, create a result entry
			createResultEntry( PlayerRace.players[i] );
		}
	}

	void adjustSizeOfResultsScreen( int playerCount )
	{
		float titleHeight = resultPrefab.GetComponent<RectTransform>().sizeDelta.y; //It has the same height as a result entry
		float singleEntryHeight = resultPrefab.GetComponent<RectTransform>().sizeDelta.y;
		float spacing = GetComponent<VerticalLayoutGroup>().spacing;
		float desiredHeight = titleHeight + playerCount * ( singleEntryHeight + spacing );
		resultsHolder.sizeDelta = new Vector2( resultsHolder.sizeDelta.x, desiredHeight );
	}

	void createResultEntry( PlayerRace playerRace )
	{
		int racePosition = playerRace.racePosition;

		PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( playerRace.name );
		int winStreak = pmd.currentWinStreak;
		if( racePosition == 0 )
		{
			//If player won the race, increment is win streak
			winStreak = pmd.currentWinStreak + 1;
		}

		Sprite playerIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;

		//Race time
		TimeSpan ts = TimeSpan.FromSeconds( playerRace.raceDuration );
		DateTime dt = new DateTime(ts.Ticks);
		string raceDurationString = dt.ToString("mm:ss");
		GameObject go = (GameObject)Instantiate(resultPrefab);
		go.transform.SetParent(resultsHolder,false);
		go.GetComponent<ResultEntry>().configureEntry( racePosition + 1, winStreak, pmd.level, pmd.playerName, playerIconSprite, raceDurationString );
	}

}
