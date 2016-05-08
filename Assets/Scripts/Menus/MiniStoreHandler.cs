using UnityEngine;
using System.Collections;

public class MiniStoreHandler : MonoBehaviour {

	public GameObject saveMeCanvas;
	
	public void showMiniStore()
	{
		saveMeCanvas.SetActive( false );
		GetComponent<Animator>().Play("Panel Slide In");
	}

	public void hideMiniStore()
	{
		saveMeCanvas.SetActive( true );
		GetComponent<Animator>().Play("Panel Slide Out");
	}

}
