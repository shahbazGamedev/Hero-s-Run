using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// See CardFreeze for details.
/// </summary>
public class FreezeController : CardSpawnedObject {

	[SerializeField] GameObject ice;
	[SerializeField] GameObject iceGroundDecal;
	[SerializeField] Transform iceShardsOwner;
	[SerializeField] GameObject topmostIceShard;

	[Header("Tap to break free")]
	//if you tap quickly on the Stasis sphere, you can break free without waiting for the spell expires.
	[SerializeField] ParticleSystem tapParticleSystem; //Set the stop action to Destroy on the particle system.
	[Tooltip("Abc")]
	[SerializeField] AudioClip tapSound; 
	[SerializeField] AudioClip destroyedSound;
	public int tapsDetected = 0;
	public int tapsRequiredToBreakFreeze; //7 base + 9 upgrade per level = 16
	public bool isLocalPlayer = true;
	[SerializeField] Material tapMaterial;
	public List<GameObject> iceShardsList = new List<GameObject>();

	//For player
	Transform affectedPlayerTransform;
	PlayerControl affectedPlayerControl;
	Coroutine destroyIceCoroutine;
	float animSpeedAtTimeOfFreeze;

	//For creatures
	Transform affectedCreatureTransform;
	Coroutine destroyIceCreatureCoroutine;

	#region Initialisation
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		populateIceShards();
		//Note that the Freeze prefab has its MeshRenderer and MeshCollider disabled.
		//We will enable them when the card gets activated by the lockstep manager.
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.CARD, gameObject, CardName.Freeze );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	void populateIceShards()
	{
		if( iceShardsOwner != null  )
		{
			for( int i = 0; i < iceShardsOwner.childCount; i++ )
			{
				GameObject child = iceShardsOwner.GetChild( i ).gameObject;
				if( child.GetComponent<Rigidbody>() != null && child != topmostIceShard ) iceShardsList.Add( child );
			}
		}
		else
		{
			Debug.LogError("FreezeController-populateIceShards: iceShardsOwner has not been set." );
		}
	}

	GameObject getRandomIceShard()
	{
		if( tapsDetected == 1 ) return topmostIceShard;

		if( iceShardsList.Count > 0 )
		{
			int random = Random.Range( 0, iceShardsList.Count );
			GameObject iceShard = iceShardsList[random];
			iceShardsList.RemoveAt( random );
			return iceShard;
		}
		else
		{
			return null;
		}
	}

	public override void activateCard()
	{
		if( GameManager.Instance.isCoopPlayMode() )
		{
			findAffectedCreature( gameObject.GetPhotonView ().instantiationData );
		}
		else
		{
			findAffectedPlayer( gameObject.GetPhotonView ().instantiationData );
		}
	}
	#endregion

	#region Player
	void findAffectedPlayer(object[] data) 
	{
		int viewIdOfAffectedPlayer = (int) data[0];
		tapsRequiredToBreakFreeze = (int) data[3];
		casterTransform = getCaster( (int) data[4] );
		setCasterName( casterTransform.name );

		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == viewIdOfAffectedPlayer )
			{
				//We found the spell's target
				affectedPlayerTransform = PlayerRace.players[i].transform;
				affectedPlayerControl = affectedPlayerTransform.GetComponent<PlayerControl>();

				//If in the short time between the card being cast and the card being activated
				//the player has died or is IDLE, simply ignore.
				if( affectedPlayerControl.deathType != DeathType.Alive || affectedPlayerControl.getCharacterState() == PlayerCharacterState.Idle ) return;

				affectedPlayerTransform.GetComponent<Rigidbody>().isKinematic = true;

				//isLocalPlayer is used by the code that allows a player to break out early by tapping on the ice.
				//It is used to ensure that you can only tap on the ice the local player is trapped in.
				isLocalPlayer = ( affectedPlayerTransform.GetComponent<PhotonView>().isMine && affectedPlayerTransform.GetComponent<PlayerAI>() == null );

				//If the player is affected by shrink, cancel it. The player will enlarge back to his normal size.
				affectedPlayerTransform.GetComponent<PlayerSpell>().cancelShrinkSpell();

				//Freeze the player's movement and remove player control.
				affectedPlayerControl.enablePlayerMovement( false );
				affectedPlayerControl.enablePlayerControl( false );

				affectedPlayerTransform.position = transform.position;

				animSpeedAtTimeOfFreeze = affectedPlayerTransform.GetComponent<Animator>().speed;
				affectedPlayerTransform.GetComponent<Animator>().speed = 0;
				//Set the player state to Idle so that other spells don't affect the player while he is in Statis.
				affectedPlayerControl.setCharacterState( PlayerCharacterState.Idle );

				//If the player has a Sentry, it will be destroyed.
				affectedPlayerTransform.GetComponent<PlayerSpell>().cancelSentrySpell();

				//The Freeze has a limited lifespan.
				float spellDuration =  6.5f;

				affectedPlayerTransform.GetComponent<PlayerSpell>().displayCardTimerOnHUD(CardName.Freeze, spellDuration );
				//destroyIceCoroutine = StartCoroutine( destroyIce( spellDuration ) );

				//Display the Freeze secondary icon on the minimap
				MiniMap.Instance.displaySecondaryIcon( affectedPlayerTransform.GetComponent<PhotonView>().viewID, (int) CardName.Freeze, spellDuration );

				string tapInstructions = LocalizationManager.Instance.getText("CARD_TAP_INSTRUCTIONS");
				if( isLocalPlayer ) HUDMultiplayer.hudMultiplayer.showTapInstructions( tapInstructions );

				//We can now make the ice visible and collidable
				//In order to detect taps/mouse-clicks properly, we need to change the layer to Default (it was Ignore Raycast).
				ice.layer = 0; //Default is 0
				ice.GetComponent<MeshCollider>().enabled = true;
				ice.GetComponent<MeshRenderer>().enabled = true;
				//Display the ground ice decal?
				bool displayGroundIceDecal = (bool) data[1];
				iceGroundDecal.GetComponent<MeshRenderer>().enabled = displayGroundIceDecal; 
				break;
			}
		}
		if( affectedPlayerTransform != null )
		{
			Debug.Log("FreezeController-The player affected by the Freeze is: " + affectedPlayerTransform.name );
		}
		else
		{
			Debug.LogError("FreezeController error: could not find the target player with the Photon view id of " + viewIdOfAffectedPlayer );
		}
	}

	IEnumerator destroyIce( float delayBeforeSpellExpires )
	{
		yield return new WaitForSeconds(delayBeforeSpellExpires);
		destroyIceImmediately();
	}

	void destroyIceImmediately()
	{
		if( destroyIceCoroutine != null ) StopCoroutine( destroyIceCoroutine );
		if( isLocalPlayer ) HUDMultiplayer.hudMultiplayer.hideTapInstructions();
		MiniMap.Instance.hideSecondaryIcon( affectedPlayerTransform.gameObject );
		affectedPlayerTransform.GetComponent<Rigidbody>().isKinematic = false;
		affectedPlayerTransform.GetComponent<Animator>().speed = animSpeedAtTimeOfFreeze;
		//The player starts off running
		affectedPlayerTransform.GetComponent<Animator>().SetTrigger( "Run" );
		affectedPlayerTransform.GetComponent<PlayerControl>().setCharacterState( PlayerCharacterState.Running );

		affectedPlayerControl.enablePlayerMovement( true );
		affectedPlayerControl.enablePlayerControl( true );
		affectedPlayerTransform.GetComponent<PlayerSpell>().cardDurationExpired(CardName.Freeze);
		Destroy( gameObject, 3f );
	}
	#endregion

	#region Creature
	void findAffectedCreature(object[] data) 
	{
		int viewIdOfAffectedCreature = (int) data[0];

		ZombieController[] zombies = GameObject.FindObjectsOfType<ZombieController>();
		for( int i = 0; i < zombies.Length; i ++ )
		{
			if( zombies[i].GetComponent<PhotonView>().viewID == viewIdOfAffectedCreature )
			{
				//We found the spell's target
				affectedCreatureTransform = zombies[i].transform;

				//If in the short time between the card being cast and the card being activated
				//the creature has died, simply ignore.
				if( zombies[i].getCreatureState() == CreatureState.Dying  ) return;

				//Freeze the creature's movement.
				zombies[i].freeze( true );

				affectedCreatureTransform.position = transform.position;

				//The Freeze has a limited lifespan.
				float spellDuration =  6.5f;

				destroyIceCreatureCoroutine = StartCoroutine( destroyIceCreature( spellDuration ) );

				//We can now make the ice visible and collidable
				//In order to detect taps/mouse-clicks properly, we need to change the layer to Default (it was Ignore Raycast).
				ice.layer = 0; //Default is 0
				ice.GetComponent<MeshCollider>().enabled = true;
				ice.GetComponent<MeshRenderer>().enabled = true;
				//Display the ground ice decal?
				bool displayGroundIceDecal = (bool) data[1];
				iceGroundDecal.GetComponent<MeshRenderer>().enabled = displayGroundIceDecal; 
				break;
			}
		}
		if( affectedCreatureTransform != null )
		{
			Debug.Log("FreezeController-The creature affected by the Freeze is: " + affectedCreatureTransform.name );
		}
		else
		{
			Debug.LogError("FreezeController error: could not find the target creature with the Photon view id of " + viewIdOfAffectedCreature );
		}
	}

	IEnumerator destroyIceCreature( float delayBeforeSpellExpires )
	{
		yield return new WaitForSeconds(delayBeforeSpellExpires);
		destroyIceImmediatelyCreature();
	}

	void destroyIceImmediatelyCreature()
	{
		if( destroyIceCreatureCoroutine != null ) StopCoroutine( destroyIceCreatureCoroutine );
		
		affectedCreatureTransform.GetComponent<ZombieController>().freeze( false );
		Destroy( gameObject );
	}
	#endregion

	#region Tap
	void Update () {

		if( !isLocalPlayer ) return;

		#if UNITY_EDITOR
		// User pressed the left mouse up
		if (Input.GetMouseButtonDown(0))
		{
			MouseButtonDown(0);
		}
		#else
		detectTaps();
		#endif
	}

	void MouseButtonDown(int Button)
	{
		validateTappedObject(Input.mousePosition);
	}

	void detectTaps()
	{
		for ( int i = 0; i < Input.touchCount; i++ )
		{
			if( Input.GetTouch(i).phase == TouchPhase.Began  )
			{
				validateTappedObject(Input.GetTouch(i).position);
			}
		}
	}

	void validateTappedObject( Vector2 touchPosition )
	{
		// We need to actually hit an object
		RaycastHit hit;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(touchPosition), out hit, 20))
		{
			if ( hit.collider )
			{
				if( hit.collider.gameObject == ice )
				{
					tapsDetected++;
					ParticleSystem ps = GameObject.Instantiate( tapParticleSystem, hit.point, Quaternion.identity );
					tapParticleSystem.Play();
					GameObject iceShard = getRandomIceShard();
					iceShard.transform.parent = null;
					iceShard.GetComponent<Collider>().enabled = true;
					iceShard.GetComponent<Rigidbody>().isKinematic = false;
					iceShard.GetComponent<MeshRenderer>().material = tapMaterial;
					Destroy( iceShard, 6f );
					Color currentIceColor = ice.GetComponent<MeshRenderer>().material.GetColor("_Color");
					//We want the Ice object transparency to be 0.4 when tapsDetected is equal to tapsRequiredToBreakFreeze. This is so it looks nice.
					//Freeze is a Rare card with 9 upgrade levels.
					//The number of taps required will vary between 8 (base + 1) and 16 (base + 9).
					float iceAlpha = (0.4f - 1f)/tapsRequiredToBreakFreeze * tapsDetected + 1f;
					ice.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color( currentIceColor.r, currentIceColor.g, currentIceColor.b, iceAlpha ) );
					if( tapsDetected == tapsRequiredToBreakFreeze )
					{
						ice.gameObject.SetActive( false );
						iceShardsOwner.gameObject.SetActive( false );
						GetComponent<AudioSource>().PlayOneShot( destroyedSound );
						destroyIceImmediately();
					}
					else
					{
						GetComponent<AudioSource>().PlayOneShot( tapSound );
					}
				}
			}
		}
	}
	#endregion

}
