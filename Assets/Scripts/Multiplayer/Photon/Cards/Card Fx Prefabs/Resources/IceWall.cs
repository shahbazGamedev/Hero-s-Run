using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IceWall : Photon.PunBehaviour {
	
	[SerializeField] Sprite  minimapIcon;
	public string casterName = string.Empty; //The caster is immune to the ice wall.

	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") && other.gameObject.name != casterName )
		{
			other.GetComponent<PlayerControl>().managePlayerDeath ( DeathType.Obstacle );
		}
	}

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;
		casterName = data[0].ToString();
		float delayBeforeSpellExpires = (float) data[1];
		GameObject.Destroy( gameObject, delayBeforeSpellExpires );
		Debug.Log( "IceWall-OnPhotonInstantiate: name of caster: " + casterName + " delay before spell expires: " + delayBeforeSpellExpires );
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );
		float objectHalfHeight = transform.localScale.y * 0.5f;
		//Determine the ground height
		RaycastHit hit;
		gameObject.layer = 2; //Ignore Raycast
		if (Physics.Raycast( new Vector3( transform.position.x, transform.position.y + transform.localScale.y, transform.position.z ), Vector3.down, out hit, 2 * transform.localScale.y ))
		{
			print(" OnPhotonInstantiate " + hit.collider.name + " " + hit.point.y + " " + objectHalfHeight );
			//Try to center the object in the middle of the road
			if(  hit.collider.transform.parent.GetComponent<SegmentInfo>() != null )
			{
				Transform tile = hit.collider.transform.parent;
				float tileRotationY = Mathf.Floor ( tile.eulerAngles.y );
				print(" OnPhotonInstantiate 2 tile " + tile.name + "  tileRotationY " + tileRotationY );
				transform.SetParent( tile );
				if( tileRotationY == 0 )
				{
					transform.localPosition = new Vector3( 0, hit.point.y + objectHalfHeight, transform.localPosition.z );
				}
				else if( tileRotationY == 90f || tileRotationY == -270f || tileRotationY == -90f || tileRotationY == 270f )
				{
					transform.localPosition = new Vector3( transform.localPosition.x, hit.point.y + objectHalfHeight, 0 );
				}
				transform.SetParent( null );
			}
			else
			{
				transform.position = new Vector3( transform.position.x, hit.point.y + objectHalfHeight, transform.position.z );
			}
		}
		gameObject.layer = 0; //Default

	}


}