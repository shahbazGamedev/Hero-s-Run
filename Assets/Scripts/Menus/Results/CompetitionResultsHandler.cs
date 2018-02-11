using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//To Do
//Note: Issue with pmd data if two players have the same name.
//Add an animated flame if the win streak is 5 or more.
public class CompetitionResultsHandler : ResultsHandler {

	[Header("Opponent")]
	[SerializeField] GameObject ribbonOpponent;
	[SerializeField] TextMeshProUGUI winnerOpponent;
	[SerializeField] Image iconOpponent;
	[SerializeField] TextMeshProUGUI nameOpponent;
	[SerializeField] GameObject emoteGameObjectOpponent;

	[Header("Versus Text")]
	[SerializeField] GameObject versusText;

	[Header("Player")]
	[SerializeField] TextMeshProUGUI winnerPlayer;
	[SerializeField] Image iconPlayer;
	[SerializeField] TextMeshProUGUI namePlayer;
	[SerializeField] GameObject emoteGameObjectPlayer;
	[SerializeField] TextMeshProUGUI cpPlayer;

	[Header("Reward Text")]
	[SerializeField] GameObject rewardText;

	public void showResults( PlayerRace localPlayerRace )
	{
		//From top to bottom
		PlayerMatchData pmd;
		#region Opponent area
		if( GameManager.Instance.getPlayMode() == PlayMode.PlayAlone )
		{
			winnerOpponent.gameObject.SetActive( false );
			ribbonOpponent.SetActive( false ); //also hides the name, and the icon.
			versusText.SetActive( false );
		}
		else
		{
			PlayerRace opponent = getOtherPlayer( localPlayerRace );
			pmd = LevelManager.Instance.getPlayerMatchDataByName( opponent.name );
			winnerOpponent.gameObject.SetActive( opponent.racePosition == RacePosition.FIRST_PLACE ) ;
			Sprite opponentIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;
			iconOpponent.sprite = opponentIconSprite;
			nameOpponent.text = opponent.name;
			emoteGameObjectOpponent.name = opponent.name;
			emotesList.Add( emoteGameObjectOpponent );
		}
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
		else
		{
			cpPlayer.text = competitivePointsEarnedLastRace.ToString();
		}
		emoteGameObjectPlayer.name = localPlayerRace.name;
		emotesList.Add( emoteGameObjectPlayer );
		#endregion

		#region Reward boxes
		if( GameManager.Instance.getPlayMode() == PlayMode.PlayAlone )
		{
			//You do not get any rewards when playing alone.
			rewardText.SetActive( false );
		}
		else
		{
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
		}
		#endregion

		//Okay button
		okayButton.onClick.RemoveAllListeners();
		okayButton.onClick.AddListener(() => this.OnClickOkay() );
	}
}
