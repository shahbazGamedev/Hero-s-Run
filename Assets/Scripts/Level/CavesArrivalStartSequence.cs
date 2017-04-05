using UnityEngine;
using System.Collections;

public class CavesArrivalStartSequence : MonoBehaviour {

	FairyController fairyController;
	public Rigidbody skull;

	void Start ()
	{
		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();

		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			Invoke("step1", 1.5f );
		}
		else
		{
			showTapToPlay();
		}
	}
	
	void step1()
	{
		fairyController.speak( "VO_FA_DARK_QUEEN_SABOTAGED", 4f, false );
		Invoke("step2", 5f );
	}

	void step2()
	{
		skull.isKinematic = false;
		skull.AddForce( 0,10f,5f);
		Invoke("showTapToPlay", 2.7f );
	}

	void showTapToPlay()
	{
		GameManager.Instance.setGameState( GameState.Menu );
	}

}
