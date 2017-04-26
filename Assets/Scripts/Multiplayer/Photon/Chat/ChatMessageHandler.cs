using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatMessageHandler {

	public void sendAddFriendMessage ( string target )
	{
		string playerFriendData = GameManager.Instance.playerFriends.getMyFriendData().getJson();
		ChatMessage chatMessage = new ChatMessage();
		chatMessage.chatMessageType = ChatMessageType.ADD_FRIEND;
		chatMessage.chatMessageContent = playerFriendData;
		ChatManager.Instance.sendPrivateMessage( target, chatMessage.getJson() );
	}

	public void parseMessage ( string target, string message )
	{
		ChatMessage chatMessage = JsonUtility.FromJson<ChatMessage>( message );
		Debug.Log("parseMessage-Type: " + (ChatMessageType)chatMessage.chatMessageType );
		switch ( (ChatMessageType)chatMessage.chatMessageType )
		{
			case ChatMessageType.ADD_FRIEND:
				PlayerFriends.FriendData fd = JsonUtility.FromJson<PlayerFriends.FriendData>( chatMessage.chatMessageContent );
				fd.print();
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
