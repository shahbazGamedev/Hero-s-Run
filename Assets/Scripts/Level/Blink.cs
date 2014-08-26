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
			renderer.material.mainTexture = eyesOpened;
		}
		else
		{
			eyesAreOpened = false;
			renderer.material.mainTexture = eyesClosed;
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
			renderer.material.mainTexture = eyesClosed;
			//Open eyes after...
			Invoke("blink", Random.Range(0.3f, 0.8f) );
		}
		else
		{
			renderer.material.mainTexture = eyesOpened;
			//Close eyes after...
			Invoke("blink", Random.Range(1f, 2f) );
		}
		eyesAreOpened = !eyesAreOpened;

	}
}
