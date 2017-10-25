using System.Collections;
using UnityEngine;

//Heal also has the benefit of restoring your normal size if you were shrunk if the card is of sufficient level.
public class CardHeal : Card {

	const int MINIMUM_LEVEL_TO_UNSHRINK_PLAYER = 4;

	public void activateCard ( int photonViewId, int level )
	{
		photonView.RPC("cardHealMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardHealMasterRPC( int level, int photonViewID )
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

		//Don't send an RPC if the newHealth is equal to the currentHealth.
		if( newHealth != currentHealth ) playerTransform.GetComponent<PhotonView>().RPC("changeHealthRPC", PhotonTargets.AllViaServer, newHealth );

		//Heal also has the benefit of restoring your normal size if you were shrunk if the card level is MINIMUM_LEVEL_TO_UNSHRINK_PLAYER and up.
		if( level >= MINIMUM_LEVEL_TO_UNSHRINK_PLAYER && playerTransform.localScale.y < 1f ) playerTransform.GetComponent<PhotonView>().RPC("unshrinkRPC", PhotonTargets.AllViaServer );

	}
	#endregion
}
