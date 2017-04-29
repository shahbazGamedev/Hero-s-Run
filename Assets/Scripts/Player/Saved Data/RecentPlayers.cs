using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class RecentPlayers {

	[SerializeField] List<PlayerFriends.FriendData> recentPlayersList = new List<PlayerFriends.FriendData>();
	//Event management used to notify other classes when friends are added or removed
	public delegate void OnRecentPlayerChangedEvent();
	public static event OnRecentPlayerChangedEvent onRecentPlayerChangedEvent;

	/// <summary>
	/// Creates the dummy friends for testing.
	/// </summary>
	public void createDummyRecentPlayers()
	{
		addDummyRecentPlayer( "Régis", 69 );
		addDummyRecentPlayer( "Marie", 68 );
		addDummyRecentPlayer( "Emma", 24 );
		serializeRecentPlayers( true );
	}

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

	public void addRecentPlayerAndSave( PlayerFriends.FriendData fd )
	{
		//Don't add duplicate recent players
		if( recentPlayersList.Exists(recentPlayerData => recentPlayerData.userName == fd.userName ) ) return;
		//Remove the current friends list @important before adding the one with the added friend.
	//ChatManager.Instance.removeChatFriends();
		recentPlayersList.Add(fd);
		serializeRecentPlayers( true );
		if( onRecentPlayerChangedEvent != null ) onRecentPlayerChangedEvent();
		//Make sure we get online status updates
	//ChatManager.Instance.addChatFriends();
		
	}

	public void addDummyRecentPlayer(  string userName, int level )
	{
		//Don't add duplicate recent players
		if( recentPlayersList.Exists(friendData => friendData.userName == userName ) ) return;

		PlayerFriends.FriendData fd = new PlayerFriends.FriendData( userName, 2, level, 0, 3 );
		recentPlayersList.Add(fd);
	}

}
