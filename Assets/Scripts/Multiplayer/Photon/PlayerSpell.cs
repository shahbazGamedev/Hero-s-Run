using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityStandardAssets.ImageEffects;

/// <summary>
/// This class handles all spells that affect the player such as Shrink.
/// </summary>
public class PlayerSpell : PunBehaviour {

	#region List of active cards
	//List of active cards (that have the affectsPlayerDirectly flag) either played by the local player or cast on the local player by an opponent.
	//For example, Force Field has a duration but does not affect the player directly, so affectsPlayerDirectly would be false.
	//However, Reflect has a duration and affects the player directly, so affectsPlayerDirectly would be true and therefore would be in this list while active.
	public List<CardName> activeCardList = new List<CardName>();
	#endregion

	#region Shrink spell
	[SerializeField] AudioClip shrinkSound;
	[SerializeField] ParticleSystem shrinkParticleSystem;
	const float SHRINK_SIZE = 0.3f;
	#endregion

	#region Linked Fate spell
	bool castLinkedFate = false;
	[SerializeField] AudioClip linkedFateSound;
	#endregion

	#region Sentry spell
	SentryController sentryController;
	#endregion

	#region Cached for performance
	PlayerControl playerControl;
	PlayerSounds playerSounds;
	PlayerVoiceOvers playerVoiceOvers;
	PlayerRun playerRun;
	#endregion

	//Delegate used to communicate to other classes when an enemy has played a special card such as Hack
	public delegate void CardPlayedByOpponentEvent( CardName name, float duration );
	public static event CardPlayedByOpponentEvent cardPlayedByOpponentEvent;

	//Delegate used to communicate to other classes when a card effect is canceled.
	//For example, if the player gets hit by Stasis while using his Jet Pack, the jet pack card gets canceled.
	public delegate void CardCanceledEvent( CardName name, bool playedByOpponent );
	public static event CardCanceledEvent cardCanceledEvent;

	// Use this for initialization
	void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();
		playerSounds = GetComponent<PlayerSounds>();
		playerVoiceOvers = GetComponent<PlayerVoiceOvers>();
		playerRun = GetComponent<PlayerRun>();
	}

	void OnEnable()
	{
		TurnRibbonHandler.cardPlayedEvent += CardPlayedEvent;
		BotCardHandler.botPlayedCardEvent += BotPlayedCardEvent;
	}

	void OnDisable()
	{
		TurnRibbonHandler.cardPlayedEvent -= CardPlayedEvent;
		BotCardHandler.botPlayedCardEvent -= BotPlayedCardEvent;
	}

	/// <summary>
	/// A card was played by the local player.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="level">Level.</param>
	void CardPlayedEvent( CardName name, int level )
	{
		CardManager.CardData playedCard = CardManager.Instance.getCardByName( name );
		if( playedCard.affectsPlayerDirectly )
		{
			if( activeCardList.Contains( name ) )
			{
				Debug.LogWarning("PlayerSpell-the activeCardList already contains the card " + name );
			}
			else
			{
				activeCardList.Add( name );
			}
		}
	}

	/// <summary>
	/// A card was played by a bot.
	/// </summary>
	/// <param name="name">Name.</param>
	void BotPlayedCardEvent( CardName name )
	{
		//if I am not a bot, ignore
		if( GetComponent<PlayerAI>() == null ) return;

		CardManager.CardData playedCard = CardManager.Instance.getCardByName( name );
		if( playedCard.affectsPlayerDirectly )
		{
			if( activeCardList.Contains( name ) )
			{
				Debug.LogWarning("PlayerSpell-the activeCardList already contains the card " + name );
			}
			else
			{
				activeCardList.Add( name );
			}
		}
	}

	public bool isCardActive( CardName name )
	{
		return activeCardList.Contains( name );
	}

	void removeActiveCard( CardName name )
	{
		if( activeCardList.Contains( name ) ) activeCardList.Remove( name );
	}

	/// <summary>
	/// Call this function when a card duration has expired normally and the card should be removed from the activeCardList.
	/// Note: use a cancel function if you need to stop an active card before expiry.
	/// </summary>
	/// <param name="name">Name.</param>
	public void cardDurationExpired( CardName name )
	{
		removeActiveCard( name );
	}

	#region Shrink spell
	[PunRPC]
	void shrinkSpellRPC( float spellDuration )
	{
		//if you have a Sentry, destroy it.
		cancelSentrySpell();
		//Make the voice overs higher pitch, just for fun
		playerVoiceOvers.setPitch( 1.18f );
		playerSounds.playSound( shrinkSound, false );
		ParticleSystem shrinkEffect = ParticleSystem.Instantiate( shrinkParticleSystem, transform );
		shrinkEffect.transform.localPosition = new Vector3( 0, 1f, 0 );
		shrinkEffect.Play();
		StartCoroutine( shrink( new Vector3( SHRINK_SIZE, SHRINK_SIZE, SHRINK_SIZE ), 1.25f, spellDuration ) );
		displayCardTimerOnHUD( CardName.Shrink, spellDuration );
		StartCoroutine( playerRun.addVariableSpeedMultiplier( SpeedMultiplierType.Shrink, SHRINK_SIZE, 0.5f ) );
	}

	IEnumerator shrink( Vector3 endScale, float shrinkDuration, float spellDuration )
	{
		playerVoiceOvers.playVoiceOver(VoiceOverType.VO_Affected_by_Spell, CardName.Shrink );

		float elapsedTime = 0;
		Vector3 startScale = transform.localScale;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			transform.localScale = Vector3.Lerp( startScale, endScale, elapsedTime/shrinkDuration );

			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < shrinkDuration );
		transform.localScale = endScale;	
		StartCoroutine( enlarge( new Vector3( 1f, 1f, 1f ), 1.1f, spellDuration ) );
	}

	IEnumerator enlarge( Vector3 endScale, float shrinkDuration, float spellDuration )
	{
		yield return new WaitForSeconds( spellDuration );
		StartCoroutine( playerRun.removeVariableSpeedMultiplier( SpeedMultiplierType.Shrink, 0.5f ) );
		float elapsedTime = 0;
		float startPitch = playerVoiceOvers.getPitch();
		Vector3 startScale = transform.localScale;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			transform.localScale = Vector3.Lerp( startScale, endScale, elapsedTime/shrinkDuration );
			playerVoiceOvers.setPitch( Mathf.Lerp( startPitch, 1f, elapsedTime/shrinkDuration ) );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < shrinkDuration );
		transform.localScale = endScale;	
		playerVoiceOvers.resetPitch();
		cardDurationExpired( CardName.Shrink );
	}

	public void cancelShrinkSpell()
	{
		//Are we shrunk?
		if( transform.localScale.y != 1f )
		{
			playerVoiceOvers.resetPitch();
			StopCoroutine( "enlarge" );
			StopCoroutine( "shrink" );
			//If we died while shrunk, do nothing i.e. stay small, that's fine.
			//If we crossed the finish line while shrunk, enlarge the player quickly back to his normal size.
			if( playerControl.deathType == DeathType.Alive )
			{
				StartCoroutine( enlarge( new Vector3( 1f, 1f, 1f ), 0.9f, 0 ) );
			}
			removeActiveCard( CardName.Shrink );
		}
	}
	#endregion

	#region Linked Fate spell
	[PunRPC]
	void cardLinkedFateRPC( string casterName, float spellDuration )
	{
		//Play a creepy sound
		playerSounds.playSound( linkedFateSound, false );

		if( gameObject.name == casterName )
		{
			castLinkedFate = true;
		}
		else
		{
			displayCardTimerOnHUD( CardName.Linked_Fate, spellDuration );
		}

		//For the player affected by Linked Fate, change the color of his icon on the map
		//The local player does not have an icon on the minimap.
		//If a bot cast the linked fate spell, do not attempt to change the icon color since it does not exist.
		if( isCardActive(CardName.Linked_Fate) &&  GameManager.Instance.getPlayMode() != PlayMode.PlayAgainstEnemy ) MiniMap.Instance.changeColorOfRadarObject( GetComponent<PlayerControl>(), Color.magenta );

		//Cancel the spell once the duration has run out
		Invoke("cancelLinkedFateSpell", spellDuration );

	}

	void cancelLinkedFateSpell()
	{
		CancelInvoke( "cancelLinkedFateSpell" );
		if( isCardActive(CardName.Linked_Fate) &&  GameManager.Instance.getPlayMode() != PlayMode.PlayAgainstEnemy ) MiniMap.Instance.changeColorOfRadarObject( GetComponent<PlayerControl>(), Color.white );
		castLinkedFate = false;
		removeActiveCard( CardName.Linked_Fate );
	}

	public bool hasCastLinkedFate()
	{
		return castLinkedFate;
	}
	#endregion

	#region Sentry Spell
	public void registerSentry( SentryController sentryController )
	{
		this.sentryController = sentryController;
	}

	public void cancelSentrySpell()
	{
		if( sentryController != null )
		{
			sentryController.destroySpawnedObjectNow();
			sentryController = null;
			removeActiveCard( CardName.Sentry );
		}
	}
	#endregion

	#region Hack
	[PunRPC]
	void cardHackRPC( float spellDuration )
	{
		displayCardTimerOnHUD( CardName.Hack, spellDuration );

		//Cancel the hack effect once the duration has run out
		Invoke("cancelHack", spellDuration );

		//Display the Hacked secondary icon on the minimap
		MiniMap.Instance.displaySecondaryIcon( GetComponent<PhotonView>().viewID, (int) CardName.Hack, spellDuration );

		//To Do
		//Add a reddish glow and electric sparks to the omni-tool so it appears broken.

	}

	void cancelHack()
	{
		CancelInvoke( "cancelHack" );
		print("PlayerSpell cancelHack for " + gameObject.name );
		removeActiveCard( CardName.Hack );
	}
	#endregion

	#region Steal
	[PunRPC]
	void cardStealTargetRPC( int photonViewID )
	{
		if( GetComponent<PhotonView>().isMine )
		{
			CardName stolenCard = CardName.None;
			if( GetComponent<PlayerAI>() == null )
			{
				TurnRibbonHandler trh = GameObject.FindGameObjectWithTag("Turn-Ribbon").GetComponent<TurnRibbonHandler>();
				stolenCard = trh.stealCard();
			}
			else
			{
				BotCardHandler bch = GetComponent<BotCardHandler>();
				stolenCard = bch.stealCard();
			}
			//Tell the caster which card was stolen
			PhotonView casterPhotonView = null;
			for( int i = 0; i < PlayerRace.players.Count; i++ )
			{
				if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
				{
					casterPhotonView = PlayerRace.players[i].GetComponent<PhotonView>();
					break;
				}
			}
			if( casterPhotonView != null)
			{
				casterPhotonView.RPC("cardStealDetailsRPC", PhotonTargets.AllViaServer, (int) stolenCard );
			}
		}
	}

	[PunRPC]
	void cardStealDetailsRPC( int cardName )
	{
		if( GetComponent<PhotonView>().isMine )
		{
			if( GetComponent<PlayerAI>() == null )
			{
				TurnRibbonHandler trh = GameObject.FindGameObjectWithTag("Turn-Ribbon").GetComponent<TurnRibbonHandler>();
				trh.replaceCard( (CardName) cardName);
			}
			else
			{
				BotCardHandler bch = GetComponent<BotCardHandler>();
				bch.replaceCard( (CardName) cardName);
			}
		}
	}
	#endregion

	#region Reflect
	public void cardReflectRPC( float spellDuration )
	{
		//Cancel the hack effect once the duration has run out
		Invoke("cancelReflect", spellDuration );
	}

	void cancelReflect()
	{
		CancelInvoke( "cancelReflect" );
		print("PlayerSpell cancelReflect for " + gameObject.name );
		removeActiveCard( CardName.Reflect );
	}
	#endregion

	#region Supercharger
	[PunRPC]
	void cardSuperchargerRPC( float spellDuration )
	{
		//Cancel the supercharger effect once the duration has run out
		Invoke("cancelSupercharger", spellDuration );

		//Display the Supercharger secondary icon on the minimap
		MiniMap.Instance.displaySecondaryIcon( GetComponent<PhotonView>().viewID, (int) CardName.Supercharger, spellDuration );

		//To Do
		//Display a Supercharger timer on the HUD so the player knows when it is going to run out.
		//Add a cool glow to the omni-tool
		print("PlayerSpell cardSuperchargerRPC received " + gameObject.name );
	}

	void cancelSupercharger()
	{
		CancelInvoke("cancelSupercharger" );
		removeActiveCard( CardName.Supercharger );
	}
	#endregion

	#region Jet Pack
	public void cancelJetPack()
	{
		GetComponent<PlayerJetPack>().stopFlying( false );
		sendCancelCardEvent( CardName.Jet_Pack, false );
		removeActiveCard( CardName.Jet_Pack );
	}
	#endregion

	#region Speedboost a.k.a. Raging Bull
	public void cancelRagingBull()
	{
		if( isCardActive(CardName.Raging_Bull) )
		{
			if( this.photonView.isMine && GetComponent<PlayerAI>() == null ) Camera.main.GetComponent<MotionBlur>().enabled = false;
			GetComponent<PlayerSounds>().stopAudioSource();
			removeActiveCard( CardName.Raging_Bull );
		}
	}
	#endregion

	#region Cancel Card
	void sendCancelCardEvent( CardName name, bool playedByOpponent )
	{
		if( cardCanceledEvent != null ) cardCanceledEvent( name, playedByOpponent );
	}
	#endregion


	#region Display No Target In Range Message
	[PunRPC]
	void cardNoTargetRPC()
	{
		MiniMap.Instance.displayMessage2( LocalizationManager.Instance.getText( "CARD_NO_TARGET_IN_RANGE" ), Color.red );
	}
	#endregion

	/// <summary>
	/// Send an event to display a timer on the HUD showing the duration of the opponent's card effect.
	/// If Hack lasts 10 seconds, then show a 10-second timer for example.
	/// The timer outline color for negative effects is red.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="duration">Duration.</param>
	public void displayCardTimerOnHUD( CardName name, float duration )
	{
		//Only send an event for the local player (and not for bots).
		if( cardPlayedByOpponentEvent != null && GetComponent<PhotonView>().isMine && GetComponent<PlayerAI>() == null )
		{
			cardPlayedByOpponentEvent( name, duration );
		}
		activeCardList.Add( name );
	}

	public void cancelAllSpells()
	{
		cancelLinkedFateSpell();
		cancelSentrySpell();
		cancelShrinkSpell();
		cancelSupercharger();
		cancelHack();
		cancelJetPack();
		cancelRagingBull();
		cancelReflect();
		activeCardList.Clear();
	}

	public void playerDied()
	{
		if( castLinkedFate && PhotonNetwork.isMasterClient )
		{
			//Kill all players with the affectedByLinkedFate flag. Ignore the caster (who is dead anyway).
			for( int i = 0; i < PlayerRace.players.Count; i++ )
			{
				if( PlayerRace.players[i].GetComponent<PlayerSpell>().isCardActive(CardName.Linked_Fate) && !PlayerRace.players[i].GetComponent<PlayerSpell>().hasCastLinkedFate() )
				{
					PlayerRace.players[i].GetComponent<PlayerControl>().killPlayer( DeathType.Obstacle );
					//Reset the color
					if( GameManager.Instance.getPlayMode() != PlayMode.PlayAgainstEnemy) MiniMap.Instance.changeColorOfRadarObject( PlayerRace.players[i].GetComponent<PlayerControl>(), Color.white );
				}
			}
		}
		//Cancel any active spells if the player dies
		cancelAllSpells();
	}

}
