using System.Collections;
using UnityEngine;

public class PlayerCoop : MonoBehaviour {

	const int SCORE_PER_WAVE = 100; //coop - score points awarded per wave beaten.
	Coroutine coopResurrectPlayerCoroutine;
	const float COOP_DELAY_BEFORE_RESURRECTING = 1f;

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
