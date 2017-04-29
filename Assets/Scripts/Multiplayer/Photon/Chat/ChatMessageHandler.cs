﻿using System.Collections;
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
	DATA_REQUEST_ASK = 7,
	DATA_REQUEST_RESPOND = 8

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
		//multiplayerLevelndex and roomName are not needed just yet. They will be set when the player accepts the challenge.
		int playerIcon = GameManager.Instance.playerProfile.getPlayerIconId();
		int level = GameManager.Instance.playerProfile.getLevel();
		int prestige = GameManager.Instance.playerProfile.prestigeLevel; 
		MatchData matchData = new MatchData( PlayerStatsManager.Instance.getUserName(), 0, string.Empty, playerIcon, level, prestige );

		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.MATCH_REQUEST_SEND;
		chatMessage.chatMessageContent = matchData.getJson();
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}

	public void sendMatchRequestAcceptedMessage ( string target )
	{
		//Choose a random level
		int multiplayerLevelndex = Random.Range( 0, LevelManager.Instance.getLevelData().getNumberOfMultiplayerLevels() );
		//Set the room name to use
		string roomName = PlayerStatsManager.Instance.getUserName() + "_" + target;
		//Now save these values in the match data we got from the inviter.
		LevelManager.Instance.matchData.multiplayerLevelIndex = multiplayerLevelndex;
		LevelManager.Instance.matchData.roomName = roomName;
		//Send also our player info so that the inviter can populate the matchmaking screen.
		int playerIcon = GameManager.Instance.playerProfile.getPlayerIconId();
		int level = GameManager.Instance.playerProfile.getLevel();
		int prestige = GameManager.Instance.playerProfile.prestigeLevel; 
		MatchData matchData = new MatchData( PlayerStatsManager.Instance.getUserName(), multiplayerLevelndex, roomName, playerIcon, level, prestige );

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
			
			case ChatMessageType.DATA_REQUEST_ASK:
				sendRespondFriendData ( sender );
			break;

			case ChatMessageType.DATA_REQUEST_RESPOND:
				PlayerFriends.FriendData fd3 = JsonUtility.FromJson<PlayerFriends.FriendData>( chatMessage.chatMessageContent );
				fd3.print();
				//Update the friend's list with this information
				GameManager.Instance.playerFriends.updateFriendData( sender, fd3 );
				if( onFriendDataEvent != null ) onFriendDataEvent( sender, fd3 );
			break;
		}
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
		public int multiplayerLevelIndex;
		public string roomName;
		public int playerIcon;
		public int level;
		public int prestige;
	
		public MatchData ( string sender, int multiplayerLevelIndex, string roomName, int playerIcon, int level, int prestige )
		{
			this.sender = sender;
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
			Debug.Log("MatchData-Sender: " + sender + " Multiplayer Level Index: " + multiplayerLevelIndex + " Room Name: " + roomName + " Player Icon: " + playerIcon  + " Level: " + level + " Prestige: " + prestige );
		}
	}
}
