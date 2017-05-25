using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBoxMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	public void OnClickFreeLootBox ()
	{
		LootBoxClientManager.Instance.requestLootBox(LootBoxType.FREE);
	}
}
