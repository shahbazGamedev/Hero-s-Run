using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Cinemachine;

public sealed class CoopWaveGenerator : PunBehaviour {

	#region Waves
	[SerializeField] float timeBetweenWave = 12f;
	[SerializeField] List<GameObject> easyWaveList = new List<GameObject>();
	[SerializeField] List<GameObject> mediumWaveList = new List<GameObject>();
	[SerializeField] List<GameObject> hardWaveList = new List<GameObject>();
	public static int numberOfWavesTriggered = 0;
	public int tileIndexOfLastWave;
	public delegate void CoopNewWave( int waveNumber );
	public static event CoopNewWave coopNewWave;

	public float easyWaveProbability; //public only for debugging. Do not change in editor.
	public float mediumWaveProbability;
	public float hardWaveProbability; //this value is not required but makes debugging easier.
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
		Instance = this;
	}

	void Start ()
	{
		generateLevel = GameObject.FindObjectOfType<GenerateLevel>();
		zombieManager = GameObject.FindObjectOfType<ZombieManager>();
		numberOfWavesTriggered = 0; //important to reset since this is a static value.
	}
	
	void OnEnable()
	{
		HUDMultiplayer.startRunningEvent += StartRunningEvent;
	}
	
	void OnDisable()
	{
		HUDMultiplayer.startRunningEvent -= StartRunningEvent;
	}

	void StartRunningEvent()
	{
		if( PhotonNetwork.isMasterClient && GameManager.Instance.isCoopPlayMode() ) Invoke("activateNewWave", timeBetweenWave );
	}

	void OnMasterClientSwitched( PhotonPlayer newMaster )
	{
		Debug.LogWarning("OnMasterClientSwitched. New master is: " + newMaster.NickName );
		if( GameManager.Instance.isCoopPlayMode() ) Invoke("activateNewWave", timeBetweenWave );
	}

	#region Waves
	void activateNewWave ()
	{
		if( PhotonNetwork.isMasterClient && GameManager.Instance.isCoopPlayMode() )
		{
			GameObject tile = getAppropriateTile( getLeadingTileIndex() );
	
			setWaveDifficultyProbabilities();
	
			GameObject wave = getAppropriateWave();
			
			if( wave != null && tile != null )
			{
				createWave( wave, tile );
			}
			else
			{
				//Try again a bit later. Six seconds is enough time to clear a 100 meter long tile at 18 m/s.
				Invoke("activateNewWave", 6f );
			}
		}
	}

	private int getLeadingTileIndex()
	{
		int highestTileIndex = 0;
		for( int i = 0; i< PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PlayerControl>().tileIndex > highestTileIndex ) highestTileIndex = PlayerRace.players[i].GetComponent<PlayerControl>().tileIndex;
		}
		return highestTileIndex;
	}

	private GameObject getAppropriateTile( int highestTileIndex )
	{
		//Now that we have the highest tile index, ask generate level for a tile that we are going to use
		//to spawn the creatures in.
		//We want to spawn the creatures 2 tiles further than where the leading player is.
		int desiredTileIndex = highestTileIndex + 2;
		GameObject tile = generateLevel.getTileForNextWave( desiredTileIndex );
		if( tile != null ) 
		{
			tileIndexOfLastWave = tile.GetComponent<SegmentInfo>().tileIndex;
			return tile;
		}
		else
		{
			Debug.LogWarning("CoopWaveGenerator-getAppropriateTile did not find an appropriate tile. Returning null.");
			return null;
		}
	}

	private void setWaveDifficultyProbabilities()
	{
		if( numberOfWavesTriggered < 10 )
		{
			easyWaveProbability = 0.6f;
			mediumWaveProbability = 0.4f;
			hardWaveProbability = 0;
		}
		else if( numberOfWavesTriggered < 20 )
		{
			easyWaveProbability = 0.3f;
			mediumWaveProbability = 0.7f;
			hardWaveProbability = 0f;
		}
		else if( numberOfWavesTriggered < 30 )
		{
			easyWaveProbability = 0.1f;
			mediumWaveProbability = 0.6f;
			hardWaveProbability = 0.3f;
		}
		else if( numberOfWavesTriggered < 40 )
		{
			easyWaveProbability = 0.05f;
			mediumWaveProbability = 0.55f;
			hardWaveProbability = 0.4f;
		}
		else if( numberOfWavesTriggered < 50 )
		{
			easyWaveProbability = 0.05f;
			mediumWaveProbability = 0.4f;
			hardWaveProbability = 0.55f;
		}
		else
		{
			easyWaveProbability = 0.05f;
			mediumWaveProbability = 0.35f;
			hardWaveProbability = 0.6f;
		}
	}

	private GameObject getAppropriateWave()
	{
		float rdn = Random.value;
		int randomIndex;

		if( rdn <= easyWaveProbability )
		{
			randomIndex = Random.Range( 0, easyWaveList.Count );
			return easyWaveList[randomIndex];
		}
		else if( rdn <= mediumWaveProbability )
		{
			randomIndex = Random.Range( 0, mediumWaveList.Count );
			return mediumWaveList[randomIndex];
		}
		else
		{
			randomIndex = Random.Range( 0, hardWaveList.Count );
			return hardWaveList[randomIndex];
		}
	}

	private void createWave( GameObject wave, GameObject tile )
	{
		Debug.LogWarning("createWave on tile " + tile.name + " for wave " + numberOfWavesTriggered + " tileEndex " + tileIndexOfLastWave );
		//trigger zombie wave takes care of instantiating the zombies using Photon.
		GameObject clone = Instantiate( wave, tile.transform );
		ZombieWave activeZombieWave = clone.GetComponent<ZombieWave>();
		//To facilitate testing
		DebugInfoType debugInfoType = GameManager.Instance.playerDebugConfiguration.getDebugInfoType();
		if( debugInfoType != DebugInfoType.DONT_SPAWN_ZOMBIES ) zombieManager.triggerZombieWave( activeZombieWave.spawnLocations );
		photonView.RPC("coopWaveRPC", PhotonTargets.All );
		Invoke("activateNewWave", timeBetweenWave );
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
			}

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
					CancelInvoke("activateNewWave");
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
				CancelInvoke("activateNewWave");				
				photonView.RPC("coopGameOverRPC", PhotonTargets.All );
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
