using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Steal card is a Rare card with 11 levels. You steal a random card from the turn-ribbon of a random opponent. The Steal card in your turn-ribbon becomes the card you have just stolen.
/// The stolen card replaces the Steal card until the end of the race.
/// The Steal card level is NOT used for the time being.
/// </summary>
public class CardSteal : Card {

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardStealMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardStealMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Find a random target excluding yourself
		PlayerRace target = null;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].name != playerTransform.name )
			{
				target = PlayerRace.players[i];
				break;
			}
		}
	
		//Test that we have a target. Other players may have abandoned or have lost connection.
		if( target != null )
		{	
			//Send an RPC to the player we will be stealing a card from
			target.GetComponent<PhotonView>().RPC("cardStealTargetRPC", PhotonTargets.All, photonViewID, level );
		}
	}

	#endregion
}