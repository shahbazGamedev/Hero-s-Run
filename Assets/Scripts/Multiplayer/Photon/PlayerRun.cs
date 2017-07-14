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

	#region Run speed and acceleration	
	//This value is used to accelerate the run speed as time goes by
	float timeSessionStarted = 0;

	//The run start speed specified in the level data.
	float levelRunStartSpeed = 0;
	//when a new level starts or if the player dies and he is revived, he will
	//start running at runStartSpeed.
	float runStartSpeed = 0;
	//The run speed of the player
	public float runSpeed = 0;
	//Run acceleration is used to determine how fast the player's run speed
	//will increase. It is specified in the level data. A good value is 0.1f.
	float runAcceleration = 0;
	//The run speed is reduced slightly during turns to make them easier
	float runSpeedAtTimeOfTurn;
	float runSpeedTurnMultiplier = 0.9f;
	//If the player stumbles, his speed will be reduced while he tumbles.
	float runSpeedAtTimeOfStumble;
	float runSpeedStumbleMultiplier = 0.9f; 
	//allowRunSpeedToIncrease is set to false while jumping
	bool allowRunSpeedToIncrease = true;
	float runSpeedAtTimeOfJump;
	//By what percentage should we reduce the run speed during a jump
	float runSpeedJumpMultiplier = 0.75f;
	//The maximum run speed allowed.
	const float MAX_RUN_SPEED = 42f;
	//The speed to reduce to after crossing finish line
	const float SLOW_DOWN_END_SPEED = 5f;
	float MAX_RUN_SPEED_FOR_DOUBLE_JUMP = 18f; //We want to cap the maximum run speed during a double jump. If the player is sprinting for example, we don't want him to leap into a wall.
	#endregion

	//Used to modify the blend amount between Run and Sprint animations based on the current run speed. Also used by the troll.
	float blendFactor;
	int speedBlendFactor = Animator.StringToHash("Speed");
	int RunTrigger = Animator.StringToHash("Run");

	Animator anim;
	PlayerControl playerControl;
	[SerializeField] List<SpeedMultiplier> speedMultipliersList =  new List<SpeedMultiplier>();
	public List<SpeedMultiplier> activeSpeedMultipliersList =  new List<SpeedMultiplier>();
	float defaultOverallSpeedMultiplier = 1f;
	float overallSpeedMultiplier;

	// Use this for initialization
	void Start ()
	{
		anim = GetComponent<Animator>();
		playerControl = GetComponent<PlayerControl>();
		defaultOverallSpeedMultiplier = LevelManager.Instance.speedOverrideMultiplier;
		overallSpeedMultiplier = defaultOverallSpeedMultiplier;
	}

	void OnEnable()
	{
		HUDMultiplayer.startRunningEvent += StartRunningEvent;
		GameManager.gameStateEvent += GameStateChange;
		PlayerControl.multiplayerStateChanged += MultiplayerStateChanged;
	}

	void OnDisable()
	{
		HUDMultiplayer.startRunningEvent -= StartRunningEvent;
		PlayerControl.multiplayerStateChanged -= MultiplayerStateChanged;
	}

	void StartRunningEvent()
	{
		startRunning();
	}
	
	void GameStateChange( GameState previousState, GameState newState )
	{
		/*
		//Ignore game state changes if we are not the owner
		if ( !this.photonView.isMine ) return;
		if( newState == GameState.Normal )
		{
			if( previousState == GameState.Paused )
			{
				this.photonView.RPC( "unpauseRemotePlayers", PhotonTargets.AllViaServer );
			}
		}
		else if( newState == GameState.Paused )
		{
			this.photonView.RPC( "pauseRemotePlayers", PhotonTargets.AllViaServer, transform.position, transform.eulerAngles.y, PhotonNetwork.time );
		}*/
	}

	void MultiplayerStateChanged( PlayerCharacterState newState )
	{
		print("PlayerRun MultiplayerStateChanged " + newState );
	    switch (newState)
		{
	        case PlayerCharacterState.Dying:
				runSpeed = 0;
				allowRunSpeedToIncrease = false;
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

	SpeedMultiplier getActiveSpeedMultiplierByType( SpeedMultiplierType type )
	{
		if( activeSpeedMultipliersList.Exists( mult => mult.type == type ) )
		{
			return activeSpeedMultipliersList.Find( mult => mult.type == type );
		}
		else
		{
			Debug.LogError("PlayerRun error: could not find speed multiplier of type " + type + " in activeSpeedMultipliersList.");
			return null;
		}
	}

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

	public IEnumerator removeVariableSpeedMultiplier( SpeedMultiplierType type, float duration )
	{
		print("removeVariableSpeedMultiplier " + type + " " + duration );
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
			print("removeVariableSpeedMultiplier does not exists " + type  );
		}
	}

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

	string getActiveSpeedMultipliers()
	{
		string activeSpeedMultipliersString = string.Empty;
		for( int i = 0; i < activeSpeedMultipliersList.Count; i++ )
		{
			activeSpeedMultipliersString = activeSpeedMultipliersString + " " + activeSpeedMultipliersList[i].type;
		}
		return activeSpeedMultipliersString;
	}

	void setInitialRunningParameters()
	{
		//Use the level data to determine what the start run speed, run acceleration since they can vary
		LevelData.MultiplayerInfo multiplayerInfo = LevelManager.Instance.getSelectedCircuit();
		levelRunStartSpeed = multiplayerInfo.RunStartSpeed;
		runAcceleration = multiplayerInfo.RunAcceleration;
		runSpeedTurnMultiplier = 0.9f; //Hack = how does this impact player synchro? Should I use 1f?
		runStartSpeed = levelRunStartSpeed;
		runSpeed = levelRunStartSpeed;
	}

	public void startRunning()
	{	
		//Mecanim Hack - we call rebind because the animation states are not reset properly when you die in the middle of an animation.
		//For example, if you die during a double jump, after you get resurrected and start running again, if you do another double jump, only part of the double jump animation will play, never the full animation.
		anim.Rebind();

		setInitialRunningParameters();

		//The player starts off running
		playerControl.setAnimationTrigger(RunTrigger);
		playerControl.setCharacterState( PlayerCharacterState.StartRunning );
		playerControl.setCharacterState( PlayerCharacterState.Running );
	
		//When the GameState is NORMAL, we display the HUD
		if( this.photonView.isMine && GetComponent<PlayerAI>() == null ) GameManager.Instance.setGameState( GameState.Normal );

		playerControl.enablePlayerControl( true );
	}

	public float getRunSpeed()
	{
		return runSpeed;
	}

	public float getSpeedForState( PlayerCharacterState state )
	{
		return runSpeed;
	}

	public void setSpeedForState( PlayerCharacterState state )
	{

	}

	void FixedUpdate()
	{
		updateRunSpeed();
	}

	void updateRunSpeed()
	{
		if( allowRunSpeedToIncrease && playerControl.getCharacterState() != PlayerCharacterState.Dying && runSpeed <= MAX_RUN_SPEED )
		{
			//runSpeed = (Time.time - timeSessionStarted) * runAcceleration + runStartSpeed; //in seconds
		}
	}

	//We pass the triggerPositionZ value because we need its position. We cannot rely on the position of the player at the moment of trigger because it can fluctuate based on frame rate and such.
	//Therefore the final destination is based on the trigger's Z position plus the desired distance (and not the player's z position plus the desired distance, which is slightly inaccurate).
	//The player slows down but keeps control.
	public IEnumerator slowDownPlayerAfterFinishLine( float distance, float triggerPositionZ )
	{
		GetComponent<Rigidbody>().velocity = new Vector3( 0,playerControl.moveDirection.y,0 );
		GetComponent<PlayerSpell>().cancelSpeedBoost();
		allowRunSpeedToIncrease = false;
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
		this.blendFactor = blendFactor;
		//If the blendFactor is set to one we will only play the Sprint animation
		//and the Run animation will stop playing. Because of that, we will no longer hear any footsteps.
		//For this reason, cap the blend factor to 0.98f so that we always blend in a little of the run animation and therefore
		//continue to get the run animation sound callbacks.
		if( blendFactor > 0.98f ) blendFactor = 0.98f;
		anim.SetFloat(speedBlendFactor, blendFactor);
	}

	/// <summary>
	/// Gets the sprint blend factor.
	/// </summary>
	/// <returns>The sprint blend factor.</returns>
	public float getSprintBlendFactor()
	{
		return blendFactor;
	}

	public void syncRunSpeed( float remoteSpeed )
	{

	}

	public void setAllowRunSpeedToIncrease( bool value )
	{
		allowRunSpeedToIncrease = value;
	}

	public bool getAllowRunSpeedToIncrease()
	{
		return allowRunSpeedToIncrease;
	}

	protected IEnumerator changeSprintBlendFactor( float endBlendFactor, float duration, PlayerControl playerControl )
	{
		float elapsedTime = 0;
		float startBlendFactor = getSprintBlendFactor();
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			setSprintBlendFactor( Mathf.Lerp( startBlendFactor, endBlendFactor, elapsedTime/duration ) );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		setSprintBlendFactor( endBlendFactor );	
	}

	public void startSpeedBoost( float spellDuration, float speedMultiplier, PlayerControl playerControl, bool isMine )
	{
		//Only affect the camera for the local player
		if( isMine ) Camera.main.GetComponent<MotionBlur>().enabled = true;
		setAllowRunSpeedToIncrease( false );
		GetComponent<PlayerSpell>().isSpeedBoostActive = true;
		runSpeed = runSpeed * speedMultiplier;
	//playerControl.GetComponent<PlayerSounds>().playSound( soundFx, false );
		StartCoroutine( changeSprintBlendFactor( 1f, 0.7f, playerControl ) );
		StartCoroutine( stopSpeedBoost( spellDuration, playerControl, isMine) );
	}

	IEnumerator stopSpeedBoost( float spellDuration, PlayerControl playerControl, bool isMine )
	{
		yield return new WaitForSeconds( spellDuration );
		if( isMine ) Camera.main.GetComponent<MotionBlur>().enabled = false;
		if( playerControl.getCharacterState() != PlayerCharacterState.Dying ) setAllowRunSpeedToIncrease( true );
		playerControl.GetComponent<PlayerSounds>().stopAudioSource();
		GetComponent<PlayerSpell>().isSpeedBoostActive = false;
		StartCoroutine( changeSprintBlendFactor( 0, 0.7f, playerControl ) );
	}

	public void startSprint( float spellDuration, float speedMultiplier, PlayerControl playerControl )
	{
		setAllowRunSpeedToIncrease( false );
		runSpeed = runSpeed * speedMultiplier;
	//playerControl.GetComponent<PlayerSounds>().playSound( soundFx, false );
		StartCoroutine( changeSprintBlendFactor( 0.85f, 0.8f, playerControl ) );
		StartCoroutine( stopSprint( spellDuration, playerControl ) );
	}

	IEnumerator stopSprint( float spellDuration, PlayerControl playerControl )
	{
		yield return new WaitForSeconds( spellDuration );
		if( playerControl.getCharacterState() != PlayerCharacterState.Dying ) setAllowRunSpeedToIncrease( true );
		playerControl.GetComponent<PlayerSounds>().stopAudioSource();
		StartCoroutine( changeSprintBlendFactor( 0, 0.8f, playerControl ) );
	}

	/*
double jump
				//Cap the run speed to a maximum.
				if( runSpeed > MAX_RUN_SPEED_FOR_DOUBLE_JUMP ) runSpeed = MAX_RUN_SPEED_FOR_DOUBLE_JUMP;

jump
				//Lower the run speed during a normal jump
				runSpeed = runSpeed * runSpeedJumpMultiplier;
				//Don't go lower then levelRunStartSpeed
				if( runSpeed < levelRunStartSpeed ) runSpeed = levelRunStartSpeed;

Fall
		allowRunSpeedToIncrease = false;
		runSpeed = runSpeed * 0.65f;

Land
		allowRunSpeedToIncrease = true;

Turn
		//Reset the run speed to what it was at the beginning of the turn.
		allowRunSpeedToIncrease = true;
		runSpeed = runSpeedAtTimeOfTurn;

runSpeed = base + stack of modifiers that multiply each other + acceleration

Death
		runSpeed = 0;
		runSpeedAtTimeOfJump = 0;
		allowRunSpeedToIncrease = false;

Resurrect end
		allowRunSpeedToIncrease = true;

Stumble
			//If the player stumbles, he loses a bit of speed and momentarily stops accelerating.
			allowRunSpeedToIncrease = false;
			runSpeedAtTimeOfStumble = runSpeed;
			runSpeed = runSpeedStumbleMultiplier * runSpeed; //lower speed a bit

Stumble_completed
		runSpeed = runSpeedAtTimeOfStumble;
		allowRunSpeedToIncrease = true;

		allowRunSpeedToIncrease = false;

Turn
		if( other.CompareTag( "deadEnd" ) )
		{
			isInDeadEnd = true;
			wantToTurn = false;

			currentDeadEndType = other.GetComponent<deadEnd>().deadEndType;
			deadEndTrigger = other;
			//Slow dow the player to make it easier to turn
			allowRunSpeedToIncrease = false;
			runSpeedAtTimeOfTurn = runSpeed;
			runSpeed = runSpeed * runSpeedTurnMultiplier;
		}

Enlarge player spell
		playerControl.setAllowRunSpeedToIncrease( true );
		playerControl.runSpeed = runSpeedBeforeSpell;

Shrink in player spell
		playerControl.setAllowRunSpeedToIncrease( false );
		runSpeedBeforeSpell = playerControl.getSpeed();

			playerControl.runSpeed = runSpeedBeforeSpell * transform.localScale.y;
enlarge
			playerControl.runSpeed = runSpeedBeforeSpell * transform.localScale.y;

*/

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
