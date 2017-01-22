using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MPLobbyMenu : MonoBehaviour {

	public GameObject carouselCanvas;
	public GameObject lobbyManager;
	public MultiPurposePopup multiPurposePopup;
	public Button playButton;
	public Text playButtonText;
	public Text exitButtonText;
	public Text playerName;
	public Text versusText;
	public Text remotePlayerName;
	public FacebookPortraitHandler playerPortrait;
	public FacebookPortraitHandler remotePlayerPortrait;

	//Circuit
	public Text circuitName;
	public Image circuitImage;
	public Text entryFee;

	StoreManager storeManager;

	void Start ()
	{
		GameObject storeManagerObject = GameObject.FindGameObjectWithTag("Store");
		storeManager = storeManagerObject.GetComponent<StoreManager>();

		//The left portrait is always the local player.
		//Populate his name and player portrait
		playerName.text = PlayerStatsManager.Instance.getUserName();
		remotePlayerName.text = LocalizationManager.Instance.getText( "CIRCUIT_OPPONENT" );
		versusText.text = LocalizationManager.Instance.getText( "CIRCUIT_VERSUS" );
		playButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_PLAY" );
		exitButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_EXIT" );
		playerPortrait.setPlayerPortrait();
	}

	public void configureCircuitData( Sprite circuitImageSprite, string circuitNameString, string entryFeeString )
	{
		circuitName.text = circuitNameString;
		circuitImage.sprite = circuitImageSprite;
		entryFee.text = entryFeeString;
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
		playButton.interactable = true;
	}

	public void showConnectionTimedOut()
	{
		multiPurposePopup.configurePopup( "MENU_CONNECTION_FAILED_TITLE", "MENU_MP_TIMED_OUT", "MENU_OK" );
		multiPurposePopup.display();
		playButton.interactable = true;
	}

	public void showUnableToCreateMatch()
	{
		multiPurposePopup.configurePopup( "MENU_CONNECTION_FAILED_TITLE", "MENU_MP_UNABLE_CREATE_MATCH", "MENU_OK" );
		multiPurposePopup.display();
		playButton.interactable = true;
	}

	public void showUnableToJoinMatch()
	{
		multiPurposePopup.configurePopup( "MENU_CONNECTION_FAILED_TITLE", "MENU_MP_UNABLE_JOIN_MATCH", "MENU_OK" );
		multiPurposePopup.display();
		playButton.interactable = true;
	}

	public void OnClickPlay()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		if( playerCanPayEntryFee() )
		{
			playButton.interactable = false;
			playButtonText.color = Color.gray;
			MPNetworkLobbyManager.mpNetworkLobbyManager.startMatch();
		}
		else
		{
			//Player does not have enough for entry fee. Open the store.
			storeManager.showStore(StoreTab.Store,StoreReason.Need_Stars);
		}

	}

	bool playerCanPayEntryFee()
	{
		LevelData.MultiplayerInfo multiplayerInfo = LevelManager.Instance.getSelectedMultiplayerLevel();
		//Validate if the player has enough currency for the entry fee
		int entryFee = multiplayerInfo.circuitInfo.entryFee;
		if( entryFee <= PlayerStatsManager.Instance.getCurrentCoins() )
		{
			//Yes, he has enough
			return true;
		}
		else
		{
			//No, he does not have enough
			return false;
		}
	}

	public void OnClickCloseMenu()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		carouselCanvas.SetActive( true );
	}

}
