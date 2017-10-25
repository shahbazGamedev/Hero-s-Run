using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

/// <summary>
//// The Speed Boost card is a Rare card with 11 levels.
/// </summary>
public class CardSpeedBoost : Card {

	[SerializeField] AudioClip  soundFx;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardSpeedBoostMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardSpeedBoostMasterRPC( int level, int photonViewID )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		this.photonView.RPC("cardSpeedBoostRPC", PhotonTargets.AllViaServer, cd.getCardPropertyValue( CardPropertyType.DURATION, level ), cd.getCardPropertyValue( CardPropertyType.SPEED_MULTIPLIER, level ), photonViewID );	
	}
	#endregion

	[PunRPC]
	void cardSpeedBoostRPC( float spellDuration, float speedMultiplier, int photonViewID )
	{
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				startSpeedBoost( spellDuration, speedMultiplier, PlayerRace.players[i].GetComponent<PlayerRun>(), PlayerRace.players[i].GetComponent<PhotonView>().isMine && PlayerRace.players[i].GetComponent<PlayerAI>() == null );
				break;
			}
		}
	}

	void startSpeedBoost( float spellDuration, float speedMultiplier, PlayerRun playerRun, bool isMine )
	{
		//Only affect the camera for the local player
		if( isMine ) Camera.main.GetComponent<MotionBlur>().enabled = true;
		StartCoroutine( playerRun.addVariableSpeedMultiplier( SpeedMultiplierType.Raging_Bull, speedMultiplier, 0.5f ) );
		playerRun.GetComponent<PlayerSounds>().playSound( soundFx, false );
		StartCoroutine( stopSpeedBoost( spellDuration, playerRun, isMine) );
		playerRun.GetComponent<PlayerSpell>().isRagingBullActive = true;
	}

	IEnumerator stopSpeedBoost( float spellDuration, PlayerRun playerRun, bool isMine )
	{
		yield return new WaitForSeconds( spellDuration );
		if( isMine ) Camera.main.GetComponent<MotionBlur>().enabled = false;
		playerRun.GetComponent<PlayerSounds>().stopAudioSource();
		StartCoroutine( playerRun.removeVariableSpeedMultiplier( SpeedMultiplierType.Raging_Bull, 0.5f ) );
		playerRun.GetComponent<PlayerSpell>().cardDurationExpired( CardName.Raging_Bull );
		playerRun.GetComponent<PlayerSpell>().isRagingBullActive = false;
	}

}
