﻿using UnityEngine;
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
	public MPLobbyMenu mpLobbyMenu;
	public MatchInfo hostedMatchInfo = null;
	public MatchInfo joinedMatchInfo = null;
	public int lobbyPlayerCount = 0;
	public int minimumPlayersToStartMatch = 2;

	void Start()
	{
		Debug.Log("\nENTERING MULTIPLAYER");
	    mpNetworkLobbyManager = this;
	    DontDestroyOnLoad(gameObject);
	}

	public void startMatch()
	{
		//Player is connected to the Internet
		if( Application.internetReachability != NetworkReachability.NotReachable )
		{
			StartMatchMaker();
			//Ask for a list of matches. This will determine if we need to create a match or not.
			matchMaker.ListMatches( 0, 20, "", false, 0, 0, OnMatchList );
		}
		else
		{
			mpLobbyMenu.showNoInternetPopup();
		}
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
			Debug.LogWarning("MPNetworkLobbyManager-OnMatchList: Error: " + extendedInfo );
			if( extendedInfo.Contains("Connection time-out") ) mpLobbyMenu.showConnectionTimedOut();
		}
	}
	
	public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
	{
		base.OnMatchCreate(success, extendedInfo, matchInfo);
		if( success )
		{
			Debug.Log("MPNetworkLobbyManager-OnMatchCreate: Success" );
			hostedMatchInfo = matchInfo;
		}
		else
		{
			Debug.LogWarning("MPNetworkLobbyManager-OnMatchCreate: Error: " + extendedInfo );
			mpLobbyMenu.showUnableToCreateMatch();
		}
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
				Debug.Log("MPNetworkLobbyManager-OnLobbyServerPlayersReady: Loading Play Scene\n");
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
		if( success )
		{
			Debug.Log("MPNetworkLobbyManager-OnMatchJoined: Success" );
			joinedMatchInfo = matchInfo;
		}
		else
		{
			Debug.LogWarning("MPNetworkLobbyManager-OnMatchJoined: Error: " + extendedInfo );
			mpLobbyMenu.showUnableToJoinMatch();
		}
	}

	public void cleanUpOnExit()
	{
		if( hostedMatchInfo != null )
		{
			matchMaker.DestroyMatch( hostedMatchInfo.networkId, 0, OnDestroyMatch );
			StopHost();
			hostedMatchInfo = null;
			lobbyPlayerCount = 0;
		}
		else if( joinedMatchInfo != null )
		{
			matchMaker.DropConnection(joinedMatchInfo.networkId, joinedMatchInfo.nodeId, 0, OnDropConnection );
			joinedMatchInfo = null;
			if( lobbyPlayerCount > 0 )lobbyPlayerCount--;
		}
	}

	public override void OnDestroyMatch(bool success, string extendedInfo)
	{
		Debug.Log("MPNetworkLobbyManager-OnDestroyMatch: Success: " + success);
		base.OnDestroyMatch(success, extendedInfo);
		StopMatchMaker();
	}

	public override void OnDropConnection(bool success, string extendedInfo)
	{
		Debug.Log("MPNetworkLobbyManager-OnDropConnection: Success: " + success);
		base.OnDropConnection(success, extendedInfo);
		StopMatchMaker();
	}

}
