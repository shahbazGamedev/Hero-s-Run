using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieTrigger : MonoBehaviour {

	static ZombieManager zombieManager;
	public List<GameObject> zombieWaveList = new List<GameObject>();

	// Use this for initialization
	void Awake () {
	
		GameObject zombieManagerObject = GameObject.FindGameObjectWithTag("ZombieManager");
		zombieManager = zombieManagerObject.GetComponent<ZombieManager>();
	}
	
	private void configureWave( int waveToUse )
	{
		GameObject zombieWaveObject = zombieWaveList[waveToUse];
		zombieWaveObject.SetActive( true );
		ZombieWave activeZombieWave = zombieWaveObject.GetComponent<ZombieWave>();
		zombieManager.triggerZombieWave( activeZombieWave.spawnLocations );
	}
	
	public void activateNextWave()
	{
		Debug.Log ("Zombie wave number: " + ZombieManager.numberOfZombieWavesTriggered );
		if( ZombieManager.numberOfZombieWavesTriggered < zombieWaveList.Count )
		{
			configureWave( ZombieManager.numberOfZombieWavesTriggered );
		}
		else if( ZombieManager.numberOfZombieWavesTriggered >= zombieWaveList.Count && zombieWaveList.Count > 0 )
		{
			//We have gone through all the configured waves in succession.
			//From now on, just choose a random one from the list.
			configureWave( Random.Range(0, zombieWaveList.Count ) );
		}
		ZombieManager.numberOfZombieWavesTriggered++;
	}

	//Called by TileReset
	public void reset()
	{
		for( int i = 0; i < zombieWaveList.Count; i++ )
		{
			zombieWaveList[i].SetActive( false );
		}
	}
}
