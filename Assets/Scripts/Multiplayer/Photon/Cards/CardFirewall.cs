using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Firewall card is a Rare card with 11 levels. The firewall appears a few meters in front of the player. The duration of the firewall depends on the level of the caster.
/// The player is immune to its effect.
/// </summary>
public class CardFirewall : Photon.PunBehaviour {

	[SerializeField] float  baseDuration = 5f;
	[SerializeField] float  durationUpgradePerLevel = 1f;
	[SerializeField]  string firewallPrefabName;
	Vector3 offset = new Vector3( 0, 0, 6f );

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardFirewallMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardFirewallMasterRPC( int level, int photonViewID )
	{
		//Find out which player activated the card
		GameObject playerGameObject = null;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				playerGameObject = (GameObject)PhotonNetwork.player.TagObject;
			}
		}

		//Spawn a firewall a few meters in front of the player
		Vector3 firewallPosition = playerGameObject.transform.TransformPoint( offset );
		Quaternion firewallRotation = playerGameObject.transform.rotation;

		object[] data = new object[2];

		//We want the caster to be immune to the firewall
		data[0] = playerGameObject.name;

		//We want the firewall to disappear after a while
		data[1] = baseDuration + level * durationUpgradePerLevel;

		PhotonNetwork.InstantiateSceneObject( firewallPrefabName, firewallPosition, firewallRotation, 0, data );
	}
	#endregion


}
