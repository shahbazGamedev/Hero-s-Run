using UnityEngine;
using System.Collections;

public class HellArrivalStartSequence : MonoBehaviour {

	FairyController fairyController;

	// Use this for initialization
	void Awake () {

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();
	
	}
	void Start () {

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
