using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// See CardFreeze for details.
/// If you tap quickly on the ice, you can break free without waiting for the spell expires.
/// </summary>
public class FreezeController : CardSpawnedObject {

	#region Ice Ground Decal
	[Header("Ice Ground Decal")]
	[SerializeField] GameObject iceGroundDecal;
   	[SerializeField] AnimationCurve appearIceDecalCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
   	[SerializeField] AnimationCurve disappearIceDecalCurve = AnimationCurve.EaseInOut(0, 1, 3, 0);
	bool displayGroundIceDecal = false; //We don't want to display the ground decal if there is no ground underneath the target.
    int cutOffPropertyID; 		//cached for performance
	Material decalMaterial;		//cached for performance
	#endregion

	#region Ice and Ice Shards
	[Header("Ice and Ice Shards")]
	[SerializeField] GameObject ice;
 	[SerializeField] Transform iceShardsOwner;
	List<GameObject> iceShardsList = new List<GameObject>();
	[SerializeField] GameObject topmostIceShard;
	[Tooltip("Audio clip that plays when the target breaks free from the ice.")]
	[SerializeField] AudioClip destroyedSound;
	#endregion

	#region Tap to break free
	[Header("Tap to break free")]
	[Tooltip("Particle system that plays each time the player taps on the ice. Set the stop action to Destroy on the particle system.")]
	[SerializeField] ParticleSystem tapParticleSystem;
	[Tooltip("Audio clip that plays each time the player taps on the ice.")]
	[SerializeField] AudioClip tapSound; 
	[Tooltip("When a ice shard is broken off, change its material to a darker one.")]
	[SerializeField] Material tapMaterial;
	int tapsDetected = 0;
	int tapsRequiredToBreakFreeze; //This value is sent by CardFreeze. It depends on the level of the card. It varies between 8 and 16.
	#endregion

	#region player
	Transform affectedPlayerTransform;
	PlayerControl affectedPlayerControl;
	Coroutine destroyIceCoroutine;
	float animSpeedAtTimeOfFreeze;
	bool isLocalPlayer = true;
	#endregion

	#region creature
	Transform affectedCreatureTransform;
	Coroutine destroyIceCreatureCoroutine;
	#endregion

	#region Initialisation
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		populateIceShards();
		setIceDecalProperties();
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

	void setIceDecalProperties()
	{
        decalMaterial = iceGroundDecal.GetComponent<Renderer>().material; 
        if (decalMaterial.HasProperty("_Cutoff")) cutOffPropertyID = Shader.PropertyToID("_Cutoff");
        decalMaterial.SetFloat(cutOffPropertyID, 0);
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

	IEnumerator fadeGroundDecal( float duration, bool appear )
	{
 		float elapsedTime = 0;		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			if( appear)
			{
				decalMaterial.SetFloat( cutOffPropertyID, appearIceDecalCurve.Evaluate( elapsedTime ) );
			}
			else
			{
				decalMaterial.SetFloat( cutOffPropertyID, disappearIceDecalCurve.Evaluate( elapsedTime ) );
			}
			yield return new WaitForEndOfFrame();  
			
		} while ( elapsedTime < duration );
	}

	/// <summary>
	/// Gets a random ice shard.
	/// If tapsDetected = 1, it will return the topmost ice shard, else it will return a random ice shard.
	/// We want to remove the topmost ice shard first because it looks silly of most ice shards have been removed and it is still there.
	/// </summary>
	/// <returns>The random ice shard.</returns>
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

	#region Player
	void findAffectedPlayer(object[] data) 
	{
		tapsRequiredToBreakFreeze = (int) data[3];

		casterTransform = getPlayerByViewID( (int) data[4] );
		setCasterName( casterTransform.name );

		affectedPlayerTransform = getPlayerByViewID( (int) data[0] );

		if( affectedPlayerTransform != null )
		{
			//We found the spell's target
			affectedPlayerControl = affectedPlayerTransform.GetComponent<PlayerControl>();

			//If in the short time between the card being cast and the card being activated
			//the player has died or is IDLE, simply ignore.
			if( affectedPlayerControl.getCharacterState() == PlayerCharacterState.Dying || affectedPlayerControl.getCharacterState() == PlayerCharacterState.Idle ) return;

			affectedPlayerTransform.GetComponent<Rigidbody>().isKinematic = true;

			//isLocalPlayer is used by the code that allows a player to break out early by tapping.
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
			//Set the player state to Idle so that other spells don't affect the player while he is frozen.
			affectedPlayerControl.setCharacterState( PlayerCharacterState.Idle );

			//If the player has a Sentry, it will be destroyed.
			affectedPlayerTransform.GetComponent<PlayerSpell>().cancelSentrySpell();

			//Freeze has a limited lifespan.
			float spellDuration =  (float) data[2];
			affectedPlayerTransform.GetComponent<PlayerSpell>().displayCardTimerOnHUD(CardName.Freeze, spellDuration );
	//destroyIceCoroutine = StartCoroutine( destroyIce( spellDuration ) );
			//Display the Freeze secondary icon on the minimap
			MiniMap.Instance.displaySecondaryIcon( affectedPlayerTransform.GetComponent<PhotonView>().viewID, (int) CardName.Freeze, spellDuration );

			string tapInstructions = LocalizationManager.Instance.getText("CARD_TAP_INSTRUCTIONS");
			if( isLocalPlayer ) HUDMultiplayer.hudMultiplayer.showTapInstructions( tapInstructions );

			//We can now make the ice visible and collidable
			ice.GetComponent<MeshCollider>().enabled = true;
			ice.GetComponent<MeshRenderer>().enabled = true;

			//Display the ground ice decal?
			displayGroundIceDecal = (bool) data[1];
			iceGroundDecal.GetComponent<MeshRenderer>().enabled = displayGroundIceDecal; 
			if( displayGroundIceDecal ) StartCoroutine( fadeGroundDecal( 0.9f, true ) );
			Debug.Log("FreezeController-The player affected by the Freeze is: " + affectedPlayerTransform.name );
		}
		else
		{
			Debug.LogError("FreezeController error: couldn't find the target player with the Photon view id of " + (int) data[0] );
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
				displayGroundIceDecal = (bool) data[1];
				iceGroundDecal.GetComponent<MeshRenderer>().enabled = displayGroundIceDecal;
				if( displayGroundIceDecal ) StartCoroutine( fadeGroundDecal( 0.9f, true ) );
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
			if( hit.collider.gameObject == ice )
			{
				tapsDetected++;
		
				//Play a VFX at the hit location.
				ParticleSystem ps = GameObject.Instantiate( tapParticleSystem, hit.point, Quaternion.identity );

				//Break off an ice shard and change its material.
				GameObject iceShard = getRandomIceShard();
				iceShard.transform.parent = null;
				iceShard.GetComponent<Collider>().enabled = true;
				iceShard.GetComponent<Rigidbody>().isKinematic = false; //This will make it break away and fall.
				iceShard.GetComponent<MeshRenderer>().material = tapMaterial;
				Destroy( iceShard, 6f );

				//We want the Ice object transparency to be 0.4 when tapsDetected is equal to tapsRequiredToBreakFreeze. This is so it looks nice.
				//Freeze is a Rare card with 9 upgrade levels.
				//The number of taps required will vary between 8 (base + 1) and 16 (base + 9).
				Color currentIceColor = ice.GetComponent<MeshRenderer>().material.GetColor("_Color");
				float iceAlpha = (0.4f - 1f)/tapsRequiredToBreakFreeze * tapsDetected + 1f;
				ice.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color( currentIceColor.r, currentIceColor.g, currentIceColor.b, iceAlpha ) );

				if( tapsDetected == tapsRequiredToBreakFreeze )
				{
					//The player can finally break free.
					ice.gameObject.SetActive( false );
					iceShardsOwner.gameObject.SetActive( false );
					GetComponent<AudioSource>().PlayOneShot( destroyedSound );
					if( displayGroundIceDecal ) StartCoroutine( fadeGroundDecal( 3f, false ) );
					destroyIceImmediately();
				}
				else
				{
					//Not enough taps to break free.
					//Play the tap sound.
					GetComponent<AudioSource>().PlayOneShot( tapSound );
				}
			}
		}
	}
	#endregion

}
