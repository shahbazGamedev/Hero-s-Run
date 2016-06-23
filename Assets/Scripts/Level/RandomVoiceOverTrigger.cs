using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomVoiceOverTrigger : MonoBehaviour {

	public GameObject objectWhichWillPlayVO;
	public List<AudioClip> possibleVoiceOvers = new List<AudioClip>(1);
	public float percentageChanceEventTriggered = 1f;

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" && Random.value <= percentageChanceEventTriggered )
		{
			int selectedVOIndex = Random.Range(0, possibleVoiceOvers.Count);
			AudioClip selectedVO = possibleVoiceOvers[selectedVOIndex];
			if( selectedVO != null ) objectWhichWillPlayVO.GetComponent<AudioSource>().PlayOneShot( selectedVO );
		}
	}

	void Update ()
	{
		#if UNITY_EDITOR
		if ( Input.GetKeyDown (KeyCode.B) ) 
		{
			int selectedVOIndex = Random.Range(0, possibleVoiceOvers.Count);
			AudioClip selectedVO = possibleVoiceOvers[selectedVOIndex];
			if( selectedVO != null ) objectWhichWillPlayVO.GetComponent<AudioSource>().PlayOneShot( selectedVO );
		
		}
		#endif
	}


}
