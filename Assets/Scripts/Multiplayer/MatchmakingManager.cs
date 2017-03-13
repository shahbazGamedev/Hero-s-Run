using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MatchmakingManager : MonoBehaviour {

	[Header("General")]
	[SerializeField] GameObject endOfGameCanvas;
	[SerializeField] MultiPurposePopup multiPurposePopup;
	[SerializeField] Button playButton;
	[SerializeField] Text playButtonText;
	[SerializeField] Button exitButton;
	[SerializeField] Text exitButtonText;
	[SerializeField] Text versusText;
	[Tooltip("The label to inform the user that the connection progress.")]
	[SerializeField] Text connectionProgress;
	[Header("Local Player")]
	[SerializeField] Image playerIcon;
	[SerializeField] Text playerName;
	[Header("Remote Player 1")]
	[SerializeField] Image remotePlayerIcon1;
	[SerializeField] Text remotePlayerName1;
	[SerializeField] GameObject preloader1; //animates while looking for an opponent for the match
	[Header("Remote Player 2")]
	[SerializeField] Image remotePlayerIcon2;
	[SerializeField] Text remotePlayerName2;
	[SerializeField] GameObject preloader2; //animates while looking for an opponent for the match
	[Header("Circuit")]
	[SerializeField] Text circuitName;
	[SerializeField] Image circuitImage;
	[SerializeField] Text entryFee;

	bool levelLoading = false;
	private Color originalPlayButtonTextColor;


	void Start ()
	{
		Handheld.StopActivityIndicator();
		originalPlayButtonTextColor = playButtonText.color;

		//The left portrait is always the local player.
		configureLocalPlayerData();

		//Use default values for remote player portraits until they connect
		setRemotePlayerData( 1, LocalizationManager.Instance.getText( "CIRCUIT_OPPONENT" ), 1, 0  );
		setRemotePlayerData( 2, LocalizationManager.Instance.getText( "CIRCUIT_OPPONENT" ), 1, 0  );

		//Localize
		versusText.text = LocalizationManager.Instance.getText( "CIRCUIT_VERSUS" );
		playButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_PLAY" );
		exitButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_EXIT" );

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

	public void configureLocalPlayerData()
	{
		//Name
		playerName.text = PlayerStatsManager.Instance.getUserName();
		//Frame based on level
		Color frameColor = ProgressionManager.Instance.getFrameColor( GameManager.Instance.playerProfile.getLevel() );
		playerIcon.GetComponent<Outline>().effectColor = frameColor;
		//Player Icon
		playerIcon.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( GameManager.Instance.playerProfile.getPlayerIconId() ).icon;
	}

	/// <summary>
	/// Sets the remote player data for the specified index.
	/// In a 2-player match, there is the local player and one remote player.
	/// In a 3-player match, there is the local player and two remote players.
	/// Use an index of 1 for remote player one and 2 for remote player two.
	/// </summary>
	/// <param name="remotePlayerIndex">Remote player index.</param>
	public void setRemotePlayerData( int index, string name, int level, int iconId  )
	{
		Debug.Log("MatchmakingManager-setRemotePlayerData Index: " + index + " Name: " + name + " Level: " + level + " Icon ID: " + iconId );
		switch ( index )
		{
			//Remote PLayer 1
			case 1:
				//Name
				remotePlayerName1.text = name;
				//Frame based on level
				Color frameColor = ProgressionManager.Instance.getFrameColor( level );
				remotePlayerIcon1.GetComponent<Outline>().effectColor = frameColor;
				//Player Icon
				remotePlayerIcon1.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( iconId ).icon;
			break;

			//Remote PLayer 2
			case 2:
				//Name
				remotePlayerName2.text = name;
				//Frame based on level
				frameColor = ProgressionManager.Instance.getFrameColor( level );
				remotePlayerIcon2.GetComponent<Outline>().effectColor = frameColor;
				//Player Icon
				remotePlayerIcon2.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( iconId ).icon;
			break;
		}
	}

	public void hideRemotePlayer()
	{
		remotePlayerIcon1.gameObject.SetActive( false );
		versusText.gameObject.SetActive( false );
	}

	public void configureCircuitData( Sprite circuitImageSprite, string circuitNameString, string entryFeeString )
	{
		circuitName.text = circuitNameString;
		circuitImage.sprite = circuitImageSprite;
		entryFee.text = entryFeeString;
		enablePlayButton( true );
	}

	public void setConnectionProgress( string value )
	{
		if( connectionProgress != null ) connectionProgress.text = value;
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

	public void OnClickPlay()
	{
		UISoundManager.uiSoundManager.playButtonClick();
 		if( playerCanPayEntryFee() )
		{
			enablePlayButton( false );
			MPNetworkLobbyManager.Instance.startMatch();
		}
		else
		{
			//Player does not have enough for entry fee. Open the store.
			StoreManager.Instance.showStore(StoreTab.Store,StoreReason.Need_Coins);
		}

	}

	public void OnClickShowStore()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		StoreManager.Instance.showStore( StoreTab.Store, StoreReason.None );
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

	//Do not allow the player to exit the matchmaking screen if he has initiated matchmaking and a remote player has joined.
	//If the player still wants to quit, he will be able to do so via the pause menu.
	//Also, hide the preloader.
	public void disableExitButton()
	{
		preloader1.SetActive( false );
		preloader2.SetActive( false );
		enableExitButton( false );
	}

	public void OnClickReturnToHeroSelection()
	{
		if( PhotonNetwork.inRoom ) PhotonNetwork.LeaveRoom();
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
			preloader1.SetActive( false );
			preloader2.SetActive( false );
		}
		else
		{
			playButton.interactable = false;
			playButtonText.color = Color.gray;
			preloader1.SetActive( true );
			preloader2.SetActive( true );
		}
	}

	void enableExitButton( bool enable )
	{
		if( enable )
		{
			exitButton.interactable = true;
			exitButtonText.color = originalPlayButtonTextColor;
		}
		else
		{
			exitButton.interactable = false;
			exitButtonText.color = Color.gray;
		}
	}

}
