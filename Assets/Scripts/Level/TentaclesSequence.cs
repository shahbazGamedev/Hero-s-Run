using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TentaclesSequence : MonoBehaviour {

	public GameObject tentaclePrefab;
	public GameObject debrisPrefab;
	public GameObject groundDebrisPrefab;
	public ParticleSystem tentacleAboutToAppearFx;
	
	void Start()
	{
		GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
		GameEventManager gem = gameEventManagerObject.GetComponent<GameEventManager>();
		gem.setOpeningSequence( this );
	}

}
