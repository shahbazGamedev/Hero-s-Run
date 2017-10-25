using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon.Chat;

public class RecentPlayerUIDetails : MonoBehaviour {

	[SerializeField] Image background;
	[SerializeField] Image playerIcon;
	[SerializeField] Text userName;
	[SerializeField] Text levelText;
	[SerializeField] Text winStreakText;
	[SerializeField] Text addFriendButtonConfirmationText;
	[SerializeField] Button addFriendButton;
	[SerializeField] Text addFriendButtonText;
	[SerializeField] Image onlineStatusIcon;
	[SerializeField] Text onlineText;
	Color lightGray = new Color( 180f/255f, 180f/255f, 180f/255f, 0.5f );
	Color darkGray = Color.gray;
	[HideInInspector]
	public string user;

	public void configureRecentPlayer ( int index, PlayerFriends.FriendData fd )
	{
		user = fd.userName;

		addFriendButtonConfirmationText.gameObject.SetActive( false );

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

	public void updateRecentPlayerData ( PlayerFriends.FriendData fd )
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

		//Button
		configureAddFriendButton( status );

	}

	/// <summary>
	/// Configures the add friend button.
	/// The add friend button will be interactable if 3 conditions are met:
	/// 1) The player is online.
	/// 2) The recent player is online.
	/// 3) The recent player is not already a friend.
	/// </summary>
	/// <param name="status">Status.</param>
	void configureAddFriendButton( int status )
	{
		bool isRecentPlayerAFriend = GameManager.Instance.playerFriends.isFriend( user );

		if( isRecentPlayerAFriend )
		{
			//This recent player is already your friend.
			addFriendButtonText.text = "Already a Friend";
		}
		else
		{
			//This recent player is not our friend.
			addFriendButtonText.text = "Add Friend";
		}

		if( ChatManager.Instance.canChat() && status == ChatUserStatus.Online && !isRecentPlayerAFriend )
		{
			addFriendButton.interactable = true;
		}
		else
		{
			addFriendButton.interactable = false;
		}
	}

	public void OnClickSendFriendRequest()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		print("OnClickSendFriendRequest " );
		ChatManager.Instance.chatMessageHandler.sendAddFriendMessage( user );
		addFriendButtonConfirmationText.gameObject.SetActive( true );
		Invoke("hideAddFriendButtonConfirmationText", 2.5f );
	}

	void hideAddFriendButtonConfirmationText()
	{
		addFriendButtonConfirmationText.gameObject.SetActive( false );
	}


}
