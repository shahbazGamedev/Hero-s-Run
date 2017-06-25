using System.Collections;
using UnityEngine;

public class Taunt : MonoBehaviour {

	float  timeTauntLastPlayed;
	const float TAUNT_COOLDOWN = 5f;

	public void playTaunt ()
	{
		if( Time.time - timeTauntLastPlayed > TAUNT_COOLDOWN )
		{
			VoiceOverManager.Instance.playTaunt();
			timeTauntLastPlayed = Time.time;
		}
	}
}
