using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
		if( addFriendButtonConfirmationText != null ) addFriendButtonConfirmationText.gameObject.SetActive( false );
		if( index%2 == 0 )
		{
			background.color = darkGray;
		}
		else
		{
			background.color = lightGray;
		}
		playerIcon.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( fd.playerIcon ).icon;
		userName.text = fd.userName;
		levelText.text = fd.level.ToString();
		if( fd.currentWinStreak >= 3 )
		{
			winStreakText.gameObject.SetActive( true );
			winStreakText.text = string.Format( LocalizationManager.Instance.getText( "WIN_STREAK" ), fd.currentWinStreak );
		}
		else
		{
			winStreakText.gameObject.SetActive( false );
		}

		if( GameManager.Instance.playerFriends.isFriend( fd.userName ) )
		{
			//This recent player is already your friend.
			addFriendButtonText.text = "Already a Friend";
			addFriendButton.interactable = false;
		}
		else
		{
			//This recent player is not our friend.
			addFriendButtonText.text = "Add Friend";
			addFriendButton.interactable = true;
		}
		configureStatus( fd );
	}

	public void updateRecentPlayerData ( PlayerFriends.FriendData fd )
	{
		playerIcon.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( fd.playerIcon ).icon;
		levelText.text = fd.level.ToString();
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

	public void configureStatus( PlayerFriends.FriendData fd )
	{
		configureStatus( fd.status );
	}

	public void configureStatus( int status )
	{
		//Text
		switch ( status )
		{
			//Offline
			case 0:
				onlineText.text = "Offline";
			break;

			//Invisible
			case 1:
				onlineText.text = "Invisible";
			break;
			
			//Online
			case 2:
				onlineText.text = "Online";
			break;

			//Away
			case 3:
				onlineText.text = "Away";
			break;

			//DND
			case 4:
				onlineText.text = "Do Not Disturb";
			break;
		}

		//Color
		onlineStatusIcon.color = ChatManager.Instance.getStatusColor( status );

		//Buttons
		//if the friend is Online and the player is connected to the chat backend, enable the Invite and Chat buttons
		if( status == 2 && ChatManager.Instance.canChat() )
		{
			if( addFriendButton != null ) addFriendButton.interactable = true;
		}
		else
		{
			if( addFriendButton != null ) addFriendButton.interactable = false;
		}
	}

	public void hideAddFriendButtonConfirmationText()
	{
		addFriendButtonConfirmationText.gameObject.SetActive( false );
	}

	public void OnClickSendFriendRequest()
	{
		print("OnClickSendFriendRequest " );
		ChatManager.Instance.chatMessageHandler.sendAddFriendMessage( user );
	}

}
