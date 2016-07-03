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
	Animation maleAnimation;
	Animation femaleAnimation;

	Avatar selectedAvatar = Avatar.Hero;

	public Text titleText;
	public Text pickMeContentText;
	public Text changeButtonText;
	public Button confirmButton;
	public Text confirmButtonText;

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
		SoundManager.soundManager.playButtonClick();
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

	public void confirmSelection()
	{
		SoundManager.soundManager.playButtonClick();
		//It looks nicer if we center the text and hide the button to confirm to the player that his action worked
		pickMeContentText.alignment = TextAnchor.UpperCenter;
		confirmButton.gameObject.SetActive(false);
		if( selectedAvatar == Avatar.Hero )
		{
			pickMeContentText.text = LocalizationManager.Instance.getText("MENU_THANKS_CHOOSING_ME_MALE");
		}
		else
		{
			pickMeContentText.text = LocalizationManager.Instance.getText("MENU_THANKS_CHOOSING_ME_FEMALE");
		}

		PlayerStatsManager.Instance.setAvatar(selectedAvatar);
		StartCoroutine( loadWorldMapAfterDelay( 2.25f ) );
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
