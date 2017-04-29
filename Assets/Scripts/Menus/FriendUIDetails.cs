using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FriendUIDetails : MonoBehaviour {

	[SerializeField] Image background;
	[SerializeField] Image playerIcon;
	[SerializeField] Text userName;
	[SerializeField] Text levelText;
	[SerializeField] Text winStreakText;
	[SerializeField] Button raceMeButton;
	[SerializeField] Text raceMeButtonText;
	[SerializeField] Text raceMeButtonConfirmationText;
	[SerializeField] Button chatButton;
	[SerializeField] Text chatButtonText;
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

		configureStatus( fd );
	}

	public void updateFriendData ( PlayerFriends.FriendData fd )
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
			if( raceMeButton != null ) raceMeButton.interactable = true;
			if( chatButton != null ) chatButton.interactable = true;
		}
		else
		{
			if( raceMeButton != null ) raceMeButton.interactable = false;
			if( chatButton != null ) chatButton.interactable = false;
		}
	}

	public void OnClickSendMatchRequest()
	{
		print("OnClickSendMatchRequest " + user );
		ChatManager.Instance.chatMessageHandler.sendMatchRequestMessage( user );
		raceMeButtonConfirmationText.gameObject.SetActive( true );
		Invoke("hideRaceMeButtonConfirmationText", 2.5f );
	}

	public void hideRaceMeButtonConfirmationText()
	{
		raceMeButtonConfirmationText.gameObject.SetActive( false );
	}

	public void OnClickChat()
	{
		print("OnClickChat " );
	}

	public void OnClickSendFriendRequest()
	{
		print("OnClickSendFriendRequest " );
		ChatManager.Instance.chatMessageHandler.sendAddFriendMessage( user );
	}

}
