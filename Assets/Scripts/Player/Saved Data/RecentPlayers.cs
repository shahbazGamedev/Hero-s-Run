using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class RecentPlayers {

	const int MAX_NUMBER_OF_RECENT_PLAYERS = 8;
	[SerializeField] List<PlayerFriends.FriendData> recentPlayersList = new List<PlayerFriends.FriendData>();
	//Event management used to notify other classes when friends are added or removed
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
			currentRecentPlayerData.prestige = updatedRecentPlayerData.prestige;
			currentRecentPlayerData.currentWinStreak = updatedRecentPlayerData.currentWinStreak;
		}
		else
		{
			Debug.LogError("RecentPlayers-updateRecentPlayerData: The user name specified " + userName + " is not in the recent player's list.");
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
		}
		else
		{
			Debug.LogError("RecentPlayers-updateStatus: The user name specified " + userName + " is not in the recent player's list.");
		}
	}

	public void serializeRecentPlayers( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setRecentPlayers( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}

	public void addRecentPlayer( PlayerFriends.FriendData fd )
	{
		if( fd == null ) return;

		//We want the most recent players displayed on top

		//If the player is already in the list, move him to the top
		if( recentPlayersList.Exists(recentPlayerData => recentPlayerData.userName == fd.userName ) )
		{
			PlayerFriends.FriendData current = recentPlayersList.Find(recentPlayerData => recentPlayerData.userName == fd.userName );
			int index = recentPlayersList.IndexOf( current );
			recentPlayersList.RemoveAt( index );
		}
		else if( recentPlayersList.Count == MAX_NUMBER_OF_RECENT_PLAYERS )
		{
			//Our list is full and doesn't contain this player. Remove the oldest entry. Add the most recent entry. We keep the MAX_NUMBER_OF_RECENT_PLAYERS most recent.
			recentPlayersList.RemoveAt( MAX_NUMBER_OF_RECENT_PLAYERS - 1 );
		}
		recentPlayersList.Insert( 0, fd);

		if( onRecentPlayerChangedEvent != null ) onRecentPlayerChangedEvent();
		Debug.LogWarning( "RecentPlayers-addRecentPlayer: " + fd.userName );
	}

	public void addDummyRecentPlayer(  string userName, int level )
	{
		//Don't add duplicate recent players
		if( recentPlayersList.Exists(friendData => friendData.userName == userName ) ) return;

		PlayerFriends.FriendData fd = new PlayerFriends.FriendData( userName, 2, level, 0, 3 );
		recentPlayersList.Add(fd);
	}

}
