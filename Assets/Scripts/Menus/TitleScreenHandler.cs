using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System;

public class TitleScreenHandler : MonoBehaviour {

	[Header("Title Screen")]
	[SerializeField] AudioMixer mainMixer;
	[SerializeField] Slider progressBar;
	[SerializeField] Text progressBarPercentage;

	void Awake ()
	{
		//Handheld.StopActivityIndicator();
		initialiseGame();
	}

	void initialiseGame()
	{
		Application.targetFrameRate = 60;

		//Get the level data. Level Data has the parameters for all the levels of the game.
		LevelData levelData = GameObject.FindObjectOfType<LevelData>();
		//Now set it in the LevelManager
		LevelManager.Instance.setLevelData( levelData );

		#if UNITY_IPHONE
		Handheld.SetActivityIndicatorStyle(UnityEngine.iOS.ActivityIndicatorStyle.WhiteLarge);
		#elif UNITY_ANDROID
		Handheld.SetActivityIndicatorStyle(AndroidActivityIndicatorStyle.Large;
		#endif

		LocalizationManager.Instance.initialize();

		GameManager.Instance.loadVersionNumber();
		#if UNITY_EDITOR
		if( UnityEditor.PlayerSettings.bundleVersion != GameManager.Instance.getVersionNumber() )
		{
			Debug.LogError("TitleScreenHandler: Version number mismatch. The version indicated in version.txt (" + GameManager.Instance.getVersionNumber() + ") does not match the bundle version in PlayerSettings (" + UnityEditor.PlayerSettings.bundleVersion + "). This needs to be corrected before submitting the build.");
		}
		#endif

		PlayerStatsManager.Instance.loadPlayerStats();

		//If the player agreed to use Facebook, auto-login for him
		//We don't want to do this in the Editor as it slows the work flow since it causes a popup to appear
		#if !UNITY_EDITOR
		if( PlayerStatsManager.Instance.getUsesFacebook() && Application.internetReachability != NetworkReachability.NotReachable )
		{
			FacebookManager.Instance.CallFBInit();
		}
		#endif
	}

	void Start()
	{
		mainMixer.SetFloat( "SoundEffectsVolume", PlayerStatsManager.Instance.getSoundFxVolume() );
		mainMixer.SetFloat( "MusicVolume", PlayerStatsManager.Instance.getMusicVolume() );
		StartCoroutine( loadMainMenu( 2.5f ) );
	}
	
	IEnumerator loadMainMenu( float timeToLoad )
	{
		float elapsedTime = 0;
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			progressBar.value = elapsedTime/timeToLoad;
			progressBarPercentage.text = String.Format("{0:P0}", progressBar.value );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime <= timeToLoad );
		SceneManager.LoadScene( (int)GameScenes.MainMenu);
	}


	
}
