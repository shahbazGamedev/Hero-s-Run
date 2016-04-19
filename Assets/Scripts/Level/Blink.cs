using UnityEngine;
using System.Collections;

public class Blink : MonoBehaviour {

	public Texture eyesOpened;
	public Texture eyesClosed;
	bool eyesAreOpened = true;

	// Use this for initialization
	void OnEnable()
	{
		if( Random.value > 0.5f )
		{
			eyesAreOpened = true;
			GetComponent<Renderer>().material.mainTexture = eyesOpened;
		}
		else
		{
			eyesAreOpened = false;
			GetComponent<Renderer>().material.mainTexture = eyesClosed;
		}
		Invoke("blink", 0.2f );
	}
	
	void OnDisable()
	{
		CancelInvoke();
	}

	void blink ()
	{
		if( eyesAreOpened )
		{
			GetComponent<Renderer>().material.mainTexture = eyesClosed;
			//Open eyes after...
			Invoke("blink", Random.Range(0.3f, 0.8f) );
		}
		else
		{
			GetComponent<Renderer>().material.mainTexture = eyesOpened;
			//Close eyes after...
			Invoke("blink", Random.Range(1.5f, 2.5f) );
		}
		eyesAreOpened = !eyesAreOpened;

	}
}
