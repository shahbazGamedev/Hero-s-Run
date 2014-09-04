using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	// Audio - GUI
	static AudioSource guiAudioSource;
	static AudioClip buttonClick;

	// Audio - Music
	static AudioSource musicSource;
	const float MUSIC_VOLUME = 0.75f;

	//Audio - Level ambience
	static AudioSource levelAmbienceSource;

	const float MAX_VOLUME = 0.3f;

	public const float STANDARD_FADE_TIME = 1.5f;

	void Awake ()
	{
		DontDestroyOnLoad( gameObject );

		//Set the global audio volume based on the player's saved preference.
		AudioListener.volume = PlayerStatsManager.Instance.getSoundVolume();

		// Used to play GUI sounds for the HUD and all menus
		guiAudioSource = gameObject.AddComponent<AudioSource>();
		guiAudioSource.ignoreListenerPause = true;

		//Play the music track that we load from the Resources folder
		musicSource = gameObject.AddComponent<AudioSource>();
		musicSource.clip = Resources.Load("Audio/Temple Run 2 Soundtrack") as AudioClip;
		musicSource.loop = true;
		musicSource.volume = MUSIC_VOLUME;

		buttonClick = Resources.Load("Audio/menu_open") as AudioClip;

		//For level ambience sound track
		levelAmbienceSource = gameObject.AddComponent<AudioSource>();
		levelAmbienceSource.loop = true;

	}
	
	public static void playMusic()
	{
		musicSource.Play();
	}

	public static void stopMusic()
	{
		musicSource.Stop();
	}

	public static void pauseMusic()
	{
		musicSource.Pause();
	}
		
	public static IEnumerator fadeOutMusic( float duration, float endVolume )
	{
		float elapsedTime = 0;
		
		float startVolume = musicSource.volume;

		endVolume = endVolume * MUSIC_VOLUME;

		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			musicSource.volume =  Mathf.Lerp( startVolume, endVolume, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  

		} while ( elapsedTime < duration );

		musicSource.volume = endVolume;

		print ("SoundManager-Finished fading out music to volume: " + endVolume );
	}

	public static IEnumerator fadeOutMusic( float duration )
	{
		float elapsedTime = 0;

		float startVolume = musicSource.volume;

		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			musicSource.volume =  Mathf.Lerp( startVolume, 0, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  

		} while ( elapsedTime < duration );

		stopMusic();
		musicSource.volume = 0;
		print ("SoundManager-Finished fading out music completely" );
	}

	public static IEnumerator fadeInMusic( float duration )
	{
		if( !musicSource.isPlaying ) playMusic();
		float elapsedTime = 0;
		
		float startVolume = musicSource.volume;
	
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			musicSource.volume =  Mathf.Lerp( startVolume, MUSIC_VOLUME, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  

		} while ( elapsedTime < duration );
		musicSource.volume = MUSIC_VOLUME;
		print ("SoundManager-Finished fading in music completely" );
	}

	public static void playButtonClick()
	{
		if( guiAudioSource != null )
		{
			guiAudioSource.PlayOneShot( buttonClick );
		}
	}

	public static void playGUISound( AudioClip clip )
	{
		guiAudioSource.PlayOneShot( clip );
	}

	public static void playAmbienceClip( AudioClip ambienceClip )
	{
		if( ambienceClip != null )
		{
			levelAmbienceSource.clip = ambienceClip;
			levelAmbienceSource.volume = MAX_VOLUME;
			levelAmbienceSource.Play();
			Debug.Log("SoundManager-playAmbienceClip: playing ambience track titled " +  ambienceClip.name );
			
		}
	}
	
	public static IEnumerator fadeInAmbience( AudioClip audioClip, float duration )
	{
		if( audioClip != null )
		{
			Debug.Log("SoundManager-fadeIn: fading in audio clip titled " +  audioClip.name );
			
			levelAmbienceSource.clip = audioClip;
			levelAmbienceSource.volume = 0;
			levelAmbienceSource.Play ();
			float startTime = Time.time;
			float endTime = startTime + duration;
			while (Time.time < endTime)
			{
				levelAmbienceSource.volume = ( (Time.time - startTime) / duration ) * MAX_VOLUME;
				yield return new WaitForFixedUpdate();  
			}
		}
	}
	
	public static IEnumerator fadeOutAmbience( float duration )
	{
		if( levelAmbienceSource.clip != null )
		{
			Debug.Log("SoundManager-fadeOut: fading out audio clip titled " +  levelAmbienceSource.clip.name );
			float startTime = Time.time;
			float elapsedTime = 0;
			
			float startVolume = levelAmbienceSource.volume;
			float endVolume = 0;
			
			while ( elapsedTime <= duration )
			{
				elapsedTime = Time.time - startTime;
				levelAmbienceSource.volume =  Mathf.Lerp( startVolume, endVolume, elapsedTime/duration );
				yield return new WaitForEndOfFrame();  
			}
			levelAmbienceSource.volume = 0;
			levelAmbienceSource.Stop();
		}
	}

	public static void stopAmbience()
	{
		if( levelAmbienceSource.clip != null )
		{
			Debug.Log("SoundManager-stopAmbience: stopping audio clip titled " +  levelAmbienceSource.clip.name );
			levelAmbienceSource.Stop();
		}
	}

	public static IEnumerator fadeInClip( AudioSource audioSource, AudioClip audioClip, float duration )
	{
		if( audioSource != null && audioClip != null )
		{
			Debug.Log("SoundManager-fadeInClip: fading in audio clip titled " +  audioClip.name );
			audioSource.loop = true;
			audioSource.clip = audioClip;
			audioSource.volume = 0;
			if( !audioSource.isPlaying ) audioSource.Play ();

			float elapsedTime = 0;

			do
			{
				elapsedTime = elapsedTime + Time.deltaTime;
				audioSource.volume =  Mathf.Lerp( 0, MAX_VOLUME, elapsedTime/duration );
				yield return new WaitForFixedUpdate();  
				
			} while ( elapsedTime < duration );
			audioSource.volume = MAX_VOLUME;
		}
	}
	
	public static IEnumerator fadeOutClip( AudioSource audioSource, AudioClip audioClip, float duration )
	{
		if( audioSource != null && audioClip != null )
		{
			Debug.Log("SoundManager-fadeOutClip: fading out audio clip titled " +  audioClip.name );
			
			audioSource.clip = audioClip;
			float startVolume = audioSource.volume;
			float elapsedTime = 0;
			
			do
			{
				elapsedTime = elapsedTime + Time.deltaTime;
				audioSource.volume =  Mathf.Lerp( startVolume, 0, elapsedTime/duration );
				yield return new WaitForFixedUpdate();  
				
			} while ( elapsedTime < duration );
			audioSource.volume = 0;
			audioSource.Stop ();
		}
	}
}
