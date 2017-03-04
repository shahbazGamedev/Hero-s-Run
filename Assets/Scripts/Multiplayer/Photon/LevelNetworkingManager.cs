using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using Photon;

public class LevelNetworkingManager : PunBehaviour
{
	[SerializeField] HUDMultiplayer hudMultiplayer;
	[SerializeField] GameObject playerPrefab;
	[SerializeField] GameObject botPrefab;

	int numberOfPlayersReadyToRace = 0;	
	bool levelLoading = false;
	Vector3 leftStartPosition = new Vector3( -1.3f, 0.57f, 0 );
	Vector3 rightStartPosition = new Vector3( 1.3f, 0.57f, 0 );
	Vector3 centerStartPosition = new Vector3( 0, 0.57f, 0 );
	
	void Start()
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
			}
		}
	}
	
	void createBot()
	{
		//In the play against enemy mode, the player plays offline against a bot.
		if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstEnemy )
		{
			PhotonNetwork.InstantiateSceneObject(this.botPrefab.name, rightStartPosition, Quaternion.identity, 0, null );
		}
	}

	//Called when the local player left the room. We need to load the matchmaking scene.
	public void OnLeftRoom()
	{
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
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
		
	//Only the master client gets these calls
	public void playerReady()
	{
		numberOfPlayersReadyToRace++;			
		Debug.Log("playerReady " + numberOfPlayersReadyToRace );
		if( numberOfPlayersReadyToRace == LevelManager.Instance.getNumberOfPlayersRequired() )
		{
			Debug.Log("RPC countdown" );
			this.photonView.RPC("initiateCountdown", PhotonTargets.AllViaServer, null );
		} 
	}

	[PunRPC]
	void initiateCountdown()
	{
		Debug.Log("initiateCountdown We have everyone ready. Start countdown" );
		hudMultiplayer.startCountdown();
	}


}
