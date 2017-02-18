using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class PlayerInput : PunBehaviour {

	PlayerControl playerControl;
	PowerUpManager powerUpManager;

	#region Swipe variables
    float minSwipeDistancePixels;
    bool touchStarted = false;
    Vector2 touchStartPos;
	float minSwipeDistance = 0.022f; //About 17 pixels if width is 768 pixels
	#endregion

	// Use this for initialization
	void Awake ()
	{
		//If we are not the owner of this component, disable it.
		if( !this.photonView.isMine ) this.enabled = false;
		playerControl = GetComponent<PlayerControl>();

		//For power ups
		GameObject powerUpManagerObject = GameObject.FindGameObjectWithTag("PowerUpManager");
		powerUpManager = (PowerUpManager) powerUpManagerObject.GetComponent("PowerUpManager");
	}
	
	//This is called by PlayerControl. It can not be replaced by an Update() method.
	//The reason is that if this method is not processed at the appropriate time, some actions like jumping will not work.
	//For example, controller.isGrounded might be true before the character has jumped from the ground and therefore
	//reset the jump.
	public void processInputs()
	{
		#if UNITY_EDITOR
		handleKeyboard();
		#endif

		//Handle mobile device swipes
		handleSwipes();
		detectTaps();
	
	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.LeftArrow) ) 
		{
			playerControl.sideSwipe( false );
		}
		else if ( Input.GetKeyDown (KeyCode.RightArrow) ) 
		{
			playerControl.sideSwipe( true );
		}
		else if ( Input.GetKeyDown (KeyCode.DownArrow) ) 
		{
			playerControl.startSlide();
		}
		else if ( Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.Space)  ) 
		{
			//if( isInZiplineTrigger && playerControl.getCharacterState() != PlayerCharacterState.Ziplining )
			if( false && playerControl.getCharacterState() != PlayerCharacterState.Ziplining )
			{
				playerControl.attachToZipline();
			}
			else
			{
				playerControl.jump();
			}
		}
		else if ( Input.GetKeyDown (KeyCode.P ) )
		{
			GetComponent<CharacterController>().enabled = !GetComponent<CharacterController>().enabled;
		}
		else if ( Input.GetKeyDown (KeyCode.K ) )
		{
			playerControl.managePlayerDeath(DeathType.Obstacle);
		}
		else if ( Input.GetKeyDown (KeyCode.S ) )
		{
			//Slow down time
			if( Time.timeScale < 1f )
			{
				Time.timeScale = 1f;
			}
			else 
			{
				Time.timeScale = 0.5f;
			}
		}
		else if ( Input.GetKeyDown (KeyCode.D ) )
		{
			handlePowerUp();
		}
		else if ( Input.GetKeyDown (KeyCode.Q ) )
		{
			Debug.Log("Activating shield powerup " );
			powerUpManager.activatePowerUp(PowerUpType.Shield);
		}
		else if ( Input.GetKeyDown (KeyCode.T ) )
		{
			Debug.Log("Listing all textures: " );
			Utilities.printAllTexturesInScene();
		}
	}

	void handleSwipes()
	{
		//Verify if the player swiped across the screen
		if (Input.touchCount > 0)
		{
            Touch touch = Input.GetTouch( 0 );
            
            switch (touch.phase)
			{
		        case TouchPhase.Began:
		            touchStarted = true;
		            touchStartPos = touch.position;
	                break;
		                
		        case TouchPhase.Ended:
	                if (touchStarted)
					{
                        touchStarted = false;
	                }
	                break;
		                
		        case TouchPhase.Canceled:
	                touchStarted = false;
	                break;
		                
		        case TouchPhase.Stationary:
	                break;
		
		        case TouchPhase.Moved:
	                if (touchStarted)
					{
                        TestForSwipeGesture(touch);
	                }
	                break;
			}
		}	

	}
	
	void TestForSwipeGesture(Touch touch)
	{
		Vector2 lastPos = touch.position;
		float distance = Vector2.Distance(lastPos, touchStartPos);
		
		if (distance > minSwipeDistancePixels)
		{
			touchStarted = false;
	        float dy = lastPos.y - touchStartPos.y;
	        float dx = lastPos.x - touchStartPos.x;
	        
	        float angle = Mathf.Rad2Deg * Mathf.Atan2(dx, dy);
	        
	        angle = (360 + angle - 45) % 360;

			if (angle < 90)
			{
				//player swiped RIGHT
				playerControl.sideSwipe( true );
	        }
			else if (angle < 180)
			{
				//player swiped DOWN
				playerControl.startSlide ();
	        }
			else if (angle < 270)
			{
				//player swiped LEFT
				playerControl.sideSwipe( false );
			}
			else
			{
				//player swiped UP
				//if( isInZiplineTrigger && playerControl.getCharacterState() != PlayerCharacterState.Ziplining )
				if( false && playerControl.getCharacterState() != PlayerCharacterState.Ziplining )
				{
					playerControl.attachToZipline();
				}
				else
				{
					playerControl.jump();
				}
	        }
		}
	}

	void detectTaps()
	{
		if ( Input.touchCount > 0 )
		{
			Touch touch = Input.GetTouch(0);
			if( touch.tapCount == 2 )
			{
				if( touch.phase == TouchPhase.Ended  )
				{
					handlePowerUp();
				}
			}
		}
	}

	void handlePowerUp()
	{
		if( GameManager.Instance.getGameState() == GameState.Normal && playerControl.getCharacterState() != PlayerCharacterState.Dying )
		{
			powerUpManager.activatePowerUp( PlayerStatsManager.Instance.getPowerUpSelected() );
		}
	}

}
