using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageUI : MonoBehaviour {

	[SerializeField] Image icon;
	[SerializeField] Text mainText;
	[SerializeField] Button acceptButton;
	[SerializeField] Button declineButton;

	public void configureUI ( ChatMessageType type, string sender, PlayerFriends.FriendData fd = null )
	{
		switch ( type )
		{
			case ChatMessageType.FRIEND_REQUEST_SEND:
				mainText.text = string.Format( "{0} wants to add you as a friend.", sender );
				acceptButton.gameObject.SetActive( true );
				declineButton.gameObject.SetActive( true );
				acceptButton.onClick.RemoveAllListeners();
				acceptButton.onClick.AddListener(() => OnAcceptFriendRequest( type, sender, fd ));
				declineButton.onClick.RemoveAllListeners();
				declineButton.onClick.AddListener(() => OnDeclineFriendRequest( type, sender, fd ));
				gameObject.SetActive( true );
			break;

			case ChatMessageType.FRIEND_REQUEST_ACCEPTED:
				mainText.text = string.Format( "You are now friends with {0}.", sender );
				acceptButton.gameObject.SetActive( false );
				declineButton.gameObject.SetActive( false );
				gameObject.SetActive( true );
			break;

			default:
			break;
		}
	}

	void OnAcceptFriendRequest( ChatMessageType type, string sender, PlayerFriends.FriendData fd )
	{
		Debug.Log( "OnAcceptFriendRequest from " + sender );
		gameObject.SetActive( false );
		//Send a message back saying that the friend request has been accepted.
		ChatManager.Instance.chatMessageHandler.sendFriendRequestAcceptedMessage ( sender );
		//Add friend to player's friends and save. This will also get the friend's list in social menu re-populated.
		GameManager.Instance.playerFriends.addFriendAndSave( fd );
		//We want to get status updates from this new friend.

		}

	void OnDeclineFriendRequest( ChatMessageType type, string sender, PlayerFriends.FriendData fd )
	{
		Debug.Log( "OnDeclineFriendRequest" );
		gameObject.SetActive( false );
	}

	public void OnClose()
	{
		Debug.Log( "OnClose" );
		gameObject.SetActive( false );
	}

}
