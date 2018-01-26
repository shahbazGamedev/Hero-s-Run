using System.Collections;
using UnityEngine;

/// <summary>
/// The Freeze card is a Rare card with 9 levels.
/// The spell traps the player in ice for the spell duration which depends on the level of the card.
/// if the player is in the air, he will be forced to the ground.
/// The player can break free early by tapping on the ice.
/// If the trapped player has a Sentry, it will be destroyed.
/// The player is immune to other spells while frozen.
/// The spell range and duration depends on the level of the card.
/// </summary>
public class CardFreeze : Card {

	[SerializeField]  string prefabName = "Freeze";

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
	
				//We want the target to be on the ground or else the effect will not look nice.
				//We add 0.1f because we do not want to start the raycast at the target's feet.
				RaycastHit hit;
				Vector3 freezePosition = randomTarget.position;
				if (Physics.Raycast(new Vector3( randomTarget.position.x, randomTarget.position.y + 0.1f, randomTarget.position.z ), Vector3.down, out hit, 20f, ~(1 << MaskHandler.creatureLayer ), QueryTriggerInteraction.Ignore ) )
				{
					freezePosition = new Vector3( randomTarget.position.x, hit.point.y + 0.02f, randomTarget.position.z );
				}
				else
				{
					Debug.LogWarning("CardFreeze-there is no ground below the affected creature: " + randomTarget.name + ". Creature will not be freezed." );
					return;
				}
	
				object[] data = new object[2];
		
				//We will need to find a reference to the creature we are targeting
				data[0] = randomTarget.GetComponent<PhotonView>().viewID;
				
				//We want the freese to disappear after a while
				data[1] = cd.getCardPropertyValue( CardPropertyType.DURATION, level );

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
				RaycastHit hit;
				Vector3 freezePosition = randomTarget.position;
				bool isThereGroundBelowPlayer;
				if (Physics.Raycast(new Vector3( randomTarget.position.x, randomTarget.position.y + 0.1f, randomTarget.position.z ), Vector3.down, out hit, 20f, ~(1 << MaskHandler.playerLayer ), QueryTriggerInteraction.Ignore ) )
				{
					freezePosition = new Vector3( randomTarget.position.x, hit.point.y + 0.02f, randomTarget.position.z );
				}
				else
				{
					Debug.LogWarning("CardFreeze-there is no ground below the affected player: " + randomTarget.name + ". Player will not be freezed." );
					return;
				}
	
				object[] data = new object[5];

				//We will need to find a reference to the player we are targeting
				data[0] = randomTarget.GetComponent<PhotonView>().viewID;

				//We want the freese to disappear after a while
				data[1] = cd.getCardPropertyValue( CardPropertyType.DURATION, level );
		
				//Number of taps required to break free early
				data[2] = (int) cd.getCardPropertyValue( CardPropertyType.TAPS, level );

				//Caster viewID
				data[3] = photonViewID;
				
	
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
