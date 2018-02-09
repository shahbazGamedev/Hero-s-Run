using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//To Do
//Note: Issue with pmd data if two players have the same name.
//Add an animated flame if the win streak is 5 or more.
public class CompetitionResultsHandler : ResultsHandler {

	[Header("Opponent")]
	[SerializeField] TextMeshProUGUI winnerOpponent;
	[SerializeField] Image iconOpponent;
	[SerializeField] TextMeshProUGUI nameOpponent;
	[SerializeField] GameObject emoteGameObjectOpponent;

	[Header("Player")]
	[SerializeField] TextMeshProUGUI winnerPlayer;
	[SerializeField] Image iconPlayer;
	[SerializeField] TextMeshProUGUI namePlayer;
	[SerializeField] GameObject emoteGameObjectPlayer;
	[SerializeField] TextMeshProUGUI cpPlayer;

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

		//Okay button
		okayButton.onClick.RemoveAllListeners();
		okayButton.onClick.AddListener(() => this.OnClickOkay() );
	}
}
