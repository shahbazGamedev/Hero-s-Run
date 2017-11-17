using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

public class CoopResultsScreenHandler : MonoBehaviour {

	[SerializeField] RectTransform coopResultsHolder;
	[SerializeField] GameObject coopResultPrefab;
	[SerializeField] TextMeshProUGUI highestWavesBeaten;
	public List<GameObject> emotesList = new List<GameObject>();

	public void showResults()
	{
		int wavesBeaten = ZombieManager.numberOfZombieWavesTriggered - 1;
		if( wavesBeaten < 0 ) wavesBeaten = 0;
		string wavesBeatenString = LocalizationManager.Instance.getText( "COOP_RESULTS_WAVES_BEATEN" );
		wavesBeatenString = string.Format( wavesBeatenString, wavesBeaten );
		highestWavesBeaten.text = wavesBeatenString;

		adjustSizeOfResultsScreen( PlayerRace.players.Count );

		//Order by the race position of the players i.e. 1st place, 2nd place, and so forth
		PlayerRace.players = PlayerRace.players.OrderBy( p => p.racePosition ).ToList();

		for(int i=0; i<PlayerRace.players.Count;i++)
		{
			//For each player, create a result entry
			createResultEntry( PlayerRace.players[i] );
		}
	}

	void adjustSizeOfResultsScreen( int playerCount )
	{
		float titleHeight = coopResultPrefab.GetComponent<RectTransform>().sizeDelta.y; //It has the same height as a result entry
		float wavesBeatenHeight = coopResultPrefab.GetComponent<RectTransform>().sizeDelta.y; //It has the same height as a result entry
		float singleEntryHeight = coopResultPrefab.GetComponent<RectTransform>().sizeDelta.y;
		float spacing = GetComponent<VerticalLayoutGroup>().spacing;
		float desiredHeight = titleHeight + wavesBeatenHeight + playerCount * ( singleEntryHeight + spacing );
		coopResultsHolder.sizeDelta = new Vector2( coopResultsHolder.sizeDelta.x, desiredHeight );
	}

	//Note that win streak is not used for now. An animated flame will be used in the future to convey the win streak, this is why I am keeping the code.
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
		GameObject go = (GameObject)Instantiate(coopResultPrefab);
		go.transform.SetParent(coopResultsHolder,false);
		go.GetComponent<CoopResultEntry>().configureEntry( pmd.level, pmd.playerName, playerIconSprite, UnityEngine.Random.Range(0, 100000).ToString("N0") );
		go.GetComponent<CoopResultEntry>().emoteGameObject.name = pmd.playerName;
		emotesList.Add( go.GetComponent<CoopResultEntry>().emoteGameObject );
	}

	public GameObject getEmoteGameObjectForPlayerNamed( string playerName )
	{
		GameObject emote = emotesList.Find( go => go.name == playerName);
		if ( emote == null ) Debug.LogError("CoopResultsScreenHandler-could not find emote game object for player " + playerName );
		return emote;
	}
}
