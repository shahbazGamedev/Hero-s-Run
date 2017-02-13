using UnityEngine;
using Photon;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
 
 
namespace Com.MyCompany.MyGame
{
    public class Launcher : PunBehaviour
    {
        #region Public Variables
		/// <summary>
		/// The PUN loglevel. 
		/// </summary>
		public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
		public byte maxPlayersInRace = 2;
		/// <summary>
		/// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
		/// </summary>   
		[Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
		public byte MaxPlayersPerRoom = 4;
		public Text skins;
		public Text localPlayerName;

		public Text remotePlayerName;

		[Tooltip("The Ui Panel to let the user enter name, connect and play")]
		public GameObject controlPanel;
		[Tooltip("The UI Label to inform the user that the connection is in progress")]
		public GameObject progressLabel;
		public List<Sprite> playerIcons = new List<Sprite>();
		public Image localPlayerIcon;
		public Image remotePlayerIcon;
		public Text countOfPlayers;
		public Text countOfPlayersLookingForRoom;

		public ExitGames.Client.Photon.Hashtable someCustomPropertiesToSet = new ExitGames.Client.Photon.Hashtable();
	    #endregion
 
 
        #region Private Variables
		/// <summary>
		/// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
		/// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
		/// Typically this is used for the OnConnectedToMaster() callback.
		/// </summary>
		bool isConnecting; 
 
        /// <summary>
        /// This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).
        /// </summary>
        string _gameVersion = "1";
		bool playAlone = false;
 
        #endregion
 
 
        #region MonoBehaviour CallBacks
 
 
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
			//Hashtable someCustomPropertiesToSet = new Hashtable() {{"Skin", "Cute Girl" + Random.Range(69,269).ToString() }};
			//PhotonNetwork.player.SetCustomProperties(someCustomPropertiesToSet); 
			// #NotImportant
			// Force LogLevel
			PhotonNetwork.logLevel = Loglevel;

            // #Critical
            // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
            PhotonNetwork.autoJoinLobby = false;
 
 
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;
        }
 
 
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
			someCustomPropertiesToSet.Clear();
			progressLabel.SetActive(false);
			controlPanel.SetActive(true);
			localPlayerName.text = PlayerPrefs.GetString("PlayerName", "default name");
			InvokeRepeating("updatePlayerStats", 1f, 5f );
		}
 
 
        #endregion
 
 
        #region Public Methods
 
		void updatePlayerStats()
		{
			//The count of players currently using this application (available on MasterServer in 5sec intervals).
			countOfPlayers.text = PhotonNetwork.countOfPlayers.ToString();
			//The count of players currently looking for a room (available on MasterServer in 5sec intervals).
			countOfPlayersLookingForRoom.text = PhotonNetwork.countOfPlayersOnMaster.ToString();
		}

        /// <summary>
        /// Start the connection process. 
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {
			// keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
			isConnecting = true;
 			progressLabel.SetActive(true);
			controlPanel.SetActive(false);
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.connected)
            {
				// we don't want to do anything if we are not attempting to join a room. 
				// this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
				// we don't want to do anything.
				if (isConnecting)
				{
				    // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
					ExitGames.Client.Photon.Hashtable desiredRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Track", 12 }, { "Elo", 24 } };
				    PhotonNetwork.JoinRandomRoom( desiredRoomProperties, maxPlayersInRace);
				}
           }else{
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings(_gameVersion);
            }
        }
 
 
    #endregion
 
 	#region Photon.PunBehaviour CallBacks
	 
	 
	public override void OnConnectedToMaster()
	{
	    Debug.Log("DemoAnimator/Launcher: OnConnectedToMaster() was called by PUN");
		// #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()  
		PhotonNetwork.JoinRandomRoom();	 
	 
	}
	 
	public void setIconLocalPlayer( int index )
	{
		localPlayerIcon.sprite = playerIcons[index];
 Debug.Log("setIconLocalPlayer " + index);
	}	 

	public void setIconRemotePlayer( int index )
	{
		remotePlayerIcon.sprite = playerIcons[index];
 Debug.Log("setIconRemotePlayer " + index);
		
	}	 
	public override void OnDisconnectedFromPhoton()
	{
	    Debug.LogWarning("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called by PUN");  
			progressLabel.GetComponent<Text>().text =  "OnDisconnectedFromPhoton";   
		//progressLabel.SetActive(false);
			isConnecting = false;

		controlPanel.SetActive(true);
	}
	 
	public override void OnPhotonRandomJoinFailed (object[] codeAndMsg)
	{
	    Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
	 
	    // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Track", 12 }, { "Elo", 24 } };
		roomOptions.MaxPlayers = maxPlayersInRace;
		//In CreateRoom, do not specify a mach name. Let the server assign a random name.
	    PhotonNetwork.CreateRoom(null, roomOptions, null);
	}
	 
	public override void OnJoinedRoom()
	{
	    Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room." + PhotonNetwork.room.CustomProperties["Elo"].ToString() + " " + PhotonNetwork.room.CustomProperties["Track"].ToString());
		string skinsString = string.Empty;
		foreach(PhotonPlayer player in PhotonNetwork.playerList)
		{
				Debug.Log("Custom property " + player.NickName + " " + player.CustomProperties["Skin"].ToString() );
				skinsString = skinsString + player.CustomProperties["Skin"].ToString() + ",";
				if( !player.IsLocal ) remotePlayerName.text = player.NickName;
				if( !player.IsLocal ) setIconRemotePlayer( (int)player.CustomProperties["Icon"] );

		}
		skins.text = skinsString;
		someCustomPropertiesToSet.Add("PlayerPosition", PhotonNetwork.room.PlayerCount );
		PhotonNetwork.player.SetCustomProperties(someCustomPropertiesToSet); 

		// #Critical: We only load if we are the first player, else we rely on  PhotonNetwork.automaticallySyncScene to sync our instance scene.
	    if (playAlone)
	    {
	        Debug.Log("playAlone We load the 'Room for 1' ");
	 
	 
	        // #Critical
	        // Load the Room Level. 
	        PhotonNetwork.LoadLevel("Room for 1");
		}
	}
	/// <summary>
	/// Called when a remote player entered the room. This PhotonPlayer is already added to the playerlist at this time.
	/// </summary>
	/// <remarks>If your game starts with a certain number of players, this callback can be useful to check the
	/// Room.playerCount and find out if you can start.</remarks>
	/// <param name="newPlayer">New player.</param>
	public override void OnPhotonPlayerConnected (PhotonPlayer newPlayer )
	{
			if( playAlone) return;
			Debug.Log("DemoAnimator/Launcher: OnPhotonPlayerConnected() called by PUN. " + newPlayer.NickName + " " + newPlayer.IsLocal + " nbr " + PhotonNetwork.room.PlayerCount );

			progressLabel.GetComponent<Text>().text = "OnPhotonPlayerConnected " + newPlayer.NickName + " nbr " + PhotonNetwork.room.PlayerCount;
			remotePlayerName.text = newPlayer.NickName;
			int iconIndexRemotePlayer = (int)newPlayer.CustomProperties["Icon"];
			setIconRemotePlayer( iconIndexRemotePlayer );
			LoadArena();		 
	}
	 

		 
		public override void OnPhotonPlayerDisconnected( PhotonPlayer other  )
		{
		    Debug.Log( "launcher OnPhotonPlayerDisconnected() " + other.NickName ); // seen when other disconnects
		 
		 
		}
		void LoadArena()
		{
		    if ( ! PhotonNetwork.isMasterClient ) 
		    {
		        Debug.LogError( "PhotonNetwork : Trying to Load a level but we are not the master Client" );
				return;
		    }
		    Debug.Log( "PhotonNetwork : Loading Level : " + PhotonNetwork.room.PlayerCount );
			if( PhotonNetwork.room.PlayerCount == 2 )
			{
		   	 	Invoke("load",5f);
			}
		}
		 
		void load()
		{
		   	 	PhotonNetwork.LoadLevel("Room for 1");
		}
	#endregion

   }

}
