using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSpeedBoost : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	public void activateCard ( string name, int level)
	{
		Debug.Log("CardSpeedBoost-activating card " + name + " with a level of " + level );
	}
}
