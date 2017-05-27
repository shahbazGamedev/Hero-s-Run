using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class FreeLootBoxHandler : MonoBehaviour {

	[SerializeField] TextMeshProUGUI availableLootBoxes;

	void Start ()
	{
		DateTime openTime = GameManager.Instance.playerProfile.getLastFreeLootBoxOpenedTime().AddHours(4);
		if( DateTime.UtcNow > openTime )
		{
			availableLootBoxes.text = "1";
		}
		else
		{
			availableLootBoxes.text = "0";
		}
	}
	
}
