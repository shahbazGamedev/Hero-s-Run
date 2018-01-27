﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBoxClientManager : MonoBehaviour {

	public static LootBoxClientManager Instance;
	//Event management used to notify other classes when a loot box has been granted by the server
	public delegate void LootBoxGrantedEvent( LootBox lootBox );
	public static event LootBoxGrantedEvent lootBoxGrantedEvent;

	// Use this for initialization
	void Awake ()
	{
		if(Instance)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
	}

	void OnEnable()
	{
		AnimatedLootBox.lootBoxOpenedEvent += LootBoxOpenedEvent;
	}

	void OnDisable()
	{
		AnimatedLootBox.lootBoxOpenedEvent -= LootBoxOpenedEvent;
	}

	public void LootBoxOpenedEvent ( LootBoxType lootBoxType )
	{
		requestLootBox(lootBoxType);
	}
	
	public void requestLootBox( LootBoxType lootBoxType )
	{
		Debug.Log( GameManager.Instance.playerProfile.getUserName() + " is requesting a client loot box of type " + lootBoxType + ". His current sector is " + GameManager.Instance.playerProfile.getCurrentSector() );
		LootBoxServerManager.Instance.requestLootBox( lootBoxType, GameManager.Instance.playerProfile.getCurrentSector() );
	}

	public void lootBoxGranted( string lootBoxJson )
	{
		LootBox lootBox = JsonUtility.FromJson<LootBox>( lootBoxJson );
		Debug.Log( GameManager.Instance.playerProfile.getUserName() + " was granted a loot box by the server: " + lootBoxJson );
		if( lootBoxGrantedEvent != null ) lootBoxGrantedEvent( lootBox );
	}

}
