using UnityEngine;
using System.Collections;

public class Ruler : MonoBehaviour {

	public float distanceToGround;

	void OnDrawGizmos ()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Vector3.down, out hit, 100.0F ))
		{
			distanceToGround = hit.distance;
			//print ("below " + hit.collider.name );
		}
		else
		{
			//Ground is further than 100 meters or possibly there is no collider below the player.
			//Just set an arbitrarely big value.
			distanceToGround = 1000f;
		}
	}
}
