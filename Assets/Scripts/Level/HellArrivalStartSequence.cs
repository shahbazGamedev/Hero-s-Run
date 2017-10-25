using UnityEngine;
using System.Collections;

public class HellArrivalStartSequence : MonoBehaviour {

	FairyController fairyController;

	void Start ()
	{
		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();

		Invoke("step1", 2f );
	}
	
	void step1()
	{
		fairyController.speak( "VO_FA_HOT_IN_HERE", 2f, false );
		Invoke("showTapToPlay", 4f );
	}

	void showTapToPlay()
	{
		GameManager.Instance.setGameState( GameState.Menu );
	}

}
