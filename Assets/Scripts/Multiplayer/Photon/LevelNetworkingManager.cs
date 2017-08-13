using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using Photon;
using UnityEngine.Apple.ReplayKit;

public class LevelNetworkingManager : PunBehaviour
{
	[SerializeField] HUDMultiplayer hudMultiplayer;
	[SerializeField] GameObject playerPrefab;
	[SerializeField] GameObject botPrefab;

	int numberOfPlayersReadyToRace = 0;	
	bool levelLoading = false;
	//The ground height is 1 meter.
	Vector3 leftStartPosition = new Vector3( -2f, 1f, 0 );
	Vector3 rightStartPosition = new Vector3( 2f, 1f, 0 );
	Vector3 centerStartPosition = new Vector3( 0, 1f, 0 );
	const double DELAY_TO_ALLOW_ALL_PACKETS_TO_ARRIVE = 0.5;
	
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
		//In the play against enemy mode, the player plays offline against a bot.
		if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstEnemy )
		{
			object[] dataBot1 = new object[1];
			dataBot1[0] = LevelManager.Instance.selectedBotHeroIndex;
			PhotonNetwork.InstantiateSceneObject(this.botPrefab.name, rightStartPosition, Quaternion.identity, 0, dataBot1 );
		}
		else if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstTwoEnemies )
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
		//Only display the disconnect message on the HUD if the race is not completed
		if( PlayerRaceManager.Instance.getRaceStatus() != RaceStatus.COMPLETED )
		{
			string leftTheRace = LocalizationManager.Instance.getText("MULTI_LEFT_THE_RACE");
			HUDMultiplayer.hudMultiplayer.activateUserMessage( string.Format( leftTheRace, other.NickName ), 0, 2f );
		}
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

	//Not used yet. Need to hook up to pause menu and save-me menu.
	public void OnClickLeaveRoom()
	{
	    PhotonNetwork.LeaveRoom();
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

}
