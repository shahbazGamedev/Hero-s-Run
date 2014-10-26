using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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

	[Header("Dark Queen")]
	string darkQueenNameTextId = "GALLERY_NAME_DARK_QUEEN";
	string darkQueenBioTextId  = "GALLERY_BIO_DARK_QUEEN";
	public GameObject darkQueen3DGroup;

	[Header("Hero")]
	string heroNameTextId = "GALLERY_NAME_HERO";
	string heroBioTextId  = "GALLERY_BIO_HERO";
	public GameObject hero3DGroup;

	[Header("Heroine")]
	string heroineNameTextId = "GALLERY_NAME_HEROINE";
	string heroineBioTextId  = "GALLERY_BIO_HEROINE";
	public GameObject heroine3DGroup;

	[Header("Zombie")]
	string zombieNameTextId = "GALLERY_NAME_ZOMBIE";
	string zombieBioTextId  = "GALLERY_BIO_ZOMBIE";
	public GameObject zombie3DGroup;

	[Header("Troll")]
	string trollNameTextId = "GALLERY_NAME_TROLL";
	string trollBioTextId  = "GALLERY_BIO_TROLL";
	public GameObject troll3DGroup;

	[Header("Misc")]
	bool levelLoading = false;
	public ScrollRect characterBioScrollRect;
	float lastScrollBarPosition = 0f; //Used to filter scroll bar events
	public Scrollbar scrollbarIndicator;

	//Variables for swipe
	bool touchStarted = false;
	Vector2 touchStartPos;

	void Awake ()
	{
		//Reset
		levelLoading = false;
		lastScrollBarPosition = 0;

		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu

		//The activity indicator may have been started
		Handheld.StopActivityIndicator();

		//Title
		menuTitle.text = LocalizationManager.Instance.getText(menuTitleTextId);

		//Fairy
		//Character Name
		characterName.text = LocalizationManager.Instance.getText(fairyNameTextId);

		//Character Bio
		string characterTextString = LocalizationManager.Instance.getText(fairyBioTextId);
		characterTextString = characterTextString.Replace("\\n", System.Environment.NewLine );
		characterBio.text = characterTextString;

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
		
		if (distance > 10)
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
		//We are on the first page, we are only allowed to go right
		if( lastScrollBarPosition == 0 && !isGoingRight ) return; 

		//We are on the last page, we are only allowed to go left
		if( lastScrollBarPosition == 1f && isGoingRight ) return; 


		if( isGoingRight )
		{
			OnValueChanged( lastScrollBarPosition + 0.25f );
		}
		else
		{
			OnValueChanged( lastScrollBarPosition - 0.25f );
		}
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
			Application.LoadLevel( 3 );
		}
	}
}
