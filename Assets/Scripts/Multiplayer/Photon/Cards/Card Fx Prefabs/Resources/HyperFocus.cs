using System.Collections;
using UnityEngine;

public class HyperFocus : AutoPilot {

	// Use this for initialization
	void Awake ()
	{
		base.Awake();
		percentageWillTryToAvoidObstacle = 1f;
		percentageWillTurnSuccesfully = 1f;

		//Reduce the change lane speed while in hyper focus. A high speed does not look natural.
		playerControl.sideMoveSpeed = 3.5f;
	}

	// Update is called once per frame
	void Update ()
	{
		detectObstacles();
	}

}
