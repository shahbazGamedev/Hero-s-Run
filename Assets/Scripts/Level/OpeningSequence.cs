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
		//Invoke("displayTapToPlay", 3.25f );		
	}

	public void playCrowSound()
	{
		audio.PlayOneShot( crow );
	}

	public void playExplosionSound()
	{
		//Explosion sound
		audio.Play();
	}

	void displayTapToPlay()
	{
		GameManager.Instance.setGameState( GameState.Menu );
	}
}



