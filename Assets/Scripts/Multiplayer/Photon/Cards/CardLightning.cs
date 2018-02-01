﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Lightning card is an Epic card with 8 levels.
/// Lightning strikes the nearest leading player within range, causing damage.
/// </summary>
public class CardLightning : Card {

	[SerializeField]  string lightningPrefabName = "Lightning";
	[SerializeField]  string coopLightningPrefabName = "Lightning Coop";
	const float COOP_MIN_SPAWN_DELAY = 0.1f;
	public const float COOP_MAX_SPAWN_DELAY = 0.3f;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardLightningMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardLightningMasterRPC( int level, int photonViewID )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );

		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		if( GameManager.Instance.isCoopPlayMode() )
		{
			object[] data = new object[1];

			//Caster PhotonView ID.
			data[0] = photonViewID;

			//Find one or more creatures to target that are within range.
			List<Transform> creatureList = getAllCreatureTransformsWithinRange( playerTransform, cd.getCardPropertyValue( CardPropertyType.RADIUS, level ) );
 
			//Only continue if we found at least one target.
			if( creatureList.Count > 0 )
			{
				//We have at least one target. Play an appropriate VO such as "Gotcha!".
				playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );

				GameObject lightningSystem = null;

				int numberOfTargets = (int) Mathf.Min( cd.getCardPropertyValue( CardPropertyType.MAX_TARGETS, level ), creatureList.Count );
				for( int i = 0; i < numberOfTargets; i++ )
				{
					//Spawn a lightning system over the first creature.
					//We only need one lightning system even though we might be striking several creatures.
					if( i == 0 )
					{
						Vector3 lightningPosition = creatureList[i].TransformPoint( getSpawnOffset() );
						lightningSystem = PhotonNetwork.InstantiateSceneObject( coopLightningPrefabName, lightningPosition, creatureList[i].rotation, 0, data );
					}
			
					//Zap creature
					ICreature creatureController = creatureList[i].GetComponent<ICreature>();
					float smallDelay = Random.Range( COOP_MIN_SPAWN_DELAY, COOP_MAX_SPAWN_DELAY );
					creatureController.zap( lightningSystem.GetComponent<PhotonView>().viewID, smallDelay );
				}
				SkillBonusHandler.Instance.GetComponent<PhotonView>().RPC("grantComboScoreBonusRPC", PhotonTargets.All, ZombieController.SCORE_PER_KNOCKBACK, "COOP_SCORE_BONUS_COMBO_ZAP_ZOMBIE", photonViewID, numberOfTargets );
			}
			else
			{
				//Display a Minimap message stating that no target was found in range
				playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
				Debug.Log("CardLightning: no target(s) found." );
			}
		}
		else
		{
			//Find a player target
			Transform target = detectBestTarget( playerTransform.GetComponent<PlayerRace>(), cd.getCardPropertyValue( CardPropertyType.RANGE, level ) );
	
			if( target != null )
			{
				if( target.GetComponent<PlayerSpell>().isCardActive( CardName.Reflect) )
				{
					MiniMap.Instance.reflectMessage( photonViewID, (int)cardName, target.GetComponent<PhotonView>().viewID );
	
					//The target has the Reflect spell active.
					//Reflect to caster
					target = playerTransform;
				
				}
	
				//1) We do have a target.
				//2) The target is not the caster.
				//3) Play an appropriate VO such as "Gotcha!" for Stasis.
				if( target != playerTransform ) playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );
	
				//Spawn a lightning on the player
				Vector3 lightningPosition = target.transform.TransformPoint( getSpawnOffset() );
				PhotonNetwork.InstantiateSceneObject( lightningPrefabName, lightningPosition, target.rotation, 0, null );
		
				//Damage nearest target
				int damageAmount = (int) cd.getCardPropertyValue( CardPropertyType.DAMAGE, level );
				target.GetComponent<PlayerHealth>().deductHealth( damageAmount, playerTransform.GetComponent<PlayerControl>() );	
			}
			else
			{
				//Display a Minimap message stating that no target was found in range
				playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
				Debug.Log("CardLightning: no target found." );
			}
		}
	}


	#endregion

}
