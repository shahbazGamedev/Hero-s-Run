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

		//Find the nearest target
		Transform nearestTarget = detectNearestTarget( playerTransform.GetComponent<PlayerRace>(), cd.getCardPropertyValue( CardPropertyType.RANGE, level ));

		//Only continue if we found a target
		if( nearestTarget != null )
		{
			if( nearestTarget.GetComponent<PlayerSpell>().isReflectEnabled() )
			{
				MiniMap.Instance.reflectMessage( photonViewID, (int)cardName, nearestTarget.GetComponent<PhotonView>().viewID );

				//The target has the Reflect spell active.
				//Reflect to caster
				nearestTarget = playerTransform;
			
			}
			object[] data = new object[2];
	
			//We will need to find a reference to the player we are targeting
			data[0] = nearestTarget.GetComponent<PhotonView>().viewID;
	
			//We want the stasis sphere to disappear after a while
			data[1] = cd.getCardPropertyValue( CardPropertyType.DURATION, level );
	
			PhotonNetwork.InstantiateSceneObject( prefabName, nearestTarget.position, nearestTarget.rotation, 0, data );
		}
		else
		{
			Debug.Log("CardStasis - No target found.");
		}
	}
	#endregion

}
