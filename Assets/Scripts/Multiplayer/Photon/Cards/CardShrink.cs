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

	const float CREATURE_SHRINK_RADIUS = 35f; //in meters

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardShrinkMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardShrinkMasterRPC( int level, int photonViewID )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );

		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );
		bool atLeastOneTarget = false;

		if( GameManager.Instance.isCoopPlayMode() )
		{
			List<ICreature> creatures = getAllCreaturesWithinRange( playerTransform, CREATURE_SHRINK_RADIUS );
			for( int i = 0; i < creatures.Count; i++ )
			{
				creatures[i].shrink( playerTransform, true );
			}
			if( creatures.Count > 0 ) atLeastOneTarget = true;

		}
		else
		{
			//Send the RPC to everyone excluding the caster
			//Shrink affects all opponents
			for( int i = 0; i < PlayerRace.players.Count; i++ )
			{
				if( PlayerRace.players[i].name != playerTransform.name )
				{
					//You can't shrink a player who is ziplining because it causes too many bugs
					if( !isPlayerImmune( PlayerRace.players[i].transform ) && PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Ziplining )
					{
						if( PlayerRace.players[i].GetComponent<PlayerSpell>().isCardActive( CardName.Reflect) )
						{
							MiniMap.Instance.reflectMessage( photonViewID, (int)cardName, PlayerRace.players[i].GetComponent<PhotonView>().viewID );
		
							//The target has the Reflect spell active.
							//Reflect to caster
							playerTransform.GetComponent<PhotonView>().RPC("shrinkSpellRPC", PhotonTargets.AllViaServer, cd.getCardPropertyValue( CardPropertyType.DURATION, level ) );
						
						}
						else
						{
							PlayerRace.players[i].GetComponent<PhotonView>().RPC("shrinkSpellRPC", PhotonTargets.AllViaServer, cd.getCardPropertyValue( CardPropertyType.DURATION, level ) );
							atLeastOneTarget = true;
						}
					}
				}
			}
			//1) We do have at least one target.
			//2) Play an appropriate VO such as "Wicked!" for Shrink.
			if( atLeastOneTarget ) playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );
		}
	}
	#endregion

}
