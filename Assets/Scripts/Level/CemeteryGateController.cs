using UnityEngine;
using System.Collections;

public class CemeteryGateController : MonoBehaviour {

	void Start ()
	{
		Invoke( "openCemeteryGate", 1.5f );
	}
	
	void openCemeteryGate()
	{
		Animation animation = GetComponent<Animation>();
		GetComponent<AudioSource>().Play ();
		animation.Play("open slowly");
		animation.PlayQueued("sway in wind");
		float delay = animation["open slowly"].length;
		Invoke("displayTapToPlay", delay );		
	}

	void displayTapToPlay()
	{
		GameManager.Instance.setGameState( GameState.Menu );
	}

}
