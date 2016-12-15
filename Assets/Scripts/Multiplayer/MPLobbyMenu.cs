using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MPLobbyMenu : MonoBehaviour {

	public MultiPurposePopup multiPurposePopup;
	public Text playerName;
	public Text remotePlayerName;
	public FacebookPortraitHandler playerPortrait;
	public FacebookPortraitHandler remotePlayerPortrait;
	bool levelLoading = false;

	void Awake ()
	{
		Handheld.StopActivityIndicator();
	}

	void Start ()
	{
		//The left portrait is always the local player.
		//Populate his name and player portrait
		playerName.text = PlayerStatsManager.Instance.getUserName();
		playerPortrait.setPlayerPortrait();
	}

	//The other player is on the right, so remotePlayerName.
	public void setRemotePlayerName( string name )
	{
		remotePlayerName.text = name;
	}

	//The other player is on the right, so remotePlayerPortrait.
	public void setRemotePlayerPortrait( string facebookID )
	{
		remotePlayerPortrait.setPortrait( facebookID );
	}

	public void showNoInternetPopup()
	{
		multiPurposePopup.configurePopup( "MENU_CONNECTION_FAILED_TITLE", "MENU_CONNECTION_FAILED_TEXT", "MENU_OK" );
		multiPurposePopup.display();
	}

	public void showConnectionTimedOut()
	{
		multiPurposePopup.configurePopup( "MENU_CONNECTION_FAILED_TITLE", "MENU_MP_TIMED_OUT", "MENU_OK" );
		multiPurposePopup.display();
	}

	public void play()
	{
		MPNetworkLobbyManager.mpNetworkLobbyManager.startMatch();
	}

	public void closeMenu()
	{
		StartCoroutine( close() );
	}

	IEnumerator close()
	{
		if( !levelLoading )
		{
			Debug.Log("MPLobbyMenu - closing lobby.");
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			GameManager.Instance.setMultiplayerMode( false );
			MPNetworkLobbyManager.mpNetworkLobbyManager.StopHost();
			MPNetworkLobbyManager.mpNetworkLobbyManager.StopMatchMaker();
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.WorldMap );
		}
	}

}
