﻿using UnityEngine;
using System.Collections;

public class TrapMagicGate : MonoBehaviour {

	PlayerController playerController;
	public static float delayBeforeBeingPulledDown = 0.5f;
	public static float timeRequiredToGoDown = 2.3f;
	public static float distanceTravelledDown = 20f;

	// Use this for initialization
	void Start ()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.gameObject.GetComponent<PlayerController>();
	}
	
	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") )
		{
			Debug.Log ("Player triggered magic gate trap "  );
			playerController.managePlayerDeath(DeathType.MagicGate);
			LevelManager.Instance.getLevelData().setSunParameters(SunType.Afternoon);
		}
	}

}
