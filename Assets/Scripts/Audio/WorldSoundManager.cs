using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class WorldSoundManager : MonoBehaviour {

	[Header("Audio Sources")]
   	public AudioSource quietMusicAudioSource;
   	public AudioSource actionMusicAudioSource;
    public AudioSource stingAudioSource;
    public AudioSource mainAmbienceAudioSource;
    public AudioSource secondaryAmbienceAudioSource;

	[Header("Snapshots")]
    public AudioMixerSnapshot onlyQuietMusic;
    public AudioMixerSnapshot withActionMusic;
    public AudioMixerSnapshot lowMusic;
	public AudioMixerSnapshot ambienceNoneActive;
    public AudioMixerSnapshot ambienceMainActive;
	public AudioMixerSnapshot ambienceSecondaryActive;

	[Header("Audio Mixers")]
	public AudioMixer worldEffectsMixer;
	public AudioMixer worldDialogMixer;
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
		mainAmbienceAudioSource.clip = currentEpisode.mainAmbienceTrack;
		secondaryAmbienceAudioSource.clip = currentEpisode.secondaryAmbienceTrack;
		//Reset values
        ambienceNoneActive.TransitionTo(0f);
		worldEffectsMixer.SetFloat("Reverb Intensity", -80f );
		worldDialogMixer.SetFloat("Echo Intensity", -80f );
    }

	void Start()
	{
		if( !mainAmbienceAudioSource.isPlaying ) mainAmbienceAudioSource.Play();
		ambienceMainActive.TransitionTo( 4f );
	}

    void PlaySting()
    {
        int randClip = Random.Range (0, stings.Length);
        stingAudioSource.clip = stings[randClip];
        stingAudioSource.Play();
    }

	public void crossFadeToSecondaryAmbience( float timeToReach )
	{
		if( !secondaryAmbienceAudioSource.isPlaying ) secondaryAmbienceAudioSource.Play();
		ambienceSecondaryActive.TransitionTo( timeToReach );
	}

	void OnEnable()
	{
		TriggerSoundEvent.sendSoundEvent += SendSoundEvent;
		PlayerController.playerStateChanged += PlayerStateChange;
		GameManager.gameStateEvent += GameStateChange;
	}
	
	void OnDisable()
	{
		TriggerSoundEvent.sendSoundEvent -= SendSoundEvent;
		PlayerController.playerStateChanged -= PlayerStateChange;
		GameManager.gameStateEvent -= GameStateChange;
	}

	void SendSoundEvent( SoundEvent eventType, float intensity )
	{
		if( eventType == SoundEvent.To_Combat_Music )
		{
			//Player entered combat, play more action-oriented music
			isActionMusicPlaying = true;
			if( !actionMusicAudioSource.isPlaying ) actionMusicAudioSource.Play();
            withActionMusic.TransitionTo(m_TransitionIn);
            PlaySting();
		}
		else if( eventType == SoundEvent.To_Quiet_Music )
		{
			//Player left combat zone, resume to quieter music
			isActionMusicPlaying = false;
			onlyQuietMusic.TransitionTo(m_TransitionOut);
		} 
		else if( eventType == SoundEvent.Start_Reverb )
		{
			worldEffectsMixer.SetFloat("Reverb Intensity", intensity );
		} 
		else if( eventType == SoundEvent.Stop_Reverb )
		{
			worldEffectsMixer.SetFloat("Reverb Intensity", -80f );
		} 
		else if( eventType == SoundEvent.Start_Echo )
		{
			worldEffectsMixer.SetFloat("Echo Intensity", intensity );
		} 
		else if( eventType == SoundEvent.Stop_Echo )
		{
			worldEffectsMixer.SetFloat("Echo Intensity", -80f );
		} 
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.StartRunning )
		{
			startMusic();
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

	void startMusic()
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
}
