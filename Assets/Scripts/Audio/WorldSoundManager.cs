using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class WorldSoundManager : MonoBehaviour {

	[Header("Audio Sources")]
   	public AudioSource quietMusicAudioSource;
   	public AudioSource actionMusicAudioSource;
    public AudioSource stingAudioSource;
	[Header("Snapshots")]
    public AudioMixerSnapshot onlyQuietMusic;
    public AudioMixerSnapshot withActionMusic;
    public AudioMixerSnapshot lowMusic;
	[Header("Stings")]
    public AudioClip[] stings;

	private bool isActionMusicPlaying = false;
    private float m_TransitionIn = 1.5f;
    private float m_TransitionOut = 3f;
 
    void Awake () 
    {
		LevelData.EpisodeInfo currentEpisode = LevelManager.Instance.getCurrentEpisodeInfo();
		quietMusicAudioSource.clip = currentEpisode.quietMusicTrack;
		actionMusicAudioSource.clip = currentEpisode.actionMusicTrack;
    }

    void PlaySting()
    {
        int randClip = Random.Range (0, stings.Length);
        stingAudioSource.clip = stings[randClip];
        stingAudioSource.Play();
    }

	void OnEnable()
	{
		MusicTrigger.triggerMusic += TriggerMusic;
		PlayerController.playerStateChanged += PlayerStateChange;
		GameManager.gameStateEvent += GameStateChange;
	}
	
	void OnDisable()
	{
		MusicTrigger.triggerMusic -= TriggerMusic;
		PlayerController.playerStateChanged -= PlayerStateChange;
		GameManager.gameStateEvent -= GameStateChange;
	}

	void TriggerMusic( MusicEvent eventType )
	{
		if( eventType == MusicEvent.To_Combat_Music )
		{
			//Player entered combat, play more action-oriented music
			isActionMusicPlaying = true;
			if( !actionMusicAudioSource.isPlaying ) actionMusicAudioSource.Play();
            withActionMusic.TransitionTo(m_TransitionIn);
            PlaySting();
		}
		else if( eventType == MusicEvent.To_Quiet_Music )
		{
			//Player left combat zone, resume to quieter music
			isActionMusicPlaying = false;
			onlyQuietMusic.TransitionTo(m_TransitionOut);
		} 
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.StartRunning )
		{
			if( isActionMusicPlaying )
			{
				withActionMusic.TransitionTo(m_TransitionOut);
			}
			else
			{
				if( quietMusicAudioSource.clip != null )
				{
					if( !quietMusicAudioSource.isPlaying ) quietMusicAudioSource.Play();
					onlyQuietMusic.TransitionTo(2f);
				}
			}
		}
		else if( newState == CharacterState.Dying )
		{
         	lowMusic.TransitionTo(1f);
		}
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Checkpoint )
		{
         	lowMusic.TransitionTo(1f);
		}
	}

	void Update()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.F) ) 
		{
			isActionMusicPlaying = true;
			if( !actionMusicAudioSource.isPlaying ) actionMusicAudioSource.Play();
            withActionMusic.TransitionTo(m_TransitionIn);
            PlaySting();
		}
		else if ( Input.GetKeyDown (KeyCode.G) ) 
		{
			isActionMusicPlaying = false;
			onlyQuietMusic.TransitionTo(m_TransitionOut);
		}
	}

}
