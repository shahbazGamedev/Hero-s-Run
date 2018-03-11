using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Shrink card is a Rare card with 9 levels.
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
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );

		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		if( GameManager.Instance.isCoopPlayMode() )
		{
			List<ICreature> creatureList = getAllCreaturesWithinRange( playerTransform, cd.getCardPropertyValue( CardPropertyType.RADIUS, level ) );

			//Only continue if we found at least one target.
			if( creatureList.Count > 0 )
			{
				//We have at least one target. Play an appropriate VO such as "Gotcha!".
				playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );

				int numberOfTargets = (int) Mathf.Min( cd.getCardPropertyValue( CardPropertyType.MAX_TARGETS, level ), creatureList.Count );
				for( int i = 0; i < numberOfTargets; i++ )
				{
					creatureList[i].shrink( playerTransform, true );
				}
				SkillBonusHandler.Instance.GetComponent<PhotonView>().RPC("grantComboScoreBonusRPC", PhotonTargets.All, ZombieController.SCORE_PER_KNOCKBACK, "COOP_SCORE_BONUS_COMBO_SHRINK_ZOMBIE", photonViewID, numberOfTargets );
			}
			else
			{
				//Display a Minimap message stating that no target was found in range
				playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
				Debug.Log("CardShrink: no target(s) found." );
			}
		}
		else
		{
			//Send the RPC to everyone excluding the caster
			//Shrink affects all opponents
			bool atLeastOneTarget = false;
			for( int i = 0; i < PlayerRace.players.Count; i++ )
			{
				if( PlayerRace.players[i].name != playerTransform.name )
				{
					if( TargetManager.Instance.isPlayerValidTarget( PlayerRace.players[i].transform, true, true, false ) )
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
