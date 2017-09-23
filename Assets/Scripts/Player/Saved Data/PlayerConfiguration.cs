using System.Collections;
using UnityEngine;

[System.Serializable]
public class PlayerConfiguration {

	[SerializeField] bool firstTimePlaying = true;
	[SerializeField] bool usesFacebook = false;
	//If true, tilting the device will allow the player to switch lanes or turn corners
	[SerializeField] bool isTiltEnabled = false;
	//Sound effects volume between -80db and 0f.
	[SerializeField] float soundFxVolume = 0f;
	//Music volume between -80db and 0f.
	[SerializeField] float musicVolume = 0f;

	public void createNewConfiguration()
	{
		serializePlayerConfiguration( true );
	}

	//Used to identify if the player is a new user and it is his first time launching the game
	public bool isFirstTimePlaying()
	{
		return firstTimePlaying;
	}

	public void setFirstTimePlaying( bool value )
	{
		firstTimePlaying = value;
	}

	public void setUsesFacebook( bool value )
	{
		usesFacebook = value;
	}
	
	public bool getUsesFacebook()
	{
		return usesFacebook;
	}

	public float getSoundFxVolume()
	{
		return soundFxVolume;
	}
	
	public void setSoundFxVolume( float volume )
	{
		soundFxVolume = volume;
	}

	public float getMusicVolume()
	{
		return musicVolume;
	}
	
	public void setMusicVolume( float volume )
	{
		musicVolume = volume;
	}

	public void setEnableTilt( bool value )
	{
		isTiltEnabled = value;
	}
	
	public bool getTiltEnabled()
	{
		return isTiltEnabled;
	}

	public void serializePlayerConfiguration( bool saveImmediately )
	{
		string json  = JsonUtility.ToJson( this );
		PlayerStatsManager.Instance.setPlayerConfiguration( json );
		if( saveImmediately ) PlayerStatsManager.Instance.savePlayerStats();
	}

}
