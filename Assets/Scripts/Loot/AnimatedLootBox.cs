using System.Collections;
using UnityEngine;

public class AnimatedLootBox : MonoBehaviour {

	[SerializeField] GameObject lid;
	bool isOpen = false;

	public void Open ()
	{
		if( isOpen ) return;
		isOpen = true;
		LeanTween.rotateX( lid, -60f, 0.4f ).setEaseOutCubic();
		GetComponent<AudioSource>().Play();
	}
	
}
