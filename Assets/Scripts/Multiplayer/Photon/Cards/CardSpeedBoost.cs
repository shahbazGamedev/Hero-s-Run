using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

/// <summary>
//// The Speed Boost card is a Rare card with 11 levels.
/// </summary>
public class CardSpeedBoost : Card {

	[SerializeField] AudioClip  soundFx;
	[SerializeField] float  baseDuration = 1.5f;
	[SerializeField] float  baseSpeed = 1.5f;
	[SerializeField] float  durationUpgradePerLevel = 0.15f;
	[SerializeField] float  speedUpgradePerLevel = 0.06f;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardSpeedBoostMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardSpeedBoostMasterRPC( int level, int photonViewID )
	{
		float spellDuration = baseDuration + level * durationUpgradePerLevel;
		float speedMultiplier = baseSpeed + level * speedUpgradePerLevel;
		this.photonView.RPC("cardSpeedBoostRPC", PhotonTargets.All, spellDuration, speedMultiplier, photonViewID );	
	}
	#endregion

	[PunRPC]
	void cardSpeedBoostRPC( float spellDuration, float speedMultiplier, int photonViewID )
	{
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				startSpeedBoost( spellDuration, speedMultiplier, PlayerRace.players[i].GetComponent<PlayerControl>(), PlayerRace.players[i].GetComponent<PhotonView>().isMine && PlayerRace.players[i].GetComponent<PlayerAI>() == null );
			}
		}
	}

	void startSpeedBoost( float spellDuration, float speedMultiplier, PlayerControl playerControl, bool isMine )
	{
		//Only affect the camera for the local player
		if( isMine ) Camera.main.GetComponent<MotionBlur>().enabled = true;
		playerControl.setAllowRunSpeedToIncrease( false );
		playerControl.isSpeedBoostActive = true;
		playerControl.runSpeed = playerControl.runSpeed * speedMultiplier;
		playerControl.GetComponent<PlayerSounds>().playSound( soundFx, false );
		StartCoroutine( changeSprintBlendFactor( 1f, 0.7f, playerControl ) );
		StartCoroutine( stopSpeedBoost( spellDuration, playerControl, isMine) );
	}

	IEnumerator stopSpeedBoost( float spellDuration, PlayerControl playerControl, bool isMine )
	{
		yield return new WaitForSeconds( spellDuration );
		if( isMine ) Camera.main.GetComponent<MotionBlur>().enabled = false;
		if( playerControl.getCharacterState() != PlayerCharacterState.Dying ) playerControl.setAllowRunSpeedToIncrease( true );
		playerControl.GetComponent<PlayerSounds>().stopAudioSource();
		playerControl.isSpeedBoostActive = false;
		StartCoroutine( changeSprintBlendFactor( 0, 0.7f, playerControl ) );
	}

}
