﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class PlayerInput : PunBehaviour {

	PlayerControl playerControl;

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
	}
	
	void Update()
	{
		#if UNITY_EDITOR
		handleKeyboard();
		#endif

		//Handle mobile device swipes
		handleSwipes();
	}

	public void startSlide()
	{
		playerControl.startSlide();
		this.photonView.RPC("startSlideRPC", PhotonTargets.Others, transform.position, transform.eulerAngles.y, PhotonNetwork.time, playerControl.getSpeed() );
	}

	public void sideSwipe( bool isGoingRight )
	{
		playerControl.sideSwipe( isGoingRight );
		this.photonView.RPC("sideSwipeRPC", PhotonTargets.Others, isGoingRight, transform.position, transform.eulerAngles.y, PhotonNetwork.time, playerControl.getSpeed() );
	}

	public void jump()
	{
		playerControl.jump();
		this.photonView.RPC("jumpRPC", PhotonTargets.Others, transform.position, transform.eulerAngles.y, PhotonNetwork.time, playerControl.getSpeed() );
	}

	public void doubleJump( float doubleJumpSpeed )
	{
		playerControl.doubleJump( doubleJumpSpeed );
		this.photonView.RPC("doubleJumpRPC", PhotonTargets.Others, transform.position, transform.eulerAngles.y, PhotonNetwork.time, playerControl.getSpeed(), doubleJumpSpeed );
	}

	public void teleport( Vector3  destinationPosition, float destinationRotationY )
	{
		this.photonView.RPC("teleportRPC", PhotonTargets.AllViaServer, destinationPosition, destinationRotationY );
	}

	public void attachToZipline()
	{
		playerControl.attachToZipline();
		this.photonView.RPC("attachToZiplineRPC", PhotonTargets.Others, transform.position, transform.eulerAngles.y, PhotonNetwork.time, playerControl.getSpeed() );
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
			playerControl.managePlayerDeath(DeathType.FallForward);
		}
		else if ( Input.GetKeyDown (KeyCode.N ) )
		{
			//Create sentry
			Vector3 sentryOffsetToPlayer = new Vector3( 0.8f, 2.3f, 0 );
			Vector3 sentrySpawnPosition = transform.TransformPoint( sentryOffsetToPlayer );

			object[] data = new object[4];
			data[0] = this.photonView.viewID;

			//Level related parameters
			float duration = 15f;
			float aimRange = 40f;
			float accuracy = 0.008f;
			data[1] = duration;
			data[2] = aimRange;
			data[3] = accuracy;

			PhotonNetwork.InstantiateSceneObject( "sentry", sentrySpawnPosition, transform.rotation, 0, data );
		}
		else if ( Input.GetKeyDown (KeyCode.P ) )
		{
			//Stop the character from moving for testing
			GetComponent<CharacterController>().enabled = !GetComponent<CharacterController>().enabled;
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
