using UnityEngine;
using System.Collections;

public enum SoundEvent {
	To_Combat_Music = 1,
	To_Quiet_Music = 2,
	Start_Reverb = 3,
	Stop_Reverb = 4,
	Start_Echo = 5,
	Stop_Echo = 6
}

public class TriggerSoundEvent : MonoBehaviour {

	public delegate void SendSoundEvent( SoundEvent eventType, float reverbValue );
	public static event SendSoundEvent sendSoundEvent;
	public SoundEvent eventType;
	[Range(-80f, 0)]
	[Tooltip("Intensity of reverb/echo send level between -80db and 0.")]
	public float intensity = -80f;
	[Tooltip("If sendOnStart is true, the event will be sent as soon as the Start method gets called. This allows you to send an event without a trigger.")]
	public bool sendOnStart = false;

	void Start()
	{
		if( sendOnStart && sendSoundEvent != null ) sendSoundEvent( eventType, intensity );
	}

	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") )
		{
			//Send an event to interested classes
			if(sendSoundEvent != null) sendSoundEvent( eventType, intensity );
		}
	}
}
