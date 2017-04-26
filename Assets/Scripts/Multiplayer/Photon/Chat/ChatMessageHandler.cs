using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChatMessageType {
	FRIEND_REQUEST_SEND = 1,
	FRIEND_REQUEST_ACCEPTED = 2,
	FRIEND_REQUEST_DECLINED = 3,
	MATCH_REQUEST_SEND = 4,
	MATCH_REQUEST_ACCEPTED = 5,
	MATCH_REQUEST_DECLINED = 6

}

public class ChatMessageHandler {

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
		int multiplayerLevelndex = 0;
		string roomName = PlayerStatsManager.Instance.getUserName() + "_" + target;
		int playerIcon = GameManager.Instance.playerProfile.getPlayerIconId();
		int level = GameManager.Instance.playerProfile.getLevel();
		int prestige = GameManager.Instance.playerProfile.prestigeLevel; 
		MatchData matchData = new MatchData( multiplayerLevelndex, roomName, playerIcon, level, prestige );

		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.MATCH_REQUEST_SEND;
		chatMessage.chatMessageContent = matchData.getJson();
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}

	public void sendMatchRequestAcceptedMessage ( string target )
	{
		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.MATCH_REQUEST_ACCEPTED;
		chatMessage.chatMessageContent = string.Empty;
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}

	public void sendMatchRequestDeclinedMessage ( string target )
	{
		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.MATCH_REQUEST_DECLINED;
		chatMessage.chatMessageContent = string.Empty;
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
				MatchData md = JsonUtility.FromJson<MatchData>( chatMessage.chatMessageContent );
				md.print();
				ChatManager.Instance.chatMessageUI.configureMatchUI( (ChatMessageType)chatMessage.chatMessageType, sender, md );
				md.sender = sender;
				LevelManager.Instance.matchData = md;

			break;

			case ChatMessageType.MATCH_REQUEST_ACCEPTED:
				ChatManager.Instance.chatMessageUI.configureMatchUI( (ChatMessageType)chatMessage.chatMessageType, sender );
			break;

			case ChatMessageType.MATCH_REQUEST_DECLINED:
				ChatManager.Instance.chatMessageUI.configureMatchUI( (ChatMessageType)chatMessage.chatMessageType, sender );
			break;
		}
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
		public int multiplayerLevelIndex;
		public string roomName;
		public int playerIcon;
		public int level;
		public int prestige;
	
		public MatchData ( int multiplayerLevelIndex, string roomName, int playerIcon, int level, int prestige )
		{
			this.sender = string.Empty;
			this.multiplayerLevelIndex = multiplayerLevelIndex;
			this.roomName = roomName;
			this.playerIcon = playerIcon;
			this.level = level;
			this.prestige = prestige;
		}

		public string getJson()
		{
			return JsonUtility.ToJson( this );
		}

		public void print()
		{
			Debug.Log("MatchData-Sender: " + sender + " Multiplayer Level Index: " + multiplayerLevelIndex + " Rroom Name: " + roomName + " Player Icon: " + playerIcon  + " Level: " + level + " Prestige: " + prestige );
		}
	}
}
