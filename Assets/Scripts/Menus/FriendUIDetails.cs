using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon.Chat;

public class FriendUIDetails : MonoBehaviour {

	[SerializeField] Image background;
	[SerializeField] Image playerIcon;
	[SerializeField] Text userName;
	[SerializeField] Text levelText;
	[SerializeField] Text winStreakText;
	[SerializeField] Button raceMeButton;
	[SerializeField] Text raceMeButtonText;
	[SerializeField] Text raceMeButtonConfirmationText;
	[SerializeField] Image onlineStatusIcon;
	[SerializeField] Text onlineText;
	Color lightGray = new Color( 180f/255f, 180f/255f, 180f/255f, 0.5f );
	Color darkGray = Color.gray;
	[HideInInspector]
	public string user;

	public void configureFriend ( int index, PlayerFriends.FriendData fd )
	{
		user = fd.userName;
		if( raceMeButtonConfirmationText != null ) raceMeButtonConfirmationText.gameObject.SetActive( false );
		if( index%2 == 0 )
		{
			background.color = darkGray;
		}
		else
		{
			background.color = lightGray;
		}
		playerIcon.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( fd.playerIcon ).icon;
		userName.text = fd.userName;
		levelText.text = string.Format( LocalizationManager.Instance.getText( "SOCIAL_FRIEND_LEVEL" ), fd.level );
		if( fd.currentWinStreak >= 3 )
		{
			winStreakText.gameObject.SetActive( true );
			winStreakText.text = string.Format( LocalizationManager.Instance.getText( "WIN_STREAK" ), fd.currentWinStreak );
		}
		else
		{
			winStreakText.gameObject.SetActive( false );
		}

		configureStatus( fd );
	}

	public void updateFriendData ( PlayerFriends.FriendData fd )
	{
		playerIcon.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( fd.playerIcon ).icon;
		levelText.text = string.Format( LocalizationManager.Instance.getText( "SOCIAL_FRIEND_LEVEL" ), fd.level );
		if( fd.currentWinStreak >= 3 )
		{
			winStreakText.gameObject.SetActive( true );
			winStreakText.text = string.Format( LocalizationManager.Instance.getText( "WIN_STREAK" ), fd.currentWinStreak );
		}
		else
		{
			winStreakText.gameObject.SetActive( false );
		}
	}

	void configureStatus( PlayerFriends.FriendData fd )
	{
		print("configureStatus " + fd.userName + " " + fd.status );
		configureStatus( fd.status );
	}

	public void configureStatus( int status )
	{
		//Text
		switch ( status )
		{
			case ChatUserStatus.Offline:
				onlineText.text = "Offline";
			break;

			case ChatUserStatus.Invisible:
				onlineText.text = "Invisible";
			break;
			
			case ChatUserStatus.Online:
				onlineText.text = "Online";
			break;

			case ChatUserStatus.Away:
				onlineText.text = "Away";
			break;

			case ChatUserStatus.DND:
				onlineText.text = "Do Not Disturb";
			break;
		}

		//Color
		onlineStatusIcon.color = ChatManager.Instance.getStatusColor( status );

		//Buttons
		//if the friend is Online and the player is connected to the chat backend, enable the Invite and Chat buttons
		if( status == ChatUserStatus.Online && ChatManager.Instance.canChat() )
		{
			if( raceMeButton != null ) raceMeButton.interactable = true;
		}
		else
		{
			if( raceMeButton != null ) raceMeButton.interactable = false;
		}
	}

	public void OnClickSendMatchRequest()
	{
		print("OnClickSendMatchRequest " + user );
		UISoundManager.uiSoundManager.playButtonClick();
		ChatManager.Instance.chatMessageHandler.sendMatchRequestMessage( user );
		raceMeButtonConfirmationText.gameObject.SetActive( true );
		Invoke("hideRaceMeButtonConfirmationText", 2.5f );
	}

	void hideRaceMeButtonConfirmationText()
	{
		raceMeButtonConfirmationText.gameObject.SetActive( false );
	}

	public void OnClickChat()
	{
		print("OnClickChat " );
		UISoundManager.uiSoundManager.playButtonClick();
	}

	public void OnClickSendFriendRequest()
	{
		print("OnClickSendFriendRequest " );
		UISoundManager.uiSoundManager.playButtonClick();
		ChatManager.Instance.chatMessageHandler.sendAddFriendMessage( user );
	}

}
