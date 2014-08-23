using UnityEngine;
using System.Collections;

public enum Avatar {
	None = 0,
	Hero = 1,
	Heroine = 2
}


public class CharacterScreenMenu : MonoBehaviour {

	public GameObject male;
	public GameObject female;
	bool toggle = true;
	Vector3 frontLocation = new Vector3( 0,0,0);		//Selected
	Vector3 backLeftLocation = new Vector3(-1.6f,0,5.5f); //Male
	Vector3 backRightLocation = new Vector3(1.6f,0,5.5f);	//Female
	Animation maleAnimation;
	Animation femaleAnimation;
	float margin = 20f;
	Vector2 buttonSize = new Vector2( 100f, 100f );
	float buttonMargin = 20f;
	public GUIStyle avatarTextStyle;
	public GUIStyle buttonStyle;
	public GUIStyle titleStyle;
	GUIContent titleContent = new GUIContent(LocalizationManager.Instance.getText("MENU_CHARACTER_SELECTION_TITLE") );
	GUIContent pickMeContent = new GUIContent(System.String.Empty);
	GUIContent changeButtonContent = new GUIContent(LocalizationManager.Instance.getText("MENU_CHANGE") );
	GUIContent selectButtonContent = new GUIContent(LocalizationManager.Instance.getText("MENU_SELECT") );
	Avatar selectedAvatar = Avatar.Hero;

	// Use this for initialization
	void Awake () {
	
		//Transform CoreManagers = GameObject.FindGameObjectWithTag("CoreManagers").transform;
		//popupHandler = CoreManagers.GetComponent<PopupHandler>();
		maleAnimation = male.GetComponent<Animation>();
		femaleAnimation = female.GetComponent<Animation>();
		//By default, the male character is selected
		PlayerStatsManager.Instance.setAvatar(selectedAvatar);
		pickMeContent.text = LocalizationManager.Instance.getText("MENU_PICK_ME_MALE");
		avatarTextStyle.fixedWidth = Screen.width * 0.7f;

		PopupHandler.changeFontSizeBasedOnResolution( avatarTextStyle );
		PopupHandler.changeFontSizeBasedOnResolution( buttonStyle );
		PopupHandler.changeFontSizeBasedOnResolution( titleStyle );

	}

	void Start()
	{
		playHerofrontAnims();
	}

	// Update is called once per frame
	void Update () {
		handleKeyboard();
	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.LeftArrow) ) 
		{
			changeCharacter();
		}
		else if ( Input.GetKeyDown (KeyCode.RightArrow) ) 
		{
			//for debugging
			goToMainMenu();
		}
	}

	private void moveCompleted()
	{
		toggle = !toggle;
	}

	void OnGUI ()
	{
		renderMenu();
	}

	void changeCharacter()
	{
		if( toggle )
		{
			//if toggle is true, the male hero is in the front and the female hero is in the back right
			//Player selected female character
			pickMeContent.text = LocalizationManager.Instance.getText("MENU_PICK_ME_FEMALE");
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
			pickMeContent.text = LocalizationManager.Instance.getText("MENU_PICK_ME_MALE");
			selectedAvatar = Avatar.Hero;
			LeanTween.move( male, frontLocation, 1f ).setEase(LeanTweenType.easeOutQuad).setOnComplete(moveCompleted).setOnCompleteParam(gameObject).setDelay(0);
			playHerofrontAnims();
			femaleAnimation.Play( "Heroine_Idle" );
			femaleAnimation.CrossFadeQueued( "Waiting_in_the_back", 1.3f );
			LeanTween.move( female, backRightLocation, 1f ).setEase(LeanTweenType.easeOutQuad);
		}
	}

	void goToMainMenu()
	{
		//Go to main menu
		PlayerStatsManager.Instance.setAvatar(selectedAvatar);
		Application.LoadLevel( 2 );
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


	//Title (top-center)
	void drawTitle()
	{
		Rect textRect = GUILayoutUtility.GetRect( titleContent, titleStyle );
		float textCenterX = (Screen.width-textRect.width)/2f;
		Rect titleTextRect = new Rect( textCenterX, Screen.height * 0.15f, textRect.width, textRect.height );
		Utilities.drawLabelWithDropShadow( titleTextRect, titleContent, titleStyle );
	}

	//Title (top-center)
	void drawPickMeText()
	{
		Rect textRect = GUILayoutUtility.GetRect( pickMeContent, avatarTextStyle );
		float textCenterX = (Screen.width-textRect.width)/2f;
		Rect titleTextRect = new Rect( textCenterX, Screen.height * 0.75f, textRect.width, textRect.height );
		GUI.Label( titleTextRect, pickMeContent, avatarTextStyle );
	}

	public void renderMenu()
	{
		drawTitle();

		float height = Screen.height * 0.5f - buttonSize.y * 0.5f;
		if( GUI.Button( new Rect(margin,height,buttonSize.x,buttonSize.y), changeButtonContent) )
		{
			changeCharacter();
		}
		if( GUI.Button( new Rect(margin+2*buttonSize.x+2*buttonMargin,Screen.height * 0.85f,buttonSize.x,buttonSize.y), selectButtonContent)  )
		{
			goToMainMenu();
		}

		drawPickMeText();
	}

}
