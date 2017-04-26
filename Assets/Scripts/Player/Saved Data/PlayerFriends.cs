using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class PlayerFriends {

	[SerializeField] List<FriendData> onlineFriendDataList = new List<FriendData>();
	//Event management used to notify other classes when friends are added or removed
	public delegate void OnFriendChangedEvent();
	public static event OnFriendChangedEvent onFriendChangedEvent;

	/// <summary>
	/// Creates the dummy friends for testing.
	/// </summary>
	public void createDummyFriends()
	{
		addDummyFriend( "Régis", 69 );
		addDummyFriend( "Marie", 68 );
		addDummyFriend( "Emma", 24 );
		serializePlayerFriends( true );
	}

	public List<FriendData> getFriendList()
	{
		return onlineFriendDataList;
	}

	public string[] getFriendNames()
	{
		string[] friendNamesArray = new string[onlineFriendDataList.Count];
		for( int i = 0; i < onlineFriendDataList.Count; i++ )
		{
			friendNamesArray[i] = onlineFriendDataList[i].userName;
		}
		return friendNamesArray;
	}

	public void updateStatus( string userName, int status )
	{
		if( onlineFriendDataList.Exists(ofd => ofd.userName == userName ) )
		{
			onlineFriendDataList.Find(ofd => ofd.userName == userName ).status = status;
		}
		else
		{
			Debug.LogError("PlayerFriends-updateStatus: The user name specified " + userName + " is not in the friend's list.");
		}
	}

	public void serializePlayerFriends( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerFriends( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}

	public void addFriendAndSave( FriendData fd )
	{
		//Don't add duplicate friends
		if( onlineFriendDataList.Exists(friendData => friendData.userName == fd.userName ) ) return;
		onlineFriendDataList.Add(fd);
		serializePlayerFriends( true );
		if( onFriendChangedEvent != null ) onFriendChangedEvent();
	}

	public void addDummyFriend(  string userName, int level )
	{
		//Don't add duplicate friends
		if( onlineFriendDataList.Exists(friendData => friendData.userName == userName ) ) return;

		FriendData fd = new FriendData( userName, 2, level, 0, 3 );
		onlineFriendDataList.Add(fd);
	}

	/// <summary>
	/// Gets the friend data for the player.
	/// </summary>
	/// <returns>The player's friend data.</returns>
	public FriendData getMyFriendData()
	{
		PlayerFriends.FriendData fd = new PlayerFriends.FriendData(
			PlayerStatsManager.Instance.getUserName(),
			GameManager.Instance.playerProfile.getPlayerIconId(), 
			GameManager.Instance.playerProfile.getLevel(), 
			GameManager.Instance.playerProfile.prestigeLevel, 
			GameManager.Instance.playerStatistics.currentWinStreak );
		return fd;
	}

	[System.Serializable]
	public class FriendData
	{
		public string userName;
		[System.NonSerialized]
		public int status;
		public int playerIcon;
		public int level;
		public int prestige;
		public int currentWinStreak;

		public FriendData ( string userName, int playerIcon, int level, int prestige, int currentWinStreak )
		{
			this.userName = userName;
			this.playerIcon = playerIcon;
			this.level = level;
			this.prestige = prestige;
			this.currentWinStreak = currentWinStreak;
		}

		public string getJson()
		{
			return JsonUtility.ToJson( this );
		}

		public void print()
		{
			Debug.Log("FriendData-User Name: " + userName + " Status: " + status + " Player Icon: " + playerIcon  + " Level: " + level + " Prestige: " + prestige + " Current Win Streak: " + currentWinStreak );
		}
	}

}
