using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerDebugConfiguration {

	[SerializeField] CloudRegionCode overrideCloudRegionCode = CloudRegionCode.none;
	[SerializeField] int overrideMap = 3; //Normal value is -1. For the demo, we will override this value to 3 (which is Hanging Rock).
	[SerializeField] DebugInfoType debugInfoType = DebugInfoType.NONE; //The type of debug information to display on the HUD such as FPS, Latency, etc.
	[SerializeField] float speedOverrideMultiplier = 1f;
	[SerializeField] bool allowBotToPlayCards = true;
	[SerializeField] bool autoPilot = false; //Allows the player to use the same AI as bots to avoid obstacles

	public void createNewDebugConfiguration()
	{
		serializeDebugConfiguration( true );
	}

	public void setOverrideCloudRegionCode( CloudRegionCode overrideCloudRegionCode )
	{
		this.overrideCloudRegionCode = overrideCloudRegionCode;
	}

	public CloudRegionCode getOverrideCloudRegionCode()
	{
		return overrideCloudRegionCode;
	}

	public void setOverrideMap( int overrideMap )
	{
		this.overrideMap = overrideMap;
	}

	public int getOverrideMap()
	{
		return overrideMap;
	}

	public void setDebugInfoType( DebugInfoType debugInfoType )
	{
		this.debugInfoType = debugInfoType;
	}

	public DebugInfoType getDebugInfoType()
	{
		return debugInfoType;
	}

	public void setSpeedOverrideMultiplier( float speedOverrideMultiplier )
	{
		this.speedOverrideMultiplier = speedOverrideMultiplier;
	}

	public float getSpeedOverrideMultiplier()
	{
		return speedOverrideMultiplier;
	}

	public void setAllowBotToPlayCards( bool allowBotToPlayCards )
	{
		this.allowBotToPlayCards = allowBotToPlayCards;
	}

	public bool getAllowBotToPlayCards()
	{
		return allowBotToPlayCards;
	}

	public void setAutoPilot( bool autoPilot )
	{
		this.autoPilot = autoPilot;
	}

	public bool getAutoPilot()
	{
		return autoPilot;
	}

	public void serializeDebugConfiguration( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setDebugConfiguration( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}

}
