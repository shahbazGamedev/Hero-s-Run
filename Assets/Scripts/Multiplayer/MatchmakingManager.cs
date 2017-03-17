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

	[Header("Circuit")]
	[SerializeField] Text circuitName;
	[SerializeField] Image circuitImage;

	[Header("Single Player Panel")]
	[SerializeField] GameObject singlePlayerPanel;
	[Header("Local Player")]
	[SerializeField] Image playerIcon1P;
	[SerializeField] Text playerName1P;

	[Header("Two Player Panel")]
	[SerializeField] GameObject twoPlayerPanel;
	[Header("Local Player")]
	[SerializeField] Image playerIcon2P;
	[SerializeField] Text playerName2P;
	[Header("Remote Player 1")]
	[SerializeField] Image remotePlayer1Icon2P;
	[SerializeField] Text remotePlayer1Name2P;
	[SerializeField] GameObject preloader12P; //animates while looking for an opponent for the match

	[Header("Three Player Panel")]
	[SerializeField] GameObject threePlayerPanel;
	[Header("Local Player")]
	[SerializeField] Image playerIcon3P;
	[SerializeField] Text playerName3P;
	[Header("Remote Player 1")]
	[SerializeField] Image remotePlayer1Icon3P;
	[SerializeField] Text remotePlayer1Name3P;
	[SerializeField] GameObject preloader13P; //animates while looking for an opponent for the match
	[Header("Remote Player 2")]
	[SerializeField] Image remotePlayer2Icon3P;
	[SerializeField] Text remotePlayer2Name3P;
	[SerializeField] GameObject preloader23P; //animates while looking for an opponent for the match

	bool levelLoading = false;
	private Color originalPlayButtonTextColor;

	void Start ()
	{
		Handheld.StopActivityIndicator();
		originalPlayButtonTextColor = playButtonText.color;

		//Configure the lobby according to the number of players in the race
		configureLobby();

		//The left portrait is always the local player.
		configureLocalPlayerData();

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
			CircuitDetails selected = LevelManager.Instance.selectedRaceDetails;
			configureCircuitData( selected.circuitImage.sprite, selected.circuitName.text );
			endOfGameCanvas.SetActive( false );
		}
	}

	void configureLobby()
	{
		singlePlayerPanel.SetActive( false );
		twoPlayerPanel.SetActive( false );
		threePlayerPanel.SetActive( false );

		switch ( GameManager.Instance.getPlayMode() )
		{
			case PlayMode.PlayAlone:
				singlePlayerPanel.SetActive( true );
			break;

			case PlayMode.PlayAgainstEnemy:
			case PlayMode.PlayOthers:
			case PlayMode.PlayWithFriends:
				twoPlayerPanel.SetActive( true );
				//Use default values for remote player portrait until he connects
				setRemotePlayerData( 1, LocalizationManager.Instance.getText( "CIRCUIT_OPPONENT" ), 1, 0  );
			break;

			case PlayMode.PlayThreePlayers:
				threePlayerPanel.SetActive( true );
				//Use default values for remote player portraits until they connect
				setRemotePlayerData( 1, LocalizationManager.Instance.getText( "CIRCUIT_OPPONENT" ), 1, 0  );
				setRemotePlayerData( 2, LocalizationManager.Instance.getText( "CIRCUIT_OPPONENT" ), 1, 0  );
			break;
		}
	}

	public void configureLocalPlayerData()
	{
		//Frame based on level
		Color frameColor = ProgressionManager.Instance.getFrameColor( GameManager.Instance.playerProfile.getLevel() );

		switch ( GameManager.Instance.getPlayMode() )
		{
			case PlayMode.PlayAlone:
				playerName1P.text = PlayerStatsManager.Instance.getUserName();
				playerIcon1P.GetComponent<Outline>().effectColor = frameColor;
				playerIcon1P.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( GameManager.Instance.playerProfile.getPlayerIconId() ).icon;
			break;

			case PlayMode.PlayAgainstEnemy:
			case PlayMode.PlayOthers:
			case PlayMode.PlayWithFriends:
				playerName2P.text = PlayerStatsManager.Instance.getUserName();
				playerIcon2P.GetComponent<Outline>().effectColor = frameColor;
				playerIcon2P.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( GameManager.Instance.playerProfile.getPlayerIconId() ).icon;
			break;

			case PlayMode.PlayThreePlayers:
				playerName3P.text = PlayerStatsManager.Instance.getUserName();
				playerIcon3P.GetComponent<Outline>().effectColor = frameColor;
				playerIcon3P.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( GameManager.Instance.playerProfile.getPlayerIconId() ).icon;
			break;
		}
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

		//Frame based on level
		Color frameColor = ProgressionManager.Instance.getFrameColor( level );

		switch ( GameManager.Instance.getPlayMode() )
		{
			case PlayMode.PlayAgainstEnemy:
			case PlayMode.PlayOthers:
			case PlayMode.PlayWithFriends:
				remotePlayer1Name2P.text = name;
				remotePlayer1Icon2P.GetComponent<Outline>().effectColor = frameColor;
				remotePlayer1Icon2P.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( iconId ).icon;
			break;

			case PlayMode.PlayThreePlayers:
				if( index == 1 )
				{
					remotePlayer1Name3P.text = name;
					remotePlayer1Icon3P.GetComponent<Outline>().effectColor = frameColor;
					remotePlayer1Icon3P.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( iconId ).icon;
				}
				else if( index == 2 )
				{
					remotePlayer2Name3P.text = name;
					remotePlayer2Icon3P.GetComponent<Outline>().effectColor = frameColor;
					remotePlayer2Icon3P.sprite = ProgressionManager.Instance.getPlayerIconDataByUniqueId( iconId ).icon;
				}
			break;
		}
	}

	public void configureCircuitData( Sprite circuitImageSprite, string circuitNameString )
	{
		circuitName.text = circuitNameString;
		circuitImage.sprite = circuitImageSprite;
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
		enablePlayButton( false );
		MPNetworkLobbyManager.Instance.startMatch();
	}

	public void OnClickShowStore()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		StoreManager.Instance.showStore( StoreTab.Store, StoreReason.None );
	}

	//Do not allow the player to exit the matchmaking screen if he has initiated matchmaking and a remote player has joined.
	//If the player still wants to quit, he will be able to do so via the pause menu.
	//Also, hide the preloader.
	public void disableExitButton()
	{
		enablePreloader( false );
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
			enablePreloader( false );
		}
		else
		{
			playButton.interactable = false;
			playButtonText.color = Color.gray;
			enablePreloader( true );
		}
	}

	void enablePreloader( bool enable )
	{
		if( enable )
		{
			preloader12P.SetActive( true );
			preloader13P.SetActive( true );
			preloader23P.SetActive( true );
		}
		else
		{
			preloader12P.SetActive( false );
			preloader13P.SetActive( false );
			preloader23P.SetActive( false );
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
