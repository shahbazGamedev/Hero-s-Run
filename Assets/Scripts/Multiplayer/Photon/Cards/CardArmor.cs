using System.Collections;
using UnityEngine;

public class CardArmor : Card {

	public void activateCard ( int photonViewId, int level )
	{
		photonView.RPC("cardArmorMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardArmorMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Determine the armor amount
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		int armor = (int) cd.getCardPropertyValue( CardPropertyType.ARMOR, level );
		
		//Get the current armor
		int currentArmor = playerTransform.GetComponent<PlayerHealth>().getArmor();

		//Calculate the new armor value, but do not exceed the maximum.
		int newArmor = Mathf.Min( currentArmor + armor, PlayerHealth.MAXIMUM_ARMOR );

		//Don't send an RPC if the newArmor is equal to the currentArmor.
		if( newArmor != currentArmor ) playerTransform.GetComponent<PhotonView>().RPC("addArmorRPC", PhotonTargets.AllViaServer, newArmor );

	}
	#endregion
}
