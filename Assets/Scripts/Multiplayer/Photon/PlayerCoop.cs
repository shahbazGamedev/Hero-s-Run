using System.Collections;
using UnityEngine;
using Photon;
using Cinemachine;

public class PlayerCoop : PunBehaviour {

	public Transform coopPartner; //Assumes coop is only 2 players.
	const float COOP_DELAY_BEFORE_RESURRECTING = 1.4f;
	Coroutine coopResurrectPlayerCoroutine;
	const int SCORE_PER_WAVE = 100; //coop - score points awarded per wave beaten.
	PlayerAI playerAI;

	void Awake ()
	{
		enabled = GameManager.Instance.isCoopPlayMode();
		playerAI = GetComponent<PlayerAI>();
	}

	void Start ()
	{
		//Save a reference to our coop partner.
		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i] != GetComponent<PlayerRace>() ) 
			{
				coopPartner = PlayerRace.players[i].transform;
				break;
			}
		}		
	}

	//Called by PlayerControl if the game mode is coop.
	public void playerDied()
	{
		//You just died.
		PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( name );
		pmd.downs++;

		//Is your coop partner dead or alive?
		if( coopPartner.GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying )
		{
			//Your coop partner is also dead.
			//This means game over.
			Debug.Log("playerDied: game over! Your name: " + name + " Your partner's name: "  + coopPartner.name );
			//Inform all via an RPC.
			photonView.RPC("coopGameOverRPC", PhotonTargets.All );
		}
		else
		{
			//Your coop partner is alive.
			//Start spectating your partner. You will be revived if your coop partner survives to the next wave.
			//Display the message "SPECTATING" on the HUD.
			if( GetComponent<PlayerAI>() == null ) HUDMultiplayer.hudMultiplayer.displayTopMessage( LocalizationManager.Instance.getText( "COOP_SPECTATING" ) );
			//Have the main camera track your partner.
			CinemachineVirtualCamera cmvc = GameObject.FindGameObjectWithTag("Main Virtual Camera").GetComponent<CinemachineVirtualCamera>();
			cmvc.m_Follow = coopPartner;
			cmvc.m_LookAt = coopPartner;
		}
	}

	[PunRPC]
	void coopGameOverRPC()
	{
		if( coopResurrectPlayerCoroutine != null ) StopCoroutine( coopResurrectPlayerCoroutine );

		if( photonView.isMine && GetComponent<PlayerAI>() == null )
		{
			//Display the results screen (player details, score, rounds survived, etc.) and return to the lobby.
			PlayerRaceManager.Instance.setRaceStatus( RaceStatus.COMPLETED );
			StartCoroutine( HUDMultiplayer.hudMultiplayer.leaveRoomShortly() );
			StartCoroutine( HUDMultiplayer.hudMultiplayer.displayCoopResultsAndEmotesScreen( 0.25f ) );
			//Save the high scores (highest score and highest wave reached) as they may have changed.
			GameManager.Instance.playerStatistics.serializePlayerStatistics( true );
		}
	}

	//Called by LevelNetworkingManager when a new wave starts.
	public void nextWaveActivated( string nameOfTileEntered )
	{
		if( PlayerRaceManager.Instance.getRaceStatus() == RaceStatus.COMPLETED ) return;

		//Each completed wave survived increases the player's score.
		PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( name );
		int wavesCompleted = ZombieManager.numberOfZombieWavesTriggered - 1;
		if( wavesCompleted < 0 ) wavesCompleted = 0;
		pmd.score = pmd.score + wavesCompleted * SCORE_PER_WAVE;

		if( GetComponent<PlayerControl>().deathType == DeathType.Alive ) return;
		if( coopResurrectPlayerCoroutine != null ) StopCoroutine( coopResurrectPlayerCoroutine );
		coopResurrectPlayerCoroutine = StartCoroutine( coopResurrectPlayer( nameOfTileEntered ) );
	}

	IEnumerator coopResurrectPlayer( string nameOfTileEntered )
	{
		yield return new WaitForSeconds( COOP_DELAY_BEFORE_RESURRECTING );

		GetComponent<PlayerControl>().coopResurrectBegin( nameOfTileEntered );
		//You were resurrected thanks to your partner.
		//Increase his Revives count.
		PlayerMatchData partner_pmd = LevelManager.Instance.getPlayerMatchDataByName( coopPartner.name );
		partner_pmd.revives++;
	}
	
}
