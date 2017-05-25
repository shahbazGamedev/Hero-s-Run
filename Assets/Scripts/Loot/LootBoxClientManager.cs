using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBoxClientManager : MonoBehaviour {

	public static LootBoxClientManager Instance;

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
		Debug.Log( GameManager.Instance.playerProfile.getUserName() + " is requesting a client loot box of type " + lootBoxType );
		LootBoxServerManager.Instance.requestLootBox(LootBoxType.FREE);
	}

	public void lootBoxGranted( string lootBoxJson )
	{
		LootBox lootBox = JsonUtility.FromJson<LootBox>( lootBoxJson );
		Debug.Log( GameManager.Instance.playerProfile.getUserName() + " was granted a loot box by the server: " + lootBoxJson );
		lootBox.print();
	}

}
