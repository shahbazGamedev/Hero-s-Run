using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Lightning card is an Epic card with 8 levels.
/// Lightning strikes the nearest player, killing it instantly.
/// Because of its long range, it can be useful when an opponent is far away.
/// The spell range depends on the level of the caster.
/// </summary>
public class CardLightning : Card {

	[SerializeField]  string lightningPrefabName = "Lightning";
	Vector3 offset = new Vector3( 0, 1f, 1f );

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

		//Get nearest target which could be either a player or creature
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		Transform nearestTarget = detectNearestTarget( playerTransform.GetComponent<PlayerRace>(), cd.getCardPropertyValue( CardPropertyType.RANGE, level ) );

		if( nearestTarget != null )
		{
			if( nearestTarget.GetComponent<PlayerSpell>().isReflectEnabled() )
			{
				MiniMap.Instance.reflectMessage( photonViewID, (int)cardName, nearestTarget.GetComponent<PhotonView>().viewID );

				//The target has the Reflect spell active.
				//Reflect to caster
				nearestTarget = playerTransform;
			
			}

			//1) We do have a target.
			//2) The target is not the caster.
			//3) Play an appropriate VO such as "Gotcha!" for Stasis.
			if( nearestTarget != playerTransform ) playActivateCardVoiceOver( playerTransform.GetComponent<PhotonView>() );

			//Spawn a lightning on the nearest player or creature
			Vector3 lightningPosition = nearestTarget.transform.TransformPoint( offset );
			PhotonNetwork.InstantiateSceneObject( lightningPrefabName, lightningPosition, nearestTarget.rotation, 0, null );
	
			//Kill nearest target
			strike( nearestTarget );

		}
		else
		{
			Debug.Log("CardLightning: no target found." );
		}
	}

	void strike( Transform nearestTarget )
	{
		nearestTarget.GetComponent<PhotonView>().RPC("playerDied", PhotonTargets.All, DeathType.Obstacle );
		MiniMap.Instance.displaySecondaryIcon( nearestTarget.GetComponent<PhotonView>().viewID, (int)CardName.Lightning, 2.5f );
	}

	#endregion

}
