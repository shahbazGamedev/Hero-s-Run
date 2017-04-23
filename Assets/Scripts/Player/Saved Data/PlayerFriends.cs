using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class PlayerFriends {

	[SerializeField] List<OnlineFriendData> onlineFriendDataList = new List<OnlineFriendData>();

	public void createNewPlayerFriends()
	{
		addFriend( "Régis", 69 );
		addFriend( "Marie", 68 );
		addFriend( "Emma", 24 );
		serializePlayerFriends( true );
	}

	public List<OnlineFriendData> getFriendList()
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

	public void addFriend(  string userName, int level )
	{
		//Don't add duplicate friends
		if( onlineFriendDataList.Exists(friendData => friendData.userName == userName ) ) return;

		OnlineFriendData ofd = new OnlineFriendData();
		ofd.userName = userName;
		ofd.level = level;
		ofd.status = 0;
		onlineFriendDataList.Add(ofd);
	}

	[System.Serializable]
	public class OnlineFriendData
	{
		public string userName;
		public int level;
		public int status;

		public void print()
		{
			Debug.Log("OnlineFriendData-userName: " + userName + " Level: " + level + " Status " + status );
		}
	}

}
