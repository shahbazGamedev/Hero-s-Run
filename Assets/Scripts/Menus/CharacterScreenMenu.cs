using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum Avatar {
	None = 0,
	Hero = 1,
	Heroine = 2
}

public class CharacterScreenMenu : MonoBehaviour {

	public GameObject male;
	public GameObject female;
	bool toggle = true;
	Vector3 frontLocation = new Vector3( 0,0,0);			//Selected
	Vector3 backLeftLocation = new Vector3(-1.6f,0,5.5f); 	//Male
	Vector3 backRightLocation = new Vector3(1.6f,0,5.5f);	//Female
	Vector3 nextToPopupLocation = new Vector3(-1f,0f,1f);
	Animation maleAnimation;
	Animation femaleAnimation;

	Avatar selectedAvatar = Avatar.Hero;

	public Text titleText;
	public Text pickMeContentText;
	public Button changeButton;
	public Text changeButtonText;
	public Button confirmButton;
	public Text confirmButtonText;

	[Header("User Name Popup")]
	public GameObject userNamePopup;
	public InputField userNameText;
	public Text userNameTitleText;
	public Text userNamePlaceholderText;
	public Text okayButtonText;
	public Text skipButtonText;

	// Use this for initialization
	void Awake () {
	
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif

		Handheld.StopActivityIndicator();

		maleAnimation = male.GetComponent<Animation>();
		femaleAnimation = female.GetComponent<Animation>();
		//By default, the male character is selected
		PlayerStatsManager.Instance.setAvatar(selectedAvatar);

		titleText.text = LocalizationManager.Instance.getText("MENU_CHARACTER_SELECTION_TITLE");
		pickMeContentText.text = LocalizationManager.Instance.getText("MENU_PICK_ME_MALE");
		changeButtonText.text = LocalizationManager.Instance.getText("MENU_CHANGE");
		confirmButtonText.text = LocalizationManager.Instance.getText("MENU_SELECT");

		//User Name Popup
		userNameTitleText.text = LocalizationManager.Instance.getText("USER_NAME_POPUP_TITLE");
		okayButtonText.text = LocalizationManager.Instance.getText("MENU_OK");
		skipButtonText.text = LocalizationManager.Instance.getText("MENU_SKIP");

		//If the player connected to Facebook in the Title Screen, display the player's first name in the User Name input field.
		if( FacebookManager.Instance.firstName != null ) userNameText.text = FacebookManager.Instance.firstName;
	}

	void Start()
	{
		playHerofrontAnims();
	}

	private void moveCompleted()
	{
		toggle = !toggle;
	}

	public void changeSelection()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		if( toggle )
		{
			//if toggle is true, the male hero is in the front and the female hero is in the back right
			//Player selected female character
			pickMeContentText.text = LocalizationManager.Instance.getText("MENU_PICK_ME_FEMALE");
			selectedAvatar = Avatar.Heroine;
			LeanTween.move( male, backLeftLocation, 1f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(moveCompleted).setOnCompleteParam(gameObject).setDelay(0);
			playHeroinefrontAnims();
			maleAnimation.Play( "Hero_Idle" );
			maleAnimation.CrossFadeQueued( "Waiting_in_the_back", 1.3f );
			LeanTween.move( female, frontLocation, 1f ).setEase(LeanTweenType.easeOutQuad);
		}
		else
		{
			//if toggle is false, the female hero is in the front and the male hero is in the back
			//Player selected male character
			pickMeContentText.text = LocalizationManager.Instance.getText("MENU_PICK_ME_MALE");
			selectedAvatar = Avatar.Hero;
			LeanTween.move( male, frontLocation, 1f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(moveCompleted).setOnCompleteParam(gameObject).setDelay(0);
			playHerofrontAnims();
			femaleAnimation.Play( "Heroine_Idle" );
			femaleAnimation.CrossFadeQueued( "Waiting_in_the_back", 1.3f );
			LeanTween.move( female, backRightLocation, 1f ).setEase(LeanTweenType.easeOutQuad);
		}
	}


	public void moveNextToPopup()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		if( toggle )
		{
			//if toggle is true, the male hero is in the front
			maleAnimation.Play( "Hero_Idle" );
			maleAnimation.CrossFadeQueued( "Waiting_in_the_back", 1.3f );
			LeanTween.move( male, nextToPopupLocation, 1f ).setEase(LeanTweenType.easeOutQuad);
		}
		else
		{
			//if toggle is false, the female hero is in the front
			femaleAnimation.Play( "Heroine_Idle" );
			femaleAnimation.CrossFadeQueued( "Waiting_in_the_back", 1.3f );
			LeanTween.move( female, nextToPopupLocation, 1f ).setEase(LeanTweenType.easeOutQuad);
		}
	}
	public void showUserNamePopup()
	{
		UISoundManager.uiSoundManager.playButtonClick();

		//hide content below popup
		titleText.gameObject.SetActive( false );
		pickMeContentText.gameObject.SetActive( false );
		changeButton.gameObject.SetActive(false);
		confirmButton.gameObject.SetActive(false);

		if( selectedAvatar == Avatar.Hero )
		{
			userNamePlaceholderText.text = LocalizationManager.Instance.getText("MALE_USER_NAME_PLACEHOLDER");
			female.SetActive( false );
		}
		else
		{
			userNamePlaceholderText.text = LocalizationManager.Instance.getText("FEMALE_USER_NAME_PLACEHOLDER");
			male.SetActive( false );
		}
		moveNextToPopup();
		userNamePopup.SetActive( true );
	}

	public void confirmSelection()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		
		//Save a default user name if the user has not entered anything
		if( userNameText.text == "")
		{
			if( selectedAvatar == Avatar.Hero )
			{
				userNameText.text = LocalizationManager.Instance.getText("DEFAULT_MALE_USER_NAME");
			}
			else
			{
				userNameText.text = LocalizationManager.Instance.getText("DEFAULT_FEMALE_USER_NAME");
			}
		}
		Debug.Log("User Name is : " + userNameText.text );
		PlayerStatsManager.Instance.saveUserName( userNameText.text );
		PlayerStatsManager.Instance.setAvatar(selectedAvatar);
		StartCoroutine( loadWorldMapAfterDelay( 0.9f ) );
	}

	IEnumerator loadWorldMapAfterDelay( float waitPeriod )
	{
		//Give time to the player to understand what is going on before loading the world map
		Handheld.StartActivityIndicator();
		yield return new WaitForSeconds( waitPeriod );
		SceneManager.LoadScene( (int) GameScenes.WorldMap );
	}

	void playHerofrontAnims()
	{
		maleAnimation.CrossFade( "Hero_Idle", 1f );
		maleAnimation.PlayQueued( "Hero_Var2" );
		maleAnimation.PlayQueued( "Hero_Idle" );
		maleAnimation.PlayQueued( "Hero_Var1" );
		maleAnimation.PlayQueued( "Hero_Idle_Loop" );
	}

	void playHeroinefrontAnims()
	{
		femaleAnimation.CrossFade( "Heroine_Idle", 1f );
		femaleAnimation.PlayQueued( "Heroine_Var1" );
		femaleAnimation.PlayQueued( "Heroine_Idle" );
		femaleAnimation.PlayQueued( "Heroine_Var2" );
		femaleAnimation.PlayQueued( "Heroine_Idle_Loop" );
	}
	
}
