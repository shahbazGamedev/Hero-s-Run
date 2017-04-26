using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon.Chat;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon;

public enum InvitationType
{
	INVITE_FRIEND = 1,
	INVITATION_ACCEPTED = 2,
	INVITATION_DECLINED = 3
}

public class ChatManager : PunBehaviour, IChatClientListener {

	public static ChatManager Instance;
	public ChatClient chatClient;
	public ChatMessageHandler chatMessageHandler;
	public ChatMessageUI chatMessageUI;

	[Header("Online Indicator")]
	[SerializeField] Image onlineIndicator;
	[Header("Channels")]
	[SerializeField] string[] ChannelsToJoinOnConnect; 	// set in inspector. Channels to join automatically.
	[SerializeField] int HistoryLengthToFetch; 			// set in inspector. Up to a certain degree, previously sent messages can be fetched for context
	//Event management used to notify other classes when the status for the player or a friend changes
	public delegate void OnStatusUpdateEvent( string userName, int newStatus );
	public static event OnStatusUpdateEvent onStatusUpdateEvent;

	void Awake ()
	{
		if(Instance)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
	}
	
 	void Start()
	{
		chatMessageHandler = new ChatMessageHandler();
		ChatConnect();	
	}
	
	public void ChatConnect()
	{
		//If this is a new player, don't try to connect to chat yet as we don't have a user name.
		//We will connect to chat when a user name has been entered.
		if( PlayerStatsManager.Instance.isFirstTimePlaying() ) return;

		//Verify that the mandatory Photon Chat App Id is configured
		if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.ChatAppID))
		{
			Debug.LogError("ChatManager-You need to set the chat app ID in the PhotonServerSettings file in order to continue.");
			return;
		}

		chatClient = new ChatClient(this);
		chatClient.ChatRegion =  "EU";
		string userName = PlayerStatsManager.Instance.getUserName();
		if( string.IsNullOrEmpty( userName ) )
		{
			Debug.LogError("ChatManager-Unable to connect to chat because the player's user name is either null or empty.");
		}
		else
		{
			chatClient.Connect(PhotonNetwork.PhotonServerSettings.ChatAppID, GameManager.Instance.getVersionNumber(), new ExitGames.Client.Photon.Chat.AuthenticationValues(userName));
		}
	}
	
	public void OnConnected()
	{
		//Now that we are connected, subscribe to chat channels
		if (this.ChannelsToJoinOnConnect != null && this.ChannelsToJoinOnConnect.Length > 0)
		{
			this.chatClient.Subscribe(this.ChannelsToJoinOnConnect, this.HistoryLengthToFetch);
		}
		//Set our chat status to Online
		this.chatClient.SetOnlineStatus(ChatUserStatus.Online); // You can set your online state (without a message).

		//Limit the number of messages that are cached
		this.chatClient.MessageLimit = HistoryLengthToFetch;

		//Add friends so we can get their online status. See OnStatusUpdate for more details.
		string[] friendNamesArray = GameManager.Instance.playerFriends.getFriendNames();
		if( friendNamesArray.Length > 0 )
		{
			chatClient.AddFriends( friendNamesArray );
		}
	}
	
	public void OnDisconnected()
	{
		Debug.LogWarning( PlayerStatsManager.Instance.getUserName() + " has been disconnected from chat." );
		configureStatus( 0 );
	}
	
	public bool canChat()
	{
		return chatClient.CanChat;
	}

	void OnApplicationFocus( bool hasFocus )
	{
		if ( hasFocus )
		{
			ChatConnect();
		}
		else
		{
			chatClient.Disconnect();
		}
	}

	public void OnSubscribed(string[] channels, bool[] results)
	{
		//Send a message into each channel.
		foreach (string channel in channels)
		{
			this.chatClient.PublishMessage(channel, "says 'hi'.");
			
		}
		
		Debug.Log("OnSubscribed: " + string.Join(", ", channels));
		
	}

	public void OnUnsubscribed(string[] channels)
	{
		foreach (string channelName in channels)
		{
			Debug.Log("Unsubscribed from channel '" + channelName + "'.");
		}
	}

	public void OnChatStateChange(ChatState state)
	{
		Debug.Log("ChatManager-OnChatStateChange: " + PlayerStatsManager.Instance.getUserName() + " is now in chat state: " + state.ToString() );
	}

	public void Update()
	{
		if (chatClient != null)
		{
			chatClient.Service(); // make sure to call this regularly! it limits effort internally, so calling often is ok!
		}
	}

	public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
	{
		if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
		{
			UnityEngine.Debug.LogError(message);
		}
		else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
		{
			UnityEngine.Debug.LogWarning(message);
		}
		else
		{
			UnityEngine.Debug.Log(message);
		}
	}

	/// <summary>
	/// New status of another user (you get updates for users set in your friends list).
	/// </summary>
	/// <param name="user">Name of the user.</param>
	/// <param name="status">New status of that user.</param>
	/// <param name="gotMessage">True if the status contains a message you should cache locally. False: This status update does not include a
	/// message (keep any you have).</param>
	/// <param name="message">Message that user set.</param>
	public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
	{
		Debug.Log("OnStatusUpdate: " + string.Format("{0} is {1}.", user, status));
		if( user == PlayerStatsManager.Instance.getUserName() )
		{
			configureStatus( status );
		}
		else
		{
 			GameManager.Instance.playerFriends.updateStatus( user, status );
			if( onStatusUpdateEvent != null ) onStatusUpdateEvent( user, status );
		}
	}

	void configureStatus( int status )
	{
		onlineIndicator.color = getStatusColor( status );
	}

	public Color getStatusColor( int status )
	{
		Color statusColor;
		switch ( status )
		{
			//Offline
			case 0:
				statusColor = Color.red;
			break;

			//Invisible
			case 1:
				statusColor = Color.red;
			break;
			
			//Online
			case 2:
				statusColor = Color.green;
			break;

			//Away
			case 3:
				statusColor = Color.yellow;
			break;

			//DND
			case 4:
				statusColor = Color.blue;
			break;

			default:
				statusColor = Color.white;
			break;
		}
		return  statusColor;
	}

	/// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnApplicationQuit.</summary>
	public void OnApplicationQuit()
	{
		if (this.chatClient != null)
		{
			this.chatClient.Disconnect();
		}
	}

	public void OnGetMessages( string channelName, string[] senders, object[] messages )
	{
		string msgs = "";
		for ( int i = 0; i < senders.Length; i++ )
		{
			msgs = string.Format("{0}{1}={2}, ", msgs, senders[i], messages[i]);
		}
		Debug.Log( "OnGetMessages: " + channelName + " " + senders.Length + " " + msgs );
	}

	#region Invite Friend
	/// <summary>
	/// Raises the end edit event when the name of the friend has been entered.
	/// </summary>
	/// <param name="friendName">Friend name.</param>
 	public void OnEndEdit( string friendName )
    {
		if (!string.IsNullOrEmpty(friendName))
		{
			sendInvitationToFriend( friendName.Trim() );
 		}
   	}

	public void sendInvitationToFriend(string friendName )
	{
		Debug.Log("sendInvitationToFriend " + friendName );
		string roomName = PlayerStatsManager.Instance.getUserName() + "_" + friendName;
		int multiplayerLevelndex = 0;
		MatchInvitation match = new MatchInvitation( InvitationType.INVITE_FRIEND, PlayerStatsManager.Instance.getUserName(), multiplayerLevelndex, roomName, GameManager.Instance.playerProfile.getPlayerIconId(), GameManager.Instance.playerProfile.getLevel(), GameManager.Instance.playerProfile.prestigeLevel );
		chatClient.SendPrivateMessage( friendName, match.getAsCSV()  );
	}

	public void sendPrivateMessage( string target, string message )
	{
		Debug.Log("sendPrivateMessage " + target + " " + message );
		chatClient.SendPrivateMessage( target, message );
	}

	
	public void OnPrivateMessage( string sender, object message, string channelName )
	{
		//The sender also gets a copy of the messages he sends.
		//Ignore those.
		if( sender == PlayerStatsManager.Instance.getUserName() ) return;

		Debug.Log("OnPrivateMessage " + sender + " " + message.ToString() );

		chatMessageHandler.parseMessage( sender, message.ToString() );

		/*MatchInvitation match = new MatchInvitation();
		match.parseMessage(message.ToString());
		LevelManager.Instance.matchInvitation = match;
		switch ( match.type )
		{
			case InvitationType.INVITE_FRIEND:
				invitationReceivedText.text = sender + " has challenged you.";
				invitationReceivedPanel.SetActive( true );
			break;
			case InvitationType.INVITATION_ACCEPTED:
				invitationStatusText.text = sender + " has accepted.";
				invitationStatusPanel.SetActive( true );
			break;
			case InvitationType.INVITATION_DECLINED:
				invitationStatusText.text = sender + " has declined.";
				invitationStatusPanel.SetActive( true );
			break;
		}*/

	}

	#endregion

	IEnumerator loadScene(GameScenes value)
	{
		UISoundManager.uiSoundManager.playButtonClick();
		Handheld.StartActivityIndicator();
		yield return new WaitForSeconds(0);
		SceneManager.LoadScene( (int)value );
	}

	[System.Serializable]
	public class MatchInvitation
	{
		public InvitationType type; 
		public string sender;		
		public int multiplayerLevelIndex;
		public string roomName;
		public int playerIcon;
		public int level;
		public int prestige;
	
		public MatchInvitation()
		{
		}

		public MatchInvitation( InvitationType type, string sender, int multiplayerLevelIndex, string roomName, int playerIcon, int level, int prestige )
		{
			this.type = type;
			this.sender = sender;
			this.multiplayerLevelIndex = multiplayerLevelIndex;
			this.roomName = roomName;
			this.roomName = roomName;
			this.playerIcon = playerIcon;
			this.level = level;
			this.prestige = prestige;
		}

		public void parseMessage( string message )
		{
			//Message parameters are comma-separated
			string[] messageParameters = message.Split(',');
			//Invitation type
			int invitationType;
			int.TryParse(messageParameters[0], out invitationType);
			type = (InvitationType) invitationType;
			//Sender
			sender = messageParameters[1];
			//Multiplayer Level Index
			int index;
			int.TryParse(messageParameters[2], out index);
			multiplayerLevelIndex = index;
			//Room Name
			roomName = messageParameters[3];
			//Player Icon Index
			int.TryParse(messageParameters[4], out playerIcon);
			//Player Level
			int.TryParse(messageParameters[5], out level);
			//Player Prestige Level
			int.TryParse(messageParameters[6], out prestige);

			printInvitation();
		}

		public string getAsCSV()
		{
			return ((int)type).ToString() + "," + sender + "," + multiplayerLevelIndex.ToString() + "," + roomName + "," + playerIcon.ToString() + "," + level.ToString() + "," + prestige.ToString();
		}

		public void printInvitation()
		{
			Debug.Log( "Type: " + type.ToString() + " Sender: " + sender + " Multiplayer Index Level: " + multiplayerLevelIndex  + " Room Name: " + roomName +
				" Player Icon Index: " + playerIcon + " Player Level: " + level + " Player Prestige Level: " + prestige );
		}
	}
	
}
