using UnityEngine;
using System.Collections;

public class CoinManager : MonoBehaviour {

	// Audio - Coin
	static AudioSource coinAudioSource;
	public static AudioClip coinPickup;
	
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
