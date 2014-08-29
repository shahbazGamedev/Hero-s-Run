using UnityEngine;
using System.Collections;

public class CoinManager : MonoBehaviour {

	// Audio - Coin
	static AudioSource coinAudioSource;
	public static AudioClip coinPickup;
	/*
	The following value (realNumberCoinsSpawned) is printed when the level has been created.
	It contains the total number of stars available in the randomly generated level.
	This value is useful because it allows the level designer to specify star objectives that are coherent with the number of stars in the level.
	Currently, the stars given by breakables, zombies and chickens are not counted in this number.
	*/
	public static int realNumberCoinsSpawned = 0;

	void Awake ()
	{
		coinAudioSource = gameObject.AddComponent<AudioSource>();
		coinPickup = Resources.Load("Audio/coinPickup") as AudioClip;
		coinAudioSource.clip = coinPickup;
	}
	
	//Note: it is not possible to play multiple clips simultaneously from one audio source, and it is not possible
	//to have mutliple audio sources on one game object.
	public static void playCoinPickupSound()
	{
		// Used to play the coin pick-up sound, but only if one is not already playing
		if ( false && !coinAudioSource.isPlaying )
		{	
			coinAudioSource.Play();
		}
	}	
}
