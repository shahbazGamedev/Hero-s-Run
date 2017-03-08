using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Firewall : Photon.PunBehaviour {
	
	[SerializeField] Sprite  minimapIcon;
	public string nameOfCaster = string.Empty; //The caster is immune to the firewall.

	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") && other.gameObject.name != nameOfCaster )
		{
			other.GetComponent<PlayerControl>().managePlayerDeath ( DeathType.Flame );
		}
	}

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;
		nameOfCaster = data[0].ToString();
		float delayBeforeSpellExpires = (float) data[1];
		GameObject.Destroy( gameObject, delayBeforeSpellExpires );
		Debug.Log( "Firewall-OnPhotonInstantiate: name of caster: " + nameOfCaster + " delay before spell expires: " + delayBeforeSpellExpires );
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );
	}

}
