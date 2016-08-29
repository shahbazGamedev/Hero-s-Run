using UnityEngine;
using System.Collections;

public enum TrollState {
	None = -1,
	Idle = 0,
	Running = 1,
	Attack = 2,
	FarBehind = 3,
	Smashing = 4,
	StartRunning = 5,
	Laughing = 6
}

public class TrollController : MonoBehaviour {

	Transform player;
	PlayerController playerController;

	//Sound that plays if the troll sess the player fall into a river
	public AudioClip laugh;

	//Sound that plays when the club hits the ground
	public AudioClip smash;

	//Particle effect that plays when the clup hits the ground
	public ParticleSystem smashParticles;

	//At what speed the troll is moving forward
	float Speed = 0;

	//At what distance does the troll stop pursuing the player
	const float DEFAULT_DEACTIVATION_DISTANCE = 7f;
	float deactivationDistance = DEFAULT_DEACTIVATION_DISTANCE;

	//If the player is attacked, at what distance to the player will the troll be placed
	const float STOP_DISTANCE = 3.8f;

	//The minimum distance bwetween the troll and the player
	const float MINIMUM_DISTANCE = 2.9f;


	//Speed boost to give to troll when he starts pursuing so that he does not disappear too quickly
	const float SPEED_BOOST = 0.4f;

	//Troll state
	//Not that while jumping, the troll remains in the running state
	public TrollState trollState = TrollState.None;

	bool deactivateTroll = false; //Used for debugging so troll does not pursue player. Normal value is false.

	public bool playerStumbledPreviously = false;

	void Awake ()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = player.GetComponent<PlayerController>();
	}

	void Start()
	{
		//Place the troll on the ground which might not be at y=0
		//Calculate the ground height
		RaycastHit hit;
		if (Physics.Raycast(new Vector3(transform.position.x,transform.position.y + 10f,transform.position.z), Vector3.down, out hit, 20.0F ))
		{
			transform.position = new Vector3( transform.position.x, hit.point.y, transform.position.z);
			gameObject.SetActive( false );
		}
	}

	public void startPursuing ()
	{
		if( deactivateTroll ) return;
		gameObject.SetActive( true );

		playerStumbledPreviously = false;

		setTrollState(TrollState.StartRunning);

		//Give the enemy the same speed as the player. Since the player's
		//speed increases gradually, unless he stumbles, the enemy should not catch him.
		//If in the tutorial, make the troll move slower
		if( LevelManager.Instance.isTutorialActive() )
		{
			Speed = PlayerController.getPlayerSpeed();
		}
		else
		{
			Speed = PlayerController.getPlayerSpeed() + SPEED_BOOST;

		}
		Debug.Log ("TrollController-startPursuing: start position : " + transform.position + " Troll speed " + Speed);
	}
	
	public void stopPursuing ()
	{
		playerStumbledPreviously = false;
		setTrollState(TrollState.FarBehind);
		Debug.Log ("TrollController-stopPursuing" );
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
	
		if( GameManager.Instance.getGameState() == GameState.Normal )
		{
			if( trollState == TrollState.StartRunning || trollState == TrollState.Running || trollState == TrollState.Smashing )
			{

		 		float distance = Vector3.Distance(player.position,transform.position);

				if( distance > MINIMUM_DISTANCE )
				{
					//Only move the troll forward if the distance between the troll and the player is greater than minimum distance.
					//We do not want the troll to overrun the player.
					transform.position += transform.forward * Speed * Time.deltaTime;
					transform.LookAt(player);
				}


				if( distance > deactivationDistance )
				{
		            //Player escaped
					//Hide the enemy
					stopPursuing ();
					Debug.Log ("TrollController-Update: disabling enemy because too far." );
				}
			}
		}
	}

	//The attack occurs when the player stumbles for a second time while the troll is in active pursuit.
	//The attack will kill the player.
	//After the attack, the troll will go into Idle.
	private void attackPlayer()
	{
		if( trollState != TrollState.Attack )
		{
			print ("ATTACK PLAYER ");
			Vector3 relativePos = new Vector3(0 , 0 , -STOP_DISTANCE );
			Vector3 exactPos = player.TransformPoint(relativePos);
			transform.position = exactPos;
			transform.LookAt(player);
			setTrollState(TrollState.Attack);
			playerController.managePlayerDeath( DeathType.Enemy );
		}
		else
		{
			Debug.LogError("TrollController - ignoring call to attack player because troll state is already Attack.");
		}
	}

	//The attack occurs when the player stumbles for a second time while the troll is in active pursuit.
	//The attack will kill the player.
	//After the attack, the troll will go into Idle.
	void Attack_completed ()
	{
		print ("ATTACK COMPLETED ");
		//Play a smash sound and particle effect
		GetComponent<AudioSource>().PlayOneShot( smash );
		smashParticles.Play();
		playerController.shakeCamera();

		//By putting the troll into Idle state, he will stop moving forward
		trollState = TrollState.Idle;

		//Once the attack animation completes, go to idle
		GetComponent<Animation>().CrossFadeQueued("Idle", 0.87f);
	}

	//The smash occurs when the player stumbles while the troll is not active.
	//The smash will is only for effect.
	//Once the smash animation completes, the troll will continue to run
	void Smash_completed ()
	{
		//Play a smash sound and particle effect
		GetComponent<AudioSource>().PlayOneShot( smash );
		smashParticles.Play();
		playerController.shakeCamera();

		//Once the smash animation completes, the troll will continue to run
		trollState = TrollState.Running;
		GetComponent<Animation>().CrossFadeQueued("Run", 0.87f);
		Speed = Speed - 0.7f; //Make sure the troll falls back

	}

	//Called by the player controller
	//Plays a jump animation immediately followed by the run animation
	//Does NOT change the state of the troll.
	public void jump()
	{
		if( gameObject.activeSelf )
		{
			GetComponent<Animation>().CrossFade("Jump",0.2f);
			GetComponent<Animation>().CrossFadeQueued("Run", 0.4f);
		}
	}
	
	void setTrollState( TrollState newState )
	{
		print ( "setTrollState previous state: " + trollState + " new state: " + newState );
		trollState = newState;

		switch (trollState)
		{
		case TrollState.StartRunning:
			gameObject.SetActive( true );
			GetComponent<Animation>().CrossFade("Run_threaten",0.2f);
			GetComponent<Animation>().CrossFadeQueued("Run", 0.2f);
			break;
			
		case TrollState.Running:
			gameObject.SetActive( true );
			GetComponent<Animation>().CrossFade("Run", 0.2f);
			break;

		case TrollState.Attack:
			GetComponent<Animation>().CrossFade("Attack", 0.1f);
			Invoke("Attack_completed", 0.52f);
			print ("INVOKE ATTACK COMPLETED CALLED");
			break;

		case TrollState.FarBehind:
			gameObject.SetActive( false );
			break;

		case TrollState.Smashing:
			playerStumbledPreviously = true;
			GetComponent<Animation>().CrossFade("Attack",0.2f);
			Invoke("Smash_completed", 0.52f);
			break;

		case TrollState.Idle:
			GetComponent<Animation>().CrossFade("Idle",0.2f);
			break;

		case TrollState.Laughing:
			GetComponent<Animation>().CrossFade("Laugh",0.6f);
			Invoke ("goBackToIdleAfterLaugh", 2.799f );
			GetComponent<AudioSource>().PlayOneShot( laugh, 0.7f );
			break;
		}
	}

	void goBackToIdleAfterLaugh()
	{
		GetComponent<Animation>().CrossFade("Idle", 0.92f);
	}

	public bool didPlayerStumblePreviously()
	{
		return playerStumbledPreviously;
	}

	public void placeTrollBehindPlayer()
	{
		if( deactivateTroll ) return;
		gameObject.SetActive( true );

		if( !gameObject.activeSelf )
		{
			gameObject.SetActive( true );
		}

		//Give the enemy about the same speed as the player. Since the player's
		//speed increases gradually, unless he stumbles, the enemy should not catch him.
		Speed = PlayerController.getPlayerSpeed() - 0.2f;

		//Place enemy 3 meters behind player
		Vector3 relativePos = new Vector3(0 , 0 , -3f );
		Vector3 exactPos = player.TransformPoint(relativePos);
		transform.position = exactPos;
		transform.LookAt(player);

		if( playerStumbledPreviously )
		{
			attackPlayer();
		}
		else
		{
			//When the player does a first stumble, the troll smashes the ground with his club just behind the player.
			//It does not do any damage, it is just there to make the troll menacing.
			setTrollState( TrollState.Smashing );
		}
	}

	public void runBehindPlayer()
	{
		if( !gameObject.activeSelf ) gameObject.SetActive( true );

		//Give the enemy about the same speed as the player. Since the player's
		//speed increases gradually, unless he stumbles, the enemy should not catch him.
		Speed = PlayerController.getPlayerSpeed() + 0.2f;
		
		//Place troll a few meters behind player
		Vector3 relativePos = new Vector3(0 , 0 , -STOP_DISTANCE );
		Vector3 exactPos = player.TransformPoint(relativePos);
		transform.position = exactPos;
		transform.LookAt(player);
		setTrollState( TrollState.StartRunning );

	}

	public void smashNow()
	{
		setTrollState( TrollState.Smashing );
	}

	public void adjustTrollSpeed( float delta )
	{
		Speed = Speed + delta;
	}

	public void setDeactivationDistance( float value )
	{
		deactivationDistance = value;
	}

	public void resetDeactivationDistance()
	{
		deactivationDistance = DEFAULT_DEACTIVATION_DISTANCE;
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		PlayerController.playerStateChanged += PlayerStateChange;
	}

	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerController.playerStateChanged -= PlayerStateChange;
	}
	
	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Paused )
		{
			GetComponent<Animation>().enabled = false;
		}
		else if( newState == GameState.Normal )
		{
			GetComponent<Animation>().enabled = true;
		}
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			if( trollState == TrollState.Running || trollState == TrollState.StartRunning || trollState == TrollState.Smashing )
			{

				//If player jumped and died while not on the ground, the troll will also be above ground.
				//This is why we ensure the troll is on the ground.
				RaycastHit hit;
				if (Physics.Raycast(transform.position, Vector3.down, out hit, 10.0F ))
				{
					transform.position = new Vector3( transform.position.x, transform.position.y - hit.distance, transform.position.z );
				}

				setTrollState(TrollState.Laughing);
			}
		}
	}

}
