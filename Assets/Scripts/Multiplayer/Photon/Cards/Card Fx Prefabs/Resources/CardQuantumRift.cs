using System.Collections;
using UnityEngine;

/// <summary>
/// The Quantum Rift card is an Epic card with 8 levels.
/// A rift opens in the sky in front of the nearest leading player within range. The rift spews rocks. Each rock causes some damage.
/// </summary>
public class CardQuantumRift : Card {

	[SerializeField]  string quantumRiftPrefabName = "Quantum Rift";

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardQuantumRiftMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardQuantumRiftMasterRPC( int level, int photonViewID )
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
				Debug.Log("CardQuantumRift: creature target found: " + target.name );
	
				//We do have a target. Play an appropriate VO such as "Gotcha!".
				playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );
	
				//Spawn the rift in front of the creature
				Vector3 riftPosition = target.TransformPoint( getSpawnOffset() );

				object[] data = new object[1];
				data[0] = photonViewID;

				PhotonNetwork.InstantiateSceneObject( quantumRiftPrefabName, riftPosition, Quaternion.Euler( 0, playerTransform.eulerAngles.y, 0 ) , 0, data );
		
				MiniMap.Instance.displaySecondaryIcon( target.GetComponent<PhotonView>().viewID, (int)CardName.Lightning, 2.5f );
	
			}
			else
			{
				//Display a Minimap message stating that no target was found in range
				playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
				Debug.Log("CardQuantumRift: no target found." );
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
	
				//Spawn the rift in front of the player
				Vector3 riftPosition = target.TransformPoint( getSpawnOffset() );

				object[] data = new object[3];
				data[0] = photonViewID;
				int damageAmount = (int) cd.getCardPropertyValue( CardPropertyType.DAMAGE, level );
				data[1] = damageAmount;
				//Send the viewID of the target
				data[2] = target.GetComponent<PhotonView>().viewID;
				PhotonNetwork.InstantiateSceneObject( quantumRiftPrefabName, riftPosition, target.rotation, 0, data );
		
				MiniMap.Instance.displaySecondaryIcon( target.GetComponent<PhotonView>().viewID, (int)CardName.Lightning, 2.5f );
	
			}
			else
			{
				//Display a Minimap message stating that no target was found in range
				playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
				Debug.Log("CardQuantumRift: no target found." );
			}
		}
	}


	#endregion

}
