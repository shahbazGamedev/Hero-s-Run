using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GalleryManager : MonoBehaviour {

	[Header("Frame")]
	public Text menuTitle;
	string menuTitleTextId = "GALLERY_TITLE";
	public Text characterName;
	public Text characterBio;

	[Header("Fairy")]
	string fairyNameTextId = "GALLERY_NAME_FAIRY";
	string fairyBioTextId  = "GALLERY_BIO_FAIRY";
	public GameObject fairy3DGroup;
	public AudioClip fairyAmbience;

	[Header("Dark Queen")]
	string darkQueenNameTextId = "GALLERY_NAME_DARK_QUEEN";
	string darkQueenBioTextId  = "GALLERY_BIO_DARK_QUEEN";
	public GameObject darkQueen3DGroup;
	public AudioClip darkQueenAmbience;

	[Header("Hero")]
	string heroBioTextId  = "GALLERY_BIO_HERO";
	public GameObject hero3DGroup;
	public AudioClip heroAmbience;

	[Header("Heroine")]
	string heroineBioTextId  = "GALLERY_BIO_HEROINE";
	public GameObject heroine3DGroup;
	//Heroine uses the same ambience as Hero

	[Header("Zombie")]
	string zombieNameTextId = "GALLERY_NAME_ZOMBIE";
	string zombieBioTextId  = "GALLERY_BIO_ZOMBIE";
	public GameObject zombie3DGroup;
	public AudioClip zombieAmbience;

	[Header("Goblin Piker")]
	string goblinPikerNameTextId = "GALLERY_NAME_GOBLIN_PIKER";
	string goblinPikerBioTextId  = "GALLERY_BIO_GOBLIN_PIKER";
	public GameObject goblinPiker3DGroup;
	public AudioClip goblinPikerAmbience;

	[Header("Goblin Scout")]
	string goblinScoutNameTextId = "GALLERY_NAME_GOBLIN_SCOUT";
	string goblinScoutBioTextId  = "GALLERY_BIO_GOBLIN_SCOUT";
	public GameObject goblinScout3DGroup;
	public AudioClip goblinScoutAmbience;

	[Header("Troll")]
	string trollNameTextId = "GALLERY_NAME_TROLL";
	string trollBioTextId  = "GALLERY_BIO_TROLL";
	public GameObject troll3DGroup;
	public AudioClip trollAmbience;

	[Header("Misc")]
	bool levelLoading = false;
	public ScrollRect characterBioScrollRect;
	public Scrollbar scrollbarIndicator;
	public Scrollbar scrollbarBio;
	const int INDEX_OF_LAST_CHARACTER = 6;
	int indexOfDisplayedCharacter = 0; //0 is the fairy
	int previousIndexOfDisplayedCharacter = 0; //0 is the fairy

	//Variables for swipe
	bool touchStarted = false;
	Vector2 touchStartPos;
	float MINIMUM_HORIZONTAL_DISTANCE = 0.09f * Screen.width; //How many pixels you need to swipe horizontally to change page.
	
	//Audio
	public AudioMixerSnapshot oneHighTwoLow;
	public AudioMixerSnapshot oneLowTwoHigh;
	public AudioSource trackOne;
	public AudioSource trackTwo;
	bool isTrackOneMain = true;
	const float FADE_DURATION = 3f; //in seconds

	enum Characters {
			Fairy = 0,
			Dark_Queen = 1,
			Hero = 2,
			Zombie = 3,
			Goblin_Piker = 4,
			Goblin_Scout = 5,
			Troll = 6
	}

	void Awake ()
	{
		//Reset
		levelLoading = false;

		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif

		//The activity indicator may have been started
		Handheld.StopActivityIndicator();

		//Title
		menuTitle.text = LocalizationManager.Instance.getText(menuTitleTextId);

		//Fairy - she is the first character in the gallery
		//Character Name
		characterName.text = LocalizationManager.Instance.getText(fairyNameTextId);

		//Character Bio
		string characterTextString = LocalizationManager.Instance.getText(fairyBioTextId);
		characterTextString = characterTextString.Replace("\\n", System.Environment.NewLine );
		characterBio.text = characterTextString;
	
		//Audio
		trackOne.clip = fairyAmbience;
		trackTwo.clip = darkQueenAmbience;
		trackOne.Play();
		trackTwo.Play();
		oneHighTwoLow.TransitionTo(0.1f);

	}

	void fadeAmbience( AudioClip clipToPlay )
	{
		isTrackOneMain = !isTrackOneMain;
		if( isTrackOneMain )
		{
			trackOne.clip = clipToPlay;
			trackOne.Play();
			oneHighTwoLow.TransitionTo(FADE_DURATION);

		}
		else
		{
			trackTwo.clip = clipToPlay;
			trackTwo.Play();
			oneLowTwoHigh.TransitionTo(FADE_DURATION);
		}
	}


	//Only show the bio scrollbar when selected
	public void scrollSelected()
	{
		scrollbarBio.gameObject.SetActive( true );
	}

	public void scrollDeselected()
	{
		scrollbarBio.gameObject.SetActive( false );
	}
	
	void Update ()
	{
		handleSwipes();
		#if UNITY_EDITOR
		handleKeyboard();
		#endif
	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.LeftArrow) ) 
		{
			sideSwipe( false );
		}
		else if ( Input.GetKeyDown (KeyCode.RightArrow) ) 
		{
			sideSwipe( true );
		}
	}

	void handleSwipes()
	{
		//Verify if the player swiped across the screen
		if (Input.touchCount > 0)
		{
			var touch = Input.touches[0];

			switch (touch.phase)
			{
			case TouchPhase.Began:
				touchStarted = true;
				touchStartPos = touch.position;
				break;
				
			case TouchPhase.Ended:
				if (touchStarted)
				{
					touchStarted = false;
				}
				break;
				
			case TouchPhase.Canceled:
				touchStarted = false;
				break;
				
			case TouchPhase.Stationary:
				break;
				
			case TouchPhase.Moved:
				if (touchStarted)
				{
					TestForSwipeGesture(touch);
				}
				break;
			}
		}	
		
	}

	void TestForSwipeGesture(Touch touch)
	{
		Vector2 lastPos = touch.position;
		float distance = Vector2.Distance(lastPos, touchStartPos);
		
		if (distance > MINIMUM_HORIZONTAL_DISTANCE )
		{
			touchStarted = false;
			float dy = lastPos.y - touchStartPos.y;
			float dx = lastPos.x - touchStartPos.x;
			
			float angle = Mathf.Rad2Deg * Mathf.Atan2(dx, dy);
			
			angle = (360 + angle - 45) % 360;
			
			if (angle < 90)
			{
				//player swiped RIGHT
				sideSwipe( true );
			}
			else if (angle < 180)
			{
				//player swiped DOWN
				//Ignore
			}
			else if (angle < 270)
			{
				//player swiped LEFT
				sideSwipe( false );
			}
			else
			{
				//player swiped UP
				//Ignore
			}

		}
	}

	void sideSwipe( bool isGoingRight )
	{
		if( isGoingRight )
		{
			indexOfDisplayedCharacter++;
			if( indexOfDisplayedCharacter > INDEX_OF_LAST_CHARACTER ) indexOfDisplayedCharacter = 0;
		}
		else
		{
			indexOfDisplayedCharacter--;
			if( indexOfDisplayedCharacter < 0 ) indexOfDisplayedCharacter = INDEX_OF_LAST_CHARACTER;
		}
		OnValueChanged( indexOfDisplayedCharacter );

	}

	public void OnValueChanged( int newIndex )
	{
		if( newIndex == previousIndexOfDisplayedCharacter ) return; //Nothing has changed. Ignore.
		previousIndexOfDisplayedCharacter = newIndex;

		//Update the scrollbar indicator which is not interactable
		scrollbarIndicator.value = (float)newIndex/INDEX_OF_LAST_CHARACTER;

		//Reset the scroll rectangle with the character bio text to the top
		characterBioScrollRect.verticalNormalizedPosition = 1f;

		string characterTextString = "";

		switch (newIndex)
		{
			case (int)Characters.Fairy:
				//Character Name
				characterName.text = LocalizationManager.Instance.getText(fairyNameTextId);
				//Character Bio
				characterTextString = LocalizationManager.Instance.getText(fairyBioTextId);
				//3D
				fairy3DGroup.SetActive( true );
				darkQueen3DGroup.SetActive( false );
				troll3DGroup.SetActive( false );
				hero3DGroup.SetActive( false );
				heroine3DGroup.SetActive( false );
				zombie3DGroup.SetActive( false );
				goblinPiker3DGroup.SetActive( false );
				goblinScout3DGroup.SetActive( false );
				fadeAmbience( fairyAmbience );
			break;

			case (int)Characters.Dark_Queen:
				//Character Name
				characterName.text = LocalizationManager.Instance.getText(darkQueenNameTextId);
				//Character Bio
				characterTextString = LocalizationManager.Instance.getText(darkQueenBioTextId);
				//3D
				fairy3DGroup.SetActive( false );
				darkQueen3DGroup.SetActive( true );
				troll3DGroup.SetActive( false );
				hero3DGroup.SetActive( false );
				heroine3DGroup.SetActive( false );
				zombie3DGroup.SetActive( false );
				goblinPiker3DGroup.SetActive( false );
				goblinScout3DGroup.SetActive( false );
				fadeAmbience( darkQueenAmbience );
			break;
			case (int)Characters.Hero:
				//Character Name
				characterName.text = PlayerStatsManager.Instance.getUserName();
				if( PlayerStatsManager.Instance.getAvatar() == Avatar.Hero )
				{
					//Character Bio
					characterTextString = LocalizationManager.Instance.getText(heroBioTextId);
	
					//3D
					hero3DGroup.SetActive( true );
					heroine3DGroup.SetActive( false );
				}
				else
				{
					//Character Bio
					characterTextString = LocalizationManager.Instance.getText(heroineBioTextId);
	
					//3D
					hero3DGroup.SetActive( false );
					heroine3DGroup.SetActive( true );
				}
				//3D
				fairy3DGroup.SetActive( false );
				darkQueen3DGroup.SetActive( false );
				troll3DGroup.SetActive( false );
				zombie3DGroup.SetActive( false );
				goblinPiker3DGroup.SetActive( false );
				goblinScout3DGroup.SetActive( false );
				fadeAmbience( heroAmbience );
			break;
			case (int)Characters.Zombie:
				//Character Name
				characterName.text = LocalizationManager.Instance.getText(zombieNameTextId);
				//Character Bio
				characterTextString = LocalizationManager.Instance.getText(zombieBioTextId);
				//3D
				fairy3DGroup.SetActive( false );
				darkQueen3DGroup.SetActive( false );
				troll3DGroup.SetActive( false );
				hero3DGroup.SetActive( false );
				heroine3DGroup.SetActive( false );
				zombie3DGroup.SetActive( true );
				goblinPiker3DGroup.SetActive( false );
				goblinScout3DGroup.SetActive( false );
				fadeAmbience( zombieAmbience );
			break;
			case (int)Characters.Goblin_Piker:
				//Character Name
				characterName.text = LocalizationManager.Instance.getText(goblinPikerNameTextId);
				//Character Bio
				characterTextString = LocalizationManager.Instance.getText(goblinPikerBioTextId);
				//3D
				fairy3DGroup.SetActive( false );
				darkQueen3DGroup.SetActive( false );
				troll3DGroup.SetActive( false );
				hero3DGroup.SetActive( false );
				heroine3DGroup.SetActive( false );
				zombie3DGroup.SetActive( false );
				goblinPiker3DGroup.SetActive( true );
				goblinScout3DGroup.SetActive( false );
				fadeAmbience( goblinPikerAmbience );
			break;
			case (int)Characters.Goblin_Scout:
				//Character Name
				characterName.text = LocalizationManager.Instance.getText(goblinScoutNameTextId);
				//Character Bio
				characterTextString = LocalizationManager.Instance.getText(goblinScoutBioTextId);
				//3D
				fairy3DGroup.SetActive( false );
				darkQueen3DGroup.SetActive( false );
				troll3DGroup.SetActive( false );
				hero3DGroup.SetActive( false );
				heroine3DGroup.SetActive( false );
				zombie3DGroup.SetActive( false );
				goblinPiker3DGroup.SetActive( false );
				goblinScout3DGroup.SetActive( true );
				fadeAmbience( goblinScoutAmbience );
			break;
			case (int)Characters.Troll:
				//Character Name
				characterName.text = LocalizationManager.Instance.getText(trollNameTextId);
				//Character Bio
				characterTextString = LocalizationManager.Instance.getText(trollBioTextId);
				//3D
				fairy3DGroup.SetActive( false );
				darkQueen3DGroup.SetActive( false );
				troll3DGroup.SetActive( true );
				hero3DGroup.SetActive( false );
				heroine3DGroup.SetActive( false );
				zombie3DGroup.SetActive( false );
				goblinPiker3DGroup.SetActive( false );
				goblinScout3DGroup.SetActive( false );
				fadeAmbience( trollAmbience );
			break;
		}

		characterTextString = characterTextString.Replace("\\n", System.Environment.NewLine );
		characterBio.text = characterTextString;

	}

	public void closeMenu()
	{
		StartCoroutine( close() );
	}

	IEnumerator close()
	{
		if( !levelLoading )
		{
			SoundManager.soundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.WorldMap );
		}
	}
}
