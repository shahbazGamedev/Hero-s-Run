using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MPLobbyMenu : MonoBehaviour {

	public MultiPurposePopup multiPurposePopup;
	public Text playerName1;
	public Text playerName2;
	public FacebookPortraitHandler portrait1;
	public FacebookPortraitHandler portrait2;
	bool levelLoading = false;

	// Use this for initialization
	void Awake ()
	{
		Handheld.StopActivityIndicator();
	}

	public void setPlayerName( int playerIndex, string name )
	{
		if( playerIndex == 1 ) playerName1.text = name;
		if( playerIndex == 2 ) playerName2.text = name;
	}

	//The local player is always on the left, so portrait1.
	public void setPlayerPortrait( string facebookID )
	{
		portrait1.setPortrait( facebookID );
	}

	//The other player is on the right, so portrait2.
	public void setOtherPortrait( string facebookID )
	{
		portrait2.setPortrait( facebookID );
	}

	public void showNoInternetPopup()
	{
		multiPurposePopup.configurePopup( "MENU_CONNECTION_FAILED_TITLE", "MENU_CONNECTION_FAILED_TEXT", "MENU_OK" );
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
