using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBoxCanvas : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();	
	}

	public void OnClickLootBox()
	{
		LootBoxType lootBoxType = LootBoxType.SHOP_GIANT;
		LevelData.MultiplayerInfo multiplayerInfo = LevelManager.Instance.getLevelData().getRaceTrackByTrophies();
		Debug.Log( GameManager.Instance.playerProfile.getUserName() + " is requesting a client loot box of type " + lootBoxType + ". His current sector is " + multiplayerInfo.circuitInfo.sectorNumber );
		LootBoxServerManager.Instance.requestLootBox( lootBoxType, multiplayerInfo.circuitInfo.sectorNumber );
	}
	
}
