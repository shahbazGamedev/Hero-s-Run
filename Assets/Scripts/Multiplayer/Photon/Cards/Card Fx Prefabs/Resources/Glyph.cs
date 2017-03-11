using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Glyph : Photon.PunBehaviour {
	
	[SerializeField] Sprite  minimapIcon;
	string nameOfCaster = string.Empty; //The caster is immune to the spell.

	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") && other.gameObject.name != nameOfCaster )
		{
			other.GetComponent<PlayerControl>().managePlayerDeath ( DeathType.FallForward );
		}
	}

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;
		nameOfCaster = data[0].ToString();
		float delayBeforeSpellExpires = (float) data[1];
		GameObject.Destroy( gameObject, delayBeforeSpellExpires );
		Debug.Log( "Glyph-OnPhotonInstantiate: name of caster: " + nameOfCaster + " delay before spell expires: " + delayBeforeSpellExpires );
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );
		//Calculate the ground height
		RaycastHit hit;
		if (Physics.Raycast(new Vector3( transform.position.x, transform.position.y + 5f, transform.position.z ), Vector3.down, out hit, 8.0F ))
		{
			transform.position = new Vector3( transform.position.x, hit.point.y, transform.position.z);
		}
	}

}