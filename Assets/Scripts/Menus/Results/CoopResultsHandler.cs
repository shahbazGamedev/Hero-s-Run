using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoopResultsHandler : ResultsHandler {

	[Header("Rounds Survived")]
	[SerializeField] TextMeshProUGUI roundsSurvivedText;

	[Header("Partner")]
	[SerializeField] Image iconPartner;
	[SerializeField] TextMeshProUGUI namePartner;
	[SerializeField] GameObject emoteGameObjectPartner;
	[SerializeField] TextMeshProUGUI scorePartner;

	[Header("Player")]
	[SerializeField] Image iconPlayer;
	[SerializeField] TextMeshProUGUI namePlayer;
	[SerializeField] GameObject emoteGameObjectPlayer;
	[SerializeField] TextMeshProUGUI scorePlayer;

	public void showResults( PlayerRace localPlayerRace )
	{
		//From top to bottom

		//Display a reddish damage effect because it looks nice for a zombie mode.
		StartCoroutine( HUDMultiplayer.hudMultiplayer.displayPermanentDamageEffect() );

		#region Waves survived
		int wavesBeaten = CoopWaveGenerator.numberOfWavesTriggered - 1;
		if( wavesBeaten < 0 ) wavesBeaten = 0;
		string wavesBeatenString;
		if( wavesBeaten <= 1 )
		{
			wavesBeatenString = LocalizationManager.Instance.getText( "COOP_RESULTS_ROUNDS_SINGULAR" );
		}
		else
		{
			wavesBeatenString = LocalizationManager.Instance.getText( "COOP_RESULTS_ROUNDS_PLURAL" );
		}
		wavesBeatenString = string.Format( wavesBeatenString, wavesBeaten );
		roundsSurvivedText.text = wavesBeatenString;
		#endregion

		//The wave XP bonus is the same for the player and his partner.
		int waveBonus = wavesBeaten * CoopWaveGenerator.XP_EARNED_PER_WAVE;

		#region Partner area
		PlayerMatchData partner_pmd = LevelManager.Instance.getPartnerMatchDataByName( localPlayerRace.name ) ;
		Sprite opponentIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( partner_pmd.playerIcon ).icon;
		iconPartner.sprite = opponentIconSprite;
		namePartner.text = partner_pmd.playerName;
		emoteGameObjectPartner.name = partner_pmd.playerName;
		emotesList.Add( emoteGameObjectPartner );
		//The score is the sum of the score bonus and the wave bonus.
		scorePartner.text = ( partner_pmd.score + waveBonus ).ToString("N0");
		#endregion

		#region Player area
		PlayerMatchData local_player_pmd = LevelManager.Instance.getPlayerMatchDataByName( localPlayerRace.name );
		Sprite playerIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( local_player_pmd.playerIcon ).icon;
		iconPlayer.sprite = playerIconSprite;
		namePlayer.text = localPlayerRace.name;
		emoteGameObjectPlayer.name = localPlayerRace.name;
		emotesList.Add( emoteGameObjectPlayer );
		//The score is the sum of the score bonus and the wave bonus.
		scorePlayer.text = ( local_player_pmd.score + waveBonus ).ToString("N0");
		#endregion
		
		#region Reward boxes
		//Allow the players to stay as a team but only if the other player is still connected.
		bool stayAsTeamEnabled = getOtherPlayer( localPlayerRace ) != null;

		if( GameManager.Instance.getPlayMode() == PlayMode.PlayCoopWithOnePlayer )
		{
			grantCoopRewards( wavesBeaten, local_player_pmd.playerName, partner_pmd.playerName, stayAsTeamEnabled );
		}
		else if( GameManager.Instance.getPlayMode() == PlayMode.PlayCoopWithOneBot )
		{
			if( Debug.isDebugBuild )
			{
				//PlayCoopWithOneBot is a mode that only exists for testing purposes.
				//Grant the same rewards as for PlayCoopWithOnePlayer if it is a debug build.
				grantCoopRewards( wavesBeaten, local_player_pmd.playerName, partner_pmd.playerName, stayAsTeamEnabled );
			}
		}
		#endregion

		//Okay button
		okayButton.onClick.RemoveAllListeners();
		okayButton.onClick.AddListener(() => this.OnClickOkay() );
	}

	private void grantCoopRewards( int wavesBeaten, string localPlayerName, string partnerName, bool stayAsTeamEnabled )
	{
		//Loot Box.
		//You do not get a loot box in coop.

		//Soft Currency.
		//You win soft currency based on the number of waves completed.
		int softCurrencyGranted = wavesBeaten * CoopWaveGenerator.SOFT_CURRENCY_EARNED_PER_WAVE;
		displaySoftCurrency( softCurrencyGranted );

		//XP
		//In coop, you gain XP for each wave that you complete as well as score bonuses (for example, because you knocked back a zombie).
		displayXP();	  		

		//Stay as team
		if( stayAsTeamEnabled ) displayStayAsTeam( localPlayerName, partnerName );

		//Save the rewards
		saveRewards();	
	}
}
