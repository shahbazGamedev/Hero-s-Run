using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Glyph : CardSpawnedObject {
	
	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") && other.gameObject.name != casterName )
		{
			other.GetComponent<PlayerControl>().managePlayerDeath ( DeathType.FallForward );
		}
	}

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;

		casterName = data[0].ToString();

		float delayBeforeSpellExpires = (float) data[1];
		GameObject.Destroy( gameObject, delayBeforeSpellExpires );

		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Calculate the ground height
		RaycastHit hit;
		if (Physics.Raycast(new Vector3( transform.position.x, transform.position.y + 5f, transform.position.z ), Vector3.down, out hit, 8.0F ))
		{
			transform.position = new Vector3( transform.position.x, hit.point.y + 0.03f, transform.position.z);
		}
		gameObject.layer = 0; //remove the ignoreRaycast so the bot can detect it
	}

}