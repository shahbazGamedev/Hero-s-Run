using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CardCloak : Card {

	[SerializeField] AudioClip  soundFx;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardCloakMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardCloakMasterRPC( int level, int photonViewID )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		this.photonView.RPC("cardCloakRPC", PhotonTargets.AllViaServer, cd.getCardPropertyValue( CardPropertyType.DURATION_WITH_TIMER, level ), cd.getCardPropertyValue( CardPropertyType.SPEED_MULTIPLIER, level ), photonViewID );	
	}
	#endregion

	[PunRPC]
	void cardCloakRPC( float spellDuration, float speedMultiplier, int photonViewID )
	{
		Transform caster = getPlayerTransform( photonViewID );
		if( caster != null ) startCloaking( spellDuration, speedMultiplier, caster.GetComponent<PlayerRun>(), caster.GetComponent<PhotonView>().isMine && caster.GetComponent<PlayerAI>() == null );
	}

	void startCloaking( float spellDuration, float speedMultiplier, PlayerRun playerRun, bool isMine )
	{
		playerRun.GetComponent<PlayerSounds>().playSound( soundFx, false );
		StartCoroutine( playerRun.addVariableSpeedMultiplier( SpeedMultiplierType.Cloak, speedMultiplier, 0.8f ) );
		StartCoroutine( stopCloaking( spellDuration, playerRun, isMine ) );
		//Only affect the camera for the local player
		if( isMine ) StartCoroutine( controlSaturation( 0f, 0.8f ) );
		MiniMap.Instance.changeAlphaOfRadarObject( playerRun.GetComponent<PlayerControl>(), 0 );
	}

	IEnumerator stopCloaking( float spellDuration, PlayerRun playerRun, bool isMine  )
	{
		yield return new WaitForSeconds( spellDuration );
		playerRun.GetComponent<PlayerSounds>().stopAudioSource();
		StartCoroutine( playerRun.removeVariableSpeedMultiplier( SpeedMultiplierType.Cloak, 0.8f ) );
		//Only affect the camera for the local player
		if( isMine ) StartCoroutine( controlSaturation( 1f, 0.8f ) );
		playerRun.GetComponent<PlayerSpell>().cardDurationExpired( CardName.Cloak);
		MiniMap.Instance.changeAlphaOfRadarObject( playerRun.GetComponent<PlayerControl>(), 1f );
	}

	IEnumerator controlSaturation( float endSaturationLevel, float duration )
	{
		ColorCorrectionCurves ccc = Camera.main.GetComponent<ColorCorrectionCurves>();
 		ccc.enabled = true;

		float elapsedTime = 0;
		
		float startSaturationLevel = ccc.saturation;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			ccc.saturation = Mathf.Lerp( startSaturationLevel, endSaturationLevel, elapsedTime/duration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		
		ccc.saturation = endSaturationLevel;
		if( ccc.saturation == 1f ) ccc.enabled = false;
	}
	
}
