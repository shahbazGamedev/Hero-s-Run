using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChatMessageUI : MonoBehaviour {

	[SerializeField] Image icon;
	[SerializeField] Text mainText;
	[SerializeField] Button acceptButton;
	[SerializeField] Button declineButton;

	public void configureFriendUI ( ChatMessageType type, string sender, PlayerFriends.FriendData fd = null )
	{
		switch ( type )
		{
			case ChatMessageType.FRIEND_REQUEST_SEND:
				mainText.text = string.Format( "{0} wants to be friend.", sender );
				acceptButton.gameObject.SetActive( true );
				declineButton.gameObject.SetActive( true );
				acceptButton.onClick.RemoveAllListeners();
				acceptButton.onClick.AddListener(() => OnAcceptFriendRequest( sender, fd ));
				declineButton.onClick.RemoveAllListeners();
				declineButton.onClick.AddListener(() => OnDeclineFriendRequest( sender ));
				gameObject.SetActive( true );
			break;

			case ChatMessageType.FRIEND_REQUEST_ACCEPTED:
				mainText.text = string.Format( "You are now friends with {0}.", sender );
				acceptButton.gameObject.SetActive( false );
				declineButton.gameObject.SetActive( false );
				gameObject.SetActive( true );
				//Add friend to player's friends and save. This will also get the friend's list in social menu re-populated.
				GameManager.Instance.playerFriends.addFriendAndSave( fd );
				//We want to get status updates from this new friend.
				//To do
			break;

			case ChatMessageType.FRIEND_REQUEST_DECLINED:
				mainText.text = string.Format( "{0} declined your friend request.", sender );
				acceptButton.gameObject.SetActive( false );
				declineButton.gameObject.SetActive( false );
				gameObject.SetActive( true );
			break;
		}
	}

	public void configureMatchUI ( ChatMessageType type, string sender, ChatMessageHandler.MatchData md = null )
	{
		switch ( type )
		{
			case ChatMessageType.MATCH_REQUEST_SEND:
				mainText.text = string.Format( "{0} challenges you to a race.", sender );
				acceptButton.gameObject.SetActive( true );
				declineButton.gameObject.SetActive( true );
				acceptButton.onClick.RemoveAllListeners();
				acceptButton.onClick.AddListener(() => OnAcceptMatchRequest( sender, md ));
				declineButton.onClick.RemoveAllListeners();
				declineButton.onClick.AddListener(() => OnDeclineMatchRequest( sender ));
				gameObject.SetActive( true );
			break;

			case ChatMessageType.MATCH_REQUEST_ACCEPTED:
				mainText.text = string.Format( "{0} has accepted the challenge.", sender );
				acceptButton.gameObject.SetActive( false );
				declineButton.gameObject.SetActive( false );
				gameObject.SetActive( true );
			break;

			case ChatMessageType.MATCH_REQUEST_DECLINED:
				mainText.text = string.Format( "{0} declined your race request.", sender );
				acceptButton.gameObject.SetActive( false );
				declineButton.gameObject.SetActive( false );
				gameObject.SetActive( true );
			break;
		}
	}

	void OnAcceptFriendRequest( string sender, PlayerFriends.FriendData fd )
	{
		Debug.Log( "OnAcceptFriendRequest from " + sender );
		gameObject.SetActive( false );
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
		gameObject.SetActive( false );
		//Send a message back saying that the friend request has been declined.
		ChatManager.Instance.chatMessageHandler.sendFriendRequestDeclinedMessage ( sender );
	}

	void OnAcceptMatchRequest( string sender, ChatMessageHandler.MatchData md )
	{
		Debug.Log( "OnAcceptMatchRequest from " + sender );
		gameObject.SetActive( false );
		//Send a message back saying that the match request has been accepted.
		ChatManager.Instance.chatMessageHandler.sendMatchRequestAcceptedMessage ( sender );
	}

	void OnDeclineMatchRequest( string sender )
	{
		Debug.Log( "OnDeclineMatchRequest" );
		gameObject.SetActive( false );
		//Send a message back saying that the match request has been declined.
		ChatManager.Instance.chatMessageHandler.sendMatchRequestDeclinedMessage ( sender );
	}

	public void OnClose()
	{
		Debug.Log( "OnClose" );
		gameObject.SetActive( false );
	}

	IEnumerator loadScene(GameScenes value)
	{
		UISoundManager.uiSoundManager.playButtonClick();
		Handheld.StartActivityIndicator();
		yield return new WaitForSeconds(0);
		SceneManager.LoadScene( (int)value );
	}	

/*
	public void OnAcceptInvitation() receiver side
	{
		Debug.Log( "OnAcceptInvitation" );
		//Reply to the sender that we have accepted
		MatchInvitation match = new MatchInvitation( InvitationType.INVITATION_ACCEPTED, PlayerStatsManager.Instance.getUserName(), LevelManager.Instance.matchInvitation.multiplayerLevelIndex, LevelManager.Instance.matchInvitation.roomName, GameManager.Instance.playerProfile.getPlayerIconId(), GameManager.Instance.playerProfile.getLevel(), GameManager.Instance.playerProfile.prestigeLevel );
		chatClient.SendPrivateMessage( LevelManager.Instance.matchInvitation.sender, match.getAsCSV()  );
		invitationReceivedPanel.SetActive( false );
		//Since we are skipping the PlayModes scene, make sure we set the play mode.
		GameManager.Instance.setPlayMode( PlayMode.PlayWithFriends );
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}

	public void OnCloseInvitationStatusPanel() sender side
	{
		invitationStatusPanel.SetActive( false );
		switch ( LevelManager.Instance.matchInvitation.type )
		{
			case InvitationType.INVITATION_ACCEPTED:
				LevelManager.Instance.setCurrentMultiplayerLevel( LevelManager.Instance.matchInvitation.multiplayerLevelIndex );
				GameManager.Instance.setPlayMode(PlayMode.PlayWithFriends);
				Debug.Log( "OnCloseInvitationStatusPanel ACCEPT" );
				StartCoroutine( loadScene(GameScenes.Matchmaking) );
			break;
			case InvitationType.INVITATION_DECLINED:
				LevelManager.Instance.matchInvitation = null;
				Debug.Log( "OnCloseInvitationStatusPanel DECLINE" );
			break;
		}
	}
*/
}
