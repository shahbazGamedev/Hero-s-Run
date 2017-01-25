using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProfile {

	public int currentXP = 0;

	public void addXP( int xpAmount, bool saveImmediately )
	{
		if( xpAmount <= 0 || xpAmount > XPManager.MAX_XP_IN_ONE_RACE ) return;	
		currentXP = currentXP + xpAmount;
		if( saveImmediately ) serializePlayerprofile();
		Debug.Log("PlayerProfile-addXP: adding XP: " + xpAmount + " New total is: " +  currentXP );
	}

	public void serializePlayerprofile()
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerProfile( json );
		PlayerStatsManager.Instance.savePlayerStats();
	}
}
