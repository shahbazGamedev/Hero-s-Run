﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LootBoxCanvas : MonoBehaviour {

	int currentIndex = 0;
	int lootBoxesOwned = 0;
	LootBoxData selectedLootBoxData;
	[SerializeField] TextMeshProUGUI lootBoxTypeText;
	[SerializeField] RadialTimerButton radialTimerButton;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
		if( Debug.isDebugBuild ) addLootBoxesForTesting ();
		lootBoxesOwned = GameManager.Instance.playerInventory.getNumberOfLootBoxesOwned();
		if( lootBoxesOwned > 0 )
		{
			currentIndex = 0;
			configureLootBox();
		}
		else
		{
			radialTimerButton.isActive = false;
			lootBoxTypeText.text = "No loot boxes";
		}
	}
	
	void addLootBoxesForTesting ()
	{
		GameManager.Instance.playerInventory.removeAllLootBoxesOwned();
		GameManager.Instance.playerInventory.addLootBox( new LootBoxData( LootBoxType.LEVEL_UP, 1, 1 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxData( LootBoxType.RACE_WON, 1, 1 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxData( LootBoxType.FREE, 1, 1 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxData( LootBoxType.LEVEL_UP, 2, 2 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxData( LootBoxType.RACE_WON, 2, 2 ) );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxData( LootBoxType.FREE, 2, 2 ) );
	}

	public void OnClickLootBox()
	{
		LevelData.MultiplayerInfo multiplayerInfo = LevelManager.Instance.getLevelData().getRaceTrackByTrophies();
		Debug.Log( GameManager.Instance.playerProfile.getUserName() + " is requesting a client loot box of type " + selectedLootBoxData.type + ". His current sector is " + multiplayerInfo.circuitInfo.sectorNumber );
		LootBoxServerManager.Instance.requestLootBox( selectedLootBoxData.type, multiplayerInfo.circuitInfo.sectorNumber );

		GameManager.Instance.playerInventory.removeLootBoxAt( currentIndex );

		lootBoxesOwned = GameManager.Instance.playerInventory.getNumberOfLootBoxesOwned();
		if( lootBoxesOwned > 0 )
		{
			currentIndex++;
			if( currentIndex >= lootBoxesOwned )
			{
				currentIndex = 0;
			}
			configureLootBox();
		}
		else
		{
			radialTimerButton.isActive = false;
			lootBoxTypeText.text = "No loot boxes";
		}
	}

	void configureLootBox()
	{
		selectedLootBoxData = GameManager.Instance.playerInventory.getLootBoxAt( currentIndex );
		lootBoxTypeText.text = selectedLootBoxData.type.ToString() + "-" + lootBoxesOwned;
	}

	public void OnClickPrevious()
	{
		if( lootBoxesOwned == 0 ) return;
		currentIndex--;
		if( currentIndex < 0 ) currentIndex = lootBoxesOwned  - 1;
		configureLootBox();
	}

	public void OnClickNext()
	{
		if( lootBoxesOwned == 0 ) return;
		currentIndex++;
		if( currentIndex >= lootBoxesOwned )
		{
			currentIndex = 0;
		}
		configureLootBox();
	}
	
}
