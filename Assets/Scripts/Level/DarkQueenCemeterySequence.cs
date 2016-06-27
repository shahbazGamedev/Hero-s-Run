using UnityEngine;
using System.Collections;

public class DarkQueenCemeterySequence : MonoBehaviour {

	public ParticleSystem poisonMist;

	public GameObject zombieWaveObject;

	void Start()
	{
		GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
		GameEventManager gem = gameEventManagerObject.GetComponent<GameEventManager>();
		gem.setOpeningSequence( this );
	}

}
