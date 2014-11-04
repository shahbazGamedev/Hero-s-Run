using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//This menu displays the game logo
public class LoadScreenHandler : MonoBehaviour {
	
	public Text gameTitle;		//Needed to localize game title
	const float WAIT_TIME = 2f; //Time before loading main menu

	// Use this for initialization
	void Start ()
	{
		Application.targetFrameRate = 60;

		#if UNITY_IPHONE
		Handheld.SetActivityIndicatorStyle(iOSActivityIndicatorStyle.WhiteLarge);
		#elif UNITY_ANDROID
		Handheld.SetActivityIndicatorStyle(AndroidActivityIndicatorStyle.Large;
		#endif

		LocalizationManager.Instance.initialize();
		gameTitle.text = LocalizationManager.Instance.getText("GAME_TITLE");

		GameManager.Instance.loadVersionNumber();
		#if UNITY_EDITOR
		if( UnityEditor.PlayerSettings.bundleVersion != GameManager.Instance.getVersionNumber() )
		{
			Debug.LogError("LoadScreenHandler: Version number mismatch. The version indicated in version.txt (" + GameManager.Instance.getVersionNumber() + ") does not match the bundle version in PlayerSettings (" + UnityEditor.PlayerSettings.bundleVersion + "). This needs to be corrected before submitting the build.");
		}
		#endif

		PlayerStatsManager.Instance.loadPlayerStats();

		Debug.Log("LoadScreenHandler: Start : uses Facebook " + PlayerStatsManager.Instance.getUsesFacebook() );
		//If the player agreed to use Facebook, auto-login for him
		if( PlayerStatsManager.Instance.getUsesFacebook() && Application.internetReachability != NetworkReachability.NotReachable )
		{
			FacebookManager.Instance.CallFBInit();
		}
		StartCoroutine( WaitForLoadScreen() );

	}

	private IEnumerator WaitForLoadScreen ()
	{	
		Handheld.StartActivityIndicator();
		yield return new WaitForSeconds(WAIT_TIME);
		if( PlayerStatsManager.Instance.getAvatar() == Avatar.None )
		{
			//Bring the player to the character selection screen
			//In build settings, the character selection screen has an index of 1
			Application.LoadLevel( 1 );
		}
		else
		{
			//Player has already selected an avatar, go directly to the main menu
			//In build settings, main menu has an index of 2
			//for debugging
			Application.LoadLevel( 2 );
		}
	}
	
}
