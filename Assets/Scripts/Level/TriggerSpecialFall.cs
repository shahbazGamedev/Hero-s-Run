using UnityEngine;
using System.Collections;

public class TriggerSpecialFall : MonoBehaviour {

	PlayerController playerController;

	// Use this for initialization
	void Awake ()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.GetComponent<PlayerController>();
	}

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero"  )
		{
			Debug.Log("*********Special fall");
			playerController.managePlayerDeath(DeathType.SpecialFall);
		}
	}
}