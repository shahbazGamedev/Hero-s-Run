using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomVoiceOverTrigger : MonoBehaviour {

	public GameObject objectWhichWillPlayVO;
	public List<string> possibleVoiceOvers = new List<string>();
	public float percentageChanceEventTriggered = 1f;

	void OnTriggerEnter(Collider other)
	{
		if( gameObject.CompareTag("Player") && Random.value <= percentageChanceEventTriggered )
		{
			int selectedVOIndex = Random.Range(0, possibleVoiceOvers.Count);
			AudioClip selectedVO = DialogManager.dialogManager.getVoiceOver( possibleVoiceOvers[selectedVOIndex] );
			if( selectedVO != null ) objectWhichWillPlayVO.GetComponent<AudioSource>().PlayOneShot( selectedVO );
		}
	}

}
