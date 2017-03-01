using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

/// <summary>
//// The Speed Boost card is a Common card with 13 levels.
/// </summary>
public class CardSpeedBoost : Photon.PunBehaviour {

	[SerializeField] AudioClip  soundFx;
	[SerializeField] float  baseDuration = 1.5f;
	[SerializeField] float  baseSpeed = 1.5f;
	[SerializeField] float  durationUpgradePerLevel = 0.15f;
	[SerializeField] float  speedUpgradePerLevel = 0.06f;

	public void activateCard ( string name, int level )
	{
		GameObject playerGameObject = (GameObject)PhotonNetwork.player.TagObject;
		this.photonView.RPC("cardSpeedBoostRPC", PhotonTargets.AllViaServer, level, playerGameObject.GetComponent<PhotonView>().viewID );	
	}

	[PunRPC]
	void cardSpeedBoostRPC( int level, int photonViewID )
	{
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				startSpeedBoost( level, PlayerRace.players[i].GetComponent<PlayerControl>(), PlayerRace.players[i].GetComponent<PhotonView>().isMine );
			}
		}
	}

	void startSpeedBoost( int level, PlayerControl playerControl, bool isMine )
	{
		//Only affect the camera for the local player
		if( isMine ) Camera.main.GetComponent<MotionBlur>().enabled = true;
		playerControl.setAllowRunSpeedToIncrease( false );
		playerControl.runSpeed = playerControl.runSpeed * ( baseSpeed + level * speedUpgradePerLevel );
		playerControl.GetComponent<PlayerSounds>().playSound( soundFx, false );
		StartCoroutine( stopSpeedBoost( level, playerControl, isMine) );
	}

	IEnumerator stopSpeedBoost( int level, PlayerControl playerControl, bool isMine )
	{
		yield return new WaitForSeconds( baseDuration + level * durationUpgradePerLevel );
		if( isMine ) Camera.main.GetComponent<MotionBlur>().enabled = false;
		if( playerControl.getCharacterState() != PlayerCharacterState.Dying ) playerControl.setAllowRunSpeedToIncrease( true );
		playerControl.GetComponent<PlayerSounds>().stopAudioSource();
	}

}
