using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Homing Missile card is a Epic card with 8 levels. When summoned, the homing missile appears a few meters above the player. The lifetime of the homing missile depends on the level of the caster.
/// The player is immune to its effect. The missile will fly towards the nearest target and explode on impact. If the target dies while the homing missile is in flight, the homing missile will look 
/// for another target. The impact explosion could kill additional players that are nearby but not the caster.
/// </summary>
public class CardHomingMissile : Card {

	[SerializeField]  string prefabName = "Homing Missile";

	public void activateCard ( int photonViewId, int level )
	{
		if( isAllowed( photonViewId ) ) this.photonView.RPC("cardHomingMissileMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardHomingMissileMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Create homing missile
		object[] data = new object[2];
		data[0] = photonViewID;
		//We want the homing missile to self-destruct after a while
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		data[1] = cd.getCardPropertyValue( CardPropertyType.DURATION, level );
		PhotonNetwork.InstantiateSceneObject( prefabName, playerTransform.TransformPoint( getSpawnOffset() ), playerTransform.rotation, 0, data );

	}
	#endregion
}
