using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardRule
{
	MANA_COST = 0,
	OPPONENT_NEAR_LEADING = 1,
	OPPONENT_NEAR_TRAILING = 2,
	OPPONENT_FAR_LEADING = 3,
	OPPONENT_FAR_TRAILING = 4,
	OPPONENT_NEAR = 5,
	NO_OBSTACLES_IN_FRONT = 6

}

public class CardHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	public void activateCard ( int photonViewId, string name, int level)
	{
		switch (name)
		{
			case "Barbarians":
				CardSpeedBoost cardSpeedBoost = GetComponent<CardSpeedBoost>();
				if( cardSpeedBoost != null )
				{
					cardSpeedBoost.activateCard( photonViewId, level );
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
					cardExplosion.activateCard( photonViewId,level );
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
					cardDoubleJump.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardDoubleJump component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case "Giant Skeleton":
				CardSprint cardSprint = GetComponent<CardSprint>();
				if( cardSprint != null )
				{
					cardSprint.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardSprint component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case "Inferno Tower":
				CardFirewall cardFirewall = GetComponent<CardFirewall>();
				if( cardFirewall != null )
				{
					cardFirewall.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardFirewall component is not attached to the CardHandler in the Level scene.");
				}
			break;
			case "Goblin Hut":
				CardLightning cardLightning = GetComponent<CardLightning>();
				if( cardLightning != null )
				{
					cardLightning.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardLightning component is not attached to the CardHandler in the Level scene.");
				}
			break;
			default:
				CardLightning cardLightning2 = GetComponent<CardLightning>();
				if( cardLightning2 != null )
				{
					cardLightning2.activateCard( photonViewId, level );
				}
				else
				{
					Debug.LogError("CardHandler-The CardLightning component is not attached to the CardHandler in the Level scene.");
				}
				//Debug.LogError("Cardhandler-The card name specified, " + name + ", is unknown.");
			break;
		}
	}
}
