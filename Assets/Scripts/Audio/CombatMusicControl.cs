using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class CombatMusicControl : MonoBehaviour {

    public AudioMixerSnapshot outOfCombat;
    public AudioMixerSnapshot inCombat;
    public AudioClip[] stings;
    public AudioSource stingSource;
    public float bpm = 128;


    private float m_TransitionIn;
    private float m_TransitionOut;
    private float m_QuarterNote;

    void Start () 
    {
        m_QuarterNote = 60 / bpm;
        m_TransitionIn = m_QuarterNote;
        m_TransitionOut = m_QuarterNote * 32;

    }

    void PlaySting()
    {
        int randClip = Random.Range (0, stings.Length);
        stingSource.clip = stings[randClip];
        stingSource.Play();
    }

	void OnEnable()
	{
		MusicTrigger.triggerMusic += TriggerMusic;
	}
	
	void OnDisable()
	{
		MusicTrigger.triggerMusic -= TriggerMusic;
	}

	void TriggerMusic( MusicEvent eventType )
	{
		if( eventType == MusicEvent.To_Combat_Music )
		{
			//Player entered combat, play more action-oriented music
            inCombat.TransitionTo(m_TransitionIn);
            PlaySting();
		}
		else if( eventType == MusicEvent.To_Quiet_Music )
		{
			//Player left combat zone, resume to quieter music
           outOfCombat.TransitionTo(m_TransitionOut);
		} 
	}

	void Update()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.F) ) 
		{
            inCombat.TransitionTo(m_TransitionIn);
            PlaySting();
		}
		else if ( Input.GetKeyDown (KeyCode.G) ) 
		{
           outOfCombat.TransitionTo(m_TransitionOut);
		}
	}

}
