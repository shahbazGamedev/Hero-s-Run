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
public class PlayerAI : Photon.PunBehaviour {

	[Header("General")]
	/// <summary>
	/// The card handler. We need access so that the bot can play cards from his deck.
	/// </summary>
	float percentageWillTryToAvoidObstacle;
	float percentageWillTurnSuccesfully;
	const float BASE_RUN_SPEED = 18f;
	const float BASE_OBSTACLE_DETECTION_LOW_DISTANCE = 5.2f; //assuming a run speed of BASE_RUN_SPEED
	const float BASE_OBSTACLE_DETECTION_HIGH_DISTANCE = 8f; //assuming a run speed of BASE_RUN_SPEED
	Vector3 xOffsetStartLow = new Vector3( 0, 0.5f, 0 );	//For low obstacles
	Vector3 xOffsetStartHigh = new Vector3( 0, 1.5f, 0 );	//For high obstacles

	PlayerControl playerControl;
	PlayerInput playerInput;

	// Use this for initialization
	void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();
		playerInput = GetComponent<PlayerInput>();

		//Get the bot that was selected in MPNetworkLobbyManager and saved in LevelManager.
		HeroManager.BotHeroCharacter botHero = HeroManager.Instance.getBotHeroCharacter( LevelManager.Instance.selectedBotHeroIndex );

		//Get the bot skill data for that hero.
		HeroManager.BotSkillData botSkillData = HeroManager.Instance.getBotSkillData( botHero.skillLevel );

		//Save frequently used values for performance
		percentageWillTryToAvoidObstacle = botSkillData.percentageWillTryToAvoidObstacle;
		percentageWillTurnSuccesfully = botSkillData.percentageWillTurnSuccesfully;
		Debug.Log("Bot " + botHero.userName + " will try to avoid obstacled " + (percentageWillTryToAvoidObstacle * 100) + "% of the time." + " and will turn successfully " + (percentageWillTurnSuccesfully * 100) + "% of the time.");

		//Reduce the change lane speed for bots. A high speed does not look natural.
		playerControl.sideMoveSpeed = 3.5f;
	}

	// Update is called once per frame
	void Update ()
	{
		#if UNITY_EDITOR
		handleKeyboard();
		#endif
		detectObstacles();
	}

	void detectObstacles()
	{
		//Step 1) Adjust the obstacle detection range based on our run speed. If we are running fast, we need more time to react.
		float obstacleDetectionDistance = BASE_OBSTACLE_DETECTION_LOW_DISTANCE * playerControl.getSpeed()/BASE_RUN_SPEED;

		//Step 2) Detect if there are any low level obstacles
        RaycastHit hit;
		Vector3 exactPosStart = transform.TransformPoint( xOffsetStartLow );
		//Debug.DrawLine( exactPosStart, exactPosStart + transform.forward * obstacleDetectionDistance, Color.green );

        if (Physics.Raycast(exactPosStart, transform.forward, out hit, obstacleDetectionDistance ))
		{
			if( playerControl.getCharacterState() == PlayerCharacterState.Running )
			{
				//Debug.Log("detectObstacles LOW: " + hit.collider.name );
				if( hit.collider.name == "DeadTree" && shouldAvoidObstacle() )
				{
					playerInput.jump();
				}
				else if( hit.collider.name.StartsWith( "Breakable Barrel" ) && shouldAvoidObstacle() )
				{
					if( Random.value < 0.8f )
					{
						playerInput.startSlide();
					}
					else
					{
						playerInput.jump();
					}
				}
				else if( hit.collider.CompareTag( "Chicken" ) && shouldAvoidObstacle() )
				{
					playerInput.startSlide();
				}
				else if( hit.collider.CompareTag( "Cart" ) && shouldAvoidObstacle() )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Cow" ) && shouldAvoidObstacle() )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Firewall" ) && shouldAvoidObstacle() )
				{
					//Did we cast this firewall?
					string caster = hit.collider.GetComponent<Firewall>().casterName;
					if( gameObject.name != caster )
					{
						//We did not cast it. We need to jump over it.
						//As a reminder, the caster is immune to the firewall he casted.
						playerInput.jump();
					}
				}
				else if( hit.collider.CompareTag( "Player" ) )
				{
					if( hit.collider.GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying )
					{
						//Jump over dead player
						playerInput.jump();
					}
				}
				else if( hit.collider.CompareTag( "Obstacle_B" ) )
				{
					if( Random.value < 0.8f )
					{
						playerInput.startSlide();
					}
					else
					{
						playerInput.jump();
					}
				}
				else if( hit.collider.CompareTag( "Obstacle_L" ) )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Obstacle_DJ" ) )
				{
					moveToCenterLane();
				}
			}
		}

		obstacleDetectionDistance = BASE_OBSTACLE_DETECTION_HIGH_DISTANCE * playerControl.getSpeed()/BASE_RUN_SPEED;
		exactPosStart = transform.TransformPoint( xOffsetStartHigh );
		//Debug.DrawLine( exactPosStart, exactPosStart + transform.forward * obstacleDetectionDistance, Color.yellow );
        if (Physics.Raycast(exactPosStart, transform.forward, out hit, obstacleDetectionDistance ))
		{
			if( playerControl.getCharacterState() == PlayerCharacterState.Running )
			{
				//Debug.Log("detectObstacles HIGH: " + hit.collider.name );
				if( hit.collider.name == "DeadTree" && shouldAvoidObstacle() )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Cart" ) && shouldAvoidObstacle() )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Cow" ) && shouldAvoidObstacle() )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Firewall" ) && shouldAvoidObstacle() )
				{
					//Did we cast this firewall?
					string caster = hit.collider.GetComponent<Firewall>().casterName;
					if( gameObject.name != caster )
					{
						//We did not cast it. We need to jump over it.
						//As a reminder, the caster is immune to the firewall he casted.
						playerInput.jump();
					}
				}
				else if( hit.collider.CompareTag( "Obstacle_M" ) )
				{
					if( Random.value < 0.5f )
					{
						playerInput.startSlide();
					}
					else
					{
						playerInput.jump();
					}
				}
			}
		}
	}

	bool shouldAvoidObstacle()
	{
		if (Random.value <= percentageWillTryToAvoidObstacle )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	bool shouldTurnSuccessfully()
	{
		if (Random.value <= percentageWillTurnSuccesfully )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		//Carefull, if you turn right inside a deadEnd OnTriggerEnter will be called a second time (but not if your turn left).
		//This is probably a Unity bug.
		if( other.name == "deadEnd" && shouldTurnSuccessfully() )
		{
			DeadEndType currentDeadEndType = other.GetComponent<deadEnd>().deadEndType;
			if ( currentDeadEndType == DeadEndType.Left )
			{
				playerInput.sideSwipe( false );
			}
			else if ( currentDeadEndType == DeadEndType.Right )
			{
				playerInput.sideSwipe( true );
			}
		}
		else if( other.gameObject.CompareTag( "AttachZiplineTrigger" ) )
		{
			if( playerControl.getCharacterState() != PlayerCharacterState.Ziplining ) playerInput.attachToZipline();
		}
		else if( other.gameObject.CompareTag( "MoveToCenterLane" ) )
		{
			moveToCenterLane();
		}
	}

	private void moveToCenterLane()
	{
		playerControl.recalculateCurrentLane();
		if( playerControl.currentLane == PlayerControl.Lanes.Left )
		{
			playerInput.sideSwipe( true );
		}
		else if( playerControl.currentLane == PlayerControl.Lanes.Right )
		{
			playerInput.sideSwipe( false );
		}
	}

	private void handleKeyboard()
	{
		BotCardHandler bch = GetComponent<BotCardHandler>();
		if ( Input.GetKeyDown (KeyCode.B ) )
		{
			//Kill bot for testing
			playerControl.managePlayerDeath(DeathType.FallForward);
		}
		else if ( Input.GetKeyDown (KeyCode.Y ) )
		{
			//Stop the character from moving for testing
			GetComponent<CharacterController>().enabled = !GetComponent<CharacterController>().enabled;
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha0) )
		{
			bch.activateCard( CardName.Stasis );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha1) )
		{
			bch.activateCard( CardName.Double_Jump );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha2 ) )
		{
			bch.activateCard( CardName.Explosion );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha3 ) )
		{
			bch.activateCard( CardName.Firewall );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha4 ) )
		{
			bch.activateCard( CardName.Glyph );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha5 ) )
		{
			bch.activateCard( CardName.Lightning );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha6 ) )
		{
			bch.activateCard( CardName.Linked_Fate );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha7 ) )
		{
			bch.activateCard( CardName.Shrink );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha8 ) )
		{
			bch.activateCard( CardName.Raging_Bull );
		}
		else if ( Input.GetKeyDown (KeyCode.Alpha9 ) )
		{
			bch.activateCard( CardName.Sprint );
		}
	}


}
