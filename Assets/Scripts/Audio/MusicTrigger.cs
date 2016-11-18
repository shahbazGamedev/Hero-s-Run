using UnityEngine;
using System.Collections;

public enum MusicEvent {
	To_Combat_Music = 1,
	To_Quiet_Music = 2,
}

public class MusicTrigger : MonoBehaviour {

	public delegate void TriggerMusic( MusicEvent eventType );
	public static event TriggerMusic triggerMusic;
	public MusicEvent eventType;

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			//Send an event to interested classes
			if(triggerMusic != null) triggerMusic( eventType );
		}
	}
}
