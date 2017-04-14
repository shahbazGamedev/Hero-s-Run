using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum IceWallState {
	
		Initialising = 1,
		Functioning = 2,
		BeingDestroyed = 3
	}

public class IceWall : Photon.PunBehaviour {
	
	[SerializeField] Sprite  minimapIcon;
	public string casterName = string.Empty;
	[Header("Materials")]
	[SerializeField] Material onCreate;
	[SerializeField] Material onDestroy;
	[SerializeField] Material onFunctioning;
	IceWallState iceWallState = IceWallState.Initialising;
	const float DELAY_BEFORE_DESTROY_EFFECTS = 1.3f;


	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Read the data
		object[] data = this.gameObject.GetPhotonView ().instantiationData;
		
		GetComponent<BoxCollider>().isTrigger = true;

		//Remember who the caster is
		casterName = data[0].ToString();

		//Destroy the ice wall when the spell expires
		float spellDuration = (float) data[1];
		StartCoroutine( destroyIceWall( spellDuration, DELAY_BEFORE_DESTROY_EFFECTS ) );

		//Display the ice wall icon on the minimap
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position the ice wall flush with the ground and try to center it in the middle of the road if possible.
		RaycastHit hit;
		gameObject.layer = 2; //Set the layer to Ignore Raycast so the raycast doesn't collide with the object itself.
		if (Physics.Raycast( new Vector3( transform.position.x, transform.position.y + transform.localScale.y, transform.position.z ), Vector3.down, out hit, 10 * transform.localScale.y ))
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
		//Now that our raycast is finished, reset the object's layer to Destroyable.
		gameObject.layer = 17;
		StartCoroutine( changeMaterialOnCreate( 1.1f ) );
	}
	IEnumerator changeMaterialOnCreate( float delayBeforeMaterialChange )
	{
		yield return new WaitForSeconds(delayBeforeMaterialChange);
		GetComponent<Renderer>().material = onFunctioning;
		setIceWallState(IceWallState.Functioning);
		GetComponent<BoxCollider>().isTrigger = false;
	}

	void setIceWallState( IceWallState newState )
	{
		iceWallState = newState;
	}

	public IceWallState getIceWallState()
	{
		return iceWallState;
	}

	public void destroyIceWallNow()
	{
		StartCoroutine( destroyIceWall( 0, DELAY_BEFORE_DESTROY_EFFECTS ) );
	}

	IEnumerator destroyIceWall( float delayBeforeSentryExpires, float delayBeforeDestroyEffects )
	{
		yield return new WaitForSeconds(delayBeforeSentryExpires);
		GetComponent<BoxCollider>().isTrigger = true;

		setIceWallState(IceWallState.BeingDestroyed);
		StopCoroutine( "changeMaterialOnCreate" );
		GetComponent<Renderer>().material = onDestroy;
		//playSoundEffect( Emotion.Sad, true );

		yield return new WaitForSeconds(delayBeforeDestroyEffects);
		//onDestroyFx.transform.SetParent( null );
		//onDestroyFx.Play();
		Destroy( gameObject );
		//Destroy( onDestroyFx, 3f );
	}


}