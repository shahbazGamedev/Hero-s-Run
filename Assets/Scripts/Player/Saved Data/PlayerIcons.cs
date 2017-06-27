using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class PlayerIcons {

	[SerializeField] List<PlayerIconData> playerIconList = new List<PlayerIconData>();
	
	public void createNewPlayerIcons()
	{
		List<ProgressionManager.IconData> iconList = ProgressionManager.Instance.getPlayerIcons();
		for( int i = 0; i < iconList.Count; i++ )
		{
			addPlayerIcon(  iconList[i].uniqueId, false, !iconList[i].isDefaultIcon );
		}
		serializePlayerIcons( true );
	}

	void addPlayerIcon(  int uniqueId, bool isNew, bool isLocked )
	{
		//Make sure the specified card exists
		if( ProgressionManager.Instance.doesPlayerIconExist( uniqueId ) )
		{
			//Don't add duplicate icons
			if( playerIconList.Exists(playerIconData => playerIconData.uniqueId == uniqueId ) ) return;
	
			PlayerIconData pid = new PlayerIconData();
			pid.uniqueId = uniqueId;
			pid.isNew = isNew;
			pid.isLocked = isLocked;
			playerIconList.Add(pid);
		}
		else
		{
			Debug.LogError("PlayerIcons-addPlayerIcon: The icon you are trying to add does not exist: " + uniqueId );
		}
	}

	//Sort the list starting with the newly unlocked icons
	public List<PlayerIconData> getSortedPlayerIconList()
	{
		return playerIconList.OrderByDescending(data=>data.isNew).ToList();
	}

	public PlayerIconData getPlayerIconDataByUniqueId( int uniqueId )
	{
		return playerIconList.Find(playerIcon => playerIcon.uniqueId == uniqueId);
	}

	public int getNumberOfNewPlayerIcons()
	{
		int counter = 0;
		for( int i = 0; i < playerIconList.Count; i++ )
		{
			if( playerIconList[i].isNew ) counter++;
		}
		return counter;
	}

	public void unlockPlayerIcon( int uniqueId )
	{
		PlayerIconData pid = playerIconList.Find(playerIcon => playerIcon.uniqueId == uniqueId);
		if( pid != null )
		{
			if( !pid.isLocked ) return; //it is already unlocked. Ignore.
			pid.isLocked = false;
			pid.isNew = true;
		}
		else
		{
			Debug.LogWarning("The player icon with id " + uniqueId + " that you want to unlock could not be found." );
		}
	}

	public void serializePlayerIcons( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerIcons( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}

	[System.Serializable]
	public class PlayerIconData
	{
		//Unique ID to identify the player icon.
		//When a player selects a player icon, the unique ID for that icon is saved in player profile.
		public int uniqueId = 0; 
		public bool isNew = false;
		public bool isLocked = true;
		[HideInInspector]
		public RectTransform rectTransform;
	}

}
