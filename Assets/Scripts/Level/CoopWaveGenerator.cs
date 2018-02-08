using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Cinemachine;

//The version of CoopWaveGenerator to use is always the Master one. Make sure that all public methods check for isMaster before executing.
//Note that all devices have an instance of the CoopWaveGenerator in case a player quits and the Master authority needs to be transfered to another player.
public sealed class CoopWaveGenerator : PunBehaviour {

	#region Waves
	[SerializeField] List<GameObject> waveList = new List<GameObject>();
	public static int numberOfWavesTriggered = 0;
	public delegate void CoopNewWave( int waveNumber );
	public static event CoopNewWave coopNewWave;
	#endregion

	#region Players
	[SerializeField] List<Transform> deadPlayerList = new List<Transform>();
	#endregion

	#region Other
	public static CoopWaveGenerator Instance;
	ZombieManager zombieManager;
	Coroutine spectatingCoroutine;
	const float DELAY_BEFORE_SPECTATING = 2.25f;
	public const int SOFT_CURRENCY_EARNED_PER_WAVE = 2;
	#endregion

	void Awake ()
	{
		if( GameManager.Instance.isCoopPlayMode() )
		{
			Instance = this;
		}
		else
		{
			Destroy( gameObject );
		}
	}

	void Start ()
	{
		zombieManager = GameObject.FindObjectOfType<ZombieManager>();
		//important to reset since this is a static value.
		numberOfWavesTriggered = 0;
	}

	#region Waves
	public void activateNewWave ()
	{
		if( PhotonNetwork.isMasterClient && GameManager.Instance.isCoopPlayMode() )
		{
			createWave( waveList[numberOfWavesTriggered] );
		}
	}

	void createWave( GameObject wave )
	{
		//trigger zombie wave takes care of instantiating the zombies using Photon.
		GameObject clone = Instantiate( wave );
		ZombieWave activeZombieWave = clone.GetComponent<ZombieWave>();
		//To facilitate testing
		DebugInfoType debugInfoType = GameManager.Instance.playerDebugConfiguration.getDebugInfoType();
		if( debugInfoType != DebugInfoType.DONT_SPAWN_ZOMBIES ) zombieManager.triggerZombieWave( activeZombieWave.spawnLocations );
		photonView.RPC("coopWaveRPC", PhotonTargets.All );
	}

	[PunRPC]
	void coopWaveRPC()
	{
		numberOfWavesTriggered++;

		Debug.Log("CoopWaveGenerator-coopWaveRPC: " + numberOfWavesTriggered );

		if( spectatingCoroutine != null ) StopCoroutine( spectatingCoroutine );

		//Send a new wave event to interested classes.
		if( coopNewWave != null ) coopNewWave( numberOfWavesTriggered );

	}
	#endregion

	#region Players
	public void playerDied( Transform player )
	{
		if( PlayerRaceManager.Instance.getRaceStatus() != RaceStatus.COMPLETED )
		{
			if( !deadPlayerList.Contains( player ) )
			{
				//Update the number of downs regardless of whether you are master or not.
				PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( player.name );
				pmd.downs++;

				//Master maintains the dead player list and takes decisions accordingly.
				if( PhotonNetwork.isMasterClient )
				{
					deadPlayerList.Add( player );
					Debug.Log( player.name + " ADDED to list of dead players on MASTER. The count of dead players is: " + deadPlayerList.Count );
	
					//There are normally 2 players in the Coop mode.
					//However, a player may have quit the match.
					//In that case, allow the remaining player to continue until he dies,
					//and then it will be game over.
					if( PlayerRace.players.Count == 2 )
					{
						//We have 2 players.
						if( deadPlayerList.Count == 2 )
						{
							PlayerRaceManager.Instance.setRaceStatus( RaceStatus.COMPLETED );
							//Both players are dead, this means game over. Send an RPC.
							photonView.RPC("coopGameOverRPC", PhotonTargets.All );
						}
						else
						{
							//Only one player is dead so the match continues.
							//Start spectating his partner.
							photonView.RPC("spectatingRPC", PhotonTargets.All, player.GetComponent<PhotonView>().viewID );	
						}
					}
					else
					{
						//There is only one player left and he died. This means game over. Send an RPC.
						PlayerRaceManager.Instance.setRaceStatus( RaceStatus.COMPLETED );
						photonView.RPC("coopGameOverRPC", PhotonTargets.All );
					}
				}
			}
			else
			{
				Debug.LogError("CoopWaveGenerator-playerDied " + player.name + " is already in the dead player list. Count: " + deadPlayerList.Count );
			}
		}
	}

	PlayerRace getPartner( PlayerRace playerRace )
	{
		PlayerRace partner = null;
		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i] != playerRace ) 
			{
				partner = PlayerRace.players[i];
				break;
			}
		}
		if( partner == null ) Debug.LogError("CoopWaveGenerator-could not find partner for " + playerRace.name + ". Player count is: " + PlayerRace.players.Count );

		return partner;
	}

	[PunRPC]
	void spectatingRPC( int deadPlayerPhotonViewID )
	{
		if( spectatingCoroutine != null ) StopCoroutine( spectatingCoroutine );
		spectatingCoroutine = StartCoroutine( startSpectating( deadPlayerPhotonViewID ) );
	}

	IEnumerator startSpectating( int deadPlayerPhotonViewID )
	{
		//We do not start spectating right away to give the player time to see how his character died.
		yield return new WaitForSeconds(DELAY_BEFORE_SPECTATING);

		Transform deadPlayer = getPlayerByPhotonViewID( deadPlayerPhotonViewID );
		
		if( deadPlayer != null )
		{
			//Have the main camera track his partner.
			PlayerRace partner = getPartner( deadPlayer.GetComponent<PlayerRace>() );
			if( partner != null )
			{
				//Have the partner say a reassuring VO such as "Don't worry. I've got this."
				partner.GetComponent<PlayerVoiceOvers>().playVoiceOver( VoiceOverType.VO_COOP_PARTNER_DIED );
				partner.GetComponent<PlayerCamera>().isBeingSpectated = true;

				if( deadPlayer.GetComponent<PhotonView>().isMine && deadPlayer.GetComponent<PlayerAI>() == null )
				{
					//Display the message "SPECTATING" on the HUD.
					HUDMultiplayer.hudMultiplayer.displayTopMessage( LocalizationManager.Instance.getText( "COOP_SPECTATING" ) );
					CinemachineVirtualCamera cmvc = GameObject.FindGameObjectWithTag("Main Virtual Camera").GetComponent<CinemachineVirtualCamera>();
					cmvc.m_Follow = partner.transform;
					cmvc.m_LookAt = partner.transform;
					//As he died, the player may have locked the camera (typically because of a great fall). See lockCamera in the PlayerCamera class.
					//This disables the CinemachineVirtualCamera.
					//Since we are now spectating, make sure it is enabled.
					cmvc.enabled = true;
				}
			}
		}
	}

	Transform getPlayerByPhotonViewID( int photonViewID )
	{
		Transform player = null;
		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				//We found the player
				player = PlayerRace.players[i].transform;
				break;
			}
		}
		return player;
	}

	public void removeDeadPlayer( Transform player )
	{
		if( PhotonNetwork.isMasterClient )
		{
			if( deadPlayerList.Contains( player ) )
			{
				deadPlayerList.Remove( player );
				Debug.Log( player.name + " SUCCESSfULLY removed this dead player." );
			}
			else
			{
				Debug.LogError( player.name + " UNABLE to remove this dead player because he isn't in the list." );
			}
		}
	}

	[PunRPC]
	void coopGameOverRPC()
	{
		if( spectatingCoroutine != null ) StopCoroutine( spectatingCoroutine );

		PlayerRaceManager.Instance.setRaceStatus( RaceStatus.COMPLETED );
		//Tell all the players that is is game over so that they can stop attempting to resurrect.
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			PlayerRace.players[i].GetComponent<PlayerCoop>().gameOver();
		}
		StartCoroutine( HUDMultiplayer.hudMultiplayer.leaveRoomShortly() );
		//Display the results screen (player details, score, rounds survived, etc.) and return to the lobby.
		StartCoroutine( HUDMultiplayer.hudMultiplayer.displayCoopResultsAndEmotesScreen( 0.25f ) );
	}
	#endregion
}
