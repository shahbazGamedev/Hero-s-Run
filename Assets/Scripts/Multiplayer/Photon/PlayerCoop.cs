using System.Collections;
using UnityEngine;

public class PlayerCoop : MonoBehaviour {

	const int SCORE_PER_WAVE = 100; //coop - score points awarded per wave beaten.
	Coroutine coopResurrectPlayerCoroutine;
	const float COOP_DELAY_BEFORE_RESURRECTING = 0.1f;
	bool wasHighScoreReached = false; //Only announce to the player the first time he reaches a new high score.

	void Awake ()
	{
		enabled = GameManager.Instance.isCoopPlayMode();
	}

	void OnEnable()
	{
		CoopWaveGenerator.coopNewWave += CoopNewWave;
	}
	
	void OnDisable()
	{
		CoopWaveGenerator.coopNewWave -= CoopNewWave;
	}

	void CoopNewWave( int waveNumber )
	{
		//With each new wave, the run speed increases slightly.
		GetComponent<PlayerRun>().calculateOverallSpeedMultiplier();

		//Since a new wave started, if this player is dead, resurrect him.
		if( GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying )
		{
			if( coopResurrectPlayerCoroutine != null ) StopCoroutine( coopResurrectPlayerCoroutine );
			coopResurrectPlayerCoroutine = StartCoroutine( coopResurrectPlayer() );
		}

		//A new wave started. Did we beat our previous high score?
		if( !wasHighScoreReached && GetComponent<PhotonView>().isMine && GetComponent<PlayerAI>() == null ) StartCoroutine( checkForHighScore( waveNumber ) );
	}

	IEnumerator checkForHighScore( int waveNumber )
	{
		//Wait until the Wave x! message disappears.
		yield return new WaitForSeconds( 3f );
		int currentHighScoreWaves = GameManager.Instance.playerStatistics.getStatisticData(StatisticDataType.COOP_HIGH_SCORE_WAVES);
		if( waveNumber > currentHighScoreWaves )
		{
			//Yes, this is a new high score. Congrats!
			wasHighScoreReached = true;
			HUDMultiplayer.hudMultiplayer.activateUserMessage( LocalizationManager.Instance.getText("COOP_HIGH_SCORE_WAVES"), 0, 2.5f, 225f );
		}
	}

	IEnumerator coopResurrectPlayer()
	{
		//Wait a tiny bit after the new wave starts before resurrecting the player.
		yield return new WaitForSeconds( COOP_DELAY_BEFORE_RESURRECTING );

		//Make sure that during that short time, this player didn't get resurrected and is still dead before continuing.
		if( GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying )
		{
			PlayerRace partner = getPartner( GetComponent<PlayerRace>() );
			if( partner != null )
			{
				CoopWaveGenerator.Instance.removeDeadPlayer( transform );
				GetComponent<PlayerControl>().coopResurrectBegin( partner.GetComponent<PlayerControl>().currentTile.name );
				partner.GetComponent<PlayerCamera>().isBeingSpectated = false;
			}
			else
			{
				yield return null;
			}
		}
	}

	public void gameOver()
	{
		Debug.Log( name + " PlayerCoop - game over called. " );
		if( coopResurrectPlayerCoroutine != null ) StopCoroutine( coopResurrectPlayerCoroutine );
		if( GetComponent<PhotonView>().isMine && GetComponent<PlayerAI>() == null )
		{
			GameManager.Instance.playerStatistics.updateCoopHighScoreWaves( CoopWaveGenerator.numberOfWavesTriggered );
			//Grant XP
			PlayerRaceManager.Instance.raceAwardList.Clear();
			PlayerRaceManager.Instance.grantXPAward(XPAwardType.SCORE_BONUS );
			PlayerRaceManager.Instance.grantXPAward(XPAwardType.WAVE_BONUS );
		}
	}

	PlayerRace getPartner( PlayerRace playerRace )
	{
		PlayerRace partner = null;

		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i] != playerRace ) 
			{
				partner = PlayerRace.players[i];
				break;
			}
		}
		if( partner == null ) Debug.LogError("PlayerCoop-could not find partner for " + playerRace.name + ". Player count is: " + PlayerRace.players.Count );

		return partner;
	}

}
