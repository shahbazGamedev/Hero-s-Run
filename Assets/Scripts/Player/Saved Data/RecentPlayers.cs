using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class RecentPlayers {

	const int MAX_NUMBER_OF_RECENT_PLAYERS = 8;
	[SerializeField] List<PlayerFriends.FriendData> recentPlayersList = new List<PlayerFriends.FriendData>();
	//Event management used to notify other classes when a recent player is added or removed
	public delegate void OnRecentPlayerChangedEvent();
	public static event OnRecentPlayerChangedEvent onRecentPlayerChangedEvent;

	public List<PlayerFriends.FriendData> getRecentPlayersList()
	{
		return recentPlayersList;
	}

	public void updateRecentPlayerData( string userName, PlayerFriends.FriendData updatedRecentPlayerData )
	{
		if( recentPlayersList.Exists( fd => fd.userName == userName ) )
		{
			PlayerFriends.FriendData currentRecentPlayerData = recentPlayersList.Find( fd => fd.userName == userName );
			currentRecentPlayerData.playerIcon = updatedRecentPlayerData.playerIcon;
			currentRecentPlayerData.level = updatedRecentPlayerData.level;
			currentRecentPlayerData.currentWinStreak = updatedRecentPlayerData.currentWinStreak;
		}
		else
		{
			Debug.LogWarning("RecentPlayers-updateRecentPlayerData: The user name specified " + userName + " is not in the recent player's list.");
		}
	}

	public string[] getRecentPlayersNames()
	{
		string[] recentPlayersNamesArray = new string[recentPlayersList.Count];
		for( int i = 0; i < recentPlayersList.Count; i++ )
		{
			recentPlayersNamesArray[i] = recentPlayersList[i].userName;
		}
		return recentPlayersNamesArray;
	}

	public void updateStatus( string userName, int status )
	{
		if( recentPlayersList.Exists(ofd => ofd.userName == userName ) )
		{
			recentPlayersList.Find(ofd => ofd.userName == userName ).status = status;
			Debug.Log("RecentPlayers-updateStatus: " + userName + " was found and the status is " + recentPlayersList.Find(ofd => ofd.userName == userName ).status);
		}
		else
		{
			Debug.Log("RecentPlayers-updateStatus: The user name specified " + userName + " is not in the recent player's list.");
		}
	}

	void serializeRecentPlayers( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setRecentPlayers( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}
	
	//This list contains the data for the last 8 players that were raced against.
	//It is sorted from the most recent to the least recent.
	public void addRecentPlayer( PlayerFriends.FriendData fd )
	{
		if( fd == null ) return;

		//If the player is already in the list, move him to the top.
		if( recentPlayersList.Exists(recentPlayerData => recentPlayerData.userName == fd.userName ) )
		{
			PlayerFriends.FriendData current = recentPlayersList.Find(recentPlayerData => recentPlayerData.userName == fd.userName );
			int index = recentPlayersList.IndexOf( current );
			recentPlayersList.RemoveAt( index );
		}
		else if( recentPlayersList.Count == MAX_NUMBER_OF_RECENT_PLAYERS )
		{
			//Our list is full and it doesn't contain this player.
			//Remove the oldest entry.
			recentPlayersList.RemoveAt( MAX_NUMBER_OF_RECENT_PLAYERS - 1 );
		}
		//Add the entry.
		recentPlayersList.Insert( 0, fd);

		//Make sure we get online status updates for this recent player
		ChatManager.Instance.addChatFriend( fd.userName );

		//Save
		serializeRecentPlayers( true );

		if( onRecentPlayerChangedEvent != null ) onRecentPlayerChangedEvent();
		Debug.LogWarning( "RecentPlayers-addRecentPlayer: " + fd.userName );
	}

	void addDummyRecentPlayer(  string userName, int level )
	{
		//Don't add duplicate recent players
		if( recentPlayersList.Exists(friendData => friendData.userName == userName ) ) return;

		PlayerFriends.FriendData fd = new PlayerFriends.FriendData( userName, 2, level, 3 );
		recentPlayersList.Add(fd);
	}

}
