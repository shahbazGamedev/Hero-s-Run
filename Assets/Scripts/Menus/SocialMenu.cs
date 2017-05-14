using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ExitGames.Client.Photon.Chat;

public class SocialMenu : MonoBehaviour {

	[Header("Social Menu")]
	[Header("Friend List")]
	[SerializeField] Transform friendsHolder;
	[SerializeField] GameObject friendEntryPrefab;
	[SerializeField] InputField addFriendInputField;
	[SerializeField] Text addFriendPlaceholderText;
	[Header("Recent Player List")]
	[SerializeField] Transform recentPlayersHolder;
	[SerializeField] GameObject recentPlayerPrefab;
	[Header("Chat Online Indicator")]
	[SerializeField] Image onlineIndicator;

	bool levelLoading = false;

	// Use this for initialization
	void Start () {

		Handheld.StopActivityIndicator();
		createFriendList();
		createRecentPlayerList();
		if( ChatManager.Instance.canChat() )
		{
			onlineIndicator.color = ChatManager.Instance.getStatusColor( (int)ChatUserStatus.Online );
		}
		else
		{
			onlineIndicator.color = ChatManager.Instance.getStatusColor( (int)ChatUserStatus.Offline );
		}
	}

	void createFriendList()
	{
		//Remove previous friends if any. We want to keep the first object which is used to add friends however.
		for( int i = friendsHolder.childCount-1; i >= 1; i-- )
		{
			Transform child = friendsHolder.GetChild( i );
			GameObject.Destroy( child.gameObject );
		}

		List<PlayerFriends.FriendData> friendList = GameManager.Instance.playerFriends.getFriendList();
		for( int i = 0; i < friendList.Count; i++ )
		{
			createFriend( i, friendList[i] );
		}

		//Adjust the length of the content based on the number of entries.
		//All entries have the same height.
		//There is spacing between the entries.
		float elementHeight = friendsHolder.GetChild( 0 ).GetComponent<LayoutElement>().preferredHeight;
		float spacing = friendsHolder.GetComponent<VerticalLayoutGroup>().spacing;
		float contentHeight = friendsHolder.childCount * elementHeight + ( friendsHolder.childCount - 1 ) * spacing;
		friendsHolder.GetComponent<RectTransform>().sizeDelta = new Vector2( friendsHolder.GetComponent<RectTransform>().sizeDelta.x, contentHeight );
	}

	void createFriend( int index, PlayerFriends.FriendData fd )
	{
		GameObject go = (GameObject)Instantiate(friendEntryPrefab);
		go.transform.SetParent(friendsHolder,false);
		go.GetComponent<FriendUIDetails>().configureFriend( index, fd );
	}

	void createRecentPlayerList()
	{
		//Remove previous recent players if any.
		for( int i = recentPlayersHolder.childCount-1; i >= 0; i-- )
		{
			Transform child = recentPlayersHolder.GetChild( i );
			GameObject.Destroy( child.gameObject );
		}

		List<PlayerFriends.FriendData> recentPlayerList = GameManager.Instance.recentPlayers.getRecentPlayersList();
		for( int i = 0; i < recentPlayerList.Count; i++ )
		{
			createRecentPlayer( i, recentPlayerList[i] );
		}

		//Adjust the length of the content based on the number of entries.
		//All entries have the same height.
		//There is spacing between the entries.
		float elementHeight = friendsHolder.GetChild( 0 ).GetComponent<LayoutElement>().preferredHeight;
		float spacing = recentPlayersHolder.GetComponent<VerticalLayoutGroup>().spacing;
		float contentHeight = recentPlayersHolder.childCount * elementHeight + ( recentPlayersHolder.childCount - 1 ) * spacing;
		recentPlayersHolder.GetComponent<RectTransform>().sizeDelta = new Vector2( recentPlayersHolder.GetComponent<RectTransform>().sizeDelta.x, contentHeight );
	}

	void createRecentPlayer( int index, PlayerFriends.FriendData fd )
	{
		GameObject go = (GameObject)Instantiate(recentPlayerPrefab);
		go.transform.SetParent(recentPlayersHolder,false);
		go.GetComponent<RecentPlayerUIDetails>().configureRecentPlayer( index, fd );
	}

	/// <summary>
	/// Raises the end edit event when the name of the friend has been entered.
	/// </summary>
	/// <param name="friendName">Friend name.</param>
 	public void OnEndEdit( string friendName )
    {
		if (!string.IsNullOrEmpty(friendName))
		{
			UISoundManager.uiSoundManager.playButtonClick();
			friendName = friendName.Trim();
			if( friendName.Length < UserNameHandler.MINIMUM_USER_NAME_LENGTH )
			{
				addFriendInputField.text = string.Empty;
				addFriendPlaceholderText.text = string.Format( LocalizationManager.Instance.getText( "USER_NAME_NOT_LONG_ENOUGH" ), UserNameHandler.MINIMUM_USER_NAME_LENGTH.ToString() );
				return;
			}
			if( GameManager.Instance.playerFriends.isFriend( friendName ) )
			{
				addFriendInputField.text = string.Empty;
				addFriendPlaceholderText.text = string.Format( LocalizationManager.Instance.getText( "SOCIAL_ALREADY_FRIEND" ), friendName );
				return;
			}
			if( friendName == GameManager.Instance.playerProfile.getUserName() )
			{
				addFriendInputField.text = string.Empty;
				addFriendPlaceholderText.text = LocalizationManager.Instance.getText( "SOCIAL_YOUR_OWN_USER_NAME" );
				return;
			}
			ChatManager.Instance.chatMessageHandler.sendAddFriendMessage( friendName );
 			addFriendInputField.text = string.Empty;
 			addFriendPlaceholderText.text = LocalizationManager.Instance.getText( "USER_NAME_PLACEHOLDER" );
		}
  	}

	public void OnClickReturnToMainMenu()
	{
		StartCoroutine( loadScene(GameScenes.MainMenu) );
	}

	IEnumerator loadScene(GameScenes value)
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)value );
		}
	}

	void OnEnable()
	{
		ChatManager.onStatusUpdateEvent += OnStatusUpdateEvent;
		PlayerFriends.onFriendChangedEvent += OnFriendChangedEvent;
		RecentPlayers.onRecentPlayerChangedEvent += OnRecentPlayerChangedEvent;
		ChatMessageHandler.onFriendDataEvent += OnFriendDataEvent;
	}

	void OnDisable()
	{
		ChatManager.onStatusUpdateEvent -= OnStatusUpdateEvent;
		PlayerFriends.onFriendChangedEvent -= OnFriendChangedEvent;
		RecentPlayers.onRecentPlayerChangedEvent -= OnRecentPlayerChangedEvent;
		ChatMessageHandler.onFriendDataEvent -= OnFriendDataEvent;
	}

	void OnStatusUpdateEvent( string userName, int newStatus )
	{
		print("OnStatusUpdateEvent " + userName + " " + newStatus );

		//Is it our own status?
		if( userName == GameManager.Instance.playerProfile.getUserName() )
		{
			//Yes, this is our own status
			onlineIndicator.color = ChatManager.Instance.getStatusColor( newStatus );
		}
		else
		{
			//Go through friends
			FriendUIDetails fud;
			for( int i = 0; i < friendsHolder.transform.childCount; i++ )
			{
				fud = friendsHolder.transform.GetChild( i ).GetComponent<FriendUIDetails>();
				if( fud != null && fud.user == userName )
				{
					fud.configureStatus( newStatus );
					break;
				}
			}
	
			//Go through recent players
			RecentPlayerUIDetails rpud;
			for( int i = 0; i < recentPlayersHolder.transform.childCount; i++ )
			{
				rpud = recentPlayersHolder.transform.GetChild( i ).GetComponent<RecentPlayerUIDetails>();
				if( rpud != null && rpud.user == userName )
				{
					rpud.configureStatus( newStatus );
					break;
				}
			}
		}
	}

	void OnFriendDataEvent( string userName, PlayerFriends.FriendData updatedFriendData )
	{
		updatedFriendData.print();
		//Go through friends
		FriendUIDetails fud;
		for( int i = 0; i < friendsHolder.transform.childCount; i++ )
		{
			fud = friendsHolder.transform.GetChild( i ).GetComponent<FriendUIDetails>();
			if( fud != null && fud.user == userName )
			{
				fud.updateFriendData( updatedFriendData );
				break;
			}
		}
		//Go through recent players
		RecentPlayerUIDetails rpud;
		for( int i = 0; i < recentPlayersHolder.transform.childCount; i++ )
		{
			rpud = recentPlayersHolder.transform.GetChild( i ).GetComponent<RecentPlayerUIDetails>();
			if( rpud != null && rpud.user == userName )
			{
				rpud.updateRecentPlayerData( updatedFriendData );
				break;
			}
		}
	}

	void OnRecentPlayerChangedEvent()
	{
		print("OnRecentPlayerChangedEvent-recreating recent player list" );
		createRecentPlayerList();
	}

	void OnFriendChangedEvent()
	{
		print("OnFriendChangedEvent-recreating friend list" );
		createFriendList();
		createRecentPlayerList();
	}
	
}
