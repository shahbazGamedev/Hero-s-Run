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
	[SerializeField] GameObject winnerOpponent;
	[SerializeField] Image iconOpponent;
	[SerializeField] TextMeshProUGUI nameOpponent;
	[SerializeField] GameObject emoteGameObjectOpponent;

	[Header("Versus Text")]
	[SerializeField] GameObject versusText;

	[Header("Player")]
	[SerializeField] GameObject ribbonPlayer;
	[SerializeField] GameObject winnerPlayer;
	[SerializeField] Image iconPlayer;
	[SerializeField] TextMeshProUGUI namePlayer;
	[SerializeField] GameObject emoteGameObjectPlayer;

	[Header("Player Solo")]
	[SerializeField] GameObject ribbonPlayerSolo;
	[SerializeField] Image iconPlayerSolo;
	[SerializeField] TextMeshProUGUI namePlayerSolo;

	[Header("Reward Text")]
	[SerializeField] GameObject rewardText;

	private void showResultsSolo( PlayerRace localPlayerRace )
	{
		#region Opponent area
		winnerOpponent.SetActive( false );
		ribbonOpponent.SetActive( false ); //also hides the name, and the icon.
		#endregion

		versusText.SetActive( false );

		#region Player area
		winnerPlayer.SetActive( false );
		ribbonPlayer.SetActive( false ); //also hides the name, and the icon.
		#endregion

		#region Player Solo area
		ribbonPlayerSolo.SetActive( true );
		PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( localPlayerRace.name );
		Sprite playerIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;
		iconPlayerSolo.sprite = playerIconSprite;
		namePlayerSolo.text = localPlayerRace.name;
		#endregion

		#region Reward area
		//You don't get any rewards when playing solo.
		rewardText.SetActive( false );
		#endregion
	}

	public void showResults( PlayerRace localPlayerRace )
	{
		if( GameManager.Instance.getPlayMode() == PlayMode.PlayAlone )
		{
			showResultsSolo( localPlayerRace );
		}
		else
		{
			ribbonPlayerSolo.SetActive( false );

			//From top to bottom
			PlayerMatchData pmd;
			#region Opponent area
			PlayerRace opponent = getOtherPlayer( localPlayerRace );
			pmd = LevelManager.Instance.getPlayerMatchDataByName( opponent.name );
			winnerOpponent.gameObject.SetActive( opponent.racePosition == RacePosition.FIRST_PLACE ) ;
			Sprite opponentIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;
			iconOpponent.sprite = opponentIconSprite;
			nameOpponent.text = opponent.name;
			emoteGameObjectOpponent.name = opponent.name;
			emotesList.Add( emoteGameObjectOpponent );
			#endregion
	
			#region Player area
			pmd = LevelManager.Instance.getPlayerMatchDataByName( localPlayerRace.name );
			winnerPlayer.SetActive( localPlayerRace.racePosition == RacePosition.FIRST_PLACE ) ;
			Sprite playerIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;
			iconPlayer.sprite = playerIconSprite;
			namePlayer.text = localPlayerRace.name;
			emoteGameObjectPlayer.name = localPlayerRace.name;
			emotesList.Add( emoteGameObjectPlayer );
			#endregion
	
			#region Reward boxes
			if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstOneFriend )
			{
				//You don't get a loot box when playing with a friend, even if you won.
		
				//You don't get soft currency when playing with a friend, even if you won.
		
				//XP
				//You earn XP when playing with a friend, regardless of whether you won or lost.
				displayXP();	  		
	
				//Save the rewards
				saveRewards();	
			}
			else if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstOnePlayer )
			{
				grantCompetitonRewards( localPlayerRace );
			}
			else if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstOneBot )
			{
				if( Debug.isDebugBuild )
				{
					//You normally only earn XP when playing against a bot.
					//However, to facilitate testing, we grant the same rewards as PlayAgainstOnePlayer if this is a Debug build.
					grantCompetitonRewards( localPlayerRace );
				}
				else
				{
					//You don't get a loot box when playing against a bot, even if you won.
			
					//You don't get soft currency when playing against a bot, even if you won.
			
					//XP
					//You can earn XP when playing against a bot, regardless of whether you won or lost.
					displayXP();	  		
					//Save the rewards
					saveRewards();	
				}
			}
			#endregion
		}

		//Okay button
		okayButton.onClick.RemoveAllListeners();
		okayButton.onClick.AddListener(() => this.OnClickOkay() );
	}

	private void grantCompetitonRewards( PlayerRace localPlayerRace )
	{
		//Competitive Points
		//You win or lose CP depending on whether you won or lost.
		displayCompetitivePoints( localPlayerRace );

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
		//You earn XP regardless of whether you won or lost.
		displayXP();	  		

		//Save the rewards
		saveRewards();	
	}
}
