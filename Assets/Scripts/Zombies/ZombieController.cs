using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieController : BaseClass {

	public enum ZombieState {
		Reserved = -1,
		Available = 0,
		Idle = 1,
		Walking = 2,
		Crawling = 3,
		Dying = 4,
		BurrowUp = 5,
		StandUpFromBack = 6,
		Victory = 7
	}

	public enum ZombieMoveType {
		Walking = 1,
		Crawling = 2,
		Any = 3, //Either walk or crawl
	}

	static Transform player;
	PlayerController playerController;
	public CharacterController controller;
	Animation anim;
	public Vector3 forward;
	float walkSpeed = 1.65f; //good value so feet don't slide
	public ZombieState zombieState = ZombieState.Available;
	public bool applyGravity = true;

	public List<string> walkTypes = new List<string>();
	public AudioClip moanLow;
	public AudioClip moanHigh;
	public AudioClip fallToGround;
	public AudioClip win;

	//For stats and achievements
	static EventCounter zombie_bowling = new EventCounter( GameCenterManager.ZombieBowling, 3, 3000 );

	//If true, the zombie heads for the player (as opposed to staying in its lane).
	public bool followsPlayer = false;

	//True if the CharacterController is allowed to move and false otherwise (because the game is paused for example).
	public bool allowMove = true;

	// Use this for initialization
	void Awake () {

		controller = (CharacterController) GetComponent("CharacterController");
		controller.enabled = false;
		Transform zombiePrefab;
		if( gameObject.name == "Zombie Boy" )
		{
			zombiePrefab = transform.FindChild("zombieBoy");
		}
		else
		{
			zombiePrefab = transform.FindChild("zombieGirl");
		}
		anim = (Animation) zombiePrefab.GetComponent("Animation");
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = (PlayerController) player.gameObject.GetComponent(typeof(PlayerController));

	}

	// Update is called once per frame
	void Update () {
		moveZombie();
	}

	public ZombieState getZombieState()
	{
		return zombieState;
	}

	public void setZombieState( ZombieState state )
	{
		zombieState = state;
	}

	public void resetZombie()
	{
		StopCoroutine("recycleZombie");
		CancelInvoke();
		setZombieState( ZombieController.ZombieState.Available );
		gameObject.SetActive( false );
		followsPlayer = false;
		controller.enabled = true;
	}

	void moveZombie()
	{
		if( allowMove && zombieState == ZombieState.Walking || zombieState == ZombieState.Crawling )
		{
			//0) Target the player but we only want the Y rotation
			if( followsPlayer )
			{
				transform.LookAt( player );
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

	public void knockbackZombie()
	{
		//zombie_bowling.incrementCounter();
		CancelInvoke( "groan" );
		setZombieState( ZombieController.ZombieState.Dying );
		controller.enabled = false;
		CapsuleCollider capsuleCollider = (CapsuleCollider) GetComponent("CapsuleCollider");
		capsuleCollider.enabled = false;
		anim.CrossFade("fallToBack", 0.25f);
		GetComponent<AudioSource>().PlayOneShot( fallToGround );
		Debug.Log("KNOCKBACK " + gameObject.name );

	}

	//The zombie falls over backwards, typically because the player slid into him.
	public void fallToBack()
	{
		zombie_bowling.incrementCounter();
		CancelInvoke( "groan" );
		setZombieState( ZombieController.ZombieState.Dying );
		controller.enabled = false;
		CapsuleCollider capsuleCollider = (CapsuleCollider) GetComponent("CapsuleCollider");
		capsuleCollider.enabled = false;
		anim.CrossFade("fallToBack");
		GetComponent<AudioSource>().PlayOneShot( fallToGround );
	}

	public void victory( bool playWinSound )
	{
		CancelInvoke( "groan" );
		if( playWinSound ) GetComponent<AudioSource>().PlayOneShot( win );
		if( zombieState == ZombieState.StandUpFromBack )
		{
			StopCoroutine("standUpFromBackCompleted");
			anim.CrossFadeQueued("happy");
			anim.CrossFadeQueued("happy");
			anim.CrossFadeQueued(selectRandomIdle());
		}
		else if( zombieState == ZombieState.BurrowUp )
		{
			StopCoroutine("burrowUpCompleted");
			anim.CrossFadeQueued("danceThriller");
			anim.CrossFadeQueued("danceThriller");
			anim.CrossFadeQueued(selectRandomIdle());
		}
		else if( zombieState == ZombieController.ZombieState.Crawling )
		{
			setZombieState( ZombieController.ZombieState.Victory );
			//Zombie was crawling
			anim.CrossFade("crouch_eat1");
			anim.CrossFadeQueued("crouch");
		}
		else
		{
			//Zombie was standing up
			setZombieState( ZombieController.ZombieState.Victory );
			if( Random.value < 0.5f )
			{
				anim.CrossFade("happy");
				anim.CrossFadeQueued("happy");
				anim.CrossFadeQueued(selectRandomIdle());
			}
			else
			{
				anim.CrossFade("danceThriller");
				anim.CrossFadeQueued("danceThriller");
				anim.CrossFadeQueued(selectRandomIdle());
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
		GetComponent<AudioSource>().PlayOneShot( moanLow );
		anim.CrossFade("hit2");
		anim.CrossFadeQueued(selectRandomWalk( ZombieMoveType.Walking ));
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
				GetComponent<AudioSource>().PlayOneShot( moanLow );
			}
			else
			{
				GetComponent<AudioSource>().PlayOneShot( moanHigh );
			}
		}
	}
 
	public void burrowUp( ParticleSystem debris )
	{
		controller.enabled = false;
		setZombieState( ZombieController.ZombieState.BurrowUp );
		anim.Play("burrowUp");
		StartCoroutine("burrowUpCompleted");
		debris = (ParticleSystem)Instantiate(debris, transform.position, transform.rotation );
		Destroy ( debris, 4f );
		debris.Play ();

	}
	
	public IEnumerator burrowUpCompleted( )
	{
		float duration = anim["burrowUp"].length;
		do
		{
			duration = duration - Time.deltaTime;
			yield return _sync();
		} while ( duration > 0 );

		controller.enabled = true;
		string walkType = selectRandomWalk( ZombieMoveType.Walking );
		anim.CrossFade(walkType);
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		if( walkType == "crouchMove" || walkType == "crawl" )
		{
			setZombieState( ZombieState.Crawling );
		}
		else
		{
			setZombieState( ZombieState.Walking );
		}
	}

	public void standUpFromBack()
	{
		groan ();
		controller.enabled = false;
		setZombieState( ZombieController.ZombieState.StandUpFromBack );
		anim.Play("standUpFromBack");
		StartCoroutine("standUpFromBackCompleted");
	}

	public IEnumerator standUpFromBackCompleted()
	{
		float duration = anim["standUpFromBack"].length;
		do
		{
			duration = duration - Time.deltaTime;
			yield return _sync();
		} while ( duration > 0 );

		string walkType = selectRandomWalk( ZombieMoveType.Walking );
		anim.CrossFade(walkType);
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		controller.enabled = true;
		setZombieState( ZombieState.Walking );
	}

	public void walk()
	{
		controller.enabled = true;
		string walkType = selectRandomWalk( ZombieMoveType.Walking );
		anim.Play(walkType);
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		setZombieState( ZombieController.ZombieState.Walking );
	}

	public void crawl()
	{
		controller.enabled = true;
		string walkType = selectRandomWalk( ZombieMoveType.Crawling );
		anim.Play(walkType);
		InvokeRepeating( "groan", Random.Range( 0.1f, 4f), 8f );
		setZombieState( ZombieController.ZombieState.Crawling );

	}

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
		GameManager.gameStateEvent += GameStateChange;
	}

	void OnDisable()
	{
		CancelInvoke( "groan" );
		PlayerController.playerStateChanged -= PlayerStateChange;
		GameManager.gameStateEvent -= GameStateChange;
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			CancelInvoke( "groan" );
		}
	}
	
	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Paused )
		{
			anim.enabled = false;
			allowMove = false;
			controller.enabled = false;
			
		}
		else if( newState == GameState.Normal )
		{
			anim.enabled = true;
			allowMove = true;
			controller.enabled = true;
		}
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if( PlayerController._characterState == CharacterState.Dying )
		{
			if( hit.collider.name.StartsWith("Zombie") )
			{
				//If a zombie collides with another zombie while the player is dead, have him stop moving and play the victory sequence.
				victory( false );
			}
		}
	}

	//Recycle zombie after a few seconds
	public IEnumerator recycleZombie( float recycleDelay )
	{
		float elapsedTime = 0;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			yield return _sync();
		} while ( elapsedTime < recycleDelay );
		
		if( playerController.getCharacterState() != CharacterState.Dying )
		{
			//Only deactivate the zombie if the player is not dead as we dont want the zombie to pop out of view.
			setZombieState( ZombieController.ZombieState.Available );
			gameObject.SetActive( false );
		}
	}

}
