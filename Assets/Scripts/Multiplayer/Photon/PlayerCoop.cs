using System.Collections;
using UnityEngine;

public class PlayerCoop : MonoBehaviour {

	const int SCORE_PER_WAVE = 100; //coop - score points awarded per wave beaten.
	Coroutine coopResurrectPlayerCoroutine;
	const float COOP_DELAY_BEFORE_RESURRECTING = 1f;
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
		//Each wave survived increases the player's score.
		PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( name );
		int wavesCompleted = waveNumber - 1;
		if( wavesCompleted < 0 ) wavesCompleted = 0;
		pmd.score = pmd.score + wavesCompleted * SCORE_PER_WAVE;

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
			HUDMultiplayer.hudMultiplayer.activateUserMessage( LocalizationManager.Instance.getText("COOP_HIGH_SCORE_WAVES"), 0, 2.5f );
		}
	}

	IEnumerator coopResurrectPlayer()
	{
		PlayerRace partner = getPartner( GetComponent<PlayerRace>() );
		if( partner != null )
		{
			PlayerMatchData partner_pmd = LevelManager.Instance.getPlayerMatchDataByName( partner.name );
			//You were resurrected thanks to your partner.
			//Increase his Revives count.
			partner_pmd.revives++;
			CoopWaveGenerator.Instance.removeDeadPlayer( transform );

			yield return new WaitForSeconds( COOP_DELAY_BEFORE_RESURRECTING );

			GetComponent<PlayerControl>().coopResurrectBegin( partner.GetComponent<PlayerControl>().currentTile.name );
			partner.GetComponent<PlayerCamera>().isBeingSpectated = false;
		}
		else
		{
			yield return null;
		}
	}

	public void gameOver()
	{
		Debug.Log( name + " PlayerCoop - game over called. " );
		if( coopResurrectPlayerCoroutine != null ) StopCoroutine( coopResurrectPlayerCoroutine );
		if( GetComponent<PhotonView>().isMine && GetComponent<PlayerAI>() == null )
		{
			GameManager.Instance.playerStatistics.updateCoopHighScoreWaves( CoopWaveGenerator.numberOfWavesTriggered );
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
