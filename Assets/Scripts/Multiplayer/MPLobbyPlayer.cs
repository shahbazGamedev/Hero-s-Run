using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Facebook.Unity;

public class MPLobbyPlayer : NetworkLobbyPlayer {

	public MPLobbyMenu mpLobbyMenu;

	//OnMyName function will be invoked on clients when server change the value of playerName
	[SyncVar(hook = "OnMyName")]
	public string playerName = "";
	//OnMyFacebookID function will be invoked on clients when server change the value of facebookID
	[SyncVar(hook = "OnMyFacebookID")]
	public string facebookID = "";

	//This method is called before OnStartClient. isLocalPlayer is NOT reliable at this point.
	public override void  OnStartServer()
	{
		base.OnStartServer();
		Debug.Log("MPLobbyPlayer-OnStartServer " + gameObject.name );
	}

	//In OnClientEnterLobby, the variable isLocalPlayer has NOT been set yet
	public override void OnClientEnterLobby()
	{
		Debug.Log("MPLobbyPlayer-OnClientEnterLobby" );

		GameObject go = GameObject.FindGameObjectWithTag("Lobby Canvas");
		mpLobbyMenu = go.GetComponent<MPLobbyMenu>();

		base.OnClientEnterLobby();
 	}

	public override void  OnStartClient()
	{
		base.OnStartClient();
		MPNetworkLobbyManager.mpNetworkLobbyManager.lobbyPlayerCount++;
		Debug.Log("MPLobbyPlayer-OnStartClient: lobbyPlayerCount: " + MPNetworkLobbyManager.mpNetworkLobbyManager.lobbyPlayerCount + "\n" );
	}

	//In this method, isLocalPlayer is reliable. This method is called after OnStartClient.
	public override void  OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
		Debug.Log("MPLobbyPlayer-OnStartLocalPlayer" );
		CmdNameChanged( PlayerStatsManager.Instance.getUserName() );
		if (FB.IsLoggedIn && AccessToken.CurrentAccessToken != null )
		{
			CmdFacebookIDChanged( AccessToken.CurrentAccessToken.UserId );
		}
		SendReadyToBeginMessage();
	}

	public override void OnClientReady(bool readyState)
	{
 		Debug.Log("MPLobbyPlayer-OnClientReady readyState: " + readyState );
	}

 	//sync var callback
	public void OnMyName(string newName)
	{
		playerName = newName;
		gameObject.name = "Lobby " + playerName;
		if ( !isLocalPlayer )
		{
			Debug.Log("MPLobbyPlayer-OnMyName: Not Local " + newName );
			mpLobbyMenu.setRemotePlayerName( playerName );
		}
	}

 	//sync var callback
	public void OnMyFacebookID(string newFacebooID )
	{
		facebookID = newFacebooID;
		if ( !isLocalPlayer )
		{
			Debug.Log("MPLobbyPlayer-OnMyFacebookID: Not Local " + newFacebooID );
			mpLobbyMenu.setRemotePlayerPortrait( newFacebooID );
		}
	}

	[Command]
	public void CmdNameChanged( string newName )
	{
		playerName = newName;
	}
	
	[Command]
	public void CmdFacebookIDChanged( string newFacebookID )
	{
		facebookID = newFacebookID;
	}

}
