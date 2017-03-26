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
/// </summary>
public class PlayerCollisions : Photon.PunBehaviour {

	PlayerControl playerControl;
	PlayerSounds playerSounds;
	#region Ground type variables
	string groundType = "Floor"; //Other choices are Water and Collapsing
	string previousGroundType = "Floor"; //Other choices are Water and Collapsing
	#endregion

	// Use this for initialization
	void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();	
		playerSounds = GetComponent<PlayerSounds>();	
	}

	/// <summary>
	/// Gets the type of the ground. Either normal (untagged), water or collapsing.
	/// </summary>
	/// <returns>The ground type.</returns>
	public string getGroundType()
	{
		return groundType;
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit )
	{
		if( playerControl.getCharacterState() != PlayerCharacterState.Dying )
		{
			groundType = hit.gameObject.tag;
			if( groundType != previousGroundType )
			{
				playerSounds.groundTypeChanged( groundType );
			}
			previousGroundType = groundType;

			//The CharacterController is constantly colliding with the Quads making up the floor. Ignore those events.
			if (hit.gameObject.CompareTag("Floor") ) return;
	
			//Debug.Log ("OnControllerColliderHit  " + hit.collider.name  );
			if (hit.collider.name == "DeadTree" )
			{
				if( hit.normal.y < 0.4f )
				{
					//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
					//Move the player back so he does not float in the air in case he hit the log while landing after a jump.
					//controller.Move( hit.normal );
					playerControl.managePlayerDeath ( DeathType.Obstacle );
				}
			}
			else if (hit.collider.name == "GroundObstacle" )
			{
				if( hit.normal.y < 0.4f )
				{
					//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
					//Move the player back so he does not float in the air in case he hit the log while landing after a jump.
					//controller.Move( hit.normal );
					playerControl.managePlayerDeath ( DeathType.Obstacle );
				}
			}
			else if( hit.gameObject.CompareTag("Chicken") )
			{
				Transform chickenTransform = hit.transform.FindChild("Chicken Trigger");
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
						float pushPower = playerControl.getSpeed() * 2.5f;

						//Make the chicken go flying
						Rigidbody body = hit.collider.attachedRigidbody;
						Vector3 force = new Vector3 (hit.controller.velocity.x, 7f, hit.controller.velocity.z) * pushPower;
						body.AddForceAtPosition(force, hit.point);
						hit.transform.eulerAngles = new Vector3( 0, transform.eulerAngles.y + Random.Range(-7,8),0);

						//Play the animations
						Animation chickenAnimation = hit.gameObject.GetComponent<Animation>();
						chickenAnimation.CrossFade("A_Chicken_Spawn");

						//Destroy the chicken after a while
						DestroyObject(hit.gameObject,10f);
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
						hit.collider.attachedRigidbody.useGravity = false;
						hit.collider.enabled = false;
					}
				}
			}
			else if (hit.collider.name.StartsWith( "Stumble" ) )
			{
				playerControl.stumble();
			}
			else if (hit.gameObject.CompareTag( "Zombie" ) )
			{
				ZombieController zombieController = (ZombieController) hit.gameObject.GetComponent("ZombieController");
				//Ignore collision event if Zombie already dead.
				if( zombieController.getCreatureState() != CreatureState.Dying )
				{
					//You can't make a crawling zombie fall backwards
					if( ( playerControl.getCharacterState() == PlayerCharacterState.Sliding || playerControl.getCharacterState() == PlayerCharacterState.Turning_and_sliding
							|| playerControl.isSpeedBoostActive ) && zombieController.getCreatureState() != CreatureState.Crawling )
					{

						giveCoins( ZombieManager.NUMBER_STARS_PER_ZOMBIE );

						zombieController.fallToBack();
						
					}
					else
					{
						Debug.Log( "Player collided with zombie: " + hit.collider.name + " Normal" + hit.normal.y + " but CANT TOPPLE HIM " + playerControl.getCharacterState() + "  STATE Z "+ zombieController.getCreatureState());
						if( hit.normal.y < 0.4f )
						{
							//Player is running up Z axis
							if( Mathf.Floor( transform.eulerAngles.y ) == 0 )
							{
								if( Mathf.Abs( hit.normal.x ) > Mathf.Abs( hit.normal.z ) )
								{
									//Player collided with zombie on the side, just play an animation on the zombie
									zombieController.sideCollision();
								}
								else
								{
									//Player collided squarely with zombie. Kill the player.
									playerControl.managePlayerDeath ( DeathType.Zombie );
									zombieController.victory( true );
								}
							}
							//Player is running along X axis
							else 
							{
								if( Mathf.Abs( hit.normal.z ) > Mathf.Abs( hit.normal.x ) )
								{
									//Player collided with zombie on the side, just play an animation on the zombie
									zombieController.sideCollision();
								}
								else
								{
									//Player collided squarely with zombie. Kill the player.
									playerControl.managePlayerDeath ( DeathType.Zombie );
									zombieController.victory( true );
								}
							}
							Debug.Log( "PLayer collided with: " + hit.collider.name + " Normal" + hit.normal );
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
			else if (hit.collider.name.StartsWith( "Goblin" ) )
			{
				handleCreatureCollision( hit, hit.gameObject.GetComponent<ICreature>() );
			}
			else if (hit.collider.name.StartsWith( "Skeleton" ) )
			{
				handleCreatureCollision( hit, hit.gameObject.GetComponent<ICreature>() );
			}
			else if (hit.collider.name.StartsWith( "Firepit" ) )
			{
				Debug.Log( "Player collided with firepit: " + hit.collider.name + " Normal" + hit.normal.y );
				if( hit.normal.y < 0.4f )
				{
					//Player is running up Z axis
					if( Mathf.Floor( transform.eulerAngles.y ) == 0 )
					{
						if( Mathf.Abs( hit.normal.x ) > Mathf.Abs( hit.normal.z ) )
						{
							//Player collided with firepit on the side. Ignore.
						}
						else
						{
							//Player collided squarely with firepit. Kill the player.
							playerControl.managePlayerDeath ( DeathType.Obstacle);
						}
					}
					//Player is running along X axis
					else 
					{
						if( Mathf.Abs( hit.normal.z ) > Mathf.Abs( hit.normal.x ) )
						{
							//Player collided with firepit on the side. Ignore.
						}
						else
						{
							//Player collided squarely with firepit. Kill the player.
							playerControl.managePlayerDeath ( DeathType.Obstacle);
						}
					}
				}
			}
			else if (hit.collider.name.StartsWith( "Breakable Barrel" ) )
			{
				//Don't break if you land on top of the barrel
				if( hit.normal.y < 0.4f )
				{
					BreakableObject bo = (BreakableObject) hit.collider.GetComponent("BreakableObject");
					Debug.Log( "PLayer collided with breakable: " + hit.collider.name );
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
			else if (hit.collider.name.StartsWith( "Thrown Barrel" ) )
			{
				playerControl.managePlayerDeath ( DeathType.Obstacle );
			}
			else if (hit.collider.name.StartsWith( "Breakable Pumpkin" ) )
			{
				BreakableObject bo = (BreakableObject) hit.collider.GetComponent("BreakableObject");
				Debug.Log( "PLayer collided with breakable: " + hit.collider.name );
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
			else if (hit.collider.CompareTag("Cart" ) )
			{
				if( hit.normal.y < 0.4f )
				{
					playerControl.managePlayerDeath ( DeathType.Obstacle );
				}
			}
			else if (hit.collider.name == "Pendulum" )
			{
				//Move the player back so he does not get stuck in the pendulum.
				//controller.Move( hit.normal ); //disable test - seems to make Unity 5 crash
				playerControl.managePlayerDeath ( DeathType.Obstacle );
			}
			else if (hit.collider.CompareTag( "Cow" ) )
			{
				SimpleController simpleController = (SimpleController) hit.collider.GetComponent("SimpleController");
				simpleController.playHitAnim();
				if( hit.normal.y < 0.4f )
				{
					//Player collided with cow squarely
					//Move the player back so he does not get stuck in the cow.
					//controller.Move( hit.normal );
					playerControl.managePlayerDeath ( DeathType.Obstacle );
				}
			}
			else if (hit.collider.name.StartsWith("Fence") || hit.collider.name.StartsWith("Wall") || hit.collider.name.StartsWith("Portcullis") )
			{
				playerControl.managePlayerDeath ( DeathType.Obstacle );
			}			
			else if (hit.collider.CompareTag( "Obstacle_F" ) )
			{
				playerControl.managePlayerDeath ( DeathType.Obstacle );
			}			
			else if (hit.collider.name.Equals("Weapon") )
			{
				//Skeleton footman or warlord, or goblin piker or wraith or demon
				if( !playerControl.isSpeedBoostActive )
				{
					playerControl.managePlayerDeath ( DeathType.Obstacle );
				}
			}			
		}
	}
	
	void handleCreatureCollision( ControllerColliderHit hit, ICreature creature )
	{
		//Ignore collision event if the creature is already dead.
		if( creature != null && creature.getCreatureState() != CreatureState.Dying )
		{
			if( ( playerControl.getCharacterState() == PlayerCharacterState.Sliding || playerControl.getCharacterState() == PlayerCharacterState.Turning_and_sliding ) || playerControl.isSpeedBoostActive )
			{
				giveCoins( CreatureManager.NUMBER_COINS_PER_CREATURE );

				creature.knockback();
				
			}
			else
			{
				if( hit.normal.y < 0.4f )
				{
					//Player is running up Z axis
					if( Mathf.Floor( transform.eulerAngles.y ) == 0 )
					{
						if( Mathf.Abs( hit.normal.x ) > Mathf.Abs( hit.normal.z ) )
						{
							//Player collided with goblin on the side, just play an animation on the goblin
							creature.sideCollision();
						}
						else
						{
							//Player collided squarely with goblin. Kill the player.
							playerControl.managePlayerDeath ( DeathType.Zombie );
							creature.victory( true );
						}
					}
					//Player is running along X axis
					else 
					{
						if( Mathf.Abs( hit.normal.z ) > Mathf.Abs( hit.normal.x ) )
						{
							//Player collided with zombie on the side, just play an animation on the zombie
							creature.sideCollision();
						}
						else
						{
							//Player collided squarely with zombie. Kill the player.
							playerControl.managePlayerDeath ( DeathType.Zombie );
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
		if( this.photonView.isMine && GetComponent<PlayerAI>() == null )
		{
			//Give coins
			PlayerStatsManager.Instance.modifyCurrentCoins( amount, true, false );
			
			//Display coin total picked up icon
			HUDHandler.hudHandler.displayCoinPickup( amount );
		}
	}
}
