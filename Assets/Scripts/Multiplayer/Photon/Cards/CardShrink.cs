using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Shrink card is a Rare card with 11 levels.
/// The spell shrinks and slows the nearest player.
/// Because of its long range, it can be useful when an opponent is far away.
/// The spell range and duration depend on the level of the caster.
/// </summary>
public class CardShrink : Card {

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardShrinkMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardShrinkMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Find the nearest target
		Transform nearestTarget = detectNearestTarget( playerTransform.GetComponent<PlayerRace>(), getRange( level ) );

		if( nearestTarget != null )
		{
			nearestTarget.GetComponent<PhotonView>().RPC("shrinkSpellRPC", PhotonTargets.All, getDuration( level ) );
		}
		else
		{
			Debug.Log("CardShrink: no target found." );
		}
	}
	#endregion

}
