﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour {

	[SerializeField] AudioClip 	slidingSound;
	[SerializeField] AudioClip 	jumpingSound;
	[SerializeField] AudioClip 	sideMoveSound;
	[SerializeField] AudioClip 	landGroundSound;
	[SerializeField] AudioClip 	landWaterSound;
	[SerializeField] AudioClip 	stumblingSound;
	[SerializeField] AudioClip 	fallingSound;
	[SerializeField] AudioClip 	ziplineSound;
	[Header("Dying")]
	[SerializeField] AudioClip 	deathFireSound;
	[SerializeField] AudioClip 	dyingSound;
	[SerializeField] AudioClip 	punchSound;
	[Header("Footsteps")]
	[SerializeField] AudioClip 	footstepLeftSound;
	[SerializeField] AudioClip 	footstepRightSound;
	[SerializeField] AudioClip 	footstepWaterSound; //We do not need one for each foot.
	[SerializeField] AudioClip 	snowFootstepLeftSound;
	[SerializeField] AudioClip 	snowFootstepRightSound;
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

	public void playZiplineSound()
	{
		playSound( ziplineSound, true );
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
		audioSource.PlayOneShot( leftFootstep, 0.12f  );
	}

	public void Footstep_right ( AnimationEvent eve )
	{
		audioSource.PlayOneShot( rightFootstep, 0.12f  );
	}

	public void Win_footstep_left ( AnimationEvent eve )
	{
		audioSource.PlayOneShot( leftFootstep  );
	}

	public void Win_footstep_right ( AnimationEvent eve )
	{
		audioSource.PlayOneShot( rightFootstep  );
	}

	public void Lose_knees_hit_ground ( AnimationEvent eve )
	{
		audioSource.PlayOneShot( leftFootstep, 0.8f  );
	}

	//The Land_sound event must be set at the beginning of the animation or else you might not hear it because of how the blending is set up.
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

	//See Rug Fall Foward animation
	public void fall_forward_belly_hits_ground ( AnimationEvent eve )
	{
		audioSource.PlayOneShot( dyingSound );
	}

	//See Rug Lose animation
	public void punch_ground ( AnimationEvent eve )
	{
		audioSource.PlayOneShot( punchSound );
	}

}
