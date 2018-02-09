using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using TMPro;


public class CompetitionResultsHandler : ResultsHandler {

	[SerializeField] RectTransform resultsHolder;
	[SerializeField] GameObject resultPrefab;

	#region Opponent area
	[SerializeField] TextMeshProUGUI winnerOpponent;
	[SerializeField] Image iconOpponent;
	[SerializeField] TextMeshProUGUI nameOpponent;
	public GameObject emoteGameObjectOpponent;
	#endregion

	#region Player area
	[SerializeField] TextMeshProUGUI winnerPlayer;
	[SerializeField] Image iconPlayer;
	[SerializeField] TextMeshProUGUI namePlayer;
	public GameObject emoteGameObjectPlayer;
	[SerializeField] TextMeshProUGUI cpPlayer;
	#endregion

	//Note: Issue if two players have the same name with pmd data.


	public void showResults( PlayerRace localPlayerRace )
	{
		//From top to bottom
		#region Opponent area
		PlayerRace opponent = getOtherPlayer( localPlayerRace );
		PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( opponent.name );
		winnerOpponent.gameObject.SetActive( opponent.racePosition == RacePosition.FIRST_PLACE ) ;
		Sprite opponentIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;
		iconOpponent.sprite = opponentIconSprite;
		nameOpponent.text = opponent.name;
		emoteGameObjectOpponent.name = opponent.name;
		emotesList.Add( emoteGameObjectOpponent );
		#endregion

		#region Player area
		pmd = LevelManager.Instance.getPlayerMatchDataByName( localPlayerRace.name );
		winnerPlayer.gameObject.SetActive( localPlayerRace.racePosition == RacePosition.FIRST_PLACE ) ;
		Sprite playerIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;
		iconPlayer.sprite = playerIconSprite;
		namePlayer.text = localPlayerRace.name;
		int competitivePointsEarnedLastRace = GameManager.Instance.playerProfile.getCompetitivePointsEarnedLastRace();
		if( competitivePointsEarnedLastRace > 0 )
		{
			cpPlayer.text = "+" + competitivePointsEarnedLastRace.ToString();
		}
		else if( competitivePointsEarnedLastRace < 0 )
		{
			cpPlayer.text = "-" + competitivePointsEarnedLastRace.ToString();
		}
		else
		{
			cpPlayer.text = competitivePointsEarnedLastRace.ToString();
		}
		emoteGameObjectPlayer.name = localPlayerRace.name;
		emotesList.Add( emoteGameObjectPlayer );
		#endregion


		okayButton.onClick.RemoveAllListeners();
		okayButton.onClick.AddListener(() => this.OnClickOkay() );
		//adjustSizeOfResultsScreen( PlayerRace.players.Count );

		#region Reward boxes
		//Loot Box.
		//You only get a loot box if you won.
		if( localPlayerRace.racePosition == RacePosition.FIRST_PLACE ) displayLootBox();

		//Soft Currency.
		//You only win soft currency if you won. The amount is determined by the player's current sector.
		if( localPlayerRace.racePosition == RacePosition.FIRST_PLACE )
		{
			int currentSector = GameManager.Instance.playerProfile.getCurrentSector();
			int softCurrencyGranted = SectorManager.Instance.getSectorVictorySoftCurrency( currentSector );
			displaySoftCurrency( softCurrencyGranted );
		}

		//XP
		//You always earn XP regardless of whether you won or lost.
		displayXP();	  		
		#endregion

		//Order by the race position of the players i.e. 1st place, 2nd place, and so forth
		PlayerRace.players = PlayerRace.players.OrderBy( p => p.racePosition ).ToList();

		for(int i=0; i<PlayerRace.players.Count;i++)
		{
			//For each player, create a result entry
			//createResultEntry( PlayerRace.players[i] );
		}
	}

	void adjustSizeOfResultsScreen( int playerCount )
	{
		float titleHeight = resultPrefab.GetComponent<RectTransform>().sizeDelta.y; //It has the same height as a result entry
		float singleEntryHeight = resultPrefab.GetComponent<RectTransform>().sizeDelta.y;
		float spacing = resultsHolder.GetComponent<VerticalLayoutGroup>().spacing;
		float desiredHeight = titleHeight + playerCount * ( singleEntryHeight + spacing );
		resultsHolder.sizeDelta = new Vector2( resultsHolder.sizeDelta.x, desiredHeight );
	}

	//Note that win streak is not used for now. An animated flame will be used in the future to convey the win streak, this is why I am keeping the code.
	void createResultEntry( PlayerRace playerRace )
	{
		RacePosition racePosition = playerRace.racePosition;

		PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( playerRace.name );
		int winStreak = pmd.currentWinStreak;
		if( racePosition == RacePosition.FIRST_PLACE )
		{
			//If player won the race, increment is win streak
			winStreak = pmd.currentWinStreak + 1;
		}

		Sprite playerIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;

		//Race time
		string raceDurationString;
		if( playerRace.playerCrossedFinishLine )
		{
			TimeSpan ts = TimeSpan.FromSeconds( playerRace.raceDuration );
			DateTime dt = new DateTime(ts.Ticks);
			raceDurationString = dt.ToString("mm:ss");
		}
		else
		{
			raceDurationString = LocalizationManager.Instance.getText( "RESULTS_NOT_AVAILABLE" );
		}
		GameObject go = (GameObject)Instantiate(resultPrefab);
		go.transform.SetParent(resultsHolder,false);
		go.GetComponent<ResultEntry>().configureEntry( racePosition, pmd.level, pmd.playerName, playerIconSprite, raceDurationString );
		go.GetComponent<ResultEntry>().emoteGameObject.name = pmd.playerName;
		emotesList.Add( go.GetComponent<ResultEntry>().emoteGameObject );
	}


}
