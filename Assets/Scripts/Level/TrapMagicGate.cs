using UnityEngine;
using System.Collections;

public class TrapMagicGate : MonoBehaviour {

	PlayerController playerController;
	public static float delayBeforeBeingPulledDown = 0.35f;
	public static float timeRequiredToGoDown = 1.5f;
	public static float distanceTravelledDown = 3.5f;

	// Use this for initialization
	void Start ()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.gameObject.GetComponent<PlayerController>();
	}
	
	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			Debug.Log ("Player triggered magic gate trap "  );
			playerController.managePlayerDeath(DeathType.MagicGate);
		}
	}

}
