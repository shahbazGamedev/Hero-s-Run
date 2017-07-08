using UnityEngine;
using System.Collections;

//The collider should have the "Obstacle_M" tag.
public class Flamethrower : MonoBehaviour {

	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Player") )
		{
			other.GetComponent<PlayerControl>().killPlayer ( DeathType.Flame );
		}
	}

}
