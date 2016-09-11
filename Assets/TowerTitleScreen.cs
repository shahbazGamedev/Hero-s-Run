using UnityEngine;
using System.Collections;

public class TowerTitleScreen : MonoBehaviour {

	public Lightning lightning;

	// Use this for initialization
	void Start ()
	{
		lightning.controlLightning(GameEvent.Start_Lightning);
	
	}
}
