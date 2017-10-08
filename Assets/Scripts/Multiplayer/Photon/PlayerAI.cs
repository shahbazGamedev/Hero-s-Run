using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player AI. Important this component MUST be placed just below the PlayerControl component to work properly.
/// Currently allows AI character to:
/// 1) automatically turn corners
/// 2) break barrels
/// 3) jump over low obstacles and high obstacles.
/// </summary>
public class PlayerAI : AutoPilot {
	
	public HeroManager.BotHeroCharacter botHero;

	// Use this for initialization
	void Awake ()
	{
		base.Awake();
		//Get the selected bot index specified by LevelNetworkingManager.
		object[] botData = gameObject.GetPhotonView ().instantiationData;
		int selectedBotHeroIndex = (int) botData[0];
		//Save the bot hero character as other classes need it
		botHero = HeroManager.Instance.getBotHeroCharacter( selectedBotHeroIndex );

		//Get the bot skill data for that hero.
		HeroManager.BotSkillData botSkillData = HeroManager.Instance.getBotSkillData( botHero.skillLevel );

		//Reduce the change lane speed for bots. A high speed does not look natural.
		playerControl.sideMoveSpeed = 3.5f;

		//Save frequently used values for performance
		percentageWillTryToAvoidObstacle = botSkillData.percentageWillTryToAvoidObstacle;
		percentageWillTurnSuccesfully = botSkillData.percentageWillTurnSuccesfully;
		Debug.Log("Bot " + botHero.userName + " will try to avoid obstacled " + (percentageWillTryToAvoidObstacle * 100) + "% of the time." + " and will turn successfully " + (percentageWillTurnSuccesfully * 100) + "% of the time.");

		//Play a taunt after a short while
		Invoke( "playTaunt", Random.Range( 45f, 55f ) );
	}

	// Update is called once per frame
	void Update ()
	{
		#if UNITY_EDITOR
		handleKeyboard();
		#endif
		detectObstacles();
	}

	private void handleKeyboard()
	{
		BotCardHandler bch = GetComponent<BotCardHandler>();
		if ( Input.GetKeyDown (KeyCode.B ) )
		{
			//Kill bot for testing
			playerControl.killPlayer(DeathType.FallForward);
		}
		else if ( Input.GetKeyDown (KeyCode.Y ) )
		{
			//Stop the character from moving for testing
			playerControl.enablePlayerMovement ( !playerControl.isPlayerMovementEnabled() );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha0) )
		{
			bch.playCard( CardName.Firewall );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha1) )
		{
			bch.playCard( CardName.Stasis );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha2 ) )
		{
			bch.playCard( CardName.Hack );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha3 ) )
		{
			bch.playCard( CardName.Grenade );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha4 ) )
		{
			bch.playCard( CardName.Trip_Mine );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha5 ) )
		{
			bch.playCard( CardName.Lightning );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha6 ) )
		{
			bch.playCard( CardName.Linked_Fate );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha7 ) )
		{
			bch.playCard( CardName.Shrink );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha8 ) )
		{
			bch.playCard( CardName.Raging_Bull );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha9 ) )
		{
			bch.playCard( CardName.Sprint );
		}
		else if ( Input.GetKeyDown (KeyCode.X ) )
		{
			bch.playCard( CardName.Sentry );
		}
		else if ( Input.GetKeyDown (KeyCode.I ) )
		{
			bch.playCard( CardName.Force_Field );
		}
	}

	void OnTriggerEnter(Collider other)
	{
		base.OnTriggerEnter( other );
		if( other.CompareTag( "DropGrenade" ) )
		{
			// This is called as the bot is exiting a bridge.
			// If the bot has a CardGrenade, is allowed to play cards, is not affected by Hack, has enough mana, and is leading, drop a grenade to destroy the bridge.
			if( isBotLeading() ) GetComponent<BotCardHandler>().tryToPlayCard( CardName.Grenade );
		}
	}

	void playTaunt()
	{
		//Only be cocky if you are in the lead. This also avoids having 2 bots saying a taunt at the same time.
		if( isBotLeading() && PlayerRaceManager.Instance.getRaceStatus() != RaceStatus.COMPLETED && GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Dying )
		{
			VoiceOverManager.VoiceOverData vod = VoiceOverManager.Instance.getRandomHeroTaunt ( botHero.name );
			if( vod != null )
			{
				GetComponent<PlayerVoiceOvers>().playTaunt( vod.clip, vod.uniqueId );
			}
		}
	}

	bool isBotLeading()
	{
		bool isLeading = true;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			//Ignore the caster
			if( PlayerRace.players[i] == playerRace ) continue;
			if( PlayerRace.players[i].racePosition < playerRace.racePosition ) return false;
		}
		return isLeading;
	}
}
