using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

public enum SpeedMultiplierType {
		Jumping = 0,
		Turning = 1,
		Falling = 2,
		Stumbling = 3,
		Shrink = 100,
		Sprint = 101,
		Raging_Bull = 102
}

public class PlayerRun : Photon.PunBehaviour {

	[Tooltip("List of run speed mofifiers to apply when the character state changes. For example, when the player stumbles, we want to temporarily slow down the player. This can be done by adding a Stumble type with a value of 0.9f.")]
	[SerializeField] List<SpeedMultiplier> speedMultipliersList =  new List<SpeedMultiplier>();

	#region Run speed
	//The run speed specified in the level data.
	float levelRunStartSpeed = 0;

	//The run speed of the player
	float runSpeed = 0;

	//List of active speed multipliers.
	public List<SpeedMultiplier> activeSpeedMultipliersList =  new List<SpeedMultiplier>();

	float defaultOverallSpeedMultiplier = 1f; //this value may be overriden in the debug menu
	float overallSpeedMultiplier; //this value is multiplied by levelRunStartSpeed to give the run speed

	//The speed to reduce to after crossing finish line
	const float SLOW_DOWN_END_SPEED = 5f;

 	//We want to cap the maximum run speed during a double jump. If the player is sprinting for example, we don't want him to leap into a wall.
	float MAX_RUN_SPEED_FOR_DOUBLE_JUMP = 18f;
	#endregion

	#region Run to Sprint animations blending
	//Used to modify the blend amount between Run and Sprint animations based on the current run speed.
	float blendFactor;
	int speedBlendFactor = Animator.StringToHash("Speed");
	#endregion

	#region Cached for performance
	Animator anim;
	PlayerControl playerControl;
	#endregion

	// Use this for initialization
	void Start ()
	{
		anim = GetComponent<Animator>();
		playerControl = GetComponent<PlayerControl>();

		defaultOverallSpeedMultiplier = LevelManager.Instance.speedOverrideMultiplier;
		overallSpeedMultiplier = defaultOverallSpeedMultiplier;

		//Get the base run speed from the level data.
		LevelData.MultiplayerInfo multiplayerInfo = LevelManager.Instance.getSelectedCircuit();
		levelRunStartSpeed = LevelManager.Instance.getSelectedCircuit().RunStartSpeed;
	}

	void OnEnable()
	{
		HUDMultiplayer.startRunningEvent += StartRunningEvent;
		PlayerControl.multiplayerStateChanged += MultiplayerStateChanged;
	}

	void OnDisable()
	{
		HUDMultiplayer.startRunningEvent -= StartRunningEvent;
		PlayerControl.multiplayerStateChanged -= MultiplayerStateChanged;
	}

	void StartRunningEvent()
	{
		runSpeed = levelRunStartSpeed;
	}

	void MultiplayerStateChanged( PlayerCharacterState newState )
	{
		print("PlayerRun MultiplayerStateChanged " + newState );
	    switch (newState)
		{
	        case PlayerCharacterState.Dying:
				runSpeed = 0;
				removeAllSpeedMultipliers( true );
				break;
	                
	        case PlayerCharacterState.StartRunning:
				runSpeed = levelRunStartSpeed;
				break;
	                
			case PlayerCharacterState.Jumping:
				addSpeedMultiplier( SpeedMultiplierType.Jumping );
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
	        case PlayerCharacterState.Ziplining:
				removeAllSpeedMultipliers( true );
				calculateOverallSpeedMultiplier();
				break;

	        case PlayerCharacterState.Running:
				removeAllSpeedMultipliers( false );
				calculateOverallSpeedMultiplier();
				break;
		}
	}

	/// <summary>
	/// Adds a speed multiplier of the given type.
	/// </summary>
	/// <param name="type">Type.</param>
	void addSpeedMultiplier( SpeedMultiplierType type )
	{
		//Don't add the same speed multiplier twice
		if( !activeSpeedMultipliersList.Exists( mult => mult.type == type ) )
		{
			print("addSpeedMultiplier: " + type );
			
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
			Debug.LogError("PlayerRun error: could not find active speed multiplier of type " + type + " in activeSpeedMultipliersList.");
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
			print("removeVariableSpeedMultiplier " + speedMultiplier.type + " " + speedMultiplier.multiplier );
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
		else
		{
			Debug.LogError("PlayerRun error: the variable speed modifier of type " + type + " you want to remove does not exists."  );
		}
	}

	/// <summary>
	/// Removes the speed multiplier of the given type.
	/// </summary>
	/// <param name="type">Type.</param>
	void removeSpeedMultiplier( SpeedMultiplierType type )
	{
		SpeedMultiplier mult = getSpeedMultiplierByType( type );
		if( activeSpeedMultipliersList.Contains( mult ) )
		{
			print("removeSpeedMultiplier: " + mult.type );
			activeSpeedMultipliersList.Remove( mult );
			calculateOverallSpeedMultiplier();
		}
	}

	/// <summary>
	/// Calculates the overall speed multiplier.
	/// </summary>
	void calculateOverallSpeedMultiplier()
	{
		overallSpeedMultiplier = defaultOverallSpeedMultiplier;
		for( int i = 0; i < activeSpeedMultipliersList.Count; i++ )
		{
			overallSpeedMultiplier = overallSpeedMultiplier * activeSpeedMultipliersList[i].multiplier;
		}
		runSpeed = levelRunStartSpeed * overallSpeedMultiplier;
		print("PlayerRun calculateOverallSpeedMultiplier: " + overallSpeedMultiplier + " runSpeed: " +  runSpeed + " defaultOverallSpeedMultiplier: " + defaultOverallSpeedMultiplier + " " + getActiveSpeedMultipliers() );

	}

	/// <summary>
	/// Removes all speed multipliers.
	/// </summary>
	/// <param name="includeCardBased">If set to <c>true</c> include card based spreed modifiers.</param>
	void removeAllSpeedMultipliers( bool includeCardBased )
	{
		print("removeAllSpeedMultipliers: " + includeCardBased );
		if( includeCardBased )
		{
			activeSpeedMultipliersList.Clear();
		}
		else
		{
			for( int i = activeSpeedMultipliersList.Count-1; i >= 0; i-- )
			{
				if( !activeSpeedMultipliersList[i].isCardBased ) activeSpeedMultipliersList.RemoveAt( i );
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

	//We pass the triggerPositionZ value because we need its position. We cannot rely on the position of the player at the moment of trigger because it can fluctuate based on frame rate and such.
	//Therefore the final destination is based on the trigger's Z position plus the desired distance (and not the player's z position plus the desired distance, which is slightly inaccurate).
	//The player slows down but keeps control.
	public IEnumerator slowDownPlayerAfterFinishLine( float distance, float triggerPositionZ )
	{
		GetComponent<Rigidbody>().velocity = new Vector3( 0,playerControl.moveDirection.y,0 );
		GetComponent<PlayerSpell>().cancelSpeedBoost();
		playerControl.enablePlayerControl( false );
		float percentageComplete = 0;

		Vector3 initialPlayerPosition = new Vector3( transform.position.x, transform.position.y, triggerPositionZ );
		float distanceTravelled = 0;
		float brakeFactor = 0.7f; //brake the player before slowing him down
		float startSpeed = runSpeed * brakeFactor;
		float endSpeed = SLOW_DOWN_END_SPEED;

		float startBlendFactor = blendFactor;

		float startAnimationSpeed = anim.speed;
		float endAnimationSpeed = 1f;

		while ( distanceTravelled <= distance )
		{
			distanceTravelled = Vector3.Distance( transform.position, initialPlayerPosition );
			percentageComplete = distanceTravelled/distance;

			//Update run speed
			runSpeed =  Mathf.Lerp( startSpeed, endSpeed, percentageComplete );

			//Update the blend amount between Run and Sprint animations based on the current run speed
			blendFactor =  Mathf.Lerp( startBlendFactor, 0, percentageComplete );
			anim.SetFloat(speedBlendFactor, blendFactor);

			//update animation speed
			anim.speed = Mathf.Lerp( startAnimationSpeed, endAnimationSpeed, percentageComplete );


			yield return new WaitForFixedUpdate(); 
		}
		//We have arrived. Stop player movement.
		playerControl.enablePlayerMovement( false );
		playerControl.playVictoryAnimation();
	}

	/// <summary>
	/// Sets the sprint blend factor. If the value is 0, we play only the run animation; if the value is 1, we play only the Sprint animation.
	/// </summary>
	/// <param name="blendFactor">Blend factor.</param>
	public void setSprintBlendFactor( float blendFactor )
	{
		//If the blendFactor is set to one we will only play the Sprint animation
		//and the Run animation will stop playing. Because of that, we will no longer hear any footsteps.
		//For this reason, cap the blend factor to 0.98f so that we always blend in a little of the run animation and therefore
		//continue to get the run animation sound callbacks.
		if( blendFactor > 0.98f ) blendFactor = 0.98f;
		anim.SetFloat(speedBlendFactor, blendFactor);
	}

	public void syncRunSpeed( float remoteSpeed )
	{
		//Needs to be implemented
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
