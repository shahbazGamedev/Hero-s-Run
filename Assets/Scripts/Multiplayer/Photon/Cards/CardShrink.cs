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

		//Find a random target
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		Transform randomTarget = detectRandomTarget( playerTransform.GetComponent<PlayerRace>(), cd.getCardPropertyValue( CardPropertyType.RANGE, level ) );

		if( randomTarget != null )
		{
			if( randomTarget.GetComponent<PlayerSpell>().isReflectEnabled() )
			{
				MiniMap.Instance.reflectMessage( photonViewID, (int)cardName, randomTarget.GetComponent<PhotonView>().viewID );

				//The target has the Reflect spell active.
				//Reflect to caster
				randomTarget = playerTransform;
			
			}

			//1) We do have a target.
			//2) The target is not the caster.
			//3) Play an appropriate VO such as "Gotcha!" for Stasis.
			if( randomTarget != playerTransform ) playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );

			randomTarget.GetComponent<PhotonView>().RPC("shrinkSpellRPC", PhotonTargets.AllViaServer, cd.getCardPropertyValue( CardPropertyType.DURATION, level ) );
		}
		else
		{
			//Display a Minimap message stating that no target was found in range
			playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
			Debug.Log("CardShrink: no target found." );
		}
	}
	#endregion

}
