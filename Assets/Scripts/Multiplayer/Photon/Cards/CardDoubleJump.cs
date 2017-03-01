using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Double Jump card is a Common card with 13 levels. The player will jump higher and further than normal.
/// </summary>
public class CardDoubleJump : Photon.PunBehaviour {

	[SerializeField] float  baseDoubleJumpSpeed = 12f; //for comparaison, the normal jump value is 8.8
	[SerializeField] float  doubleJumpUpgradePerLevel = 0.5f;

	public void activateCard ( string name, int level )
	{
		GameObject playerGameObject = (GameObject)PhotonNetwork.player.TagObject;
		this.photonView.RPC("cardDoubleJumpRPC", PhotonTargets.AllViaServer, level, playerGameObject.GetComponent<PhotonView>().viewID );	
	}

	[PunRPC]
	void cardDoubleJumpRPC( int level, int photonViewID )
	{
		float doubleJumpSpeed = baseDoubleJumpSpeed + level * doubleJumpUpgradePerLevel;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				PlayerRace.players[i].GetComponent<PlayerControl>().doubleJump( doubleJumpSpeed );
			}
		}
	}

}
