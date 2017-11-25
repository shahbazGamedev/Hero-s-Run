using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Freeze card is a Common card with 13 levels.
/// The spell traps the player in ice for the spell duration, which depends on the level of the card.
/// If the trapped player has a Sentry, it will be destroyed.
/// The player is immune to other spells while in stasis.
/// The spell range and duration depends on the level of the card.
/// </summary>
public class CardFreeze : Card {

	[SerializeField]  string prefabName = "Freeze";
	const float DISTANCE_ABOVE_GROUND = 3.5f;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardFreezeMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardFreezeMasterRPC( int level, int photonViewID )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );

		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		if( GameManager.Instance.isCoopPlayMode() )
		{
			//Find a creature to target
			Transform randomTarget = getNearestCreatureWithinRange( playerTransform, cd.getCardPropertyValue( CardPropertyType.RANGE, level ) );

			//Only continue if we found a target
			if( randomTarget != null )
			{
	
				//We do have a target. Play an appropriate VO such as "Gotcha!".
				playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );
	
				//We want the player to be on the ground or else the effect will not look nice.
				//We add 0.1f because we do not want to start the raycast at the creature's feet.
				//The Freeze prefab has the ignoreRaycast layer.
				randomTarget.gameObject.layer = MaskHandler.ignoreRaycastLayer; //change temporarily to Ignore Raycast
				RaycastHit hit;
				Vector3 freezePosition = randomTarget.position;
				bool isThereGroundBelowPlayer;
				if (Physics.Raycast(new Vector3( randomTarget.position.x, randomTarget.position.y + 0.1f, randomTarget.position.z ), Vector3.down, out hit, 30.0F ))
				{
					freezePosition = new Vector3( randomTarget.position.x, hit.point.y + 0.02f, randomTarget.position.z );
					isThereGroundBelowPlayer = true;
				}
				else
				{
					Debug.LogWarning("CardFreeze-there is no ground below the affected creature: " + randomTarget.name );
					isThereGroundBelowPlayer = false;
				}
	
				randomTarget.gameObject.layer = MaskHandler.creatureLayer; //Restore to Creature
	
				object[] data = new object[2];
		
				//We will need to find a reference to the creature we are targeting
				data[0] = randomTarget.GetComponent<PhotonView>().viewID;
			
				//If there ground below the target. This will be used to decide if we put the ground ice decal or not.
				data[1] = isThereGroundBelowPlayer;
	
				PhotonNetwork.InstantiateSceneObject( prefabName, freezePosition, randomTarget.rotation, 0, data );
			}
			else
			{
				//Display a Minimap message stating that no target was found in range
				playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
				Debug.Log("CardFreeze: No target found.");
			}
		}
		else
		{
			//Find a player target
			Transform randomTarget = detectBestTarget( playerTransform.GetComponent<PlayerRace>(), cd.getCardPropertyValue( CardPropertyType.RANGE, level ));
	
			//Only continue if we found a target
			if( randomTarget != null )
			{
				if( randomTarget.GetComponent<PlayerSpell>().isCardActive( CardName.Reflect) )
				{
					MiniMap.Instance.reflectMessage( photonViewID, (int)cardName, randomTarget.GetComponent<PhotonView>().viewID );
	
					//The target has the Reflect spell active.
					//Reflect to caster
					randomTarget = playerTransform;
				
				}
	
				//1) We do have a target.
				//2) The target is not the caster.
				//3) Play an appropriate VO such as "Gotcha!".
				if( randomTarget != playerTransform ) playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );
	
				//We want the player to be on the ground or else the effect will not look nice.
				//We add 0.1f because we do not want to start the raycast at the player's feet.
				//The Freeze prefab has the ignoreRaycast layer.
				randomTarget.gameObject.layer = MaskHandler.ignoreRaycastLayer; //change temporarily to Ignore Raycast
				RaycastHit hit;
				Vector3 freezePosition = randomTarget.position;
				bool isThereGroundBelowPlayer;
				if (Physics.Raycast(new Vector3( randomTarget.position.x, randomTarget.position.y + 0.1f, randomTarget.position.z ), Vector3.down, out hit, 30.0F ))
				{
					freezePosition = new Vector3( randomTarget.position.x, hit.point.y + 0.02f, randomTarget.position.z );
					isThereGroundBelowPlayer = true;
				}
				else
				{
					Debug.LogWarning("CardFreeze-there is no ground below the affected player: " + randomTarget.name );
					isThereGroundBelowPlayer = false;
				}
	
				randomTarget.gameObject.layer = MaskHandler.playerLayer; //Restore to Player
	
				object[] data = new object[2];
		
				//We will need to find a reference to the player we are targeting
				data[0] = randomTarget.GetComponent<PhotonView>().viewID;
		
				//If there ground below the target. This will be used to decide if we put the ground ice decal or not.
				data[1] = isThereGroundBelowPlayer;
	
				PhotonNetwork.InstantiateSceneObject( prefabName, freezePosition, randomTarget.rotation, 0, data );
			}
			else
			{
				//Display a Minimap message stating that no target was found in range
				playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
				Debug.Log("CardFreeze: No target found.");
			}
		}
	}
	#endregion

}
