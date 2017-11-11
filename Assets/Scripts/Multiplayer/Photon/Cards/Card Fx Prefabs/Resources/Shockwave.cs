using System.Collections;
using UnityEngine;

public class Shockwave : CardSpawnedObject {

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Note that the Shockwave Effect child object is disabled.
		//We will enable it when the card gets activated by the lockstep manager.
		//This will cause both the particle system and audio to play.
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.CARD, gameObject, CardName.Shockwave );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	public override void activateCard()
	{
		object[] data = this.gameObject.GetPhotonView ().instantiationData;
		int casterViewId = (int) data[0];
		Transform caster = getCaster( casterViewId );
		casterName = caster.name;

		//Do a ripple effect on the minimap
		//To do
		//MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );

		//Position flush with the ground and try to center it in the middle of the road if possible.
		positionSpawnedObject();

		if( caster != null ) shockwave( caster, (float) data[1] );

	}

	void shockwave( Transform caster, float radius )
	{
		//Shake the camera
		caster.GetComponent<PlayerCamera>().Shake();

		Transform shockwaveEffect = transform.Find("Shockwave Effect");
		shockwaveEffect.gameObject.SetActive( true );
		destroyAllTargetsWithinBlastRadius( radius, MaskHandler.getMaskWithPlayerWithoutDevices() );
		GameObject.Destroy( gameObject, 3 );
	}

}
