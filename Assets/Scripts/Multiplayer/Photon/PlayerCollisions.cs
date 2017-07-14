using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player collisions.
/// Obstacle naming:
/// Obstacle_F - the obstacle is impassable
/// Obstacle_L - the obstacle's height is low. You can jump over it.
/// Obstacle_M - the obstacle's height is medium. You can either slide under it or jump over it.
/// Obstacle_H - the obstacle's height is high. You can slide under it.
/// Obstacle_B - the obstacle is breakable. You can slide into it to break it or jump over it.
/// Obstacle_DJ - the obstacle is tall. You can double-jump over it.
/// </summary>
public class PlayerCollisions : Photon.PunBehaviour {

	PlayerControl playerControl;
	PlayerSounds playerSounds;
	PlayerRun playerRun;
	PlayerSpell playerSpell;
	#region Ground type variables
	string groundType = "Floor"; //Other choices are Water and Collapsing
	string previousGroundType = "Floor"; //Other choices are Water and Collapsing
	#endregion

	// Use this for initialization
	void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();	
		playerSounds = GetComponent<PlayerSounds>();	
		playerRun = GetComponent<PlayerRun>();	
		playerSpell = GetComponent<PlayerSpell>();	
	}

	/// <summary>
	/// Gets the type of the ground. Either normal (untagged), water or collapsing.
	/// </summary>
	/// <returns>The ground type.</returns>
	public string getGroundType()
	{
		return groundType;
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if( playerControl.getCharacterState() != PlayerCharacterState.Dying )
		{
			Transform collided = collision.transform;
			Vector3 normal =  collision.contacts[0].normal;

			groundType = collided.tag;

			if( groundType != previousGroundType )
			{
				playerSounds.groundTypeChanged( groundType );
			}
			previousGroundType = groundType;

			//The CharacterController is constantly colliding with the Quads making up the floor. Ignore those events.
			if (collided.CompareTag("Floor") ) return;
	
			//Debug.Log ("OnControllerColliderHit  " + hit.collider.name  );
			if (collided.name == "DeadTree" )
			{
				if( normal.y < 0.4f )
				{
					//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
					//Move the player back so he does not float in the air in case he hit the log while landing after a jump.
					//controller.Move( hit.normal );
					playerControl.killPlayer ( DeathType.Obstacle );
				}
			}
			else if (collided.name == "GroundObstacle" )
			{
				if( normal.y < 0.4f )
				{
					//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
					//Move the player back so he does not float in the air in case he hit the log while landing after a jump.
					//controller.Move( hit.normal );
					playerControl.killPlayer ( DeathType.Obstacle );
				}
			}
			else if( collided.CompareTag("Chicken") )
			{
				Transform chickenTransform = collided.FindChild("Chicken Trigger");
				if( chickenTransform == null )
				{
					Debug.LogError("PlayerController-OnControllerColliderHit: chicken collision error. Could not find chicken trigger.");
					return;
				}

				ChickenController chickenController = chickenTransform.gameObject.GetComponent<ChickenController>();

				if( playerControl.getCharacterState() == PlayerCharacterState.Sliding )
				{
					//We can have multiple collisions. Only give stars on the first collision.
					//However, if the chicken falls on the road, we want the player to be able to collide with it a second time.
					if( Time.time - chickenController.timeWasHit > 0.3f )
					{
						//Save the fact that this chicken was hit so the player does not stumble a second time
						chickenController.wasHit = true;

						//Save the collision time to avoid multiple collision events on impact
						chickenController.timeWasHit = Time.time;

						giveCoins( 10 );

						//The faster the player runs, the further the chicken will fly
						float pushPower = playerRun.getRunSpeed() * 2.5f;

						//Make the chicken go flying
						Rigidbody body = collision.collider.attachedRigidbody;
						Vector3 force = new Vector3 (GetComponent<Rigidbody>().velocity.x, 7f, GetComponent<Rigidbody>().velocity.z) * pushPower;
						body.AddForceAtPosition(force, collision.contacts[0].point);
						collided.eulerAngles = new Vector3( 0, transform.eulerAngles.y + Random.Range(-7,8),0);

						//Play the animations
						Animation chickenAnimation = collided.GetComponent<Animation>();
						chickenAnimation.CrossFade("A_Chicken_Spawn");

						//Destroy the chicken after a while
						DestroyObject(collided.gameObject,10f);
					}
				}
				else
				{
					//Only make the player stumble the first time around
					if( !chickenController.wasHit )
					{
						playerControl.stumble();
					}
					else
					{
						//Allow the player to go right through the chicken
						collision.collider.attachedRigidbody.useGravity = false;
						//GetComponent<CapsuleCollider>().enabled = false;
					}
				}
			}
			else if (collided.name.StartsWith( "Stumble" ) )
			{
				playerControl.stumble();
			}
			else if (collided.CompareTag( "Zombie" ) )
			{
				ZombieController zombieController = (ZombieController) collided.GetComponent("ZombieController");
				//Ignore collision event if Zombie already dead.
				if( zombieController.getCreatureState() != CreatureState.Dying )
				{
					//You can't make a crawling zombie fall backwards
					if( ( playerControl.getCharacterState() == PlayerCharacterState.Sliding || playerControl.getCharacterState() == PlayerCharacterState.Turning_and_sliding
							|| playerSpell.isSpeedBoostActive ) && zombieController.getCreatureState() != CreatureState.Crawling )
					{

						giveCoins( ZombieManager.NUMBER_STARS_PER_ZOMBIE );

						zombieController.fallToBack();
						
					}
					else
					{
						Debug.Log( "Player collided with zombie: " + collided.name + " Normal" + normal.y + " but CANT TOPPLE HIM " + playerControl.getCharacterState() + "  STATE Z "+ zombieController.getCreatureState());
						if( normal.y < 0.4f )
						{
							//Player is running up Z axis
							if( Mathf.Floor( transform.eulerAngles.y ) == 0 )
							{
								if( Mathf.Abs( normal.x ) > Mathf.Abs( normal.z ) )
								{
									//Player collided with zombie on the side, just play an animation on the zombie
									zombieController.sideCollision();
								}
								else
								{
									//Player collided squarely with zombie. Kill the player.
									playerControl.killPlayer ( DeathType.Zombie );
									zombieController.victory( true );
								}
							}
							//Player is running along X axis
							else 
							{
								if( Mathf.Abs( normal.z ) > Mathf.Abs( normal.x ) )
								{
									//Player collided with zombie on the side, just play an animation on the zombie
									zombieController.sideCollision();
								}
								else
								{
									//Player collided squarely with zombie. Kill the player.
									playerControl.killPlayer ( DeathType.Zombie );
									zombieController.victory( true );
								}
							}
							Debug.Log( "PLayer collided with: " + collided.name + " Normal" + normal );
						}
						else
						{
							//We landed on the zombie's head
							playerControl.land ();
						}
					}
				}
				else
				{
					Debug.LogError("Zombie already dead");
				}
			}
			else if (collided.name.StartsWith( "Goblin" ) )
			{
				handleCreatureCollision( collision, collided.GetComponent<ICreature>() );
			}
			else if (collided.name.StartsWith( "Skeleton" ) )
			{
				handleCreatureCollision( collision, collided.GetComponent<ICreature>() );
			}
			else if (collided.name.StartsWith( "Firepit" ) )
			{
				Debug.Log( "Player collided with firepit: " + collided.name + " Normal" + normal.y );
				if( normal.y < 0.4f )
				{
					//Player is running up Z axis
					if( Mathf.Floor( transform.eulerAngles.y ) == 0 )
					{
						if( Mathf.Abs(normal.x ) > Mathf.Abs( normal.z ) )
						{
							//Player collided with firepit on the side. Ignore.
						}
						else
						{
							//Player collided squarely with firepit. Kill the player.
							playerControl.killPlayer ( DeathType.Obstacle);
						}
					}
					//Player is running along X axis
					else 
					{
						if( Mathf.Abs( normal.z ) > Mathf.Abs( normal.x ) )
						{
							//Player collided with firepit on the side. Ignore.
						}
						else
						{
							//Player collided squarely with firepit. Kill the player.
							playerControl.killPlayer ( DeathType.Obstacle);
						}
					}
				}
			}
			else if (collided.CompareTag( "Barrel" ) )
			{
				//Don't break if you land on top of the barrel
				if( normal.y < 0.4f )
				{
					Debug.Log( "PLayer collided with breakable: " + collided.name );
					BreakableObject bo = collided.GetComponent<BreakableObject>();
					if( bo != null )
					{
						//We pass the player collider to triggerBreak() because we do not want the barrel fragments to collide with the player.
						bo.triggerBreak( GetComponent<Collider>() );
						if( playerControl.getCharacterState() == PlayerCharacterState.Sliding )
						{
							giveCoins( 10 );
						}	
						else
						{
							playerControl.stumble();
						}
					}
				}
			}
			else if (collided.name.StartsWith( "Thrown Barrel" ) )
			{
				playerControl.killPlayer ( DeathType.Obstacle );
			}
			else if (collided.name.StartsWith( "Breakable Pumpkin" ) )
			{
				BreakableObject bo = (BreakableObject) collided.GetComponent("BreakableObject");
				Debug.Log( "PLayer collided with breakable: " + collided.name );
				//We pass the player collider to triggerBreak() because we do not want the barrel fragments to collide with the player.
				bo.triggerBreak( GetComponent<Collider>() );
				if( playerControl.getCharacterState() == PlayerCharacterState.Sliding )
				{
					giveCoins( 10 );	
				}	
				else
				{
					playerControl.stumble();
				}
			}
			else if (collided.CompareTag("Cart" ) )
			{
				if( normal.y < 0.4f )
				{
					playerControl.killPlayer ( DeathType.Obstacle );
				}
			}
			else if (collided.name == "Pendulum" )
			{
				//Move the player back so he does not get stuck in the pendulum.
				//controller.Move( hit.normal ); //disable test - seems to make Unity 5 crash
				playerControl.killPlayer ( DeathType.Obstacle );
			}
			else if (collided.CompareTag( "Cow" ) )
			{
				SimpleController simpleController = (SimpleController) collided.GetComponent("SimpleController");
				simpleController.playHitAnim();
				if( normal.y < 0.4f )
				{
					//Player collided with cow squarely
					//Move the player back so he does not get stuck in the cow.
					//controller.Move( hit.normal );
					playerControl.killPlayer ( DeathType.Obstacle );
				}
			}
			else if (collided.name.StartsWith("Fence") || collided.name.StartsWith("Wall") || collided.name.StartsWith("Portcullis") )
			{
				playerControl.killPlayer ( DeathType.Obstacle );
			}			
			else if (collided.name.Equals("Weapon") )
			{
				//Skeleton footman or warlord, or goblin piker or wraith or demon
				if( !playerSpell.isSpeedBoostActive )
				{
					playerControl.killPlayer ( DeathType.Obstacle );
				}
			}			
			/// Obstacle_F - the obstacle is impassable
			/// Obstacle_L - the obstacle's height is low. You can jump over it or land on it.
			/// Obstacle_M - the obstacle's height is medium. You can either slide under it or jump over it.
			/// Obstacle_H - the obstacle's height is high. You can slide under it.
			/// Obstacle_B - the obstacle is breakable. You can slide into it to break it or jump over it.
			/// Obstacle_DJ - the obstacle is tall. You can double-jump over it.
			else if (collided.CompareTag( "Obstacle_F" ) )
			{
				playerControl.killPlayer ( DeathType.Obstacle );
			}
			else if (collided.CompareTag( "Obstacle_L" ) )
			{
				if( normal.y < 0.4f )
				{
					//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
					playerControl.killPlayer ( DeathType.Obstacle );
				}
			}
			else if (collided.CompareTag( "Obstacle_M" ) )
			{
				if( normal.y < 0.4f )
				{
					//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
					playerControl.killPlayer ( DeathType.Obstacle );
				}
			}
			else if (collided.CompareTag( "Obstacle_S" ) )
			{
				if( normal.y < 0.4f )
				{
					//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
					playerControl.killPlayer ( DeathType.Obstacle );
				}
			}
			else if (collided.CompareTag( "Obstacle_H" ) )
			{
				if( normal.y < 0.4f )
				{
					//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
					playerControl.killPlayer ( DeathType.Obstacle );
				}
			}
			else if (collided.CompareTag( "Obstacle_DJ" ) )
			{
				if( normal.y < 0.4f )
				{
					//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
					playerControl.killPlayer ( DeathType.Obstacle );
				}
			}
			else if (collided.CompareTag( "Player" ) )
			{
				if( normal.y < 0.4f )
				{
					//If this player ran squarely into another player while using SpeedBoost (i.e. Raging Bull), kill the other player.
					if( playerSpell.isSpeedBoostActive )
					{
						PlayerControl otherPlayer = collided.GetComponent<PlayerControl>();
						if( otherPlayer.getCharacterState() != PlayerCharacterState.Dying || otherPlayer.getCharacterState() != PlayerCharacterState.Idle )
						{
							otherPlayer.killPlayer ( DeathType.FallForward );
						}
					}
				}
			}
			else if (collided.CompareTag( "Destructible" ) )
			{
				if( normal.y < 0.4f )
				{
					//This player ran squarely into a destructible object.
					if( playerSpell.isSpeedBoostActive )
					{
						//Speedboost is active. Fracture the object.
						FracturedObject fracturedObject = collided.GetComponent<FracturedObject>();
						if( fracturedObject != null )
						{
							fracturedObject.Explode(collision.contacts[0].point, 400f, 2.5f, true, true, false, false );
							GameObject.Destroy( collided.gameObject );
						}
						else
						{
							Debug.LogWarning( "Player collided with object " + collided.name + ". It has a Destructible tag, but no FractureObject component." );
						}
					}
					else
					{
						//Speedboost is not active. Kill the player.
						playerControl.killPlayer ( DeathType.Obstacle );
					}
				}
			}
		}
	}
	
	void handleCreatureCollision( Collision collision, ICreature creature )
	{
		//Ignore collision event if the creature is already dead.
		if( creature != null && creature.getCreatureState() != CreatureState.Dying )
		{
			Transform collided = collision.transform;
			Vector3 normal =  collision.contacts[0].normal;

			if( ( playerControl.getCharacterState() == PlayerCharacterState.Sliding || playerControl.getCharacterState() == PlayerCharacterState.Turning_and_sliding ) || playerSpell.isSpeedBoostActive )
			{
				giveCoins( CreatureManager.NUMBER_COINS_PER_CREATURE );

				creature.knockback();
				
			}
			else
			{
				if( normal.y < 0.4f )
				{
					//Player is running up Z axis
					if( Mathf.Floor( transform.eulerAngles.y ) == 0 )
					{
						if( Mathf.Abs( normal.x ) > Mathf.Abs( normal.z ) )
						{
							//Player collided with goblin on the side, just play an animation on the goblin
							creature.sideCollision();
						}
						else
						{
							//Player collided squarely with goblin. Kill the player.
							playerControl.killPlayer ( DeathType.Zombie );
							creature.victory( true );
						}
					}
					//Player is running along X axis
					else 
					{
						if( Mathf.Abs( normal.z ) > Mathf.Abs( normal.x ) )
						{
							//Player collided with zombie on the side, just play an animation on the zombie
							creature.sideCollision();
						}
						else
						{
							//Player collided squarely with zombie. Kill the player.
							playerControl.killPlayer ( DeathType.Zombie );
							creature.victory( true );
						}
					}
				}
				else
				{
					//We landed on the goblin's head
					playerControl.land ();
				}
			}
		}
	}

	void giveCoins( int amount )
	{
		if( this.photonView.isMine && GetComponent<PlayerAI>() == null && HUDHandler.hudHandler != null )
		{
			//Give coins
			PlayerStatsManager.Instance.modifyCurrentCoins( amount, true, false );
			
			//Display coin total picked up icon
			HUDHandler.hudHandler.displayCoinPickup( amount );
		}
	}
}
