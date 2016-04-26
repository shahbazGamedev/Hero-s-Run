using UnityEngine;
using System.Collections;

public class TreasureKeyHandler : MonoBehaviour {

	void Start ()
	{
		considerActivatingTreasureKey ();
	}

	private void considerActivatingTreasureKey ()
	{
		GameObject treasureKeyPrefab = Resources.Load( "Level/Props/Treasure Key/Treasure Key") as GameObject;
		GameObject go = (GameObject)Instantiate(treasureKeyPrefab, gameObject.transform.position, gameObject.transform.rotation );
		go.gameObject.transform.parent = gameObject.transform;
	}
	
}
