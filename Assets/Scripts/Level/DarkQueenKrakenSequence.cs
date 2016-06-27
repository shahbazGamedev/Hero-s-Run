using UnityEngine;
using System.Collections;

public class DarkQueenKrakenSequence : MonoBehaviour {

	public ParticleSystem poisonMist;

	void Start()
	{
		GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
		GameEventManager gem = gameEventManagerObject.GetComponent<GameEventManager>();
		gem.setOpeningSequence( this );
	}

}
