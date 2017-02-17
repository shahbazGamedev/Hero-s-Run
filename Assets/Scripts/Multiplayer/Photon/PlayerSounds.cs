using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour {

	public AudioClip 	slidingSound;
	public AudioClip 	jumpingSound;
	public AudioClip 	sideMoveSound;
	public AudioClip 	dyingSound;
	public AudioClip 	stumblingSound;
	public AudioClip 	fallingSound;
	public AudioClip 	footstepLeftSound;
	public AudioClip 	footstepRightSound;
	public AudioClip 	footstepWaterSound; //We do not need one for each foot.
	public AudioClip 	landGroundSound;
	public AudioClip 	landWaterSound;
	public AudioClip 	deathFireSound;
	public AudioClip 	snowFootstepLeftSound;
	public AudioClip 	snowFootstepRightSound;
	AudioClip leftFootstep;	//Footsteps sounds to use for current ground type
	AudioClip rightFootstep;

	AudioSource audioSource;

	// Use this for initialization
	void Start ()
	{
		audioSource = GetComponent<AudioSource>();
	}

	void playSound(AudioClip soundToPlay, bool isLooping )
    {
		audioSource.clip = soundToPlay;
		audioSource.loop = isLooping;
		audioSource.Play();
    }

	public void stopAudioSource()
	{
		audioSource.Stop();
	}

	public void playSlidingSound()
	{
		playSound( slidingSound, true );
	}

	public void playSideMoveSound()
	{
		audioSource.PlayOneShot( sideMoveSound );
	}

	public void playDyingSound()
	{
		playSound( dyingSound, false );
	}

	public void playFireDyingSound()
	{
		playSound( deathFireSound, false );
	}

}
