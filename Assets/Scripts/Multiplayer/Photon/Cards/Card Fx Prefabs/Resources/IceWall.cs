using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IceWall : Photon.PunBehaviour {
	
	[SerializeField] Sprite  minimapIcon;
	string casterName = string.Empty; //The caster can run through the ice wall like it did not exist.

	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") && other.gameObject.name != casterName )
		{
			other.GetComponent<PlayerControl>().managePlayerDeath ( DeathType.Obstacle );
		}
	}

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Read the data
		object[] data = this.gameObject.GetPhotonView ().instantiationData;

		//Remember who the caster is
		casterName = data[0].ToString();

		//Destroy the ice wall when the spell expires
		float delayBeforeSpellExpires = (float) data[1];
		GameObject.Destroy( gameObject, delayBeforeSpellExpires );

		//Display the ice wall icon on the minimap
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position the ice wall flush with the ground and try to center it in the middle of the road if possible.
		RaycastHit hit;
		gameObject.layer = 2; //Set the layer to Ignore Raycast so the raycast doesn't collide with the object itself.
		if (Physics.Raycast( transform.position, Vector3.down, out hit, 10 * transform.localScale.y ))
		{
			if(  hit.collider.transform.parent.GetComponent<SegmentInfo>() != null )
			{
				Transform tile = hit.collider.transform.parent;
				transform.SetParent( tile );
				//Center the object in the middle of the road
				transform.localPosition = new Vector3( 0, 0, transform.localPosition.z );
				transform.SetParent( null );
			}
			//Position it flush with the ground
			float objectHalfHeight = transform.localScale.y * 0.5f;
			transform.position = new Vector3( transform.position.x, hit.point.y + objectHalfHeight, transform.position.z );
		}
		//Now that our raycast is finished, reset the object's layer to Default.
		gameObject.layer = 0;
	}


}