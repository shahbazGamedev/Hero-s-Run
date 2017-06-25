using System.Collections;
using UnityEngine;

public class Taunt : MonoBehaviour {

	public void playTaunt ()
	{
		VoiceOverManager.Instance.playTaunt();
	}
}
