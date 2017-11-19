﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Zombie controller.
/// A zombie prefab needs to have a "Zombie" tag and a "Creature" layer.
/// </summary>
public sealed class ZombieController : Creature, ICreature {


	public enum ZombieMoveType {
		Walking = 1,
		Crawling = 2,
		Any = 3, //Either walk or crawl
	}

	Animation legacyAnim;
	public Vector3 forward;
	float walkSpeed = 1.65f; //good value so feet don't slide
	public bool applyGravity = true;

	public List<string> walkTypes = new List<string>();
	public AudioClip moanLow;
	public AudioClip moanHigh;
	public AudioClip win;
	[Tooltip("The icon to use on the minimap. It is optional.")]
	public Sprite zombieIcon;
	public ParticleSystem debris; //Particle fx that plays when a zombie burrows up

	// Use this for initialization
	new void Awake ()
	{
		base.Awake();
		creatureState = CreatureState.Available;
		controller.enabled = false;

		GameObject zombieManagerObject = GameObject.FindGameObjectWithTag("Zombie Manager");
		ZombieManager zombieManager = zombieManagerObject.GetComponent<ZombieManager>();

		Transform zombiePrefab;
		Material zombieMaterial;
		if( gameObject.name == "Zombie Boy(Clone)" )
		{
			zombiePrefab = transform.Find("zombieBoy");
			zombieMaterial = zombieManager.getRandomZombieMaterial( Sex.MALE );
		}
		else
		{
			zombiePrefab = transform.Find("zombieGirl");
			zombieMaterial = zombieManager.getRandomZombieMaterial( Sex.FEMALE );
		}
		if( zombieMaterial != null )
		{
			zombiePrefab.GetComponentInChildren<SkinnedMeshRenderer>().material = zombieMaterial;
		}
		legacyAnim = zombiePrefab.GetComponent<Animation>();
	}

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		activate();
	}

	void activate()
	{
		object[] data = gameObject.GetPhotonView ().instantiationData;

		ZombieSpawnType spawnType = (ZombieSpawnType) data[0];
		switch( spawnType )
		{
			case ZombieSpawnType.BurrowUp:
				//Make the zombie burrow out of the ground
				burrowUp( debris );
			break;

			case ZombieSpawnType.StandUpFromBack:
				//The zombie is lying flat on his back and gets up
				standUpFromBack();
			break;

			case ZombieSpawnType.Walk:
				//The zombie walks immediately
				walk();
			break;

			case ZombieSpawnType.Crawl:
				//The zombie crawls immediately
				crawl();
			break;

		}

		followsPlayer = (bool) data[1];

		//Register the zombie on the minimap.
		if( zombieIcon != null ) MiniMap.Instance.registerRadarObject( gameObject, zombieIcon );
	}

	// Update is called once per frame
	void Update ()
	{
		moveZombie();
	}

	public new void resetCreature()
	{
		StopCoroutine("recycleZombie");
		CancelInvoke();
		setCreatureState( CreatureState.Available );
		gameObject.SetActive( false );
		followsPlayer = false;
		if( controller != null )controller.enabled = true;
	}

	void moveZombie()
	{
		if( creatureState == CreatureState.Walking || creatureState == CreatureState.Crawling )
		{
			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				transform.LookAt( getPlayer() );
				transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, 0 );
			}
			//1) Get the direction of the zombie
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			forward = forward * Time.deltaTime * walkSpeed;
			if( applyGravity ) forward.y -= 16f * Time.deltaTime;
			//3) Move the controller
			controller.Move( forward );
		}

	}

	public new void knockback()
	{
		base.knockback();
		if( zombieIcon != null ) MiniMap.Instance.removeRadarObject( gameObject );
		CancelInvoke( "groan" );
		legacyAnim.CrossFade("fallToBack", 0.25f);

	}

	//The zombie falls over backwards, typically because the player slid into him.
	public void fallToBack()
	{
		knockback();
	}

	public void victory( bool playWinSound )
	{
		if( creatureState != CreatureState.Dying )
		{
			CancelInvoke( "groan" );
			if( playWinSound ) audioSource.PlayOneShot( win );
			if( creatureState == CreatureState.StandUpFromBack )
			{
				StopCoroutine("standUpFromBackCompleted");
				legacyAnim.CrossFadeQueued("happy");
				legacyAnim.CrossFadeQueued("happy");
				legacyAnim.CrossFadeQueued(selectRandomIdle());
			}
			else if( creatureState == CreatureState.BurrowUp )
			{
				StopCoroutine("burrowUpCompleted");
				legacyAnim.CrossFadeQueued("danceThriller");
				legacyAnim.CrossFadeQueued("danceThriller");
				legacyAnim.CrossFadeQueued(selectRandomIdle());
			}
			else if( creatureState == CreatureState.Crawling )
			{
				setCreatureState( CreatureState.Victory );
				//Zombie was crawling
				legacyAnim.CrossFade("crouch_eat1");
				legacyAnim.CrossFadeQueued("crouch");
			}
			else
			{
				//Zombie was standing up
				setCreatureState( CreatureState.Victory );
				if( Random.value < 0.5f )
				{
					legacyAnim.CrossFade("happy");
					legacyAnim.CrossFadeQueued("happy");
					legacyAnim.CrossFadeQueued(selectRandomIdle());
				}
				else
				{
					legacyAnim.CrossFade("danceThriller");
					legacyAnim.CrossFadeQueued("danceThriller");
					legacyAnim.CrossFadeQueued(selectRandomIdle());
				}
			}
		}
	}

	string selectRandomIdle()
	{
		int rd = Random.Range(0,3);
		if( rd == 0 ) return "idle_normal";
		else if( rd == 1 ) return "idle_lookAround";
		else if( rd == 2 ) return "idle_scratchHead";
		else return "idle_normal";
	}

	public void sideCollision()
	{
		audioSource.PlayOneShot( moanLow );
		legacyAnim.CrossFade("hit2");
		legacyAnim.CrossFadeQueued(selectRandomWalk( ZombieMoveType.Walking ));
	}

	string selectRandomWalk( ZombieMoveType zmt )
	{
		//In the array, index 0 to 3 are walk types and 4, 5 are crawl types
		int rd;
		if( zmt == ZombieMoveType.Walking )
		{
			rd = Random.Range( 0, 4 );
		}
		else if( zmt == ZombieMoveType.Crawling )
		{
			rd = Random.Range( 4, 6 );
		}
		else
		{
			rd = Random.Range( 0, 6 );
		}

		return walkTypes[rd];
	}
	
	void groan()
	{
		if( gameObject.activeSelf )
		{
			float rd = Random.Range( 0, 1f );
			if( rd < 0.5f )
			{
				audioSource.PlayOneShot( moanLow );
			}
			else
			{
				audioSource.PlayOneShot( moanHigh );
			}
		}
	}
 
	public void burrowUp( ParticleSystem debris )
	{
		controller.enabled = false;
		setCreatureState( CreatureState.BurrowUp );
		legacyAnim.Play("burrowUp");
		StartCoroutine("burrowUpCompleted");
		debris = (ParticleSystem)Instantiate(debris, transform.position, transform.rotation );
		Destroy ( debris, 4f );
		debris.Play ();

	}
	
	public IEnumerator burrowUpCompleted( )
	{
		float duration = legacyAnim["burrowUp"].length;
		do
		{
			duration = duration - Time.deltaTime;
			yield return new WaitForFixedUpdate();  
		} while ( duration > 0 );

		controller.enabled = true;
		string walkType = selectRandomWalk( ZombieMoveType.Walking );
		legacyAnim.CrossFade(walkType);
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		if( walkType == "crouchMove" || walkType == "crawl" )
		{
			setCreatureState( CreatureState.Crawling );
		}
		else
		{
			setCreatureState( CreatureState.Walking );
		}
	}

	public void standUpFromBack()
	{
		groan ();
		controller.enabled = false;
		setCreatureState( CreatureState.StandUpFromBack );
		legacyAnim.Play("standUpFromBack");
		StartCoroutine("standUpFromBackCompleted");
	}

	public IEnumerator standUpFromBackCompleted()
	{
		float duration = legacyAnim["standUpFromBack"].length;
		do
		{
			duration = duration - Time.deltaTime;
			yield return new WaitForFixedUpdate();  
		} while ( duration > 0 );

		string walkType = selectRandomWalk( ZombieMoveType.Walking );
		legacyAnim.CrossFade(walkType);
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		controller.enabled = true;
		setCreatureState( CreatureState.Walking );
	}

	public void walk()
	{
		controller.enabled = true;
		string walkType = selectRandomWalk( ZombieMoveType.Walking );
		legacyAnim.Play(walkType);
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		setCreatureState( CreatureState.Walking );
	}

	public void crawl()
	{
		controller.enabled = true;
		string walkType = selectRandomWalk( ZombieMoveType.Crawling );
		legacyAnim.Play(walkType);
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		setCreatureState( CreatureState.Crawling );

	}

	void OnEnable()
	{
		PlayerControl.multiplayerStateChanged += MultiplayerStateChanged;
	}

	void OnDisable()
	{
		CancelInvoke( "groan" );
		PlayerControl.multiplayerStateChanged -= MultiplayerStateChanged;
	}

	void MultiplayerStateChanged( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			CancelInvoke( "groan" );
		}
	}
		
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//if( getPlayerController().getCharacterState() == PlayerCharacterState.Dying )
		//{
			if( hit.gameObject.CompareTag("Zombie") )
			{
				//If a zombie collides with another zombie while the player is dead, have him stop moving and play the victory sequence.
				victory( false );
			}
		//}
	}

	//Recycle zombie after a few seconds
	public IEnumerator recycleZombie( float recycleDelay )
	{
		float elapsedTime = 0;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			yield return new WaitForFixedUpdate();  
		} while ( elapsedTime < recycleDelay );
		
		if( getPlayerController().getCharacterState() != PlayerCharacterState.Dying )
		{
			//Only deactivate the zombie if the player is not dead as we dont want the zombie to pop out of view.
			setCreatureState( CreatureState.Available );
			gameObject.SetActive( false );
		}
	}

}
