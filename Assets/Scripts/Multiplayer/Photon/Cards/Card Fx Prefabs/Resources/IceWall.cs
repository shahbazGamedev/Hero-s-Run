using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Ice wall. Make sure to set the parent layer to Destructible and the children's layer to Ignore Raycast (or else the ice wall will not be positioned properly).
/// </summary>
public class IceWall : CardSpawnedObject {
	
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Read the data
		object[] data = this.gameObject.GetPhotonView ().instantiationData;
		
		GetComponent<BoxCollider>().isTrigger = true;
		enableChunkColliders( false );

		//Remember who the caster is
		casterName = data[0].ToString();

		//Destroy the ice wall when the spell expires
		float spellDuration = (float) data[1];
		StartCoroutine( destroySpawnedObject( spellDuration, DELAY_BEFORE_DESTROY_EFFECTS ) );

		//Display the ice wall icon on the minimap
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position the ice wall flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject( 1.1f );

		StartCoroutine( changeMaterialOnCreate( 1f ) );
	}

	IEnumerator changeMaterialOnCreate( float delayBeforeMaterialChange )
	{
		yield return new WaitForSeconds(delayBeforeMaterialChange);
		GetComponent<Renderer>().material = onFunctioning;
		setSpawnedObjectState(SpawnedObjectState.Functioning);
		GetComponent<BoxCollider>().isTrigger = false;
		enableChunkColliders( true );
	}

	public override void destroySpawnedObjectNow()
	{
		StartCoroutine( destroySpawnedObject( 0, DELAY_BEFORE_DESTROY_EFFECTS ) );
	}

	IEnumerator destroySpawnedObject( float delayBeforeExpires, float delayBeforeDestroyEffects )
	{
		yield return new WaitForSeconds(delayBeforeExpires);
		GetComponent<BoxCollider>().isTrigger = true;
		setSpawnedObjectState(SpawnedObjectState.BeingDestroyed);
		StopCoroutine( "changeMaterialOnCreate" );
		GetComponent<Renderer>().material = onDestroy;
		yield return new WaitForSeconds(delayBeforeDestroyEffects);
		Destroy( gameObject );
	}

	void enableChunkColliders( bool value )
	{
		for( int i = 0; i < transform.childCount; i++ )
		{
			MeshCollider mc = transform.GetChild( i ).GetComponent<MeshCollider>();
			if( mc != null ) mc.enabled = value;
		}
	}
}