using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
/// <summary>
/// Player deck.
/// </summary>
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
		onlineFriendDataList.Add(ofd);
	}

	[System.Serializable]
	public class OnlineFriendData
	{
		public string userName { get; set; }
		public int level { get; set; }
	}

}
