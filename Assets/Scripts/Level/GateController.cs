using UnityEngine;
using System.Collections;

public class GateController : MonoBehaviour {

	void Start ()
	{
		Invoke( "openGate", 1.5f );
	}
	
	void openGate()
	{
		GetComponent<Animator>().Play("open");
	}

	public void gateOpened()
	{
		//This will display the tap to play message
		GameManager.Instance.setGameState( GameState.Menu );
		GetComponent<AudioSource>().Play ();
	}
}
