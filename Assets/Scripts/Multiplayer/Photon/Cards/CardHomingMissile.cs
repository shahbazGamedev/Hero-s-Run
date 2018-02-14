using System.Collections;
using UnityEngine;

/// <summary>
/// The Homing Missile card is a Rare card with 9 levels. When cast, the homing missile appears a few meters above the player.
/// It will then fly towards the target and explode on impact.
/// </summary>
public class CardHomingMissile : Card {

	[SerializeField]  string prefabName = "Homing Missile";

	public void activateCard ( int photonViewId, int level )
	{
		if( isAllowed( photonViewId ) ) this.photonView.RPC("cardHomingMissileMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardHomingMissileMasterRPC( int level, int photonViewID )
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
	
				object[] data = new object[2];
		
				//We will need to find a reference to the creature we are targeting
				data[0] = randomTarget.GetComponent<PhotonView>().viewID;
					
				//We will need a reference to caster
				data[1] = photonViewID;

				//Create homing missile
				PhotonNetwork.InstantiateSceneObject( prefabName, playerTransform.TransformPoint( getSpawnOffset() ), playerTransform.rotation, 0, data );
			}
			else
			{
				//Display a Minimap message stating that no target was found in range
				playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
				Debug.Log("CardHomingMissile: No target found.");
			}
		}
		else
		{
			//Find a player to target which is in front of the caster.
			Transform randomTarget = detectBestTarget( playerTransform.GetComponent<PlayerRace>(), cd.getCardPropertyValue( CardPropertyType.RANGE, level ), true );
	
			//Only continue if we found a target
			if( randomTarget != null )
			{
				//1) We do have a target.
				//2) The target is not the caster.
				//3) Play an appropriate VO such as "Gotcha!".
				if( randomTarget != playerTransform ) playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );
		
				object[] data = new object[2];
		
				//We will need to find a reference to the player we are targeting
				data[0] = randomTarget.GetComponent<PhotonView>().viewID;
					
				//We will need a reference to caster
				data[1] = photonViewID;

				//Create homing missile
				PhotonNetwork.InstantiateSceneObject( prefabName, playerTransform.TransformPoint( getSpawnOffset() ), playerTransform.rotation, 0, data );
			}
			else
			{
				//Display a Minimap message stating that no target was found in range
				playerTransform.GetComponent<PhotonView>().RPC("cardNoTargetRPC", PhotonTargets.All );
				Debug.Log("CardHomingMissile: No target found.");
			}
		}

	}
	#endregion
}
