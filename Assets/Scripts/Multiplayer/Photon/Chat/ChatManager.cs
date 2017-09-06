using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon.Chat;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon;

public class ChatManager : PunBehaviour, IChatClientListener {

	public static ChatManager Instance;
	ChatClient chatClient;
	public ChatMessageHandler chatMessageHandler;
	public ChatMessageUI chatMessageUI;

	[Header("Channels")]
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
		Application.runInBackground = true; //This does nothing on mobile devices, but helps with testing.
		chatClient = new ChatClient(this);
		chatClient.ChatRegion =  "EU";
		string userName = GameManager.Instance.playerProfile.getUserName();
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
		//Set our chat status to Online
		int[] onlinePlayerData = new int[3];
		onlinePlayerData[0] = GameManager.Instance.playerProfile.getPlayerIconId();
		onlinePlayerData[1] = GameManager.Instance.playerProfile.getLevel();
		onlinePlayerData[2] = GameManager.Instance.playerStatistics.getStatisticData(StatisticDataType.CURRENT_WIN_STREAK);

		this.chatClient.SetOnlineStatus(ChatUserStatus.Online, onlinePlayerData );

		//Limit the number of messages that are cached
		this.chatClient.MessageLimit = HistoryLengthToFetch;

		//Set your own online status
		if( onStatusUpdateEvent != null ) onStatusUpdateEvent( GameManager.Instance.playerProfile.getUserName(), (int)ChatUserStatus.Online );

		//Add friends and recent players so we can get their online status. See OnStatusUpdate for more details.
		addChatFriends();
		addRecentPlayers();
	}
	
	public void removeChatFriend( string userName )
	{
		if( !string.IsNullOrEmpty( userName ) )
		{
			string[] friendNameArray = { userName };
			chatClient.RemoveFriends( friendNameArray );
		}
	}

	public void addChatFriend( string userName )
	{
		if( !string.IsNullOrEmpty( userName ) )
		{
			string[] friendNameArray = { userName };
			chatClient.AddFriends( friendNameArray );
		}
	}

	void addChatFriends()
	{
		//Add friends so we can get their online status. See OnStatusUpdate for more details.
		string[] friendNamesArray = GameManager.Instance.playerFriends.getFriendNames();
		if( friendNamesArray.Length > 0 )
		{
			chatClient.AddFriends( friendNamesArray );
		}
	}

	void addRecentPlayers()
	{
		//Add recent players so we can get their online status. See OnStatusUpdate for more details.
		string[] recentPlayersNamesArray = GameManager.Instance.recentPlayers.getRecentPlayersNames();
		if( recentPlayersNamesArray.Length > 0 )
		{
			chatClient.AddFriends( recentPlayersNamesArray );
		}
	}

	public void OnDisconnected()
	{
		Debug.LogWarning( GameManager.Instance.playerProfile.getUserName() + " has been disconnected from chat." );
		if( onStatusUpdateEvent != null ) onStatusUpdateEvent( GameManager.Instance.playerProfile.getUserName(), (int)ChatUserStatus.Offline );
	}
	
	public bool canChat()
	{
		return chatClient.CanChat;
	}

	void OnApplicationPause( bool isPaused )
	{
		#if !UNITY_EDITOR
		if ( isPaused )
		{
			if( chatClient != null ) chatClient.Disconnect();
		}
		else
		{
			ChatConnect();
		}
		#endif
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
		Debug.Log("ChatManager-OnChatStateChange: " + GameManager.Instance.playerProfile.getUserName() + " is now in chat state: " + state.ToString() );
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
		Debug.Log("OnStatusUpdate: gotMsg " + gotMessage + " "+ string.Format("{0} is {1}.", user, status));
		GameManager.Instance.playerFriends.updateStatus( user, status );
		GameManager.Instance.recentPlayers.updateStatus( user, status );
		if( onStatusUpdateEvent != null ) onStatusUpdateEvent( user, status );
		if( status == ChatUserStatus.Online && gotMessage && message != null )
		{
			int[] onlinePlayerData = (int[]) message;				
			//Update the friend's list with this information
			PlayerFriends.FriendData friendData = new PlayerFriends.FriendData( user, onlinePlayerData[0], onlinePlayerData[1], onlinePlayerData[2] );
			friendData.status = (int)ChatUserStatus.Online;
			friendData.print();
			chatMessageHandler.updateFriendData( user, friendData );
		}
	}

	public Color getStatusColor( int status )
	{
		Color statusColor;
		switch ( status )
		{
			case ChatUserStatus.Offline:
				statusColor = Color.red;
			break;

			case ChatUserStatus.Invisible:
				statusColor = Color.red;
			break;
			
			case ChatUserStatus.Online:
				statusColor = Color.green;
			break;

			case ChatUserStatus.Away:
				statusColor = Color.yellow;
			break;

			case ChatUserStatus.DND:
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


	public void sendPrivateMessage( string target, string message )
	{
		Debug.Log("sendPrivateMessage " + target + " " + message );
		chatClient.SendPrivateMessage( target, message );
	}

	public void OnPrivateMessage( string sender, object message, string channelName )
	{
		//The sender also gets a copy of the messages he sends.
		//Ignore those.
		if( sender == GameManager.Instance.playerProfile.getUserName() ) return;

		Debug.Log("OnPrivateMessage " + sender + " " + message.ToString() );

		chatMessageHandler.parseMessage( sender, message.ToString() );
	}

	
}
