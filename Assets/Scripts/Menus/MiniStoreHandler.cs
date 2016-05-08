using UnityEngine;
using System.Collections;

public class MiniStoreHandler : MonoBehaviour {

	public GameObject saveMeCanvas;
	
	public void showMiniStore()
	{
		saveMeCanvas.SetActive( false );
		saveMeCanvas.GetComponent<CanvasGroup>().alpha = 0;
		GetComponent<Animator>().Play("Panel Slide In");
	}

	public void hideMiniStore()
	{
		Invoke("reactivateSaveMeCanvas", 0.5f );
		GetComponent<Animator>().Play("Panel Slide Out");
	}

	void reactivateSaveMeCanvas()
	{
		//Wait until slide out finished
		saveMeCanvas.SetActive( true );
		StartCoroutine( Utilities.fadeInCanvasGroup( saveMeCanvas.GetComponent<CanvasGroup>(), 0.9f ) );
	}

}
