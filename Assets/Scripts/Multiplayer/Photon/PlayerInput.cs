using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class PlayerInput : PunBehaviour {

	PlayerControl playerControl;
	PlayerRun playerRun;

	#region Swipe variables
    float minSwipeDistancePixels;
    bool touchStarted = false;
    Vector2 touchStartPos;
	float minSwipeDistance = 0.022f; //About 17 pixels if width is 768 pixels
	#endregion

	#region Accelerometer
	bool isTiltingEnabled = false;
	const float ACCELEROMETER_TILT_THRESHOLD = 0.33f;
	#endregion

	// Use this for initialization
	void Awake ()
	{
		//If we are not the owner of this component, disable it.
		if( !this.photonView.isMine ) this.enabled = false;
		playerControl = GetComponent<PlayerControl>();
		playerRun = GetComponent<PlayerRun>();
		calculateMinimumSwipeDistance();
		isTiltingEnabled = PlayerStatsManager.Instance.getTiltEnabled();
	}
	
	//Calculate the minimum swipe distance in pixels.
	//Basically, the bigger the screen, the further you have to swipe.
	void calculateMinimumSwipeDistance()
	{
        float screenDiagonalSize = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
        minSwipeDistancePixels = minSwipeDistance * screenDiagonalSize;
	}

	void Update()
	{
		if( playerControl.isPlayerControlEnabled() && playerControl.getCharacterState() != PlayerCharacterState.Flying )
		{
			#if UNITY_EDITOR
			handleKeyboard();
			#endif
	
			//Handle accelerometer. If you tilt left or right quickly, you can change lanes.
			handleAccelerometer();
	
			//Handle mobile device swipes
			handleSwipes();
		}
	}

	public void startSlide()
	{
		playerControl.startSlide();
		this.photonView.RPC("startSlideRPC", PhotonTargets.Others, transform.position, transform.eulerAngles.y, PhotonNetwork.time );
	}

	public void sideSwipe( bool isGoingRight )
	{
		playerControl.sideSwipe( isGoingRight );
		this.photonView.RPC("sideSwipeRPC", PhotonTargets.Others, isGoingRight, transform.position, transform.eulerAngles.y, PhotonNetwork.time );
	}

	public void jump()
	{
		playerControl.jump( false );
		this.photonView.RPC("jumpRPC", PhotonTargets.Others, transform.position, transform.eulerAngles.y, PhotonNetwork.time );
	}

	public void doubleJump( float doubleJumpSpeed )
	{
		playerControl.jump( true, doubleJumpSpeed );
		this.photonView.RPC("doubleJumpRPC", PhotonTargets.Others, transform.position, transform.eulerAngles.y, PhotonNetwork.time, doubleJumpSpeed );
	}

	public void attachToZipline()
	{
		//stop the player control and movement immediately locally
		playerControl.enablePlayerControl( false );
		playerControl.enablePlayerMovement( false );
		this.photonView.RPC("attachToZiplineRPC", PhotonTargets.All, transform.position, transform.eulerAngles.y, PhotonNetwork.time );
	}

	private void handleAccelerometer()
	{
		if( isTiltingEnabled )
		{
			if ( Input.acceleration.x > ACCELEROMETER_TILT_THRESHOLD ) 
			{
				sideSwipe( true );
			}
			else if ( Input.acceleration.x < -ACCELEROMETER_TILT_THRESHOLD ) 
			{
				sideSwipe( false );
			}
		}
	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.LeftArrow) ) 
		{
			sideSwipe( false );
		}
		else if ( Input.GetKeyDown (KeyCode.RightArrow) ) 
		{
			sideSwipe( true );
		}
		else if ( Input.GetKeyDown (KeyCode.DownArrow) ) 
		{
			startSlide();
		}
		else if ( Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.Space)  ) 
		{
			if( playerControl.isInZiplineTrigger && playerControl.getCharacterState() != PlayerCharacterState.Ziplining )
			{
				attachToZipline();
			}
			else
			{
				jump();
			}
		}
		else if ( Input.GetKeyDown (KeyCode.K ) )
		{
			//Kill player for testing
			playerControl.killPlayer(DeathType.FallForward);
		}
		else if ( Input.GetKeyDown (KeyCode.N ) )
		{
			GameObject.FindObjectOfType<CardHandler>().activateCard( this.photonView, CardName.Cloak, GameManager.Instance.playerProfile.getUserName(), 2 );
		}
		else if ( Input.GetKeyDown (KeyCode.O ) )
		{
			GameObject.FindObjectOfType<CardHandler>().activateCard( this.photonView, CardName.Raging_Bull, GameManager.Instance.playerProfile.getUserName(), 4 );
		}
		else if ( Input.GetKeyDown (KeyCode.T ) )
		{
			GameObject.FindObjectOfType<CardHandler>().activateCard( this.photonView, CardName.Force_Field, GameManager.Instance.playerProfile.getUserName(), 1);
		}
		else if ( Input.GetKeyDown (KeyCode.P ) )
		{
			//Stop the character from moving for testing
			playerControl.enablePlayerMovement ( !playerControl.isPlayerMovementEnabled() );
		}
		else if ( Input.GetKeyDown (KeyCode.S ) )
		{
			//Slow down time for testing
			if( Time.timeScale < 1f )
			{
				Time.timeScale = 1f;
			}
			else 
			{
				Time.timeScale = 0.5f;
			}
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
				sideSwipe( true );
	        }
			else if (angle < 180)
			{
				//player swiped DOWN
				startSlide();
	        }
			else if (angle < 270)
			{
				//player swiped LEFT
				sideSwipe( false );
			}
			else
			{
				//player swiped UP
				if( playerControl.isInZiplineTrigger && playerControl.getCharacterState() != PlayerCharacterState.Ziplining )
				{
					attachToZipline();
				}
				else
				{
					jump();
				}
	        }
		}
	}
}
