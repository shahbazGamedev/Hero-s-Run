using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Double Jump card is a Common card with 13 levels. The player will jump higher and further than normal.
/// </summary>
public class CardDoubleJump : Card {

	[SerializeField] float  baseDoubleJumpSpeed = 11.75f; //for comparaison, the normal jump value is 8.8. Max value before starting to jump too high is about 15.
	[SerializeField] float  doubleJumpUpgradePerLevel = 0.25f;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardDoubleJumpMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardDoubleJumpMasterRPC( int level, int photonViewID )
	{
		float doubleJumpSpeed = baseDoubleJumpSpeed + level * doubleJumpUpgradePerLevel;
		this.photonView.RPC("cardDoubleJumpRPC", PhotonTargets.All, doubleJumpSpeed, photonViewID );	
	}
	#endregion

	[PunRPC]
	void cardDoubleJumpRPC( float doubleJumpSpeed, int photonViewID )
	{
		getPlayerControl( photonViewID ).doubleJump( doubleJumpSpeed );
	}

}
