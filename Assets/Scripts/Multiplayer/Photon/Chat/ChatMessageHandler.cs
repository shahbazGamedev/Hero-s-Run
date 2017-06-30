using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ChatMessageType {
	FRIEND_REQUEST_SEND = 1,
	FRIEND_REQUEST_ACCEPTED = 2,
	FRIEND_REQUEST_DECLINED = 3,
	MATCH_REQUEST_SEND = 4,
	MATCH_REQUEST_ACCEPTED = 5,
	MATCH_REQUEST_DECLINED = 6,
	MATCH_REQUEST_NO_LONGER_AVAILABLE = 7,
	DATA_REQUEST_ASK = 8,
	DATA_REQUEST_RESPOND = 9

}

public class ChatMessageHandler {

	//Event management used to notify other classes when we received updated friend data
	public delegate void OnFriendDataEvent( string userName, PlayerFriends.FriendData friendData );
	public static event OnFriendDataEvent onFriendDataEvent;

	#region Friend Request
	public void sendAddFriendMessage ( string target )
	{
		string playerFriendData = GameManager.Instance.playerFriends.getMyFriendData().getJson();
		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.FRIEND_REQUEST_SEND;
		chatMessage.chatMessageContent = playerFriendData;
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}

	public void sendFriendRequestAcceptedMessage ( string target )
	{
		string playerFriendData = GameManager.Instance.playerFriends.getMyFriendData().getJson();
		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.FRIEND_REQUEST_ACCEPTED;
		chatMessage.chatMessageContent = playerFriendData;
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}

	public void sendFriendRequestDeclinedMessage ( string target )
	{
		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.FRIEND_REQUEST_DECLINED;
		chatMessage.chatMessageContent = string.Empty;
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}
	#endregion

	#region Race Me
	public void sendMatchRequestMessage ( string target )
	{
		string raceTrackName = LevelManager.Instance.getLevelData().getRaceTrackByTrophies().circuitInfo.raceTrackName;
		string roomName = GameManager.Instance.playerProfile.getUserName() + "_" + target;
		int playerIcon = GameManager.Instance.playerProfile.getPlayerIconId();
		int level = GameManager.Instance.playerProfile.getLevel();
		int prestige = GameManager.Instance.playerProfile.prestigeLevel; 
		int currentWinStreak = GameManager.Instance.playerStatistics.getStatisticData(StatisticDataType.CURRENT_WIN_STREAK);
		MatchData matchData = new MatchData( GameManager.Instance.playerProfile.getUserName(), raceTrackName, roomName, playerIcon, level, prestige, currentWinStreak );

		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.MATCH_REQUEST_SEND;
		chatMessage.chatMessageContent = matchData.getJson();
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}

	public void sendMatchRequestAcceptedMessage ( string target )
	{
		//Send also our player info so that the inviter can populate the matchmaking screen.
		int playerIcon = GameManager.Instance.playerProfile.getPlayerIconId();
		int level = GameManager.Instance.playerProfile.getLevel();
		int prestige = GameManager.Instance.playerProfile.prestigeLevel; 
		int currentWinStreak = GameManager.Instance.playerStatistics.getStatisticData(StatisticDataType.CURRENT_WIN_STREAK);
		MatchData matchData = new MatchData( GameManager.Instance.playerProfile.getUserName(), LevelManager.Instance.matchData.raceTrackName, LevelManager.Instance.matchData.roomName, playerIcon, level, prestige, currentWinStreak );

		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.MATCH_REQUEST_ACCEPTED;
		chatMessage.chatMessageContent = matchData.getJson();
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}

	public void sendMatchRequestDeclinedMessage ( string target )
	{
		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.MATCH_REQUEST_DECLINED;
		chatMessage.chatMessageContent = string.Empty;
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}

	public void sendMatchRequestNoLongerAvailableMessage ( string target )
	{
		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.MATCH_REQUEST_NO_LONGER_AVAILABLE;
		chatMessage.chatMessageContent = string.Empty;
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}
	#endregion

	#region Friend Data Request
	public void sendAskFriendData ( string target )
	{
		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.DATA_REQUEST_ASK;
		chatMessage.chatMessageContent = string.Empty;
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}

	public void sendRespondFriendData ( string target )
	{
		string playerFriendData = GameManager.Instance.playerFriends.getMyFriendData().getJson();
		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.DATA_REQUEST_RESPOND;
		chatMessage.chatMessageContent = playerFriendData;
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}
	#endregion

	public void parseMessage ( string sender, string message )
	{
		ChatMessage chatMessage = JsonUtility.FromJson<ChatMessage>( message );
		Debug.Log("parseMessage-Type: " + (ChatMessageType)chatMessage.chatMessageType );
		switch ( (ChatMessageType)chatMessage.chatMessageType )
		{
			case ChatMessageType.FRIEND_REQUEST_SEND:
				PlayerFriends.FriendData fd1 = JsonUtility.FromJson<PlayerFriends.FriendData>( chatMessage.chatMessageContent );
				fd1.print();
				ChatManager.Instance.chatMessageUI.configureFriendUI( (ChatMessageType)chatMessage.chatMessageType, fd1.userName, fd1 );
			break;

			case ChatMessageType.FRIEND_REQUEST_ACCEPTED:
				PlayerFriends.FriendData fd2 = JsonUtility.FromJson<PlayerFriends.FriendData>( chatMessage.chatMessageContent );
				fd2.print();
				ChatManager.Instance.chatMessageUI.configureFriendUI( (ChatMessageType)chatMessage.chatMessageType, fd2.userName, fd2 );
			break;

			case ChatMessageType.FRIEND_REQUEST_DECLINED:
				ChatManager.Instance.chatMessageUI.configureFriendUI( (ChatMessageType)chatMessage.chatMessageType, sender );
			break;

			case ChatMessageType.MATCH_REQUEST_SEND:
				MatchData md1 = JsonUtility.FromJson<MatchData>( chatMessage.chatMessageContent );
				md1.print();
				ChatManager.Instance.chatMessageUI.configureMatchUI( (ChatMessageType)chatMessage.chatMessageType, sender );
				LevelManager.Instance.matchData = md1;

			break;

			case ChatMessageType.MATCH_REQUEST_ACCEPTED:
				ChatManager.Instance.chatMessageUI.configureMatchUI( (ChatMessageType)chatMessage.chatMessageType, sender );
				MatchData md2 = JsonUtility.FromJson<MatchData>( chatMessage.chatMessageContent );
				md2.print();
				LevelManager.Instance.matchData = md2;
			break;

			case ChatMessageType.MATCH_REQUEST_DECLINED:
				ChatManager.Instance.chatMessageUI.configureMatchUI( (ChatMessageType)chatMessage.chatMessageType, sender );
			break;
			
			case ChatMessageType.MATCH_REQUEST_NO_LONGER_AVAILABLE:
				ChatManager.Instance.chatMessageUI.configureMatchUI( (ChatMessageType)chatMessage.chatMessageType, sender );
			break;

			case ChatMessageType.DATA_REQUEST_ASK:
				sendRespondFriendData ( sender );
			break;

			case ChatMessageType.DATA_REQUEST_RESPOND:
				PlayerFriends.FriendData fd3 = JsonUtility.FromJson<PlayerFriends.FriendData>( chatMessage.chatMessageContent );
				fd3.print();
				//Update the friend's list with this information
				updateFriendData( sender, fd3 );
			break;
		}
	}

	public void updateFriendData( string sender, PlayerFriends.FriendData friendData )
	{
		GameManager.Instance.playerFriends.updateFriendData( sender, friendData );
		GameManager.Instance.recentPlayers.updateRecentPlayerData( sender, friendData );
		if( onFriendDataEvent != null ) onFriendDataEvent( sender, friendData );
	}

	public void startMatch()
	{
		//Since we are skipping the PlayModes scene, make sure we set the play mode.
		GameManager.Instance.setPlayMode(PlayMode.PlayWithFriends);
		SceneManager.LoadScene( (int) GameScenes.Matchmaking );
	}

	[System.Serializable]
	public class ChatMessage
	{
		public ChatMessageType chatMessageType;
		public string chatMessageContent;

		public string getJson()
		{
			return JsonUtility.ToJson( this );
		}
	}

	[System.Serializable]
	public class MatchData
	{
		public string sender;
		public string raceTrackName;
		public string roomName;
		public int playerIcon;
		public int level;
		public int prestige;
		public int currentWinStreak;
	
		public MatchData ( string sender, string raceTrackName, string roomName, int playerIcon, int level, int prestige, int currentWinStreak )
		{
			this.sender = sender;
			this.raceTrackName = raceTrackName;
			this.roomName = roomName;
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
			Debug.Log("MatchData-Sender: " + sender + " Race Track Name: " + raceTrackName + " Room Name: " + roomName + " Player Icon: " + playerIcon  + " Level: " + level + " Prestige: " + prestige + " Current Win Streak: " + currentWinStreak );
		}
	}
}
