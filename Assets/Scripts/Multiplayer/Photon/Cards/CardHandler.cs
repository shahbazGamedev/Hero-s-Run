using System.Collections;
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

			default:
				Debug.LogError("Cardhandler-The card name specified, " + name + ", is unknown.");
			break;
		}
	}
}
