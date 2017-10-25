using System.Collections;
using UnityEngine;

/// <summary>
/// The Grenade card is a Rare card with 11 levels. It detonates automatically after about 1 second. The player is immune to its effect.
/// </summary>
public class CardGrenade : Card {

	[SerializeField]  string prefabName;

	public void activateCard ( int photonViewId, int level )
	{
		if( isAllowed( photonViewId ) ) this.photonView.RPC("cardGrenadeMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardGrenadeMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		object[] data = new object[2];

		//We want the caster to be immune to the blast
		data[0] = playerTransform.name;

		//We want to transmit the blast radius
		data[1] = cd.getCardPropertyValue( CardPropertyType.RADIUS, level );

		//Drop the grenade in front of the player at arm's length. It has a rigidbody and hence it has gravity and will fall to the ground.
		PhotonNetwork.InstantiateSceneObject( prefabName, playerTransform.TransformPoint( spawnOffset ), playerTransform.rotation, 0, data );
	}
	#endregion

}
