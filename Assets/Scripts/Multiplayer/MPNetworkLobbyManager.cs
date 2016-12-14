using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections;
using System.Collections.Generic;

public class MPNetworkLobbyManager : NetworkLobbyManager 
{
	public static MPNetworkLobbyManager mpNetworkLobbyManager;
	public bool disconnectServer = false;
	public ulong currentMatchID;
	public int lobbyPlayerCount = 0;
	public int minimumPlayersToStartMatch = 2;
	[Tooltip("Time in second between all players ready & match start")]
	public float prematchCountdown = 5.0f;

	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex == (int) GameScenes.MultiplayerMatchmaking )
		{
			Debug.Log("MPNetworkLobbyManager-OnLevelFinishedLoading" );
			StartMatchMaker();
		}
	}

	void Start()
	{
		Debug.Log("MPNetworkLobbyManager-Start");
	    mpNetworkLobbyManager = this;
	    DontDestroyOnLoad(gameObject);
		//Ask for a list of matches. This will determine if we need to create a match or not.
		matchMaker.ListMatches( 0, 20, "", false, 0, 0, OnMatchList );
	}

	void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchInfoSnapshotList )
	{
		if( success )
		{
			if( matchInfoSnapshotList.Count == 0)
			{
				Debug.Log("MPNetworkLobbyManager-OnMatchList: No matches found!" );
				//We will therefore create a match. By doing so, we will become the host of the game.
				//When creating a match, you automatically join it.
				matchMaker.CreateMatch( "Match Name 1", (uint)2, true, "", "", "", 0, 0, OnMatchCreate );
			}
			else
			{
				Debug.Log("MPNetworkLobbyManager-OnMatchList: Matches found!" );
				//We found some matches. Let's join the first match that is not full on the list.
				MatchInfoSnapshot matchToJoin = null;
				Debug.Log("List of matches\n");
				for( int i = 0; i < matchInfoSnapshotList.Count; i++ )
				{
					if( matchInfoSnapshotList[i].currentSize < matchInfoSnapshotList[i].maxSize )
					{
						matchToJoin = matchInfoSnapshotList[i];
						break;
					}
				}
				if( matchToJoin == null )
				{
					Debug.LogWarning("MPNetworkLobbyManager-OnMatchList: All matches found are full." );
				}
				else
				{
					matchMaker.JoinMatch(matchToJoin.networkId, "","","",0,0, OnMatchJoined );
				}
			}
		}
		else
		{
			Debug.LogError("MPNetworkLobbyManager-OnMatchList: Error: " + extendedInfo );
		}
	}
	
	public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
	{
		Debug.Log("MPNetworkLobbyManager-OnMatchCreate: Success: " + success );
		base.OnMatchCreate(success, extendedInfo, matchInfo);
		currentMatchID = (System.UInt64)matchInfo.networkId;
	}

	public override void OnLobbyClientEnter()
	{
		Debug.Log("MPNetworkLobbyManager-OnLobbyClientEnter" );
		base.OnLobbyClientEnter();
	}

	public override void OnLobbyServerPlayersReady()
	{
		//Do we have enough players to start the match?
		if( lobbyPlayerCount >=  minimumPlayersToStartMatch )
		{
			bool allready = true;
			for(int i = 0; i < lobbySlots.Length; ++i)
			{
				if(lobbySlots[i] != null)
				allready &= lobbySlots[i].readyToBegin;
			}	
			if(allready)
			{
				Debug.Log("MPNetworkLobbyManager-OnLobbyServerPlayersReady: Loading Play Scene");
				ServerChangeScene(playScene);
			}
		}
		else
		{
			Debug.Log("MPNetworkLobbyManager-OnLobbyServerPlayersReady: We have " + lobbyPlayerCount + " players, but we need " + minimumPlayersToStartMatch );
		}
	}

	public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
	{
		Debug.Log("MPNetworkLobbyManager-OnLobbyServerSceneLoadedForPlayer" );
		//This hook allows you to apply state data from the lobby-player to the game-player.
		GetComponent<MPLobbyHook>().OnLobbyServerSceneLoadedForPlayer(this, lobbyPlayer, gamePlayer);
		return true;
	}

	public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
	{
		base.OnMatchJoined(success, extendedInfo, matchInfo);
		Debug.Log("MPNetworkLobbyManager-OnMatchJoined: Success: " + success);
	}

	public override void OnDestroyMatch(bool success, string extendedInfo)
	{
		Debug.Log("MPNetworkLobbyManager-OnDestroyMatch: Success: " + success);
		base.OnDestroyMatch(success, extendedInfo);
		if (disconnectServer)
		{
			StopMatchMaker();
		}
	}

	void OnEnable()
	{
		//Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
         SceneManager.sceneLoaded += OnLevelFinishedLoading;
     }
 
     void OnDisable()
     {
		//Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
         SceneManager.sceneLoaded -= OnLevelFinishedLoading;
     }
}
