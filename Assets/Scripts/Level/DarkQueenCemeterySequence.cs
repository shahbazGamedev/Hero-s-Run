using UnityEngine;
using System.Collections;

public class DarkQueenCemeterySequence : MonoBehaviour {

	public AudioClip VO_FA_NOT_HER_AGAIN; //The fairy saw the Dark Queen previously in Fairyland
	public AudioClip VO_DQ_STARTING_TO_ANNOY;
	public AudioClip VO_DQ_BRING_BACK_BOOK;

	public ParticleSystem poisonMist;

	public GameObject zombieWaveObject;

	void Start()
	{
		GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
		GameEventManager gem = gameEventManagerObject.GetComponent<GameEventManager>();
		gem.setOpeningSequence( this );
	}

}
