using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoopWaveGenerator : MonoBehaviour {

	[SerializeField] List<GameObject> waveList = new List<GameObject>();
	public static int numberOfWavesTriggered = 0;
	GenerateLevel generateLevel;
	ZombieManager zombieManager;
	#region Events
	public delegate void CoopNewWave( int waveNumber );
	public static event CoopNewWave coopNewWave;
	#endregion
	public int tileIndexOfLastWave;
	void Awake ()
	{
		//if ( PhotonNetwork.isMasterClient && GameManager.Instance.isCoopPlayMode() )
		if ( PhotonNetwork.isMasterClient )
		{
			generateLevel = GameObject.FindObjectOfType<GenerateLevel>();
			zombieManager = GameObject.FindObjectOfType<ZombieManager>();
		}
	}

	void Start ()
	{
		//if( PhotonNetwork.isMasterClient && GameManager.Instance.isCoopPlayMode() ) Invoke("activateNewWave", 10f );
		if( PhotonNetwork.isMasterClient ) Invoke("activateNewWave", 10f );
	}
	
	void FixedUpdate ()
	{
		if( isWaveFinished() ) Invoke("activateNewWave", 3f );
	}

	void activateNewWave ()
	{
		numberOfWavesTriggered++;

		if( coopNewWave != null ) coopNewWave( numberOfWavesTriggered );

		GameObject tile = getAppropriateTileForWave( getLeadingTileIndex() );

		GameObject wave = getAppropriateWave();
		
		if( wave != null && tile != null )
		{
			createWave( wave, tile );
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

	private GameObject getAppropriateTileForWave( int highestTileIndex )
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
			Debug.LogWarning("CoopWaveGenerator-getAppropriateTileForWave did not find an appropriate tile. Returning null.");
			return null;
		}
	}

	private GameObject getAppropriateWave()
	{
		if( waveList.Count > 0 ) 
		{
			return waveList[0];
		}
		else
		{
			Debug.LogWarning("CoopWaveGenerator-getAppropriateWave is returning null. You need to add waves in the wave's list.");
			return null;
		}
	}

	private void createWave( GameObject wave, GameObject tile )
	{
		Invoke("activateNewWave", 10f );
		Debug.LogError("createWave on tile " + tile.name + " for wave " + numberOfWavesTriggered + " tileEndex " + tileIndexOfLastWave );
		//trigger zombie wave takes care of instantiating the zombies using Photon.
		GameObject clone = Instantiate( wave, tile.transform );
		ZombieWave activeZombieWave = clone.GetComponent<ZombieWave>();
		zombieManager.triggerZombieWave( activeZombieWave.spawnLocations );
	}

	private bool isWaveFinished()
	{
		return false;
	}

}
