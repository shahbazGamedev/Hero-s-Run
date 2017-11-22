﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieTrigger : MonoBehaviour {

	ZombieManager zombieManager;
	
	public List<GameObject> zombieWaveList = new List<GameObject>();
	bool zombieWaveWasTriggered = false;

	void Start ()
	{
		if( LevelManager.Instance.getSelectedCircuit().isCoop ) zombieManager = GameObject.FindObjectOfType<ZombieManager>();
	}
	
	//Only the master should call this method.
	public void activateNextWave( LevelNetworkingManager levelNetworkingManager, string nameOfTileEntered )
	{
		if( zombieManager == null ) return;

		if( zombieWaveWasTriggered ) return;

		if( zombieWaveList.Count == 0 ) return;

		configureWave( Random.Range(0, zombieWaveList.Count ) );

		levelNetworkingManager.nextWaveActivated( nameOfTileEntered );
	}

	private void configureWave( int waveToUse )
	{
		GameObject zombieWaveObject = zombieWaveList[waveToUse];
		zombieWaveObject.SetActive( true );
		ZombieWave activeZombieWave = zombieWaveObject.GetComponent<ZombieWave>();
		//trigger zombie wave takes care of instantiating the zombies using Photon.
		zombieManager.triggerZombieWave( activeZombieWave.spawnLocations );
		zombieWaveWasTriggered = true;
	}

}
