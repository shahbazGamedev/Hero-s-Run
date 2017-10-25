using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityStandardAssets.ImageEffects;

/// <summary>
/// This class handles all spells that affect the player such as Shrink.
/// </summary>
public class PlayerSpell : PunBehaviour {

	#region Used when being teleported
	//See Teleporter class.
	public bool isBeingTeleported = false;
	#endregion

	#region List of active cards
	//List of active cards either played by the local player or cast on the local player by an opponent.
	public List<CardName> activeCardList = new List<CardName>();
	#endregion

	#region Shrink spell
	[SerializeField] AudioClip shrinkSound;
	[SerializeField] ParticleSystem shrinkParticleSystem;
	const float SHRINK_SIZE = 0.3f;
	Coroutine shrinkCoroutine;
	Coroutine enlargeCoroutine;
	#endregion

	#region Hacked
	[SerializeField] Renderer omniToolRenderer;
	[SerializeField] Color omniToolNormalColor;
	[SerializeField] Color omniToolHackedColor;
	[SerializeField] ParticleSystem omniToolHackedFX;
	#endregion

	#region Linked Fate spell
	bool castLinkedFate = false;
	[SerializeField] AudioClip linkedFateSound;
	#endregion

	#region Sentry spell
	SentryController sentryController;
	#endregion

	#region Cloak spell
	[SerializeField] ParticleSystem appearFx;
	bool isInvisible = false;
	#endregion

	#region Raging Bull
	public bool isRagingBullActive = false;
	#endregion

	#region Cached for performance
	PlayerControl playerControl;
	PlayerSounds playerSounds;
	PlayerVoiceOvers playerVoiceOvers;
	PlayerRun playerRun;
	PlayerVisuals playerVisuals;
	PlayerAI playerAI;
	TurnRibbonHandler turnRibbonHandler;
	#endregion

	//Delegate used to communicate to other classes when an enemy has played a special card such as Hack
	public delegate void CardPlayedByOpponentEvent( CardName name, float duration );
	public static event CardPlayedByOpponentEvent cardPlayedByOpponentEvent;

	//Delegate used to communicate to other classes when a card effect is canceled.
	public delegate void CardCanceledEvent( CardName name, bool playedByOpponent );
	public static event CardCanceledEvent cardCanceledEvent;

	// Use this for initialization
	void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();
		playerSounds = GetComponent<PlayerSounds>();
		playerVoiceOvers = GetComponent<PlayerVoiceOvers>();
		playerRun = GetComponent<PlayerRun>();
		playerVisuals = GetComponent<PlayerVisuals>();
		playerAI = GetComponent<PlayerAI>();
		turnRibbonHandler = GameObject.FindGameObjectWithTag("Turn-Ribbon").GetComponent<TurnRibbonHandler>();
	}

	/// <summary>
	/// A card was played by either the local player or a bot.
	/// </summary>
	/// <param name="name">Name.</param>
	public void playedCard( CardName name )
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
		shrinkCoroutine = StartCoroutine( shrink( new Vector3( SHRINK_SIZE, SHRINK_SIZE, SHRINK_SIZE ), 1.25f, spellDuration ) );
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
		enlargeCoroutine = StartCoroutine( enlarge( Vector3.one, 1.1f, spellDuration ) );
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
		if( transform.localScale.y < 1f )
		{
			playerVoiceOvers.resetPitch();
			if( enlargeCoroutine != null ) StopCoroutine( enlargeCoroutine );
			if( shrinkCoroutine != null ) StopCoroutine( shrinkCoroutine );
			//If we died while shrunk, do nothing i.e. stay small, that's fine.
			//If we crossed the finish line while shrunk, enlarge the player quickly back to his normal size.
			if( playerControl.deathType == DeathType.Alive )
			{
				enlargeCoroutine = StartCoroutine( enlarge( Vector3.one, 0.9f, 0 ) );
			}
			removeActiveCard( CardName.Shrink );
		}
	}

	[PunRPC]
	public void unshrinkRPC()
	{
		//Are we shrunk?
		if( transform.localScale.y < 1f )
		{
			playerVoiceOvers.resetPitch();
			if( enlargeCoroutine != null ) StopCoroutine( enlargeCoroutine );
			if( shrinkCoroutine != null ) StopCoroutine( shrinkCoroutine );
			enlargeCoroutine = StartCoroutine( enlarge( Vector3.one, 0.9f, 0 ) );
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
		MiniMap.Instance.displaySecondaryIcon( photonView.viewID, (int) CardName.Hack, spellDuration );

		//Change color of omni-tool and play hacked particle system so it appears broken.
		if( omniToolRenderer) omniToolRenderer.material.SetColor ("_EmissionColor", omniToolHackedColor);
		if( omniToolHackedFX ) omniToolHackedFX.Play();

		//Only affect the turn-ribbon on the HUD if you are the local player and not a bot
		if( photonView.isMine && playerAI == null ) turnRibbonHandler.playerIsHacked( true );
	}

	void cancelHack()
	{
		CancelInvoke( "cancelHack" );
		print("PlayerSpell cancelHack for " + gameObject.name );
		if( omniToolRenderer) omniToolRenderer.material.SetColor ("_EmissionColor", omniToolNormalColor);
		if( omniToolHackedFX ) omniToolHackedFX.Stop();
		removeActiveCard( CardName.Hack );
		//Only affect the turn-ribbon on the HUD if you are the local player and not a bot
		if( photonView.isMine && playerAI == null ) turnRibbonHandler.playerIsHacked( false );
	}
	#endregion

	#region Steal
	[PunRPC]
	void cardStealTargetRPC( int photonViewID )
	{
		if( photonView.isMine )
		{
			CardName stolenCard = CardName.None;
			if( playerAI == null )
			{
				stolenCard = turnRibbonHandler.stealCard();
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
		if( photonView.isMine )
		{
			if( playerAI == null )
			{
				turnRibbonHandler.replaceCard( (CardName) cardName);
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
		MiniMap.Instance.displaySecondaryIcon( photonView.viewID, (int) CardName.Supercharger, spellDuration );

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

	#region Cloak
	public void cancelCloak()
	{
		//Don't change camera settings if you are a bot.
		if( playerAI == null )
		{
			//Reset the color correction curves used by Cloak.
			ColorCorrectionCurves ccc = Camera.main.GetComponent<ColorCorrectionCurves>();
	 		ccc.enabled = false;
			ccc.saturation = 1f;
		}
		makePlayerVisible();
		removeActiveCard( CardName.Cloak );
	}

	public void makePlayerInvisible()
	{
		isInvisible = true;
		ParticleSystem go = GameObject.Instantiate( appearFx );
		go.transform.position = transform.TransformPoint( 0, 1.2f, 0 );
		go.Play();
		GameObject.Destroy( go, 2f );
		Transform heroSkin = transform.FindChild("Hero Skin");
		SkinnedMeshRenderer[] smr = heroSkin.GetComponentsInChildren<SkinnedMeshRenderer>();
		for( int i = 0; i < smr.Length; i++ )
		{
			smr[i].enabled = false;
		} 
		playerVisuals.enablePlayerShadow( false );
	}

	public void makePlayerVisible()
	{
		if( isInvisible )
		{
			isInvisible = false;
			ParticleSystem go = GameObject.Instantiate( appearFx );
			go.transform.position = transform.TransformPoint( 0, 1.2f, 0 );
			go.Play();
			GameObject.Destroy( go, 2f );
			Transform heroSkin = transform.FindChild("Hero Skin");
			SkinnedMeshRenderer[] smr = heroSkin.GetComponentsInChildren<SkinnedMeshRenderer>();
			for( int i = 0; i < smr.Length; i++ )
			{
				smr[i].enabled = true;
			}
			playerVisuals.enablePlayerShadow( true );
		}
	}
	#endregion

	#region Speedboost a.k.a. Raging Bull
	public void cancelRagingBull()
	{
		if( isRagingBullActive )
		{
			isRagingBullActive = false;
			if( photonView.isMine && playerAI == null ) Camera.main.GetComponent<MotionBlur>().enabled = false;
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
		if( cardPlayedByOpponentEvent != null && photonView.isMine && playerAI == null )
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
		cancelRagingBull();
		cancelReflect();
		cancelCloak();
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
					playerControl.incrementKillCounter();
					//Reset the color
					if( GameManager.Instance.getPlayMode() != PlayMode.PlayAgainstEnemy) MiniMap.Instance.changeColorOfRadarObject( PlayerRace.players[i].GetComponent<PlayerControl>(), Color.white );
				}
			}
		}
		//Cancel any active spells if the player dies
		cancelAllSpells();
	}

}
