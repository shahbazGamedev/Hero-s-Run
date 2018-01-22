using System.Collections;
using UnityEngine;

/// <summary>
/// The Lightning card is an Epic card with 8 levels.
/// Lightning strikes the nearest leading player within range, causing damage.
/// </summary>
public class CardLightning : Card {

	[SerializeField]  string lightningPrefabName = "Lightning";

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
			//Find a creature to target
			Transform target = getNearestCreatureWithinRange( playerTransform, cd.getCardPropertyValue( CardPropertyType.RANGE, level ) );

			//Only continue if we found a target
			if( target != null )
			{
	
				//We do have a target. Play an appropriate VO such as "Gotcha!".
				playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );
	
				//Spawn a lightning on the creature
				Vector3 lightningPosition = target.transform.TransformPoint( getSpawnOffset() );
				PhotonNetwork.InstantiateSceneObject( lightningPrefabName, lightningPosition, target.rotation, 0, null );
		
				//Kill creature
				ICreature creatureController = target.GetComponent<ICreature>();
				creatureController.knockback( target, true );	
			}
			else
			{
				//Display a Minimap message stating that no target was found in range
				playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
				Debug.Log("CardLightning: no target found." );
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
				MiniMap.Instance.displaySecondaryIcon( target.GetComponent<PhotonView>().viewID, (int)CardName.Lightning, 2.5f );
	
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
