using UnityEngine;
using System.Collections;

public class OpeningSequence : MonoBehaviour {

	public ParticleSystem explosionFx;
	public ParticleSystem fireFx;
	public GameObject breakableDoor;
	public GameObject door;
	public AudioClip crow;
	public Transform rainLocation;

	void Start()
	{
		GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
		GameEventManager gem = gameEventManagerObject.GetComponent<GameEventManager>();
		gem.setOpeningSequence( this );
	}

	public void playCrowSound()
	{
		GetComponent<AudioSource>().PlayOneShot( crow );
	}

	public void playExplosionSound()
	{
		//Explosion sound
		GetComponent<AudioSource>().Play();
	}
}



