using UnityEngine;
using System.Collections;

public class Blink : MonoBehaviour {

	public Texture eyesOpened;
	public Texture eyesClosed;
	bool eyesAreOpened = true;

	// Use this for initialization
	void Start () {
	
		Invoke("blink", 1f );
	}
	
	void blink ()
	{
		if( eyesAreOpened )
		{
			renderer.material.mainTexture = eyesClosed;
		}
		else
		{
			renderer.material.mainTexture = eyesOpened;
		}
		eyesAreOpened = !eyesAreOpened;
		Invoke("blink", Random.Range(0.4f, 1.5f) );

	}
}
