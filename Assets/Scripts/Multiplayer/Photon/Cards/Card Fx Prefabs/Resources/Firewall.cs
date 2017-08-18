using UnityEngine;
using System.Collections;

public class Firewall : CardSpawnedObject {
	
	int flameDamage;
	[SerializeField] AudioClip onFlameContact;
	[SerializeField] GameObject flames;

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
		//Note that the Flames game object prefab and the box collider are disabled.
		//They will be enabled when the card gets activated by the lockstep manager.
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.CARD, gameObject, CardName.Firewall );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	public override void activateCard()
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;

		casterName = data[0].ToString();

		float delayBeforeSpellExpires = (float) data[1];
		GameObject.Destroy( gameObject, delayBeforeSpellExpires );

		flameDamage = (int) data[2];

		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position the fire wall flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject();

		GetComponent<BoxCollider>().enabled = true;
		flames.SetActive( true );
	}


}
