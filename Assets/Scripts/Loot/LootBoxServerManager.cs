using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LootBoxType
 {
	FREE = 0,
	CROWNS = 1,
	SHOP = 2,
	RACE_TRACK_UNLOCK = 3
}

public class LootBoxServerManager : MonoBehaviour {

	public static LootBoxServerManager Instance;

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

	public void requestLootBox( LootBoxType lootBoxType )
	{
		Debug.Log( GameManager.Instance.playerProfile.getUserName() + " is requesting a server loot box of type " + lootBoxType );
	}
	
}
