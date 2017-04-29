using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon.Chat;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon;

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
		Application.runInBackground = true; //This does nothing on mobile devices, but helps with testing.
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
		#if !UNITY_EDITOR
		if ( hasFocus )
		{
			ChatConnect();
		}
		else
		{
			chatClient.Disconnect();
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
			if( status == 2 )
			{
				//Since that friend is online, ask him for his details (player icon, level, prestige level ...)
				chatMessageHandler.sendAskFriendData( user );
			}
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
	}

	
}
