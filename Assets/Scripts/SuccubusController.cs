using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SuccubusController : MonoBehaviour {
	
	//Animation
	Animation succubusAnimation;
	public AnimationClip animationFly;
	public AnimationClip animationVictory;
	public AnimationClip animationLeave;
	public AnimationClip animationCastNormalSpell;	
	public AnimationClip animationCastHellpit;

	//The animation to play based on the selected spell
	AnimationClip selectedSpellAnimation;

	const float TIME_TO_ARRIVE = 12f;
	const float DELAY_BEFORE_FIRST_SPELL = 3f;
	const float DELAY_BETWEEN_SPELLS = 6f;

	public Texture2D succubusPortrait;

	//for debugging
	bool spellCastingEnabled = true;

	//Where to position the succusbus relative to the player
	Vector3 succubusRelativePos = new Vector3(0 , 2.5f , 30f );


	Transform player;
	PlayerController playerController;

	GameObject hellPit;
	GameObject fireBallPrefab;
	//GameObject fireBall;

	const float CORRIDOR_WIDTH = 5.2f;
	Vector3 corridorRelativePos = new Vector3( 0, 0, CORRIDOR_WIDTH );
	int numberSuccessiveSpellCast = 0;

	List<string> returnText = new List<string>();

	//Chicken spell
	public GameObject chicken;
	Animator chickenAnimator;



	public enum SuccubusState {
		Arrive = 0,			//Arriving at the scene and not attacking
		CastSpells = 1,		//While casting spells. The update method keeps the succusbus in a constant position relative to the player
		Leave = 2,			//While leaving the scene and not attacking either because player died, player reached castle, player reached checkpoint or she has cast her series of spells
		FlyAbove = 3,		//While flying left to right once in front of the player and doing a funny animation.
	}
	public SuccubusState succubusState;

	private enum SpellType {
		Hellpit = 0,
		Fireball = 1,
		Chickens = 2
	}


	void Awake()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = player.GetComponent<PlayerController>();

		//Get a copy of the components
		succubusAnimation = (Animation) GetComponent("Animation");

		//Prepare texts
		assembleTexts();

		//Chicken
		if( chicken != null )
		{
			chickenAnimator = (Animator) chicken.GetComponent("Animator");
		}

	}
	
	void PlayerStateChange( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.StartRunning )
		{
			//Player started running
			//Tell the succubus to arrive in front of the player after a number of seconds
			//Invoke( "Arrive", TIME_TO_ARRIVE );
			
		}
		else if( newState == PlayerCharacterState.Dying )
		{
			//Player died
			CancelInvoke( "Arrive" );
			//CancelInvoke( "castSpell" );
			StopCoroutine("WaitForCastSpellAnimation");
			playAnimation( animationVictory.name, WrapMode.Once );
			StartCoroutine("WaitForVictoryAnimation");
			
		}
	}

	void Arrive()
	{
		succubusState = SuccubusState.Arrive;
		//Reset value
		numberSuccessiveSpellCast = 0;
		//Play fly animation
		playAnimation( animationFly.name, WrapMode.Loop );
		//Reset size to the normal size (succubus shrinks when she leaves)
		transform.localScale = new Vector3( 2.2f,2.2f,2.2f );
		//Place succubus to the left of the player
		Vector3 arrivalStartPos = new Vector3( -15f, 12f, playerController.getSpeed() * 2f );
		Vector3 exactPos = player.TransformPoint(arrivalStartPos);
		transform.position = exactPos;
		transform.rotation = Quaternion.Euler( 0, player.transform.eulerAngles.y + 90f, transform.eulerAngles.z );
		StartCoroutine("MoveToPosition", 2.2f );
	}

	private IEnumerator MoveToPosition( float duration )
	{
		//Step 1 - Take position in front of player
		float startTime = Time.time;
		float elapsedTime = 0;
		float startYrot = transform.eulerAngles.y;
		Vector3 startPosition = transform.position;

		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			//Percentage of time completed 
			float fracJourney = elapsedTime / duration;

			float yRot = Mathf.LerpAngle( startYrot, player.eulerAngles.y + 180f, fracJourney );
			transform.eulerAngles = new Vector3 ( transform.eulerAngles.x, yRot, transform.eulerAngles.z );

			Vector3 exactPos = player.TransformPoint(succubusRelativePos);
			transform.position = Vector3.Lerp( startPosition, exactPos, fracJourney );

			//Tilt the succubus down
			transform.rotation = Quaternion.Euler( -8f, transform.eulerAngles.y, transform.eulerAngles.z );

			yield return new WaitForFixedUpdate();  

		}
		//Step 2 - Say something funny
		DialogManager.dialogManager.activateDisplay( getFunnyText(), succubusPortrait );

		//Step 3 - Cast spell 3 spells and then leave
		succubusState = SuccubusState.CastSpells;
		InvokeRepeating( "castSpell", DELAY_BEFORE_FIRST_SPELL, DELAY_BETWEEN_SPELLS );

	}

	SpellType selectSpell()
	{
		float rd = Random.Range( 0f, 1f );
		if( rd < 0.001f )
		{
			selectedSpellAnimation = animationCastHellpit;
			return SpellType.Hellpit;
		}
		else if ( rd < 0.002f )
		{
			selectedSpellAnimation = animationCastNormalSpell;
			return SpellType.Fireball;
		}
		else
		{
			selectedSpellAnimation = animationCastNormalSpell;
			return SpellType.Chickens;
		}
	}

	void castSpell()
	{
		if( spellCastingEnabled && playerController.getCharacterState() == PlayerCharacterState.Flying && GameManager.Instance.getGameState() == GameState.Normal && playerController.getCharacterState() != PlayerCharacterState.Dying && !playerController.isPlayerOnBezierCurve() && playerController.getCurrentTileType() != TileType.Left && playerController.getCurrentTileType() != TileType.Right )
		{
			if( numberSuccessiveSpellCast < 3 )
			{
				SpellType selectedSpell = selectSpell();
				playAnimation( selectedSpellAnimation.name, WrapMode.Once );
				StartCoroutine( "WaitForCastSpellAnimation", SpellType.Fireball );
			}
			else
			{
				//Succubus is taking a brief union break
				//CancelInvoke( "castSpell" );
				numberSuccessiveSpellCast = 0;
				playAnimation( animationLeave.name, WrapMode.Once );
				StartCoroutine("WaitForLeaveAnimation");
			}
		}
	}

	private IEnumerator WaitForCastSpellAnimation( SpellType selectedSpell )
	{
		float duration = 0.8f * succubusAnimation[ selectedSpellAnimation.name ].length;
		while (duration > 0 )
		{
			duration = duration - Time.deltaTime;
			yield return new WaitForFixedUpdate();  
		}
		castSpellNow( selectedSpell );
	}

	void castSpellNow( SpellType selectedSpell )
	{
		if( GameManager.Instance.getGameState() == GameState.Normal && playerController.getCharacterState() != PlayerCharacterState.Dying && !playerController.isPlayerOnBezierCurve()  )
		{
			if( selectedSpell == SpellType.Hellpit )
			{
				if( createHellPit( 2 ) ) numberSuccessiveSpellCast++;
			}
			else if ( selectedSpell == SpellType.Fireball )
			{
				throwFireball();
				throwFireball();
				throwFireball();
				//rainOfFire();
				numberSuccessiveSpellCast++;
			}
			else if ( selectedSpell == SpellType.Chickens )
			{
				if( spawnChicken() ) numberSuccessiveSpellCast++;
			}
			succubusAnimation.CrossFadeQueued(animationFly.name, 0.3F, QueueMode.CompleteOthers);

		}
	}

	bool spawnChicken()
	{
		Vector3 exactPos = player.TransformPoint( new Vector3( 0, 0, 36f ) );
		exactPos.Set ( exactPos.x, 0, exactPos.z );
		return true;

	}

	bool throwFireball()
	{
		GameObject fireBall = (GameObject)Instantiate(fireBallPrefab, Vector3.zero, Quaternion.Euler( 0, 0, 0 ) ) ;
		Destroy( fireBall, 6f);
		Vector3 exactPos = transform.TransformPoint( new Vector3( Random.Range(-1f,1f), 1.2f, 0.69f ) );
		fireBall.transform.position = exactPos;
		Vector3 target = player.TransformPoint( new Vector3( Random.Range(-1f,1f), 2f, 0f ) );
		print ("Target is " + target );
		StartCoroutine( hurlFireball( fireBall, target ) );
		return true;
		
	}


	bool rainOfFire()
	{
		int numberOfFireballs = 15;
		for(int i=0; i < numberOfFireballs; i++ )
		{
			GameObject fireBall = (GameObject)Instantiate(fireBallPrefab, Vector3.zero, Quaternion.Euler( 0, 0, 0 ) ) ;
			Destroy( fireBall, 12f);
			Vector3 exactPos = player.TransformPoint( new Vector3( Random.Range(-10,10), Random.Range(34,36), Random.Range(20,30) ) );
			fireBall.transform.position = exactPos;
			Vector3 target = new Vector3( exactPos.x, 0, exactPos.z-6f );
			//print ("Target is " + target );
			StartCoroutine( hurlFireball( fireBall, target ) );
		}
		return true;

	}

	IEnumerator hurlFireball( GameObject fireBall, Vector3 target )
	{
		float startTime = Time.time;
		float elapsedTime = 0;
		float startYrot = fireBall.transform.eulerAngles.y;
		Vector3 startPosition = fireBall.transform.position;
		float duration = 5f;
		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;
			
			//Percentage of time completed 
			float fracJourney = elapsedTime / duration;
			
			float yRot = Mathf.LerpAngle( startYrot, startYrot + 180f, fracJourney );
			fireBall.transform.eulerAngles = new Vector3 ( fireBall.transform.eulerAngles.x, yRot, fireBall.transform.eulerAngles.z );
			
			fireBall.transform.position = Vector3.Lerp( startPosition, target, fracJourney );

			yield return new WaitForFixedUpdate();  
			
		}

	}
	
	bool createHellPit( int nbrOfCorridors )
	{
		//Don't create hell pits when the character is flying
		if( playerController.getCharacterState() == PlayerCharacterState.Flying )
		{
			return false;
		}
		RaycastHit hit;
		Vector3 testPosition = transform.position;
		List<Vector3> corridorPositionArray = new List<Vector3>();
		corridorPositionArray.Capacity = nbrOfCorridors;
		List<GameObject> corridorArray = new List<GameObject>();
		corridorArray.Capacity = nbrOfCorridors;

		//Step 1 - Verify if we have the required number of consecutive corridors
		for( int i=0; i < nbrOfCorridors; i++ )
		{
			if (Physics.Raycast(testPosition, Vector3.down, out hit, 50.0F ))
			{
				Transform corridor = hit.collider.transform.parent;
				if( hit.collider.name == "Quad" && corridor.name.StartsWith("Corridor wood") )
				{
					corridorPositionArray.Add( corridor.position);
					corridorArray.Add( corridor.gameObject );
					//Determine new test position
					testPosition = corridor.TransformPoint(corridorRelativePos);
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		if( corridorPositionArray.Count == nbrOfCorridors )
		{
			//Step 2 - Make the corridors inactive
			for( int j=0; j < corridorArray.Count; j++ )
			{
				corridorArray[j].SetActive( false );
			}

			//Step 3 - Make the hell pit appear in the center
			int lastIndex = corridorPositionArray.Count - 1;
			Vector3 center = corridorPositionArray[0] + ( corridorPositionArray[lastIndex] - corridorPositionArray[0] ) / 2f;
			//Avoid z buffer fighting
			center.Set( center.x, center.y + 0.02f, center.z );
			hellPit.SetActive( true );
			hellPit.transform.position = center;
			hellPit.transform.rotation = Quaternion.Euler( corridorArray[0].transform.eulerAngles.x, corridorArray[0].transform.eulerAngles.y, corridorArray[0].transform.eulerAngles.z );
			hellPit.GetComponent<AudioSource>().Play();
			return true;
		}
		else
		{
			return false;
		}
	}

	void assembleTexts()
	{
		returnText.Add("SUCCUBUS_IM_BACK");
		returnText.Add("YOU_ARE_CUTE");
		returnText.Add("NOTHING_PERSONAL");
		returnText.Add("NOT_PASS");

	}

	//Note: with a font size of 22, the maximum number of characters in a string is 28. This leaves one blank space on either side of the text.
	string getFunnyText()
	{
		string text;
		//Randomize the text
		int selectedTextIndex = Random.Range( 0, returnText.Count );
		string selectedText = returnText[selectedTextIndex];
		text = LocalizationManager.Instance.getText(selectedText);

		//First appearance of succubus

			//Here comes Suzie!

		//Player died previously because of her

			//a) Feeling better?

		//Player died previously, but not because of her

		//Fly above where succubus is playing funny animation

			//While putting finger in nose
			//What? Like you never do that?

		//Player successfully reached a checkpoint

		//Player reached the castle

		//Player started flying for the first time

		//Before casting huge pit of hell that makes you fall and continue in hell
			//I saved up to be able to do this one

		//Player did not die even if cast her spells

			//I'm back! Did you miss me?
			//It's nothing personal you know
			//Anyone ever tell you you have nice hair?
			//You're kinda cute you know
			//Too bad I have to detroy you
			//Darn! You can't escape for long
			//Though shall not pass!



		//Player is cute and she likes him

		//After credits, the Hero and the succubus are on a park bench and she says "You know, we should go for coffee sometime?".

		//Based on level setting when encountered for the first time

			//Cemeteries creep me out ...

		//For someone in a hurry, you sure took your sweet time

		//I have just one question for you. Does your family have a zombie infestation plan?

		return text;
	}

	AnimationClip getFunnyAnimation()
	{

		//Swimming backstroke

		//Blows kiss

		//Sleeping on arms

		//With cucumber and green mask in bathrobe  opening hell portal

		//Doing exercise like the bicycle pedal for abs

		//Painting her toe nails

		return selectedSpellAnimation;
		
	}

	// Update is called once per frame
	void Update ()
	{
		
		if( GameManager.Instance.getGameState() == GameState.Normal && succubusState == SuccubusState.CastSpells && playerController.getCharacterState() != PlayerCharacterState.Dying )
		{
			Vector3 exactPos = player.TransformPoint(succubusRelativePos);
			transform.position = exactPos;
			//transform.position = new Vector3( transform.position.x, 2.4f, transform.position.z );
			transform.LookAt(player);
			transform.rotation = Quaternion.Euler( -8f, transform.eulerAngles.y, transform.eulerAngles.z );
		}
		
	}
	
	private void playAnimation( string animationName, WrapMode mode )
	{
		succubusAnimation[ animationName ].wrapMode = mode;
		succubusAnimation[ animationName ].speed = 1f;
		succubusAnimation.CrossFade(animationName, 0.3f);
		
	}

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
		GameManager.gameStateEvent += GameStateChange;
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}
	
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
		GameManager.gameStateEvent -= GameStateChange;
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}
	
	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Call_Succubus )
		{
			print ("SimpleController-player entered trigger GameEvent.Call_Succubus");
			Invoke( "Arrive", 2f );
		}
		else if( eventType == GameEvent.Dismiss_Succubus )
		{
			print ("SimpleController-player entered trigger GameEvent.Dismiss_Succubus");
			playAnimation( animationLeave.name, WrapMode.Once );
			StartCoroutine("WaitForLeaveAnimation");
		} 
	}

	void GameStateChange( GameState previousState, GameState newState )
	{
		if( newState == GameState.Paused )
		{
			succubusAnimation.enabled = false;
		}
		else if( newState == GameState.Countdown )
		{

		}
		else if( newState == GameState.Normal )
		{
			succubusAnimation.enabled = true;

		}
	}

	//This routine is called when the player has died
	private IEnumerator WaitForVictoryAnimation()
	{
		float duration = succubusAnimation[ animationVictory.name ].length;
		while (duration > 0 )
		{
			duration = duration - Time.deltaTime;
			yield return new WaitForFixedUpdate();  
		}
		playAnimation( animationFly.name, WrapMode.Loop );
		Vector3 small = new Vector3( 0.3f, 0.3f, 0.3f );
		LeanTween.scale( gameObject, small, 4f ).setEase(LeanTweenType.easeOutQuad );
		LeanTween.moveY( gameObject, transform.position.y + 20f, 5f);
		LeanTween.rotateY( gameObject, transform.eulerAngles.y + 180f, 2.5f);
		succubusState = SuccubusState.Leave;

	}

	//This routine is called when the succubus leaves and the player is still alive
	private IEnumerator WaitForLeaveAnimation()
	{
		float duration = succubusAnimation[ animationLeave.name ].length;
		while (duration > 0 )
		{
			duration = duration - Time.deltaTime;
			yield return new WaitForFixedUpdate();  
		}
		playAnimation( animationFly.name, WrapMode.Loop );
		Vector3 small = new Vector3( 0.3f, 0.3f, 0.3f );
		LeanTween.scale( gameObject, small, 4f ).setEase(LeanTweenType.easeOutQuad);
		Vector3 exactPos = transform.TransformPoint(new Vector3( 2f, 20f, -20f ) );
		LeanTween.move( gameObject, exactPos, 5f);
		LeanTween.rotateY( gameObject, transform.eulerAngles.y + 180f, 2.5f);
		succubusState = SuccubusState.Leave;
		//Invoke( "Arrive", TIME_TO_ARRIVE );
	}

}
