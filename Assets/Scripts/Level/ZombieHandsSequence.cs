using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieHandsSequence : MonoBehaviour {

	public GameObject zombieHandPrefab;
	public GameObject debrisPrefab;
	public GameObject groundDebrisPrefab;
	public ParticleSystem zombieHandAboutToAppearFx;
	public ParticleSystem burstOutFx;

	void Start()
	{
		GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
		GameEventManager gem = gameEventManagerObject.GetComponent<GameEventManager>();
		gem.setOpeningSequence( this );
	}

}
