using System.Collections;
using System.Collections.Generic;
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
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Find a target
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		Transform bestTarget = detectBestTarget( playerTransform.GetComponent<PlayerRace>(), cd.getCardPropertyValue( CardPropertyType.RANGE, level ) );

		if( bestTarget != null )
		{
			if( bestTarget.GetComponent<PlayerSpell>().isCardActive( CardName.Reflect) )
			{
				MiniMap.Instance.reflectMessage( photonViewID, (int)cardName, bestTarget.GetComponent<PhotonView>().viewID );

				//The target has the Reflect spell active.
				//Reflect to caster
				bestTarget = playerTransform;
			
			}

			//1) We do have a target.
			//2) The target is not the caster.
			//3) Play an appropriate VO such as "Gotcha!" for Stasis.
			if( bestTarget != playerTransform ) playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );

			//Spawn a lightning on the nearest player or creature
			Vector3 lightningPosition = bestTarget.transform.TransformPoint( spawnOffset );
			PhotonNetwork.InstantiateSceneObject( lightningPrefabName, lightningPosition, bestTarget.rotation, 0, null );
	
			//Damage nearest target
			int damageAmount = (int) cd.getCardPropertyValue( CardPropertyType.DAMAGE, level );
			bestTarget.GetComponent<PlayerHealth>().deductHealth( damageAmount );
			MiniMap.Instance.displaySecondaryIcon( bestTarget.GetComponent<PhotonView>().viewID, (int)CardName.Lightning, 2.5f );

		}
		else
		{
			//Display a Minimap message stating that no target was found in range
			playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
			Debug.Log("CardLightning: no target found." );
		}
	}


	#endregion

}
