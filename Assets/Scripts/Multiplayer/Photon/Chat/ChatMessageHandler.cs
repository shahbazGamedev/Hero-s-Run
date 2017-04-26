using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChatMessageType {
	FRIEND_REQUEST_SEND = 1,
	FRIEND_REQUEST_ACCEPTED = 2,
	FRIEND_REQUEST_DECLINED = 3
}

public class ChatMessageHandler {

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

	public void parseMessage ( string target, string message )
	{
		ChatMessage chatMessage = JsonUtility.FromJson<ChatMessage>( message );
		Debug.Log("parseMessage-Type: " + (ChatMessageType)chatMessage.chatMessageType );
		switch ( (ChatMessageType)chatMessage.chatMessageType )
		{
			case ChatMessageType.FRIEND_REQUEST_SEND:
				PlayerFriends.FriendData fd1 = JsonUtility.FromJson<PlayerFriends.FriendData>( chatMessage.chatMessageContent );
				fd1.print();
				ChatManager.Instance.chatMessageUI.configureUI( (ChatMessageType)chatMessage.chatMessageType, fd1.userName, fd1 );
			break;

			case ChatMessageType.FRIEND_REQUEST_ACCEPTED:
				PlayerFriends.FriendData fd2 = JsonUtility.FromJson<PlayerFriends.FriendData>( chatMessage.chatMessageContent );
				fd2.print();
				ChatManager.Instance.chatMessageUI.configureUI( (ChatMessageType)chatMessage.chatMessageType, fd2.userName, fd2 );
			break;

			case ChatMessageType.FRIEND_REQUEST_DECLINED:
				ChatManager.Instance.chatMessageUI.configureUI( (ChatMessageType)chatMessage.chatMessageType, target );
			break;

			default:
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
	
}
