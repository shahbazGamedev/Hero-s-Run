using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Apple.ReplayKit;

public class MatchmakingManager : MonoBehaviour {

	[Header("General")]
	[SerializeField] GameObject endOfGameCanvas;
	[SerializeField] Image backgroundImage;
	[SerializeField] Button playButton;
	[SerializeField] Text playButtonText;
	[SerializeField] Text versusText;
	[Tooltip("The label to inform the user that the connection progress.")]
	[SerializeField] Text connectionProgress;
	[Tooltip("The selected Photon cloud region. Used for debugging.")]
	[SerializeField] Text PhotonCloudRegionText;

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

	void Awake ()
	{
		Screen.orientation = ScreenOrientation.Portrait;
	}

	void Start ()
	{
		Handheld.StopActivityIndicator();

		//Reset the player match data
		LevelManager.Instance.playerMatchDataList.Clear();

		originalPlayButtonTextColor = playButtonText.color;
		//Configure the lobby according to the number of players in the race
		configureLobby();

		//Make sure the audio listener is not paused.
		//It gets set to true when you Pause the game in offline mode.
		//Same for the timeScale.
		AudioListener.pause = false;
		Time.timeScale = 1f;
		Time.fixedDeltaTime = GameManager.DEFAULT_FIXED_DELTA_TIME;

		//The left portrait is always the local player.
		configureLocalPlayerData();

		//Player Match Data is used by results screen at the end of the race.
		PlayerMatchData pmd = new PlayerMatchData( GameManager.Instance.playerProfile.getUserName(), GameManager.Instance.playerProfile.getPlayerIconId(), GameManager.Instance.playerProfile.getLevel(), GameManager.Instance.playerStatistics.getStatisticData(StatisticDataType.CURRENT_WIN_STREAK) );
		LevelManager.Instance.playerMatchDataList.Add( pmd );

		//Localize
		playButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_PLAY" );

		//Only show the Photon Cloud Region text when playing in an online mode.
		PhotonCloudRegionText.gameObject.SetActive( GameManager.Instance.isOnlinePlayMode() );

		//If we are returning to the lobby after a race has completed, show the end of game screen which displays XP awarded
		if( GameManager.Instance.getGameState() == GameState.MultiplayerEndOfGame )
		{
			endOfGameCanvas.SetActive( true );
		}
		else
		{
			//If we are playing a 2 or 3 player online multiplayer match, the level is
			//selected randomly (excluding level zero which is the training level).
			//If the player is playing alone or against AI, the race track has been selected in
			//the circuit selection screen.
			//If the player is inviting a friend, they will race in a track based on the inviter's number of trophies.
			if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstOnePlayer || GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstTwoPlayers )
			{
				LevelManager.Instance.setSelectedCircuit( LevelManager.Instance.getLevelData().getRandomMap() );
			}
			else if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstOneFriend )
			{
				//Use the race track name saved in the match data
				LevelManager.Instance.setSelectedCircuit( LevelManager.Instance.getLevelData().getMapByName( LevelManager.Instance.matchData.mapName ) );
			}
			else if( GameManager.Instance.isCoopPlayMode() )
			{
				LevelManager.Instance.setSelectedCircuit( LevelManager.Instance.getLevelData().getRandomCoopMap() );
			}

			if( !GameManager.Instance.isOnlinePlayMode() || GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstOneFriend )
			{
				//Only configure the circuit/map image and title and etc. when offline or when you are inviting a friend to a match.
				//For online competition matches, the circuit/map image and title and etc. will get updated when a remote player connects.
				configureCircuitData( LevelManager.Instance.getSelectedCircuit().circuitInfo );
			}
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

			case PlayMode.PlayAgainstOneBot:
			case PlayMode.PlayAgainstOnePlayer:
			case PlayMode.PlayAgainstOneFriend:
				twoPlayerPanel.SetActive( true );
				//Use default values for remote player portrait until he connects
				versusText.text = LocalizationManager.Instance.getText( "CIRCUIT_VERSUS" );
				setRemotePlayerData( 1, LocalizationManager.Instance.getText( "CIRCUIT_OPPONENT" ), 1, 0  );
				break;
			case PlayMode.PlayCoopWithOneBot:
			case PlayMode.PlayCoopWithOnePlayer:
				twoPlayerPanel.SetActive( true );
				//Use default values for remote player portrait until he connects
				versusText.text = LocalizationManager.Instance.getText( "CIRCUIT_COOP_WITH" );
				setRemotePlayerData( 1, LocalizationManager.Instance.getText( "CIRCUIT_COOP_PARTNER" ), 1, 0  );
				break;

			case PlayMode.PlayAgainstTwoBots:
			case PlayMode.PlayAgainstTwoPlayers:
				threePlayerPanel.SetActive( true );
				//Use default values for remote player portraits until they connect
				//Note: The word Vs. is not displayed in the 3 player panel.
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
				playerName1P.text = GameManager.Instance.playerProfile.getUserName();
				playerIcon1P.GetComponent<Outline>().effectColor = frameColor;
				playerIcon1P.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( GameManager.Instance.playerProfile.getPlayerIconId() ).icon;
			break;

			case PlayMode.PlayAgainstOneBot:
			case PlayMode.PlayAgainstOnePlayer:
			case PlayMode.PlayAgainstOneFriend:
			case PlayMode.PlayCoopWithOneBot:
			case PlayMode.PlayCoopWithOnePlayer:
				playerName2P.text = GameManager.Instance.playerProfile.getUserName();
				playerIcon2P.GetComponent<Outline>().effectColor = frameColor;
				playerIcon2P.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( GameManager.Instance.playerProfile.getPlayerIconId() ).icon;
			break;

			case PlayMode.PlayAgainstTwoBots:
			case PlayMode.PlayAgainstTwoPlayers:
				playerName3P.text = GameManager.Instance.playerProfile.getUserName();
				playerIcon3P.GetComponent<Outline>().effectColor = frameColor;
				playerIcon3P.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( GameManager.Instance.playerProfile.getPlayerIconId() ).icon;
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
			case PlayMode.PlayAgainstOneBot:
			case PlayMode.PlayAgainstOnePlayer:
			case PlayMode.PlayAgainstOneFriend:
			case PlayMode.PlayCoopWithOneBot:
			case PlayMode.PlayCoopWithOnePlayer:
				remotePlayer1Name2P.text = name;
				remotePlayer1Icon2P.GetComponent<Outline>().effectColor = frameColor;
				remotePlayer1Icon2P.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( iconId ).icon;
			break;

			case PlayMode.PlayAgainstTwoBots:
			case PlayMode.PlayAgainstTwoPlayers:
				if( index == 1 )
				{
					remotePlayer1Name3P.text = name;
					remotePlayer1Icon3P.GetComponent<Outline>().effectColor = frameColor;
					remotePlayer1Icon3P.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( iconId ).icon;
				}
				else if( index == 2 )
				{
					remotePlayer2Name3P.text = name;
					remotePlayer2Icon3P.GetComponent<Outline>().effectColor = frameColor;
					remotePlayer2Icon3P.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( iconId ).icon;
				}
			break;
		}
	}

	public void configureCircuitData( LevelData.CircuitInfo circuitInfo )
	{
		string sectorName = LocalizationManager.Instance.getText( "MAP_" + circuitInfo.mapNumber.ToString() );
		circuitName.text = sectorName;
		circuitImage.color = Color.white;
		circuitImage.sprite = circuitInfo.circuitImage;
		backgroundImage.color = circuitInfo.backgroundColor;
		enablePlayButton( true );
	}

	public void setConnectionProgress( string value )
	{
		if( connectionProgress != null ) connectionProgress.text = value;
	}

	public void setPhotonCloudRegionText( string region )
	{
		PhotonCloudRegionText.text = region;
	}

	public void showNoInternetPopup()
	{
		MultiPurposePopup.Instance.displayPopup( "MENU_NO_INTERNET" );
		enablePlayButton( true );
	}

	public void OnClickPlay()
	{
		UISoundManager.uiSoundManager.playButtonClick();
 		enablePlayButton( false );
		MPNetworkLobbyManager.Instance.startMatch();
	}

	//Do not allow the player to exit the matchmaking screen if he has initiated matchmaking and a remote player has joined.
	//If the player still wants to quit, he will be able to do so via the pause menu.
	//Also, hide the preloader.
	public void enableExitButton( bool enable )
	{
		if( enable )
		{
			UniversalTopBar.Instance.enableCloseButton( true );
		}
		else
		{
			enablePreloader( false );
			UniversalTopBar.Instance.enableCloseButton( false );
		}
	}

	public void OnClickReturnToMainMenu()
	{
		if( PhotonNetwork.inRoom ) PhotonNetwork.LeaveRoom();
		LevelManager.Instance.matchData = null;
		#if UNITY_IOS
		//When returning to the main menu, discard any video that might have been recorded
		if( ReplayKit.APIAvailable ) ReplayKit.Discard();
		#endif
		StartCoroutine( loadScene(GameScenes.MainMenu) );
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

	public void hidePlayButton()
	{
		playButton.gameObject.SetActive( false );
	}

	public void enablePlayButton( bool enable )
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

}
