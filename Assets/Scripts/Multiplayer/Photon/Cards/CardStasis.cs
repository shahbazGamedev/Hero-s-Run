using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Stasis card is a Common card with 13 levels.
/// The spell traps the player in a stasis force field for the spell duration, which depends on the level of the card.
/// If the trapped player has a Sentry, it will be destroyed.
/// The player is immune to other spells while in stasis.
/// The spell range depends on the level of the card.
/// </summary>
public class CardStasis : Card {

	[SerializeField]  string prefabName = "Statis";
	const float DISTANCE_ABOVE_GROUND = 3.5f;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardStasisMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardStasisMasterRPC( int level, int photonViewID )
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
	
				//We want the statis sphere to float DISTANCE_ABOVE_GROUND above ground.
				//We add 0.1f because we do not want to start the raycast at the player's feet.
				//The Stasis prefab has the ignoreRaycast layer.
				randomTarget.gameObject.layer = MaskHandler.ignoreRaycastLayer; //change temporarily to Ignore Raycast
				RaycastHit hit;
				Vector3 stasisPosition = randomTarget.position;
				if (Physics.Raycast(new Vector3( randomTarget.position.x, randomTarget.position.y + 0.1f, randomTarget.position.z ), Vector3.down, out hit, 30.0F ))
				{
					stasisPosition = new Vector3( randomTarget.position.x, hit.point.y + DISTANCE_ABOVE_GROUND, randomTarget.position.z );
				}
				else
				{
					Debug.LogWarning("CardStasis-there is no ground below the affected creature: " + randomTarget.name );
				}
				randomTarget.gameObject.layer = MaskHandler.creatureLayer; //Restore
	
				object[] data = new object[2];
		
				//We will need to find a reference to the creature we are targeting
				data[0] = randomTarget.GetComponent<PhotonView>().viewID;
		
				//We want the stasis sphere to disappear after a while
				data[1] = cd.getCardPropertyValue( CardPropertyType.DURATION, level );
			
				PhotonNetwork.InstantiateSceneObject( prefabName, stasisPosition, randomTarget.rotation, 0, data );
			}
			else
			{
				//Display a Minimap message stating that no target was found in range
				playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
				Debug.Log("CardStasis: No target found.");
			}
		}
		else
		{
			//Find a player to target
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
				//3) Play an appropriate VO such as "Gotcha!" for Stasis.
				if( randomTarget != playerTransform ) playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );
	
				//We want the statis sphere to float DISTANCE_ABOVE_GROUND above ground.
				//We add 0.1f because we do not want to start the raycast at the player's feet.
				//The Stasis prefab has the ignoreRaycast layer.
				randomTarget.gameObject.layer = MaskHandler.ignoreRaycastLayer; //change temporarily to Ignore Raycast
				RaycastHit hit;
				Vector3 stasisPosition = randomTarget.position;
				if (Physics.Raycast(new Vector3( randomTarget.position.x, randomTarget.position.y + 0.1f, randomTarget.position.z ), Vector3.down, out hit, 30.0F ))
				{
					stasisPosition = new Vector3( randomTarget.position.x, hit.point.y + DISTANCE_ABOVE_GROUND, randomTarget.position.z );
				}
				else
				{
					Debug.LogWarning("CardStasis-there is no ground below the affected player: " + randomTarget.name );
				}
				randomTarget.gameObject.layer = MaskHandler.playerLayer; //Restore to Player
	
				object[] data = new object[3];
		
				//We will need to find a reference to the player we are targeting
				data[0] = randomTarget.GetComponent<PhotonView>().viewID;
		
				//We want the stasis sphere to disappear after a while
				data[1] = cd.getCardPropertyValue( CardPropertyType.DURATION, level );
		
				//Number of taps required to break free early
				data[2] = (int) cd.getCardPropertyValue( CardPropertyType.TAPS, level );
	
				PhotonNetwork.InstantiateSceneObject( prefabName, stasisPosition, randomTarget.rotation, 0, data );
			}
			else
			{
				//Display a Minimap message stating that no target was found in range
				playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
				Debug.Log("CardStasis: No target found.");
			}
		}
	}
	#endregion

}
