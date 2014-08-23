using UnityEngine;
using System.Collections;

public class LoadScreenHandler : MonoBehaviour {
	
	public Texture2D loadScreenBackground;
	public float scrollSpeed = 0.1f;
	Rect loadScreenBackgroundRect;
	Rect loadScreenBackgroundCoord;
	public float loadScreenDuration = 0.5f;
	GUIContent loadingTextContent;
	public GUIStyle loadScreenStyle;

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
		loadingTextContent = new GUIContent( LocalizationManager.Instance.getText("MENU_LOADING") );
		GameManager.Instance.loadVersionNumber();
		PlayerStatsManager.Instance.loadPlayerStats();
		#if UNITY_EDITOR
		if( UnityEditor.PlayerSettings.bundleVersion != GameManager.Instance.getVersionNumber() )
		{
			Debug.LogError("LoadScreenHandler: Version number mismatch. The version indicated in version.txt (" + GameManager.Instance.getVersionNumber() + ") does not match the bundle version in PlayerSettings (" + UnityEditor.PlayerSettings.bundleVersion + "). This needs to be corrected before submitting the build.");
		}
		#endif

		Debug.Log("LoadScreenHandler: Start : uses Facebook " + PlayerStatsManager.Instance.getUsesFacebook() );
		//If the player agreed to use Facebook, auto-login for him
		if( PlayerStatsManager.Instance.getUsesFacebook() && Application.internetReachability != NetworkReachability.NotReachable )
		{
			FacebookManager.Instance.CallFBInit();
		}
		StartCoroutine( WaitForLoadScreen() );

		loadScreenBackgroundRect = new Rect( 0,0, Screen.width, Screen.height );
		loadScreenBackgroundCoord = new Rect( 0,0,1f,1f );

		PopupHandler.changeFontSizeBasedOnResolution( loadScreenStyle );

	}

	private IEnumerator WaitForLoadScreen ()
	{	
		yield return new WaitForSeconds(loadScreenDuration);
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
	
	void OnGUI ()
	{
		//Draw a scrolling background 
		loadScreenBackgroundCoord.y = loadScreenBackgroundCoord.y - (scrollSpeed * Time.deltaTime);
		GUI.DrawTextureWithTexCoords( loadScreenBackgroundRect, loadScreenBackground, loadScreenBackgroundCoord );

		//Draw a centered text saying Loading...
		Rect textRect = GUILayoutUtility.GetRect( loadingTextContent, loadScreenStyle );
		float textCenterX = (Screen.width-textRect.width)/2f;
		Rect loadScreenRect = new Rect( textCenterX, Screen.height * 0.4f, textRect.width, textRect.height );
		Utilities.drawLabelWithDropShadow( loadScreenRect, loadingTextContent, loadScreenStyle );
	}

}
