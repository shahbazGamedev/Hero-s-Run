using UnityEngine;
using System.Collections;

public class DarkQueenKrakenSequence : MonoBehaviour {

	public AudioClip VO_FA_Oh_no;
	public AudioClip VO_DQ_not_keep_waiting;
	public AudioClip VO_DQ_rise_from_the_deep;

	public ParticleSystem poisonMist;

	void Start()
	{
		GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
		GameEventManager gem = gameEventManagerObject.GetComponent<GameEventManager>();
		gem.setOpeningSequence( this );
	}

}
