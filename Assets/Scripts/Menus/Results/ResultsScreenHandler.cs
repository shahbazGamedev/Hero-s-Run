using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Apple.ReplayKit;

public class ResultsScreenHandler : MonoBehaviour, IPointerDownHandler {

	[SerializeField] RectTransform resultsHolder;
	[SerializeField] GameObject resultPrefab;
	public List<GameObject> emotesList = new List<GameObject>();

	public void showResults()
	{
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
		float titleHeight = resultPrefab.GetComponent<RectTransform>().sizeDelta.y; //It has the same height as a result entry
		float singleEntryHeight = resultPrefab.GetComponent<RectTransform>().sizeDelta.y;
		float spacing = GetComponent<VerticalLayoutGroup>().spacing;
		float desiredHeight = titleHeight + playerCount * ( singleEntryHeight + spacing );
		resultsHolder.sizeDelta = new Vector2( resultsHolder.sizeDelta.x, desiredHeight );
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
		GameObject go = (GameObject)Instantiate(resultPrefab);
		go.transform.SetParent(resultsHolder,false);
		go.GetComponent<ResultEntry>().configureEntry( racePosition + 1, pmd.level, pmd.playerName, playerIconSprite, raceDurationString );
		go.GetComponent<ResultEntry>().emoteGameObject.name = pmd.playerName;
		emotesList.Add( go.GetComponent<ResultEntry>().emoteGameObject );
	}

	public GameObject getEmoteGameObjectForPlayerNamed( string playerName )
	{
		GameObject emote = emotesList.Find( go => go.name == playerName);
		if ( emote == null ) Debug.LogError("ResultsScreenHandler-could not find emote game object for player " + playerName );
		return emote;
	}

	//The player can click on the coop results screen to dismiss it immediately.
    public void OnPointerDown(PointerEventData data)
    {
		StartCoroutine( exitNow() );
    }

    IEnumerator  exitNow()
    {
 		#if UNITY_IOS
		try
		{
			if( ReplayKit.isRecording ) ReplayKit.StopRecording();
		}
   		catch (Exception e)
		{
			Debug.LogError( "Replay exception: " +  e.ToString() + " ReplayKit.lastError: " + ReplayKit.lastError );
    	}
		yield return new WaitForEndOfFrame();
		#endif
		PhotonNetwork.LeaveRoom();
   }

}
