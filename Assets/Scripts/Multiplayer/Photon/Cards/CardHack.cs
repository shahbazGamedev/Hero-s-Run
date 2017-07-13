using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Hack card is a Legendary card with 5 levels. Hack into your ALL of your opponents omni-tools and disable them for a short while. The effect remains active until it expires or the player respawns.
/// </summary>
public class CardHack : Card {

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardHackMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardHackMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );

		//Send the RPC to everyone excluding the caster
		//Hack affects all opponents
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].name != playerTransform.name )
			{
				if( !isPlayerImmune( PlayerRace.players[i].transform  ) )
				{
					if( PlayerRace.players[i].GetComponent<PlayerSpell>().isReflectEnabled() )
					{
						MiniMap.Instance.reflectMessage( photonViewID, (int)cardName, PlayerRace.players[i].GetComponent<PhotonView>().viewID );
	
						//The target has the Reflect spell active.
						//Reflect to caster
						playerTransform.GetComponent<PhotonView>().RPC("cardHackRPC", PhotonTargets.AllViaServer, cd.getCardPropertyValue( CardPropertyType.DURATION, level ) );
					
					}
					else
					{
						PlayerRace.players[i].GetComponent<PhotonView>().RPC("cardHackRPC", PhotonTargets.AllViaServer, cd.getCardPropertyValue( CardPropertyType.DURATION, level ) );
					}
				}
			}
		}
	}
	#endregion
}
