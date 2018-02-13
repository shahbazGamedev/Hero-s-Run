using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

public enum SpeedMultiplierType {
		Jumping = 0,
		Turning = 1,
		Falling = 2,
		Stumbling = 3,
		Power_Speed_Boost = 4,
		Shrink = 100,
		Sprint = 101,
		Raging_Bull = 102,
		Cloak = 103
}

public class PlayerRun : Photon.PunBehaviour {

	[Tooltip("List of run speed mofifiers to apply when the character state changes. For example, when the player stumbles, we want to temporarily slow down the player. This can be done by adding a Stumble type with a value of 0.9f.")]
	[SerializeField] List<SpeedMultiplier> speedMultipliersList =  new List<SpeedMultiplier>();

	#region Run speed
	//The run speed specified in the level data.
	float levelRunStartSpeed = 0;

	//The run speed of the player
	public float runSpeed = 0;

	//List of active speed multipliers.
	public List<SpeedMultiplier> activeSpeedMultipliersList =  new List<SpeedMultiplier>();

	float defaultOverallSpeedMultiplier = 1f; //this value may be overriden in the debug menu
	//Used to cap the speed multiplier value so the player does not run too fast especially when stacking up
	//multiple multipliers (Sprint, plus Raging Bull, plus Cloak for instance).
	const float MAX_OVERALL_SPEED_MULTIPLIER = 1.7f;

	//The speed to reduce to after a slow down.
	const float SLOW_DOWN_END_SPEED = 5f;
	Coroutine slowDownPlayerCoroutine;
	#endregion

	#region Run to Sprint animations blending
	//Used to modify the blend amount between Run and Sprint animations based on the current run speed.
	//The desired behavior is as follows:
	//If the run speed is less or equal to the level start speed, we want a blend value of 0 (pure run).
	//If the run speed is equal or bigger than RUN_SPEED_FOR_FULL_BLENDING, we want a blend value of 1 (pure sprint).
	int speedBlendFactor = Animator.StringToHash("Speed");
	float RUN_SPEED_FOR_FULL_BLENDING = 28f;
	float baseBlend;
	#endregion

	#region Cached for performance
	Animator anim;
	PlayerControl playerControl;
	#endregion

	void Awake ()
	{
		anim = GetComponent<Animator>();
		playerControl = GetComponent<PlayerControl>();
	}

	void Start ()
	{
		defaultOverallSpeedMultiplier = GameManager.Instance.playerDebugConfiguration.getSpeedOverrideMultiplier();

		//Get the base run speed from the level data.
		levelRunStartSpeed = LevelManager.Instance.getSelectedCircuit().RunStartSpeed;
		baseBlend = RUN_SPEED_FOR_FULL_BLENDING - levelRunStartSpeed;
		if ( baseBlend < 0 ) baseBlend = 1f;
	}

	void OnEnable()
	{
		PlayerRace.crossedFinishLine += CrossedFinishLine;
	}

	void OnDisable()
	{
		PlayerRace.crossedFinishLine -= CrossedFinishLine;
	}

	public void handlePlayerStateChange( PlayerCharacterState newState )
	{
	    switch (newState)
		{
	        case PlayerCharacterState.Dying:
				StopAllCoroutines();
				runSpeed = 0;
				removeAllSpeedMultipliers( true );
				break;
	                
			case PlayerCharacterState.DoubleJumping:
			case PlayerCharacterState.Jumping:
				calculateRunSpeedDuringJump();
				break;
		
			case PlayerCharacterState.Turning:
			case PlayerCharacterState.Turning_and_sliding:
				addSpeedMultiplier( SpeedMultiplierType.Turning );
				break;
	                
	        case PlayerCharacterState.Stumbling:
				addSpeedMultiplier( SpeedMultiplierType.Stumbling );
				break;

	        case PlayerCharacterState.Falling:
				addSpeedMultiplier( SpeedMultiplierType.Falling );
				break;

	        case PlayerCharacterState.Idle:
				removeAllSpeedMultipliers( true );
				calculateOverallSpeedMultiplier();
				break;

	        case PlayerCharacterState.Ziplining:
				removeAllSpeedMultipliers( false );
				calculateOverallSpeedMultiplier();
				break;

	        case PlayerCharacterState.Running:
				removeAllSpeedMultipliers( false );
				calculateOverallSpeedMultiplier();
				break;
		}
	}

	void CrossedFinishLine( Transform player, RacePosition officialRacePosition, bool isBot )
	{
		activeSpeedMultipliersList.Clear();
	}

	/// <summary>
	/// Adds a speed multiplier of the given type.
	/// </summary>
	/// <param name="type">Type.</param>
	public void addSpeedMultiplier( SpeedMultiplierType type )
	{
		//Don't add the same speed multiplier twice
		if( !activeSpeedMultipliersList.Exists( mult => mult.type == type ) )
		{
			activeSpeedMultipliersList.Add( getSpeedMultiplierByType( type ) );
			calculateOverallSpeedMultiplier();
		}
	}

	/// <summary>
	/// Returns a predefined speed multiplier of the given type. The type must exist in speedMultipliersList.
	/// </summary>
	/// <param name="type">Type.</param>
	SpeedMultiplier getSpeedMultiplierByType( SpeedMultiplierType type )
	{
		if( speedMultipliersList.Exists( mult => mult.type == type ) )
		{
			return speedMultipliersList.Find( mult => mult.type == type );
		}
		else
		{
			Debug.LogError("PlayerRun error: could not find speed multiplier of type " + type + " in speedMultipliersList.");
			return null;
		}
	}

	/// <summary>
	/// Returns an active speed multiplier of the given type from the activeSpeedMultipliersList.
	/// </summary>
	/// <param name="type">Type.</param>
	SpeedMultiplier getActiveSpeedMultiplierByType( SpeedMultiplierType type )
	{
		if( activeSpeedMultipliersList.Exists( mult => mult.type == type ) )
		{
			return activeSpeedMultipliersList.Find( mult => mult.type == type );
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Adds a variable speed multiplier of the specified type. The speed multiplier value will change to endSpeedMultiplier in the time specified by duration.
	/// </summary>
	/// <returns>Adds a variable speed multiplier.</returns>
	/// <param name="type">Type.</param>
	/// <param name="endSpeedMultiplier">End speed multiplier.</param>
	/// <param name="duration">Duration.</param>
	public IEnumerator addVariableSpeedMultiplier( SpeedMultiplierType type, float endSpeedMultiplier, float duration )
	{
		//Don't add the same speed multiplier twice
		if( !activeSpeedMultipliersList.Exists( mult => mult.type == type ) )
		{
			SpeedMultiplier speedMultiplier = new SpeedMultiplier( type, true );
			activeSpeedMultipliersList.Add( speedMultiplier );
			float elapsedTime = 0;
			float startSpeedMultiplier = 1f;
			do
			{
				elapsedTime = elapsedTime + Time.deltaTime;
				speedMultiplier.multiplier = Mathf.Lerp( startSpeedMultiplier, endSpeedMultiplier, elapsedTime/duration );
				calculateOverallSpeedMultiplier();
				yield return new WaitForFixedUpdate();  
				
			} while ( elapsedTime < duration );
			speedMultiplier.multiplier = endSpeedMultiplier;	
		}
	}

	/// <summary>
	/// Removes a variable speed multiplier of the specified type. The speed multiplier value will change back to 1f in the time specified by duration.
	/// </summary>
	/// <returns>Adds a variable speed multiplier.</returns>
	/// <param name="type">Type.</param>
	/// <param name="endSpeedMultiplier">End speed multiplier.</param>
	/// <param name="duration">Duration.</param>
	public IEnumerator removeVariableSpeedMultiplier( SpeedMultiplierType type, float duration )
	{
		//Only try to remove if it exists
		if( activeSpeedMultipliersList.Exists( mult => mult.type == type ) )
		{
			SpeedMultiplier speedMultiplier = getActiveSpeedMultiplierByType( type );
			float elapsedTime = 0;
			float startSpeedMultiplier = speedMultiplier.multiplier;
			do
			{
				elapsedTime = elapsedTime + Time.deltaTime;
				speedMultiplier.multiplier = Mathf.Lerp( startSpeedMultiplier, 1f, elapsedTime/duration );
				calculateOverallSpeedMultiplier();
				yield return new WaitForFixedUpdate();  
				
			} while ( elapsedTime < duration );
			activeSpeedMultipliersList.Remove( speedMultiplier );
		}
	}

	/// <summary>
	/// Removes the speed multiplier of the given type.
	/// </summary>
	/// <param name="type">Type.</param>
	public void removeSpeedMultiplier( SpeedMultiplierType type )
	{
		SpeedMultiplier mult = getSpeedMultiplierByType( type );
		if( activeSpeedMultipliersList.Contains( mult ) )
		{
			activeSpeedMultipliersList.Remove( mult );
			calculateOverallSpeedMultiplier();
		}
	}

	/// <summary>
	/// Calculates the overall speed multiplier. If the player is jumping, this method returns immediately.
	/// </summary>
	public void calculateOverallSpeedMultiplier()
	{
		//When the player jumps, we want exact control of where he lands.
		//Therefore we want the run speed to be constant during a jump.
		//This is why we return immediately when the player is jumping as we don't want to change the run speed.
		//Example: Player plays Raging Bull right after jumping. We do not want the player's run speed to be affected while he
		//is in the air.
		if( playerControl.getCharacterState() == PlayerCharacterState.Jumping || playerControl.getCharacterState() == PlayerCharacterState.DoubleJumping ) return;

		//When the player dies, we stop all coroutines on this behavior.
		//However, it may take a few frames for the coroutines to be fully stopped (typically 3 frames if you are curious).
		//This would mean that runSpeed, which was set to 0 when the player died, would be changed back to a non-zero value and the dead player would slide on the floor.
		//This is why we add a test here to force runSpeed to zero if the player is dead.
		if( playerControl.getCharacterState() == PlayerCharacterState.Dying )
		{
			runSpeed = 0;
			return;
		}
		float overallSpeedMultiplier = defaultOverallSpeedMultiplier;
		for( int i = 0; i < activeSpeedMultipliersList.Count; i++ )
		{
			overallSpeedMultiplier = overallSpeedMultiplier * activeSpeedMultipliersList[i].multiplier;
		}

		//If in coop mode, increase the run speed with each consecutive wave.
		if( GameManager.Instance.isCoopPlayMode() )
		{
			float waveSpeedMultiplier = CoopWaveGenerator.numberOfWavesTriggered * 0.01f;
			overallSpeedMultiplier = overallSpeedMultiplier * ( 1f + waveSpeedMultiplier );
		}

		//Cap the overall speed multiplier to MAX_OVERALL_SPEED_MULTIPLIER
		overallSpeedMultiplier = Mathf.Min( overallSpeedMultiplier, MAX_OVERALL_SPEED_MULTIPLIER );

		runSpeed = levelRunStartSpeed * overallSpeedMultiplier;
		
		//Now update the run/sprint blend factor
		if( runSpeed <= levelRunStartSpeed )
		{
			setSprintBlendFactor( 0 );
		}
		else
		{
			float blendFactor = ( runSpeed - levelRunStartSpeed )/baseBlend;
			setSprintBlendFactor( blendFactor );

		}
		//print("PlayerRun calculateOverallSpeedMultiplier: " + overallSpeedMultiplier + " runSpeed: " +  runSpeed + " defaultOverallSpeedMultiplier: " + defaultOverallSpeedMultiplier + " " + getActiveSpeedMultipliers() + " levelRunStartSpeed " + levelRunStartSpeed );

	}

	/// <summary>
	/// This method calculates the run speed while the player is jumping and ignores
	/// all speed multipliers except the Jump Speed Multiplier.
	/// When the player jumps, we want exact control of where he lands.
	/// Therefore we want the run speed to be constant during a jump.
	/// </summary>
	void calculateRunSpeedDuringJump()
	{
		//When the player dies, we stop all coroutines on this behavior.
		//However, it may take a few frames for the coroutines to be fully stopped (typically 3 frames if you are curious).
		//This would mean that runSpeed, which was set to 0 when the player died, would be changed back to a non-zero value and the dead player would slide on the floor.
		//This is why we add a test here to force runSpeed to zero if the player is dead.
		if( playerControl.getCharacterState() == PlayerCharacterState.Dying )
		{
			runSpeed = 0;
			return;
		}
		float overallSpeedMultiplier = defaultOverallSpeedMultiplier;
		overallSpeedMultiplier = overallSpeedMultiplier * getSpeedMultiplierByType(SpeedMultiplierType.Jumping).multiplier;

		//The only speed multiplier we will keep is SHRINK. We don't want the player to suddenly accelerate
		//when jumping while shrunk.
		SpeedMultiplier sm = getActiveSpeedMultiplierByType( SpeedMultiplierType.Shrink );
		if( sm != null ) overallSpeedMultiplier = overallSpeedMultiplier * sm.multiplier;

		//Cap the overall speed multiplier to MAX_OVERALL_SPEED_MULTIPLIER
		overallSpeedMultiplier = Mathf.Min( overallSpeedMultiplier, MAX_OVERALL_SPEED_MULTIPLIER );
		runSpeed = levelRunStartSpeed * overallSpeedMultiplier;
		
		//Now update the run/sprint blend factor
		if( runSpeed <= levelRunStartSpeed )
		{
			setSprintBlendFactor( 0 );
		}
		else
		{
			float blendFactor = ( runSpeed - levelRunStartSpeed )/baseBlend;
			setSprintBlendFactor( blendFactor );

		}
		//print("PlayerRun calculateJumpSpeedMultiplier: " + overallSpeedMultiplier + " runSpeed: " +  runSpeed + " defaultOverallSpeedMultiplier: " + defaultOverallSpeedMultiplier + " " + getActiveSpeedMultipliers() );

	}

	/// <summary>
	/// Removes all speed multipliers except Power_Speed_Boost. The power speed boost is kept even if you die as it is a way to help you catch up.
	/// </summary>
	/// <param name="includeCardBased">If set to <c>true</c> include card based spreed modifiers.</param>
	void removeAllSpeedMultipliers( bool includeCardBased )
	{
		if( includeCardBased )
		{
			for( int i = activeSpeedMultipliersList.Count-1; i >= 0; i-- )
			{
				if( activeSpeedMultipliersList[i].type != SpeedMultiplierType.Power_Speed_Boost ) activeSpeedMultipliersList.RemoveAt( i );
			}
		}
		else
		{
			for( int i = activeSpeedMultipliersList.Count-1; i >= 0; i-- )
			{
				if( !activeSpeedMultipliersList[i].isCardBased && activeSpeedMultipliersList[i].type != SpeedMultiplierType.Power_Speed_Boost) activeSpeedMultipliersList.RemoveAt( i );
			}
		}
	}

	/// <summary>
	/// Returns a string listing all of the active speed multipliers. Used for debugging.
	/// </summary>
	/// <returns>The active speed multipliers.</returns>
	string getActiveSpeedMultipliers()
	{
		string activeSpeedMultipliersString = string.Empty;
		for( int i = 0; i < activeSpeedMultipliersList.Count; i++ )
		{
			activeSpeedMultipliersString = activeSpeedMultipliersString + " " + activeSpeedMultipliersList[i].type;
		}
		return activeSpeedMultipliersString;
	}

	/// <summary>
	/// Returns the current run speed.
	/// </summary>
	/// <returns>The current run speed.</returns>
	public float getRunSpeed()
	{
		return runSpeed;
	}

	/// <summary>
	/// Slows down the player to an halt.
	/// The player will travel the distance specified starting from his current position.
	/// Note: Because we are not using the Z position of the finish line trigger, the end position of the player will vary depending on latency.
	/// This method only guarantees that the distance travelled will be the one specified.
	/// The alternative would be to send the Z position of the finish line trigger and then calculate the distance from that position.
	/// The problem again is latency. For the distance to be accurate, we would have to "teleport" the player at the trigger position.
	/// Assuming 50 ms latency and 20 m/s run speed, this would jerk the player back by 1 meter.
	/// To summarize, this method does not guarantee the same end position for a player crossing the finish line across all clients, however it
	/// does ensure that every player will travel the distance specified.
	/// </summary>
	/// <param name="distance">Distance.</param>
	/// <param name="onArrivalCallback">Optional method to call once the player has traveled the distance.</param>
	public void slowDownPlayer( float distance, System.Action onArrivalCallback = null )
	{
		if( slowDownPlayerCoroutine != null ) StopCoroutine( slowDownPlayerCoroutine );
		slowDownPlayerCoroutine = StartCoroutine( coroutineSlowDownPlayer( distance, onArrivalCallback ) );
	}

	IEnumerator coroutineSlowDownPlayer( float distance, System.Action onArrivalCallback = null )
	{
		GetComponent<Rigidbody>().velocity = new Vector3( 0,playerControl.moveDirection.y,0 );
		playerControl.enablePlayerControl( false );
		float percentageComplete = 0;

		Vector3 initialPlayerPosition = transform.position;
		float distanceTravelled = 0;
		float brakeFactor = 0.7f; //brake the player before slowing him down
		float startSpeed = runSpeed * brakeFactor;
		float endSpeed = SLOW_DOWN_END_SPEED;

		float startBlendFactor = anim.GetFloat( speedBlendFactor );

		float startAnimationSpeed = anim.speed;
		float endAnimationSpeed = 1f;

		while ( distanceTravelled <= distance )
		{
			distanceTravelled = Vector3.Distance( transform.position, initialPlayerPosition );
			percentageComplete = distanceTravelled/distance;

			//Update run speed
			runSpeed =  Mathf.Lerp( startSpeed, endSpeed, percentageComplete );

			//Update the blend amount between Run and Sprint animations based on the current run speed
			float blendFactor =  Mathf.Lerp( startBlendFactor, 0, percentageComplete );
			anim.SetFloat(speedBlendFactor, blendFactor);

			//update animation speed
			anim.speed = Mathf.Lerp( startAnimationSpeed, endAnimationSpeed, percentageComplete );
			yield return new WaitForEndOfFrame(); 
		}
		//We have arrived. Stop player movement.
		playerControl.enablePlayerMovement( false );
		if( onArrivalCallback != null ) onArrivalCallback();
	}

	/// <summary>
	/// Sets the sprint blend factor. If the value is 0, we play only the run animation; if the value is 1, we play only the Sprint animation.
	/// </summary>
	/// <param name="blendFactor">Blend factor.</param>
	void setSprintBlendFactor( float blendFactor )
	{
		//If the blendFactor is set to one we will only play the Sprint animation
		//and the Run animation will stop playing. Because of that, we will no longer hear any footsteps.
		//For this reason, cap the blend factor to 0.98f so that we always blend in a little of the run animation and therefore
		//continue to get the run animation sound callbacks.
		if( blendFactor > 0.98f ) blendFactor = 0.98f;
		anim.SetFloat(speedBlendFactor, blendFactor);
	}

	public float getLevelRunStartSpeed()
	{
		return levelRunStartSpeed;
	}


	[System.Serializable]
	public class SpeedMultiplier
	{
		public SpeedMultiplierType type; 
		public float multiplier = 1f;
		public bool isCardBased = false;
	
		public SpeedMultiplier( SpeedMultiplierType type, bool isCardBased )
		{
			this.type = type;
			this.isCardBased = isCardBased;
		}

		public SpeedMultiplier( SpeedMultiplierType type, float multiplier )
		{
			this.type = type;
			this.multiplier = multiplier;
		}
	}

}
