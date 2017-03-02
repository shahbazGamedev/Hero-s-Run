﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	public void activateCard ( string name, int level)
	{
		switch (name)
		{
			case "Barbarians":
			CardSpeedBoost cardSpeedBoost = GetComponent<CardSpeedBoost>();
			if( cardSpeedBoost != null )
			{
				cardSpeedBoost.activateCard( name, level );
			}
			else
			{
				Debug.LogError("CardHandler-The CardSpeedBoost component is not attached to the CardHandler in the Level scene.");
			}
			break;
			case "Ice Spirit":
			CardExplosion cardExplosion = GetComponent<CardExplosion>();
			if( cardExplosion != null )
			{
				cardExplosion.activateCard( name, level );
			}
			else
			{
				Debug.LogError("CardHandler-The CardExplosion component is not attached to the CardHandler in the Level scene.");
			}
			break;
			case "Furnace":
			CardDoubleJump cardDoubleJump = GetComponent<CardDoubleJump>();
			if( cardDoubleJump != null )
			{
				cardDoubleJump.activateCard( name, level );
			}
			else
			{
				Debug.LogError("CardHandler-The CardDoubleJump component is not attached to the CardHandler in the Level scene.");
			}
			break;
			default:
			CardExplosion cardExplosion2 = GetComponent<CardExplosion>();
			if( cardExplosion2 != null )
			{
				cardExplosion2.activateCard( name, level );
			}
			else
			{
				Debug.LogError("CardHandler-The CardExplosion component is not attached to the CardHandler in the Level scene.");
			}
			break;
			//Debug.LogError("Cardhandler-The card name specified, " + name + ", is unknown.");
			break;
		}
	}
}
