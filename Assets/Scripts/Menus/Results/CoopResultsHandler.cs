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
		PlayerRace partner = getOtherPlayer( localPlayerRace );
		PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( partner.name );
		Sprite opponentIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;
		iconPartner.sprite = opponentIconSprite;
		namePartner.text = partner.name;
		emoteGameObjectPartner.name = partner.name;
		emotesList.Add( emoteGameObjectPartner );
		//The score is the sum of the score bonus and the wave bonus.
		scorePartner.text = ( pmd.score + waveBonus ).ToString("N0");
		#endregion

		#region Player area
		pmd = LevelManager.Instance.getPlayerMatchDataByName( localPlayerRace.name );
		Sprite playerIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;
		iconPlayer.sprite = playerIconSprite;
		namePlayer.text = localPlayerRace.name;
		emoteGameObjectPlayer.name = localPlayerRace.name;
		emotesList.Add( emoteGameObjectPlayer );
		//The score is the sum of the score bonus and the wave bonus.
		scorePlayer.text = ( pmd.score + waveBonus ).ToString("N0");
		#endregion
		
		#region Reward boxes
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
		//Allow the players to stay as a team.
		displayStayAsTeam();	  		
		#endregion

		//Okay button
		okayButton.onClick.RemoveAllListeners();
		okayButton.onClick.AddListener(() => this.OnClickOkay() );
	}

	public void OnClickStayAsTeam()
	{
		Debug.LogWarning("CoopResultsHandler-Stay-as-team option not implemented yet.");
	}

}
