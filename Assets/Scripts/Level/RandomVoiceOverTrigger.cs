using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomVoiceOverTrigger : MonoBehaviour {

	public GameObject objectWhichWillPlayVO;
	public List<string> possibleVoiceOvers = new List<string>();
	public float percentageChanceEventTriggered = 1f;

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" && Random.value <= percentageChanceEventTriggered )
		{
			int selectedVOIndex = Random.Range(0, possibleVoiceOvers.Count);
			AudioClip selectedVO = DialogManager.dialogManager.getVoiceOver( possibleVoiceOvers[selectedVOIndex] );
			if( selectedVO != null ) objectWhichWillPlayVO.GetComponent<AudioSource>().PlayOneShot( selectedVO );
		}
	}

}
