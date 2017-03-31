using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Linked Fate card is an Epic card with 8 levels.
/// The spell links the fate of all of the players.
/// It has an infinite range.
/// The spell duration depends on the level of the caster.
/// Two-player scenario:
/// p1 is the caster.
/// If p1 dies, p2 dies
/// If p2 dies, p1 is not affected.
/// Three-player scenario:
/// p1 is the caster.
/// If p1 dies, p2 and p3 die
/// If p2 dies, p3 also dies.
/// If p3 dies, p2 also dies.
/// </summary>
public class CardLinkedFate : Card {

	[SerializeField] float  baseDuration = 10f;
	[SerializeField] float  durationUpgradePerLevel = 5f;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardLinkedFateMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardLinkedFateMasterRPC( int level, int photonViewID )
	{
		float spellDuration = baseDuration + level * durationUpgradePerLevel;

		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Send the RPC to everyone including the caster
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			PlayerRace.players[i].GetComponent<PhotonView>().RPC("cardLinkedFateRPC", PhotonTargets.AllViaServer, playerTransform.name, spellDuration );
		}
	}
	#endregion
}
