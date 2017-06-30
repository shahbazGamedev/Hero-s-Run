using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageUI : MonoBehaviour {

	[SerializeField] Image icon;
	[SerializeField] Text mainText;
	[SerializeField] Button acceptButton;
	[SerializeField] Button declineButton;
	[SerializeField] Button playButton;
	[SerializeField] Button closeButton;

	public void configureFriendUI ( ChatMessageType type, string sender, PlayerFriends.FriendData fd = null )
	{
		if( LeanTween.isTweening( gameObject ) ) return;
		
		playButton.gameObject.SetActive( false );

		switch ( type )
		{
			case ChatMessageType.FRIEND_REQUEST_SEND:
				mainText.text = string.Format( "{0} wants to be friend.", sender );
				acceptButton.gameObject.SetActive( true );
				declineButton.gameObject.SetActive( true );
				closeButton.gameObject.SetActive( false );
				acceptButton.onClick.RemoveAllListeners();
				acceptButton.onClick.AddListener(() => OnAcceptFriendRequest( sender, fd ));
				declineButton.onClick.RemoveAllListeners();
				declineButton.onClick.AddListener(() => OnDeclineFriendRequest( sender ));
				slideIn();
			break;

			case ChatMessageType.FRIEND_REQUEST_ACCEPTED:
				mainText.text = string.Format( "You are now friends with {0}.", sender );
				acceptButton.gameObject.SetActive( false );
				declineButton.gameObject.SetActive( false );
				closeButton.gameObject.SetActive( true );
				closeButton.onClick.RemoveAllListeners();
				closeButton.onClick.AddListener(() => OnClose());
				slideIn();
				//Add friend to player's friends and save. This will also get the friend's list in social menu re-populated.
				GameManager.Instance.playerFriends.addFriendAndSave( fd );
				//We want to get status updates from this new friend.
				//To do
			break;

			case ChatMessageType.FRIEND_REQUEST_DECLINED:
				mainText.text = string.Format( "{0} declined your friend request.", sender );
				acceptButton.gameObject.SetActive( false );
				declineButton.gameObject.SetActive( false );
				closeButton.gameObject.SetActive( true );
				closeButton.onClick.RemoveAllListeners();
				closeButton.onClick.AddListener(() => OnClose());
				slideIn();
			break;
		}
	}

	public void configureMatchUI ( ChatMessageType type, string sender )
	{
		if( LeanTween.isTweening( gameObject ) ) return;

		switch ( type )
		{
			case ChatMessageType.MATCH_REQUEST_SEND:
				mainText.text = string.Format( "{0} challenges you to a race.", sender );
				acceptButton.gameObject.SetActive( true );
				declineButton.gameObject.SetActive( true );
				playButton.gameObject.SetActive( false );
				closeButton.gameObject.SetActive( false );
				acceptButton.onClick.RemoveAllListeners();
				acceptButton.onClick.AddListener(() => OnAcceptMatchRequest( sender ));
				declineButton.onClick.RemoveAllListeners();
				declineButton.onClick.AddListener(() => OnDeclineMatchRequest( sender ));
				slideIn();
			break;

			case ChatMessageType.MATCH_REQUEST_ACCEPTED:
				mainText.text = string.Format( "{0} has accepted the challenge.", sender );
				acceptButton.gameObject.SetActive( false );
				declineButton.gameObject.SetActive( false );
				playButton.gameObject.SetActive( true );
				closeButton.gameObject.SetActive( true );
				closeButton.onClick.RemoveAllListeners();
				closeButton.onClick.AddListener(() => OnClickSendNoLongerAvailable( sender ));
				slideIn();
			break;

			case ChatMessageType.MATCH_REQUEST_DECLINED:
				mainText.text = string.Format( "{0} declined your race request.", sender );
				acceptButton.gameObject.SetActive( false );
				declineButton.gameObject.SetActive( false );
				playButton.gameObject.SetActive( false );
				closeButton.gameObject.SetActive( true );
				closeButton.onClick.RemoveAllListeners();
				closeButton.onClick.AddListener(() => OnClose());
				slideIn();
			break;

			case ChatMessageType.MATCH_REQUEST_NO_LONGER_AVAILABLE:
				mainText.text = string.Format( "{0} is no longer available.", sender );
				acceptButton.gameObject.SetActive( false );
				declineButton.gameObject.SetActive( false );
				playButton.gameObject.SetActive( false );
				closeButton.gameObject.SetActive( true );
				closeButton.onClick.RemoveAllListeners();
				closeButton.onClick.AddListener(() => OnClose());
				slideIn();
			break;
		}
	}

	void OnAcceptFriendRequest( string sender, PlayerFriends.FriendData fd )
	{
		Debug.Log( "OnAcceptFriendRequest from " + sender );
		slideOut();
		//Send a message back saying that the friend request has been accepted.
		ChatManager.Instance.chatMessageHandler.sendFriendRequestAcceptedMessage ( sender );
		//Add friend to player's friends and save. This will also get the friend's list in social menu re-populated.
		GameManager.Instance.playerFriends.addFriendAndSave( fd );
		//We want to get status updates from this new friend.
		//To do
	}

	void OnDeclineFriendRequest( string sender )
	{
		Debug.Log( "OnDeclineFriendRequest" );
		slideOut();
		//Send a message back saying that the friend request has been declined.
		ChatManager.Instance.chatMessageHandler.sendFriendRequestDeclinedMessage ( sender );
	}

	void OnAcceptMatchRequest( string sender )
	{
		Debug.Log( "OnAcceptMatchRequest from " + sender );
		slideOut();
		//Send a message back saying that the match request has been accepted.
		//This message will specify the level to play and the room name.
		//These 2 values will be saved in the match data we got from the inviter.
		ChatManager.Instance.chatMessageHandler.sendMatchRequestAcceptedMessage ( sender );
		ChatManager.Instance.chatMessageHandler.startMatch();
	}

	void OnDeclineMatchRequest( string sender )
	{
		Debug.Log( "OnDeclineMatchRequest" );
		slideOut();
		//Send a message back saying that the match request has been declined.
		ChatManager.Instance.chatMessageHandler.sendMatchRequestDeclinedMessage ( sender );
	}

	public void OnClose()
	{
		Debug.Log( "OnClose" );
		slideOut();
	}

	public void OnClickStartMatch( string sender )
	{
		Debug.Log( "OnClickStartMatch" );
		slideOut();
		ChatManager.Instance.chatMessageHandler.startMatch();
	}

	void OnClickSendNoLongerAvailable( string sender )
	{
		Debug.Log( "OnClickSendNoLongerAvailable" );
		slideOut();
		//Send a message back saying that you are no longer available to play.
		ChatManager.Instance.chatMessageHandler.sendMatchRequestNoLongerAvailableMessage ( sender );
	}

	void slideIn()
	{
		LeanTween.moveX( gameObject.GetComponent<RectTransform>(), -512, 0.28f ).setEaseInOutQuad();
	}

	void slideOut()
	{
		LeanTween.moveX( gameObject.GetComponent<RectTransform>(), 512, 0.2f ).setEaseInOutQuad();
	}


}
