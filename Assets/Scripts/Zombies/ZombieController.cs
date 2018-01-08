using UnityEngine;
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
		Any = 3 //Either walk or crawl
	}

	Animation legacyAnim;
	Vector3 forward;
	float walkSpeed = 2f; //good value so feet don't slide
	float runSpeed = 4f; //good value so feet don't slide
	public float movementSpeed;

	[SerializeField] List<string> walkTypes = new List<string>();
	[SerializeField] AudioClip moanLow;
	[SerializeField] AudioClip moanHigh;
	[SerializeField] AudioClip win;
	[Tooltip("The icon to use on the minimap. It is optional.")]
	[SerializeField] Sprite zombieIcon;
	[SerializeField] ParticleSystem spurtPrefab; //Particle fx that plays when a zombie burrows up
	public const int SCORE_PER_KNOCKBACK = 20; //coop - score points awarded per knockback.
	const float ZOMBIE_LIFESPAN = 30f;
	const float SHRINK_SIZE = 0.4f;
	public bool isInoffensive = false;
	[SerializeField] ParticleSystem confusedVFX;
	[SerializeField] Vector3 controllerCenterWhenCrawling = new Vector3( 0, 0.5f, 0.38f );
	[SerializeField] Vector3 controllerCenterOther = new Vector3( 0, 0.9f, 0f );
	[SerializeField] float controllerHeightWhenCrawling = 1.1f;
	[SerializeField] float controllerHeightOther = 1.8f;
	CapsuleCollider capsuleCollider;

	CreatureState previousCreatureState;

	// Use this for initialization
	new void Awake ()
	{
		base.Awake();
		creatureState = CreatureState.Idle;

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
		capsuleCollider = GetComponent<CapsuleCollider>();
	}

	public override void setCreatureState( CreatureState newState )
	{
		base.setCreatureState( newState );
		adjustCollider();
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
				burrowUp();
			break;

			case ZombieSpawnType.StandUpFromBack:
				//The zombie is lying flat on his back and gets up
				standUpFromBack();
			break;

			case ZombieSpawnType.Walk:
				//The zombie walks immediately
				walk();
			break;

			case ZombieSpawnType.Run:
				//The zombie runs immediately
				run();
			break;

			case ZombieSpawnType.Jump:
				//The zombie jumps up and down.
				jump();
			break;

			case ZombieSpawnType.Crawl:
				//The zombie crawls immediately
				crawl();
			break;

		}

		followsPlayer = (bool) data[1];

		//Register the zombie on the minimap.
		if( zombieIcon != null ) MiniMap.Instance.registerRadarObject( gameObject, zombieIcon );

		//Destroy zombie after ZOMBIE_LIFESPAN seconds
		GameObject.Destroy( gameObject, ZOMBIE_LIFESPAN ); 
	}

	void FixedUpdate ()
	{
		moveZombie();
	}

	void moveZombie()
	{
		if( creatureState == CreatureState.Walking || creatureState == CreatureState.Running || creatureState == CreatureState.Crawling )
		{
			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				targetPlayer();
			}
			//1) Get the direction of the zombie
			forward = transform.TransformDirection(Vector3.forward);			
			//2) Scale vector based on run speed
			forward = forward * Time.deltaTime * movementSpeed;
			//3) Move the zombie
			capsuleCollider.attachedRigidbody.velocity = forward * 25f;
		}
	}

	public void freeze( bool value )
	{
		if( value )
		{
			previousCreatureState = getCreatureState();
			setCreatureState( CreatureState.Immobilized );
			legacyAnim.enabled = false;
			capsuleCollider.attachedRigidbody.velocity = Vector3.zero;
			capsuleCollider.attachedRigidbody.isKinematic = true;		
		}
		else
		{
			setCreatureState( previousCreatureState );
			legacyAnim.enabled = true;
			capsuleCollider.attachedRigidbody.isKinematic = false;		
		}
	}

	public void stasis( bool value )
	{
		if( value )
		{
			setCreatureState( CreatureState.Immobilized );
			legacyAnim.CrossFade(selectRandomIdle(), 0.4f );
			capsuleCollider.attachedRigidbody.velocity = Vector3.zero;
			capsuleCollider.attachedRigidbody.isKinematic = true;		
		}
		else
		{
			legacyAnim.CrossFade("fallCycle", 0.3f );
			string walkType = selectRandomWalk( ZombieMoveType.Walking );
			legacyAnim.CrossFadeQueued(walkType, 0.4f);
			setCreatureState( CreatureState.Walking );
			capsuleCollider.attachedRigidbody.isKinematic = false;		
		}
	}

	public override void knockback( Transform attacker, bool grantPoints )
	{
		base.knockback( attacker, grantPoints );
	}

	//The creature falls over backwards and dies.
	[PunRPC]
	void knockbackRPC( int attackerPhotonViewID, bool grantPoints )
	{		
		if( getCreatureState() == CreatureState.Dying ) return; //Ignore. The creature is already dead.

		setCreatureState( CreatureState.Dying );
		if( zombieIcon != null ) MiniMap.Instance.removeRadarObject( gameObject );
		CancelInvoke( "groan" );
		legacyAnim.CrossFade("fallToBack", 0.25f);
		capsuleCollider.attachedRigidbody.isKinematic = true;
		capsuleCollider.enabled = false;
		audioSource.PlayOneShot( knockbackSound );

		if( grantPoints ) SkillBonusHandler.Instance.grantScoreBonus( SCORE_PER_KNOCKBACK, "COOP_SCORE_BONUS_TOPPLED_ZOMBIE", attackerPhotonViewID );
	}

	public void victory( bool playWinSound )
	{
		if( creatureState != CreatureState.Dying )
		{
			CancelInvoke( "groan" );
			capsuleCollider.attachedRigidbody.isKinematic = true;
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

	public override void shrink( Transform caster, bool value )
	{
		base.shrink( caster, value );
	}

	[PunRPC]
	void shrinkRPC( bool value, int attackerPhotonViewID )
	{		
		if( getCreatureState() == CreatureState.Dying ) return; //Ignore. The creature is already dead.
		if( value )
		{
			//Make their voice higher pitch, just for fun
			GetComponent<AudioSource>().pitch = 1.3f;
			//For now, once shrunk, they stay shrunk. The lifespan of a zombie is only 30 seconds anyway.
			LeanTween.scale( gameObject, new Vector3( SHRINK_SIZE, SHRINK_SIZE, SHRINK_SIZE ), 1f );
			isInoffensive = true;
		}
	}

	public override void confuse( Transform caster, bool value )
	{
		base.confuse( caster, value );
	}

	[PunRPC]
	void confuseRPC( bool value, int attackerPhotonViewID )
	{		
		if( getCreatureState() == CreatureState.Dying ) return; //Ignore. The creature is already dead.
		if( value )
		{
			setCreatureState(CreatureState.Immobilized);
			player = null;
			legacyAnim.CrossFade("no", 0.4f);
			legacyAnim.CrossFadeQueued(selectRandomIdle(), 0.4f);
			isInoffensive = true;
			if( confusedVFX != null ) confusedVFX.gameObject.SetActive( true );
		}
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
 
	public void burrowUp()
	{
		capsuleCollider.attachedRigidbody.useGravity = false;
		capsuleCollider.enabled = false;
		setCreatureState( CreatureState.BurrowUp );
		legacyAnim.Play("burrowUp");
		StartCoroutine("burrowUpCompleted");
		if( spurtPrefab != null )
		{
			ParticleSystem burrowUpEffect = Instantiate( spurtPrefab, transform.position, transform.rotation ) as ParticleSystem;
			burrowUpEffect.Play();
		}
	}
	
	public IEnumerator burrowUpCompleted( )
	{
		float duration = legacyAnim["burrowUp"].length;
		do
		{
			duration = duration - Time.deltaTime;
			yield return new WaitForFixedUpdate();  
		} while ( duration > 0 );

		capsuleCollider.enabled = true;
		capsuleCollider.attachedRigidbody.useGravity = true;
		string walkType = selectRandomWalk( ZombieMoveType.Walking );
		legacyAnim.CrossFade("funnierWalk");
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		setCreatureState( CreatureState.Walking );
		movementSpeed = walkSpeed;
	}

	public void standUpFromBack()
	{
		groan ();
		capsuleCollider.attachedRigidbody.useGravity = false;
		capsuleCollider.enabled = false;
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
		capsuleCollider.enabled = true;
		capsuleCollider.attachedRigidbody.useGravity = true;
		setCreatureState( CreatureState.Walking );
		movementSpeed = walkSpeed;
	}

	public void walk()
	{
		movementSpeed = walkSpeed;
		string walkType = selectRandomWalk( ZombieMoveType.Walking );
		legacyAnim.Play(walkType);
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		setCreatureState( CreatureState.Walking );
	}

	public void run()
	{
		movementSpeed = runSpeed;
		legacyAnim.Play("run");
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		setCreatureState( CreatureState.Running );
	}

	public void jump()
	{
		legacyAnim.Play("jump");
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		setCreatureState( CreatureState.Jumping );
	}

	public void crawl()
	{
		string walkType = selectRandomWalk( ZombieMoveType.Crawling );
		legacyAnim.Play(walkType);
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		setCreatureState( CreatureState.Crawling );

	}
		
	public void stop()
	{
		setCreatureState(CreatureState.Idle);
		legacyAnim.CrossFade( selectRandomIdle(), 0.3f );
	}

	void adjustCollider()
	{
		if( getCreatureState() == CreatureState.Crawling )
		{
			//Crawling
			capsuleCollider.direction = (int)Axis.Z;
			capsuleCollider.center = controllerCenterWhenCrawling;
			capsuleCollider.height = controllerHeightWhenCrawling;
		}
		else if( getCreatureState() == CreatureState.Dying )
		{
			//Dying
			capsuleCollider.direction = (int)Axis.Z;
			capsuleCollider.center = new Vector3( 0, 0.25f, -0.9f );
			capsuleCollider.radius = 0.25f;
		}
		else
		{
			//All other
			capsuleCollider.direction = (int)Axis.Y;
			capsuleCollider.center = controllerCenterOther;
			capsuleCollider.height = controllerHeightOther;
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if( collision.gameObject.CompareTag("Zombie") )
		{
			//If a zombie collides with another zombie have him halt.
			stop();
		}
		else if( collision.gameObject.CompareTag("Player") )
		{
			if( collision.gameObject.GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying )
			{
				//If a zombie collides with a dead player and he is not in the victory state, have him play victory also.
				if( getCreatureState() != CreatureState.Victory )
				{
					victory( false );
				}
			}
		}
	}

}
