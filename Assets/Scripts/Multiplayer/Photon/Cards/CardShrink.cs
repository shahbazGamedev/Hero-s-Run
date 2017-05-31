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
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		Transform nearestTarget = detectNearestTarget( playerTransform.GetComponent<PlayerRace>(), cd.getCardPropertyValue( CardPropertyType.RANGE, level ) );

		if( nearestTarget != null )
		{
			if( nearestTarget.GetComponent<PlayerSpell>().isReflectEnabled() )
			{
				//The target has the Reflect spell active.
				//Reflect to caster
				nearestTarget = playerTransform;
			
			}
			nearestTarget.GetComponent<PhotonView>().RPC("shrinkSpellRPC", PhotonTargets.All, cd.getCardPropertyValue( CardPropertyType.DURATION, level ) );
		}
		else
		{
			Debug.Log("CardShrink: no target found." );
		}
	}
	#endregion

}
