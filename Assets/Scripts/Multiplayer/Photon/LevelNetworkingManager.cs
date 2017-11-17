using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using Photon;
using UnityEngine.Apple.ReplayKit;
using System.Linq;

public sealed class LevelNetworkingManager : PunBehaviour
{
	[SerializeField] HUDMultiplayer hudMultiplayer;
	[SerializeField] GameObject playerPrefab;
	[SerializeField] GameObject botPrefab;

	int numberOfPlayersReadyToRace = 0;	
	bool levelLoading = false;
	//The ground height is 1 meter.
	Vector3 leftStartPosition = new Vector3( -2f, 0f, 0 );
	Vector3 rightStartPosition = new Vector3( 2f, 0f, 0 );
	Vector3 centerStartPosition = new Vector3( 0, 0f, 0 );
	const double DELAY_TO_ALLOW_ALL_PACKETS_TO_ARRIVE = 0.5;
	
	#region Finish Line
	List<PlayerRace> playersWhoHaveCrossedTheFinishLine = new List<PlayerRace> ();
	#endregion

	#region Race duration
	//Race duration which will get displayed in the end of race screen
	public float raceDuration = 0;
	#endregion

	#region Power boost
	//The distance the player must be losing by for the power boost to activate.
	const float REQUIRED_POWER_BOOST_DISTANCE = 100f;
	#endregion

	void Awake()
	{
		adjustStartPositions();
	}

	void adjustStartPositions()
	{
		LevelData.CircuitInfo selectedCircuit = LevelManager.Instance.getSelectedCircuit().circuitInfo;
		float spawnHeight = selectedCircuit.spawnHeight;
		leftStartPosition = new Vector3( leftStartPosition.x, spawnHeight, leftStartPosition.z );
		rightStartPosition = new Vector3( rightStartPosition.x, spawnHeight, rightStartPosition.z );
		centerStartPosition = new Vector3( centerStartPosition.x, spawnHeight, centerStartPosition.z );
	}

	IEnumerator Start()
	{
		if( GameManager.Instance.isMultiplayer() )
		{
			if (playerPrefab == null)
			{
				Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it in LevelNetworkingManager.");
			}
			else
			{
				//We're in the level. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
				int playerPosition = (int) PhotonNetwork.player.CustomProperties["PlayerPosition"];
				Debug.Log("We are Instantiating LocalPlayer. He is in player position: " + playerPosition );
				Vector3 startPosition = Vector3.zero;
				if ( playerPosition == 1 )
				{
					startPosition = leftStartPosition;
				}
				else if ( playerPosition == 2 )
				{
					startPosition = rightStartPosition;
				}
				else if ( playerPosition == 3 )
				{
					startPosition = centerStartPosition;
				}
				PhotonNetwork.Instantiate(this.playerPrefab.name, startPosition, Quaternion.identity, 0);

				createBot();
				
				//The yield is to prevent having one frame of the matchmaking screen in the video
				yield return new WaitForEndOfFrame();

				//Verify if the player wants to record the race
				#if UNITY_IOS
				if( LevelManager.Instance.isRecordingSelected  )
				{
					try
					{
						ReplayKit.StartRecording();
					}
			   		catch (Exception e)
					{
						Debug.LogError( "Replay exception: " +  e.ToString() + " ReplayKit.lastError: " + ReplayKit.lastError );
			    	}
				}
				#endif

			}
		}
	}
	
	void createBot()
	{
		//the player plays offline against a bot or with a bot.
		if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstOneBot || GameManager.Instance.getPlayMode() == PlayMode.PlayCoopWithOneBot )
		{
			object[] dataBot1 = new object[1];
			dataBot1[0] = LevelManager.Instance.selectedBotHeroIndex;
			PhotonNetwork.InstantiateSceneObject(this.botPrefab.name, rightStartPosition, Quaternion.identity, 0, dataBot1 );
		}
		else if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstTwoBots )
		{
			object[] dataBot1 = new object[1];
			dataBot1[0] =  LevelManager.Instance.selectedBotHeroIndex;
			PhotonNetwork.InstantiateSceneObject(this.botPrefab.name, leftStartPosition, Quaternion.identity, 0, dataBot1 );

			object[] dataBot2 = new object[1];
			dataBot2[0] =  LevelManager.Instance.selectedBotHeroIndex2;
			PhotonNetwork.InstantiateSceneObject(this.botPrefab.name, rightStartPosition, Quaternion.identity, 0, dataBot2 );
		}
	}

	//Called when the local player left the room. We need to load the matchmaking scene.
	public override void OnLeftRoom()
	{
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}

	public override void OnPhotonPlayerDisconnected( PhotonPlayer other  )
	{
		Debug.Log( "LevelNetworkingManager: OnPhotonPlayerDisconnected() " + other.NickName ); // seen when other disconnects
		if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.NOT_STARTED )
		{
			//The race has not started yet, send everyone back to the matchmaking screen.
			string leftTheRace = LocalizationManager.Instance.getText("MULTI_LEFT_THE_RACE");
			HUDMultiplayer.hudMultiplayer.activateUserMessage( string.Format( leftTheRace, other.NickName ), 0, 2f );
			//Give time for the player to read the message before leaving the room.
			Invoke( "LeaveRoom", 3f );
		}
		else if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS )
		{
			//The race is in progress, display a message.
			//If the race is completed, we do not show any message as it is not useful to the other player(s).
			string leftTheRace = LocalizationManager.Instance.getText("MULTI_LEFT_THE_RACE");
			HUDMultiplayer.hudMultiplayer.activateUserMessage( string.Format( leftTheRace, other.NickName ), 0, 2f );
		}
	}

	void LeaveRoom()
	{
	    PhotonNetwork.LeaveRoom();
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
		
	//Only the master client gets these calls.
	//We will initiate the countdown when all of the players are ready.
	//This includes local and remote players for EACH device.
	//In a 3-player race, we have 1 local and 2 remote players per device. This means a total of 9 players.
	//We do this to make sure all of the player instances exist before starting the countdown.
	public void playerReady()
	{
		numberOfPlayersReadyToRace++;			
		if( numberOfPlayersReadyToRace == ( LevelManager.Instance.getNumberOfPlayersRequired() * LevelManager.Instance.getNumberOfPlayersRequired() ) )
		{
			object[] data = new object[1];
			data[0] = PhotonNetwork.time + DELAY_TO_ALLOW_ALL_PACKETS_TO_ARRIVE;
			this.photonView.RPC("initiateCountdown", PhotonTargets.AllViaServer, data[0] );
		} 
	}

	[PunRPC]
	void initiateCountdown( double timeWhenFirstActionShouldBeProcessed )
	{
		Debug.Log("initiateCountdown We have everyone ready. Start countdown " + (timeWhenFirstActionShouldBeProcessed - PhotonNetwork.time) );
		LockstepManager.Instance.initiateFirstAction ( timeWhenFirstActionShouldBeProcessed - PhotonNetwork.time );
	}

	void LateUpdate()
	{
		if( PhotonNetwork.isMasterClient && PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.IN_PROGRESS )
		{
			calculateRacePositions();
			calculateRaceDuration();
			verifyIfPlayerNeedsAPowerBoost();
		}
	}

	#region Race position
	//Note that calculate race positions uses the distance remaining value which gets calculated by PlayerRace in the Update method.
	void calculateRacePositions()
	{
		//Update the race position of the players.
		//The player with the smallest distance remaining is in first place.
		List<PlayerRace> players = PlayerRace.players.OrderBy( p => p.distanceRemaining ).ToList();

		for( int i = 0; i<players.Count; i++ )
		{
			//Verify if this player's position has changed.
			if( i != players[i].previousRacePosition )
			{
				//Yes, it has.
				//Save the new values
				players[i].racePosition = i;
				players[i].previousRacePosition = i;
				//If this player has not yet crossed the finish line, inform the player of his new position so that he can update it on the HUD.
				if( !players[i].playerCrossedFinishLine ) players[i].photonView.RPC("OnRacePositionChanged", PhotonTargets.AllViaServer, i );
			}
		}
	}
	#endregion

	#region Race duration
	void calculateRaceDuration()
	{
		//Calculate the race duration. It will be sent at the end of the race.
		raceDuration = raceDuration + Time.deltaTime;
	}
	#endregion

	#region Power Boost
	/// <summary>
	/// Verifies if a power boost is needed. Only called by the MasterClient.
	/// The power boost activates only once during a race.
	/// The effect of the power boost is to increase the refill rate of the power bar.
 	/// Power is what is used to play cards during a race. The power bar recharges at a constant rate up to a maximum.
	/// If a player is losing significantly, his recharge rate will be increased thus allowing him to play more cards, and hopefully get him back into the lead.
	/// In addition, the player's run speed increases for the duration.
	/// </summary>
	void verifyIfPlayerNeedsAPowerBoost()
	{
		//The emergency power boost is irrelevant if you are playing alone.
		if( PlayerRace.players.Count == 1 ) return;

		//The emergency power boost is irrelevant when playing in a coop mode since you're not racing.
		if( GameManager.Instance.isCoopPlayMode() ) return;

		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			PlayerRace pr = PlayerRace.players[i];

			//The emergency power boost only triggers once per player during a race.
			if( pr.wasPowerBoostUsed ) continue;
	
			//The emergency power never triggers for a player in first place.
			if( pr.racePosition == 0 ) continue;
	
			//Find the player who is just ahead of you.
			PlayerRace playerAhead = PlayerRace.players.Find(a => a.racePosition == pr.racePosition - 1 );

			float distanceDifference = pr.distanceRemaining -playerAhead.distanceRemaining;

			//If the player is behind by more than REQUIRED_POWER_BOOST_DISTANCE meters, activate the power boost.
			if( distanceDifference > REQUIRED_POWER_BOOST_DISTANCE )
			{
				pr.wasPowerBoostUsed = true;
				pr.GetComponent<PhotonView>().RPC("activatePowerBoostRPC", PhotonTargets.AllViaServer );
			}
		}
	}
	#endregion

	#region Finish Line
	public void playerHasCrossedFinishLine( PlayerRace playerRace )
	{
		if( PhotonNetwork.isMasterClient )
		{
			PhotonView playerPhotonView = playerRace.GetComponent<PhotonView>();

			playerPhotonView.RPC("OnRaceCompletedRPC", PhotonTargets.AllViaServer, raceDuration );

			if( !playersWhoHaveCrossedTheFinishLine.Contains(playerRace) ) playersWhoHaveCrossedTheFinishLine.Add( playerRace );

			Debug.Log ("playerHasCrossedFinishLine " + gameObject.name + " in race position " + playerRace.racePosition + " players " + PlayerRace.players.Count);

			//if this is the first player to cross the finish line, start the End of Race countdown, but only if there is more than 1 player in the race.
			if( playersWhoHaveCrossedTheFinishLine.Count == 1 &&  PlayerRace.players.Count > 1 )
			{
				playerPhotonView.RPC("StartEndOfRaceCountdownRPC", PhotonTargets.AllViaServer );
			}
			else if( playersWhoHaveCrossedTheFinishLine.Count ==  PlayerRace.players.Count )
			{
				//Every player has crossed the finish line. We can stop the countdown and return everyone to the lobby.
				PlayerRaceManager.Instance.setRaceStatus( RaceStatus.COMPLETED );
				playerPhotonView.RPC("CancelEndOfRaceCountdownRPC", PhotonTargets.AllViaServer );
			}
		}
	}
	#endregion

}
