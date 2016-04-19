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
	string heroNameTextId = "GALLERY_NAME_HERO";
	string heroBioTextId  = "GALLERY_BIO_HERO";
	public GameObject hero3DGroup;
	public AudioClip heroAmbience;

	[Header("Heroine")]
	string heroineNameTextId = "GALLERY_NAME_HEROINE";
	string heroineBioTextId  = "GALLERY_BIO_HEROINE";
	public GameObject heroine3DGroup;
	//Heroine uses the same ambience as Hero

	[Header("Zombie")]
	string zombieNameTextId = "GALLERY_NAME_ZOMBIE";
	string zombieBioTextId  = "GALLERY_BIO_ZOMBIE";
	public GameObject zombie3DGroup;
	public AudioClip zombieAmbience;

	[Header("Troll")]
	string trollNameTextId = "GALLERY_NAME_TROLL";
	string trollBioTextId  = "GALLERY_BIO_TROLL";
	public GameObject troll3DGroup;
	public AudioClip trollAmbience;

	[Header("Misc")]
	bool levelLoading = false;
	public ScrollRect characterBioScrollRect;
	float lastScrollBarPosition = 0f; //Used to filter scroll bar events
	public Scrollbar scrollbarIndicator;
	public Scrollbar scrollbarBio;

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

	void Awake ()
	{
		//Reset
		levelLoading = false;
		lastScrollBarPosition = 0;

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
		float newPos;
		if( isGoingRight )
		{
			newPos = lastScrollBarPosition + 0.25f;
			if( newPos > 1f ) newPos = 0;
		}
		else
		{
			newPos = lastScrollBarPosition - 0.25f;
			if( newPos < 0 ) newPos = 1f;
		}
		OnValueChanged( newPos );

	}

	public void OnValueChanged( float scrollBarPosition )
	{
		if( scrollBarPosition == lastScrollBarPosition ) return; //Nothing has changed. Ignore.
		lastScrollBarPosition = scrollBarPosition;

		//Update the scrollbar indicator which is not interactable
		scrollbarIndicator.value = scrollBarPosition;

		print ("Gallery Manager " + scrollBarPosition );

		//Reset the scroll rectangle with the character bio text to the top
		characterBioScrollRect.verticalNormalizedPosition = 1f;

		string characterTextString;

		//Fairy
		if( scrollBarPosition == 0 )
		{
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
			fadeAmbience( fairyAmbience );

		}
		//Dark Queen
		else if(  scrollBarPosition == 0.25f )
		{
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
			fadeAmbience( darkQueenAmbience );

		}
		//Hero or Heroine
		else if(  scrollBarPosition == 0.5f )
		{

			if( PlayerStatsManager.Instance.getAvatar() == Avatar.Hero )
			{
				//Character Name
				characterName.text = LocalizationManager.Instance.getText(heroNameTextId);

				//Character Bio
				characterTextString = LocalizationManager.Instance.getText(heroBioTextId);

				//3D
				hero3DGroup.SetActive( true );
				heroine3DGroup.SetActive( false );
			}
			else
			{
				//Character Name
				characterName.text = LocalizationManager.Instance.getText(heroineNameTextId);

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
			fadeAmbience( heroAmbience );

		}
		//Zombie
		else if(  scrollBarPosition == 0.75f )
		{
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
			fadeAmbience( zombieAmbience );

		}
		//Troll
		else
		{
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
			fadeAmbience( trollAmbience );

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
			SoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.WorldMap );
		}
	}
}
