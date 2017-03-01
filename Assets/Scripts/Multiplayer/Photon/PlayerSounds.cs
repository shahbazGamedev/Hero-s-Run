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
	string groundType = "Normal"; //Other choices are Water and Collapsing.

	// Use this for initialization
	void Start ()
	{
		audioSource = GetComponent<AudioSource>();
	}

	public void playSound(AudioClip soundToPlay, bool isLooping )
    {
		audioSource.clip = soundToPlay;
		audioSource.loop = isLooping;
		audioSource.Play();
    }

	public void stopAudioSource()
	{
		audioSource.Stop();
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

	public void groundTypeChanged( string groundType )
	{
		this.groundType = groundType;

		//Setup proper footsteps
		if( groundType == "Water" )
		{
			leftFootstep = footstepWaterSound;
			rightFootstep = footstepWaterSound;
		}
		else if( groundType == "Snow"  )
		{
			leftFootstep = snowFootstepLeftSound;
			rightFootstep = snowFootstepRightSound;
		}
		else
		{
			leftFootstep = footstepLeftSound;
			rightFootstep = footstepRightSound;
		}
	}

	public void Footstep_left ( AnimationEvent eve )
	{
		audioSource.PlayOneShot( leftFootstep, 0.1f  );
	}

	public void Footstep_right ( AnimationEvent eve )
	{
		audioSource.PlayOneShot( rightFootstep, 0.1f  );
	}

	public void Land_sound ( AnimationEvent eve )
	{
		if( groundType != "Water" )
		{
			audioSource.PlayOneShot( landGroundSound, 0.28f );
		}
		else
		{
			audioSource.PlayOneShot( landWaterSound );
		}
	}

	public void Slide_sound_start ( AnimationEvent eve )
	{
		playSound( slidingSound, true );
	}

	public void Slide_sound_stop ( AnimationEvent eve )
	{
		audioSource.Stop();
	}

}
