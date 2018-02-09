using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;


public class CoopResultsHandler : ResultsHandler {

	[SerializeField] RectTransform coopResultsHolder;
	[SerializeField] GameObject coopResultPrefab;
	[SerializeField] TextMeshProUGUI roundsSurvivedText;
	[SerializeField] Transform footer;
	#region Partner area
	[SerializeField] Image iconPartner;
	[SerializeField] TextMeshProUGUI namePartner;
	public GameObject emoteGameObjectPartner;
	[SerializeField] TextMeshProUGUI scorePartner;
	#endregion

	#region Player area
	[SerializeField] Image iconPlayer;
	[SerializeField] TextMeshProUGUI namePlayer;
	public GameObject emoteGameObjectPlayer;
	[SerializeField] TextMeshProUGUI scorePlayer;
	#endregion

	public void showResults( PlayerRace localPlayerRace )
	{
		//From top to bottom
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

		int wavesCompleted = CoopWaveGenerator.numberOfWavesTriggered - 1;
		if( wavesCompleted < 0 ) wavesCompleted = 0;
		//The wave bonus is the same for the player and his partner.
		int waveBonus = wavesCompleted * CoopWaveGenerator.SCORE_PER_WAVE;

		#region Partner area
		PlayerRace partner = getOtherPlayer( localPlayerRace );
		PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( partner.name );
		Sprite opponentIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;
		iconPartner.sprite = opponentIconSprite;
		namePartner.text = partner.name;
		emoteGameObjectPartner.name = partner.name;
		emotesList.Add( emoteGameObjectPartner );
		scorePartner.text = ( pmd.score + waveBonus ).ToString("N0");
		#endregion

		#region Player area
		pmd = LevelManager.Instance.getPlayerMatchDataByName( localPlayerRace.name );
		Sprite playerIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;
		iconPlayer.sprite = playerIconSprite;
		namePlayer.text = localPlayerRace.name;
		emoteGameObjectPlayer.name = localPlayerRace.name;
		emotesList.Add( emoteGameObjectPlayer );
		scorePlayer.text = ( pmd.score + waveBonus ).ToString("N0");
		#endregion

		okayButton.onClick.RemoveAllListeners();
		okayButton.onClick.AddListener(() => this.OnClickOkay() );

		StartCoroutine( HUDMultiplayer.hudMultiplayer.displayPermanentDamageEffect() );

		//adjustSizeOfResultsScreen( PlayerRace.players.Count );

		#region Reward boxes
		//Loot Box.
		//You do not get a loot box in coop.

		//Soft Currency.
		//You win X soft currency per wave completed.
		int softCurrencyGranted = wavesBeaten * CoopWaveGenerator.SOFT_CURRENCY_EARNED_PER_WAVE;
		displaySoftCurrency( softCurrencyGranted );

		//XP
		//You always earn XP regardless of whether you won or lost.
		displayXP();	  		

		//Challenge
		//Allow the player to challenge someone to beat his high score.
		displayChallenge();	  		
		#endregion

		//Order by the race position of the players i.e. 1st place, 2nd place, and so forth
		PlayerRace.players = PlayerRace.players.OrderBy( p => p.racePosition ).ToList();

		for(int i=0; i<PlayerRace.players.Count;i++)
		{
			//For each player, create a result entry
			//createResultEntry( PlayerRace.players[i] );
		}

		//we want the footer at the bottom
		//footer.SetAsLastSibling();
	}

	void adjustSizeOfResultsScreen( int playerCount )
	{
		float singleEntryHeight = coopResultPrefab.GetComponent<RectTransform>().sizeDelta.y;
		float spacing = coopResultsHolder.GetComponent<VerticalLayoutGroup>().spacing;
		float desiredHeight = 390f + playerCount * ( singleEntryHeight + spacing );
		coopResultsHolder.sizeDelta = new Vector2( coopResultsHolder.sizeDelta.x, desiredHeight );
	}

	void createResultEntry( PlayerRace playerRace )
	{
		PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( playerRace.name );
		Sprite playerIconSprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( pmd.playerIcon ).icon;
		GameObject go = (GameObject)Instantiate(coopResultPrefab);
		go.transform.SetParent(coopResultsHolder,false);
		go.GetComponent<CoopResultEntry>().configureEntry( pmd.level, pmd.playerName, playerIconSprite, pmd.score, pmd.kills, pmd.downs, pmd.revives );
		go.GetComponent<CoopResultEntry>().emoteGameObject.name = pmd.playerName;
		emotesList.Add( go.GetComponent<CoopResultEntry>().emoteGameObject );
	}

	public void OnClickChallenge()
	{
		Debug.LogWarning("CoopResultsHandler-Challenge mode not implemented yet.");
	}

}
