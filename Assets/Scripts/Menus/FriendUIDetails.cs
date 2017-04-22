using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FriendUIDetails : MonoBehaviour {

	[SerializeField] Image background;
	[SerializeField] Image playerIcon;
	[SerializeField] Text userName;
	[SerializeField] Text levelText;
	[SerializeField] Text inviteButtonText;
	[SerializeField] Text chatButtonText;
	[SerializeField] Image onlineStatusIcon;
	[SerializeField] Text onlineText;
	[SerializeField] Text lastOnlineText;
	Color lightBackground = new Color( 180f/255f, 180f/255f, 180f/255f, 0.5f );
	Color darkerBackground = Color.gray;

	public void configureFriend ( int index, PlayerFriends.OnlineFriendData fd )
	{
		if( index%2 == 0 )
		{
			background.color = darkerBackground;
		}
		else
		{
			background.color = lightBackground;
		}
		userName.text = fd.userName;
		levelText.text = fd.level.ToString();
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
