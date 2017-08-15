using UnityEngine;
using System.Collections;

public class Firewall : CardSpawnedObject {
	
	int flameDamage;
	[SerializeField] AudioClip onFlameContact;

	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Player") && other.name != casterName )
		{
			other.GetComponent<AudioSource>().PlayOneShot(onFlameContact);
			other.GetComponent<PlayerHealth>().deductHealth( flameDamage );
		}
	}

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;

		casterName = data[0].ToString();

		float delayBeforeSpellExpires = (float) data[1];
		GameObject.Destroy( gameObject, delayBeforeSpellExpires );

		flameDamage = (int) data[2];

		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position the fire wall flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject();

	}

}
