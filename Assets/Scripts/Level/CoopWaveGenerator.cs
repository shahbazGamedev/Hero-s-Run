using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Cinemachine;

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
	GenerateLevel generateLevel;
	ZombieManager zombieManager;
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
		generateLevel = GameObject.FindObjectOfType<GenerateLevel>();
		zombieManager = GameObject.FindObjectOfType<ZombieManager>();
		numberOfWavesTriggered = 0; //important to reset since this is a static value.
	}

	#region Waves
	public void activateNewWave ()
	{
		if( PhotonNetwork.isMasterClient && GameManager.Instance.isCoopPlayMode() )
		{
			createWave( waveList[numberOfWavesTriggered] );
		}
	}

	private void createWave( GameObject wave )
	{
		Debug.LogWarning("Creating wave: " + numberOfWavesTriggered  );
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
				deadPlayerList.Add( player );
				Debug.Log("Coop playerDied " + player.name + " died " + deadPlayerList.Count );
				PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( player.name );
				pmd.downs++;

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
						//Only one player is dead.
						photonView.RPC("onePlayerDeadRPC", PhotonTargets.All, player.GetComponent<PhotonView>().viewID );	
					}
				}
				else
				{
					//There is only one player left and he died. This means game over. Send an RPC.
					PlayerRaceManager.Instance.setRaceStatus( RaceStatus.COMPLETED );
					photonView.RPC("coopGameOverRPC", PhotonTargets.All );
				}
			}
			else
			{
				Debug.LogError("Coop playerDied " + player.name + " is already in the dead player list. Count: " + deadPlayerList.Count );
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
	void onePlayerDeadRPC( int deadPlayerPhotonViewID )
	{
		Transform deadPlayer = getPlayerByPhotonViewID( deadPlayerPhotonViewID );
		
		Debug.LogWarning( " deadPlayerPhotonViewID isMasterClient: " + PhotonNetwork.isMasterClient + " dead player name: " + deadPlayer.name + " dead player isMine: " + deadPlayer.GetComponent<PhotonView>().isMine + " is AI null: " + (deadPlayer.GetComponent<PlayerAI>() == null) );

		if( deadPlayer != null && deadPlayer.GetComponent<PhotonView>().isMine && deadPlayer.GetComponent<PlayerAI>() == null )
		{
			//Display the message "SPECTATING" on the HUD.
			HUDMultiplayer.hudMultiplayer.displayTopMessage( LocalizationManager.Instance.getText( "COOP_SPECTATING" ) );
			//Have the main camera track his partner.
			PlayerRace partner = getPartner( deadPlayer.GetComponent<PlayerRace>() );
			if( partner != null )
			{
				CinemachineVirtualCamera cmvc = GameObject.FindGameObjectWithTag("Main Virtual Camera").GetComponent<CinemachineVirtualCamera>();
				cmvc.m_Follow = partner.transform;
				cmvc.m_LookAt = partner.transform;
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
		if( deadPlayerList.Contains( player ) ) deadPlayerList.Remove( player );
	}

	[PunRPC]
	void coopGameOverRPC()
	{
		Debug.Log( "coopGameOverRPC received." );
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
