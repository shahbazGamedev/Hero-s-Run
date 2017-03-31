using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Firewall card is a Rare card with 11 levels. The firewall appears a few meters in front of the player. The duration of the firewall depends on the level of the caster.
/// The player is immune to its effect.
/// </summary>
public class CardFirewall : Card {

	[SerializeField] float  baseDuration = 5f;
	[SerializeField] float  durationUpgradePerLevel = 1f;
	[SerializeField]  string prefabName;
	Vector3 offset = new Vector3( 0, 0, 10f );

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
		Vector3 firewallPosition = playerTransform.TransformPoint( offset );
		Quaternion firewallRotation = playerTransform.rotation;

		object[] data = new object[2];

		//We want the caster to be immune to the firewall
		data[0] = playerTransform.name;

		//We want the firewall to disappear after a while
		data[1] = baseDuration + level * durationUpgradePerLevel;

		PhotonNetwork.InstantiateSceneObject( prefabName, firewallPosition, firewallRotation, 0, data );
	}
	#endregion

	bool isAllowed( int photonViewID )
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

		//Verify that the player is not spawning a firewall inside the finish line trigger
		Vector3 firewallPosition = playerGameObject.transform.TransformPoint( offset );

		GameObject boxColliderObject = GameObject.FindGameObjectWithTag("Finish Line");
		//If the tile with the finish line is not active, boxColliderObject will be null, so check for that.
		if( boxColliderObject != null )
		{
			BoxCollider boxCollider = boxColliderObject.GetComponent<BoxCollider>();
			if( boxCollider.bounds.Contains( firewallPosition ) )
			{
				//Don't allow it
				return false;
			}
			else
			{
				return true;
			}
		}
		else
		{
			//If boxColliderObject is null, that means the tile with the finish line is not yet active and therefore, we are far from the finish line.
			//In this case, return true.
			return true;
		}
	}

	public bool willSpellBeEffective( Transform caster, int level )
	{
		//Is it likely that at least one target will encounter the firewall before it expires?
		//The result is not exact, but a best guess.
		bool result = false;
		if( isAllowed( caster.GetComponent<PhotonView>().viewID ) )
		{
			float spellDuration = baseDuration + level * durationUpgradePerLevel;
			//Not Implemented completily
			return true;
		}
		return result;
	}
}
