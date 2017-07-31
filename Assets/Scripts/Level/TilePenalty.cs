﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePenalty : MonoBehaviour {

	[SerializeField] int tilePenalty = 3;

	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Player") && other.GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Dying )
		{
			other.GetComponent<PlayerRace>().tilesLeftBeforeReachingEnd = other.GetComponent<PlayerRace>().tilesLeftBeforeReachingEnd + tilePenalty;
		}
	}
}
