﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Firewall card is a Rare card with 11 levels. The firewall appears a few meters in front of the player. The duration of the firewall depends on the level of the caster.
/// The player is immune to its effect.
/// </summary>
public class CardFirewall : Card {

	[SerializeField]  string prefabName;

	public void activateCard ( int photonViewId, int level )
	{
		if( isAllowed( photonViewId ) ) this.photonView.RPC("cardFirewallMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardFirewallMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Spawn a firewall a few meters in front of the player
		Vector3 firewallPosition = playerTransform.TransformPoint( spawnOffset );
		Quaternion firewallRotation = playerTransform.rotation;

		object[] data = new object[2];

		//We want the caster to be immune to the firewall
		data[0] = playerTransform.name;

		//We want the firewall to disappear after a while
		data[1] = getDuration( level );

		PhotonNetwork.InstantiateSceneObject( prefabName, firewallPosition, firewallRotation, 0, data );
	}
	#endregion

}
