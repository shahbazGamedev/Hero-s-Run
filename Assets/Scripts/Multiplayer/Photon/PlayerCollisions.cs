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
	#region Ground type variables
	string groundType = "Floor"; //Other choices are Water and Collapsing
	string previousGroundType = "Floor"; //Other choices are Water and Collapsing
	#endregion

	// Use this for initialization
	void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();	
		playerSounds = GetComponent<PlayerSounds>();	
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

			//Debug.Log ("OnControllerColliderHit  " + hit.collider.name  );

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
						//If this player ran squarely into another player while using SpeedBoost (i.e. Raging Bull), kill the other player.
						if( playerSpell.isRagingBullActive )
						{
							PlayerControl otherPlayer = collided.GetComponent<PlayerControl>();
							if( otherPlayer.getCharacterState() != PlayerCharacterState.Dying || otherPlayer.getCharacterState() != PlayerCharacterState.Idle )
							{
								otherPlayer.killPlayer ( DeathType.FallForward );
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
			}
		}
	}
}
