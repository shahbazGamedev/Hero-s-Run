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
	PlayerSpell playerSpell;
	PlayerRun playerRun;
	PlayerAI playerAI;
	#region Ground type variables
	string groundType = "Floor"; //Other choices are Water and Collapsing
	string previousGroundType = "Floor"; //Other choices are Water and Collapsing
	#endregion
	const float RAGING_BULL_BASE_DAMAGE = 25;
	const float SIDE_COLLISION_SENSITIVITY = 0.4f; 

	// Use this for initialization
	void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();	
		playerSounds = GetComponent<PlayerSounds>();	
		playerSpell = GetComponent<PlayerSpell>();	
		playerRun = GetComponent<PlayerRun>();	
		playerAI = GetComponent<PlayerAI>();	
	}

	/// <summary>
	/// Gets the type of the ground. Either normal (untagged), water or collapsing.
	/// </summary>
	/// <returns>The ground type.</returns>
	public string getGroundType()
	{
		return groundType;
	}
	
	/// Stumble - the obstacle makes you stumble.
	/// Obstacle_L - the obstacle's height is low. You can jump over it or land on it.
	/// Obstacle_M - the obstacle's height is medium. You can either slide under it or jump over it.
	/// Obstacle_H - the obstacle's height is high. You can slide under it.
	/// Obstacle_B - the obstacle is breakable. You can slide into it to break it or jump over it.
	/// Obstacle_DJ - the obstacle is tall. You can double-jump over it.
	/// Player - if a player runs into another player while using Raging Bull, he kills the other player.
	/// Destructible - if a player runs into a destructible object while using Raging Bull, he destroys the object (which needs to have a FracturedObject component).
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

		    switch (collided.tag)
			{
		        case "Stumble":
					playerControl.stumble();
					break;
		        case "Obstacle_F":
					playerControl.killPlayer ( DeathType.Obstacle );
					break;
		        case "Obstacle_L":
		        case "Obstacle_M":
		        case "Obstacle_S":
		        case "Obstacle_H":
		        case "Obstacle_DJ":
					if( normal.y < 0.4f )
					{
						//If the Y component of the hit normal is too small, assume that the player hit the obstacle squarely and should die.
						playerControl.killPlayer ( DeathType.Obstacle );
					}
					break;
		        case "Player":
					if( normal.y < 0.4f )
					{
						//If this player ran squarely into another player while using SpeedBoost (i.e. Raging Bull), damage the other player.
						if( playerSpell.isRagingBullActive )
						{
							PlayerControl otherPlayer = collided.GetComponent<PlayerControl>();

							//Important: OnCollisionEnter may be called multiple times.
							//To avoid having the health deducted more than once, since we know that the player that was hit will stumble, we only pursue
							//if the player is not already stumbling.
							if( otherPlayer.getCharacterState() != PlayerCharacterState.Dying && otherPlayer.getCharacterState() != PlayerCharacterState.Idle && otherPlayer.getCharacterState() != PlayerCharacterState.Stumbling )
							{
								//The damage done increases the faster the player is running.
								float levelRunStartSpeed = playerRun.getLevelRunStartSpeed();
								float runSpeed = playerRun.getRunSpeed();
								float damageMultiplier = runSpeed/levelRunStartSpeed;
								
								//Note that the maximum multiplier is: PlayerRun.MAX_OVERALL_SPEED_MULTIPLIER which is currently set to 1.7.
								//So the damage will vary between RAGING_BULL_BASE_DAMAGE and MAX_OVERALL_SPEED_MULTIPLIER * RAGING_BULL_BASE_DAMAGE
								//Using current numbers, that is between 25 and 42.5.
								otherPlayer.GetComponent<PlayerHealth>().deductHealth( (int) (RAGING_BULL_BASE_DAMAGE * damageMultiplier), playerControl );
								otherPlayer.stumble();
								if( GetComponent<PhotonView>().isMine && GetComponent<PlayerAI>() == null )
								{
									//Grant skill bonus
									SkillBonusHandler.Instance.addSkillBonus( 25, "SKILL_BONUS_RAGING_BULL" );
								}			
							}
						}
					}
					break;
		        case "Destructible":
					if( normal.y < 0.4f )
					{
						//This player ran squarely into a destructible object.
						if( playerSpell.isRagingBullActive )
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
					break;

		        case "Zombie":
					handleZombieCollision( collided, normal );
					break;
			}
		}
	}

	void handleZombieCollision( Transform collided, Vector3 normal )
	{
		ZombieController zombieController = collided.GetComponent<ZombieController>();
		//Ignore collision event if Zombie already dead.
		if( zombieController.getCreatureState() != CreatureState.Dying )
		{
			//You can't make a crawling zombie fall backwards
			if( ( playerControl.getCharacterState() == PlayerCharacterState.Sliding || playerControl.getCharacterState() == PlayerCharacterState.Turning_and_sliding || playerSpell.isRagingBullActive ) && zombieController.getCreatureState() != CreatureState.Crawling )
			{
				zombieController.knockback( transform );	
			}
			else
			{
				//Debug.Log( name + " collided with zombie: " + collided.name + " Normal: " + normal );
				if( normal.y < 0.4f )
				{
					//If normal.y < 0.4f it means the player didn't land on the zombie's head.

					//Player is running up Z axis
					if( Mathf.Floor( transform.eulerAngles.y ) == 0 )
					{
						if( Mathf.Abs( normal.x ) > ( Mathf.Abs( normal.z ) + SIDE_COLLISION_SENSITIVITY ) )
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
						if( Mathf.Abs( normal.z ) > ( Mathf.Abs( normal.x ) + SIDE_COLLISION_SENSITIVITY ) )
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
				}
			}
		}
	}

	void addSkillBonus( int skillPoints, string skillTextID )
	{
		if( photonView.isMine && playerAI == null )
		{
			SkillBonusHandler.Instance.addSkillBonus( skillPoints, skillTextID );
		}
	}

}
