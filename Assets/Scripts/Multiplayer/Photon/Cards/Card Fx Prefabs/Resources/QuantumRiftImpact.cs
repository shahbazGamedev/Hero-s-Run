using System.Collections;
using UnityEngine;

public class QuantumRiftImpact : MonoBehaviour {

	void OnCollisionEnter(Collision collision)
	{
		if( collision.collider.CompareTag("Player") || collision.collider.CompareTag("Zombie") )
		{
			transform.root.GetComponent<QuantumRift>().rockHitSomeone ( collision.collider.gameObject );
		}
	}
}
