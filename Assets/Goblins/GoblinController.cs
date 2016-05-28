using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoblinController : BaseClass {

	public enum GoblinState {
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

	public enum GoblinMoveType {
		Walking = 1,
		Crawling = 2,
		Any = 3, //Either walk or crawl
	}

	public static int NUMBER_STARS_PER_GOBLIN = 20;

	static Transform player;
	PlayerController playerController;
	public CharacterController controller;
	public Vector3 forward;
	float walkSpeed = 1.65f; //good value so feet don't slide
	public GoblinState goblinState = GoblinState.Available;
	public bool applyGravity = true;

	public List<string> walkTypes = new List<string>();
	public AudioClip moanLow;
	public AudioClip moanHigh;
	public AudioClip fallToGround;
	public AudioClip win;

	public GameObject clothItem1;
	public GameObject clothItem2;

	//For stats and achievements
	static EventCounter goblin_bowling = new EventCounter( GameCenterManager.ZombieBowling, 3, 3000 );

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
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = (PlayerController) player.gameObject.GetComponent(typeof(PlayerController));
		randomizeLook ();
	}

	//We don't want all goblins to look the same
	void randomizeLook ()
	{
		if( Random.value < 0.3f ) clothItem1.SetActive( false );
		if( Random.value < 0.3f ) clothItem2.SetActive( false );
	}

	// Update is called once per frame
	void Update () {
		moveGoblin();
	}

	void moveGoblin()
	{
		if( allowMove && goblinState == GoblinState.Walking || goblinState == GoblinState.Crawling )
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

	public GoblinState getGoblinState()
	{
		return goblinState;
	}

	public void setGoblinState( GoblinState state )
	{
		goblinState = state;
	}

	//The goblin falls over backwards, typically because the player slid into him.
	public void fallToBack()
	{
		goblin_bowling.incrementCounter();
		//CancelInvoke( "groan" );
		setGoblinState( GoblinState.Dying );
		controller.enabled = false;
		CapsuleCollider capsuleCollider = (CapsuleCollider) GetComponent("CapsuleCollider");
		capsuleCollider.enabled = false;
		GetComponent<Animator>().Play("death");
		GetComponent<AudioSource>().PlayOneShot( fallToGround );
	}

	public void sideCollision()
	{
		GetComponent<AudioSource>().PlayOneShot( moanLow );
		GetComponent<Animator>().Play("damage");
	}

	public void victory( bool playWinSound )
	{
		if( playWinSound ) GetComponent<AudioSource>().PlayOneShot( win );
		setGoblinState( GoblinState.Victory );
		GetComponent<Animator>().Play("fun2");
	}

	public void knockbackZombie()
	{
		//zombie_bowling.incrementCounter();
		CancelInvoke( "groan" );
		setGoblinState( GoblinState.Dying );
		controller.enabled = false;
		CapsuleCollider capsuleCollider = (CapsuleCollider) GetComponent("CapsuleCollider");
		capsuleCollider.enabled = false;
		GetComponent<Animator>().Play("death");
		GetComponent<AudioSource>().PlayOneShot( fallToGround );
		Debug.Log("KNOCKBACK " + gameObject.name );

	}

/*
	public void resetZombie()
	{
		StopCoroutine("recycleZombie");
		CancelInvoke( "groan" );
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

	string selectRandomIdle()
	{
		int rd = Random.Range(0,3);
		if( rd == 0 ) return "idle_normal";
		else if( rd == 1 ) return "idle_lookAround";
		else if( rd == 2 ) return "idle_scratchHead";
		else return "idle_normal";
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
*/
}
