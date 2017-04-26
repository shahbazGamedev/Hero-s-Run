using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FriendUIDetails : MonoBehaviour {

	[SerializeField] Image background;
	[SerializeField] Image playerIcon;
	[SerializeField] Text userName;
	[SerializeField] Text levelText;
	[SerializeField] Button inviteButton;
	[SerializeField] Text inviteButtonText;
	[SerializeField] Button chatButton;
	[SerializeField] Text chatButtonText;
	[SerializeField] Image onlineStatusIcon;
	[SerializeField] Text onlineText;
	[SerializeField] Text lastOnlineText;
	Color lightGray = new Color( 180f/255f, 180f/255f, 180f/255f, 0.5f );
	Color darkGray = Color.gray;
	public string user;

	public void configureFriend ( int index, PlayerFriends.FriendData fd )
	{
		user = fd.userName;
		if( index%2 == 0 )
		{
			background.color = darkGray;
		}
		else
		{
			background.color = lightGray;
		}
		userName.text = fd.userName;
		levelText.text = fd.level.ToString();
		configureStatus( fd );
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
			inviteButton.interactable = true;
			chatButton.interactable = true;
		}
		else
		{
			inviteButton.interactable = false;
			chatButton.interactable = false;
		}
	}

	public void OnClickInviteFriend()
	{
		print("OnClickInviteFriend " + userName.text);
	}

	public void OnClickChat()
	{
		print("OnClickChat " );
	}

}
