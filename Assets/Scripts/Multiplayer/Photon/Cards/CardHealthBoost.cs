using System.Collections;
using UnityEngine;

public class CardHealthBoost : Card {

	public void activateCard ( int photonViewId, int level )
	{
		photonView.RPC("cardHealthBoostMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardHealthBoostMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Determine health boost
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		int healthBoost = (int) cd.getCardPropertyValue( CardPropertyType.HEALTH, level );
		
		//Get the current health
		int currentHealth = playerTransform.GetComponent<PlayerHealth>().getHealth();

		//Calculate the new health value, but do not exceed the maximum.
		int newHealth = Mathf.Min( currentHealth + healthBoost, PlayerHealth.DEFAULT_HEALTH );
		playerTransform.GetComponent<PhotonView>().RPC("changeHealthRPC", PhotonTargets.AllViaServer, newHealth );

	}
	#endregion
}
