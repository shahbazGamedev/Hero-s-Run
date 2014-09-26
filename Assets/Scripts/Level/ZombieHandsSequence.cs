using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieHandsSequence : MonoBehaviour {

	public GameObject zombieHandPrefab; 				//The arm sticking out of the ground
	public GameObject debrisPrefab;						//The debris (with rigid bodies) that will fly around
	public ParticleSystem zombieHandAboutToAppearFx;	//The FX to play just before the hand bursts out of the groound
	public ParticleSystem burstOutFx;					//The FX that plays as the hand bursts out, sending, dust and debris up in the air

	void Start()
	{
		GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
		GameEventManager gem = gameEventManagerObject.GetComponent<GameEventManager>();
		gem.setOpeningSequence( this );
	}

}
