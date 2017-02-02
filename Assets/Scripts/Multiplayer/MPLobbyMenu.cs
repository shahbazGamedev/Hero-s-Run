using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MPLobbyMenu : MonoBehaviour {

	[Header("General")]
	bool levelLoading = false;
	[SerializeField] GameObject endOfGameCanvas;
	[SerializeField] MultiPurposePopup multiPurposePopup;
	[SerializeField] Button playButton;
	[SerializeField] Text playButtonText;
	[SerializeField] Text exitButtonText;
	[SerializeField] Text playerName;
	[SerializeField] Text versusText;
	[SerializeField] Text remotePlayerName;
	[SerializeField] FacebookPortraitHandler playerPortrait;
	[SerializeField] FacebookPortraitHandler remotePlayerPortrait;

	[Header("Preloader")]
	[SerializeField] GameObject preloader; //displays while looking for a match

	[Header("Circuit")]
	[SerializeField] Text circuitName;
	[SerializeField] Image circuitImage;
	[SerializeField] Text entryFee;

	private Color originalPlayButtonTextColor;

	void Start ()
	{
		originalPlayButtonTextColor = playButtonText.color;

		//The left portrait is always the local player.
		//Populate his name and player portrait
		playerName.text = PlayerStatsManager.Instance.getUserName();
		setPlayerFrame( GameManager.Instance.playerProfile.getLevel() );
		remotePlayerName.text = LocalizationManager.Instance.getText( "CIRCUIT_OPPONENT" );
		versusText.text = LocalizationManager.Instance.getText( "CIRCUIT_VERSUS" );
		playButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_PLAY" );
		exitButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_EXIT" );
		playerPortrait.setPlayerPortrait();

		//If we are returning to the lobby after a race has completed, show the end of game screen which displays XP awarded
		if( GameManager.Instance.getGameState() == GameState.MultiplayerEndOfGame )
		{
			endOfGameCanvas.SetActive( true );
		}
		else
		{
			CarouselEntry selected = LevelManager.Instance.selectedRaceDetails;
			configureCircuitData( selected.circuitImage.sprite, selected.circuitName.text, selected.entryFee.text );
			endOfGameCanvas.SetActive( false );
		}
		
	}

	public void setPlayerFrame( int level )
	{
		Color frameColor = ProgressionManager.Instance.getFrameColor( level );
		playerPortrait.GetComponent<Outline>().effectColor = frameColor;
	}

	public void setRemoteFrame( int level )
	{
		Color frameColor = ProgressionManager.Instance.getFrameColor( level );
		remotePlayerPortrait.GetComponent<Outline>().effectColor = frameColor;
	}

	public void configureCircuitData( Sprite circuitImageSprite, string circuitNameString, string entryFeeString )
	{
		circuitName.text = circuitNameString;
		circuitImage.sprite = circuitImageSprite;
		entryFee.text = entryFeeString;
		enablePlayButton( true );
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
		enablePlayButton( true );
	}

	public void showConnectionTimedOut()
	{
		multiPurposePopup.configurePopup( "MENU_CONNECTION_FAILED_TITLE", "MENU_MP_TIMED_OUT", "MENU_OK" );
		multiPurposePopup.display();
		enablePlayButton( true );
	}

	public void showUnableToCreateMatch()
	{
		multiPurposePopup.configurePopup( "MENU_CONNECTION_FAILED_TITLE", "MENU_MP_UNABLE_CREATE_MATCH", "MENU_OK" );
		multiPurposePopup.display();
		enablePlayButton( true );
	}

	public void showUnableToJoinMatch()
	{
		multiPurposePopup.configurePopup( "MENU_CONNECTION_FAILED_TITLE", "MENU_MP_UNABLE_JOIN_MATCH", "MENU_OK" );
		multiPurposePopup.display();
		enablePlayButton( true );
	}

	public void OnClickPlay()
	{
		UISoundManager.uiSoundManager.playButtonClick();
 		GameManager.Instance.setGameState( GameState.WorldMapNoPopup );
		if( playerCanPayEntryFee() )
		{
			enablePlayButton( false );
			MPNetworkLobbyManager.Instance.startMatch();
		}
		else
		{
			//Player does not have enough for entry fee. Open the store.
			StoreManager.Instance.showStore(StoreTab.Store,StoreReason.Need_Stars);
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

	public void OnClickReturnToHeroSelection()
	{
		StartCoroutine( loadScene(GameScenes.HeroSelection) );
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

	void enablePlayButton( bool enable )
	{
		if( enable )
		{
			playButton.interactable = true;
			playButtonText.color = originalPlayButtonTextColor;
			preloader.SetActive( false );
		}
		else
		{
			playButton.interactable = false;
			playButtonText.color = Color.gray;
			preloader.SetActive( true );
		}
	}
}
