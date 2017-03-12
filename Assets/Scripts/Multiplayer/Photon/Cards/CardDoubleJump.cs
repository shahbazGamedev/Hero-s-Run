using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Double Jump card is a Common card with 13 levels. The player will jump higher and further than normal.
/// </summary>
public class CardDoubleJump : Photon.PunBehaviour {

	[SerializeField] float  baseDoubleJumpSpeed = 11.75f; //for comparaison, the normal jump value is 8.8. Max value before starting to jump too high is about 15.
	[SerializeField] float  doubleJumpUpgradePerLevel = 0.25f;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardDoubleJumpRPC", PhotonTargets.AllViaServer, level, photonViewId );	
	}

	[PunRPC]
	void cardDoubleJumpRPC( int level, int photonViewID )
	{
		float doubleJumpSpeed = baseDoubleJumpSpeed + level * doubleJumpUpgradePerLevel;
		string casterName = string.Empty;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				PlayerRace.players[i].GetComponent<PlayerControl>().doubleJump( doubleJumpSpeed );
				casterName = PlayerRace.players[i].name;
			}
		}
		//Indicate on the minimap which card was played
		MiniMap.Instance.updateCardFeed( casterName, CardName.Double_Jump );
	}

}
