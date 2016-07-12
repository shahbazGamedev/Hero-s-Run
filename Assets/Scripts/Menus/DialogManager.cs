using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour {

	public static DialogManager dialogManager = null;
	[Header("Achievement Display")]
	[Header("Message Panel")]
	public RectTransform messagePanel;
	Vector2 messagePanelDefaultPosition;
	public Image messageIcon;
	public Text messageText;
	[Header("Various portraits you can use")]
	public Image fairyPortrait;
	public Image darkQueenPortrait;
	public Image heroPortrait;

	Vector2 slideStartDest;
	Vector2 slideEndDest;
	const float SLIDE_DURATION = 0.6f;
	float waitDuration = 2.5f;

	//The string key (for example VO_FA_OH_NO) must be in uppercase. The localised text ID and the audio clip must have the same name.
	Dictionary<string, AudioClip> voiceOvers = new Dictionary<string, AudioClip>();
	
	// Use this for initialization
	void Awake () {
	
		dialogManager = this;
		messagePanelDefaultPosition = messagePanel.anchoredPosition;
		slideStartDest = new Vector2( 0, messagePanel.anchoredPosition.y );
		slideEndDest = new Vector2( messagePanel.rect.width, messagePanel.anchoredPosition.y );
		StartCoroutine( loadVoiceOvers () );
	}

	//Used by the Game Center achievement system.
	//Achievement image can be null
	public void activateDisplay( string message, Texture2D image )
	{
		if( image != null )
		{
			Sprite	customImage = Sprite.Create( image, new Rect(0, 0, image.width, image.height ), new Vector2( 0.5f, 0.5f ) );
			activateDisplay( message, customImage, 2.5f );
		}
		else
		{
			//Use default Hero portrait since we have no achievement image
			activateDisplay( message, heroPortrait.sprite, 2.5f );
		}	
	}

	public void activateDisplayGeneric( string message, Sprite portrait, float waitDuration )
	{
		activateDisplay( message, portrait, waitDuration );
	}

	public void activateDisplayFairy( string message, float waitDuration )
	{
		activateDisplay( message, fairyPortrait.sprite, waitDuration );
	}

	public void activateDisplayDarkQueen( string message, float waitDuration )
	{
		activateDisplay( message, darkQueenPortrait.sprite, waitDuration );
	}

	void activateDisplay( string message, Sprite icon, float waitDuration )
	{
		this.waitDuration = waitDuration;
		messageText.text = message;
		messageIcon.sprite = icon;
		slideInMessage();
	}

	void slideInMessage()
	{
		messagePanel.anchoredPosition = messagePanelDefaultPosition;
		LeanTween.move( messagePanel, slideStartDest, SLIDE_DURATION ).setEase(LeanTweenType.easeOutQuad).setOnComplete(slideOutMessage).setOnCompleteParam(gameObject);
	}
		
	void slideOutMessage()
	{
		//Wait a little before continuing to slide
		LeanTween.move( messagePanel, slideEndDest, SLIDE_DURATION ).setEase(LeanTweenType.easeOutQuad).setDelay(waitDuration);
	}
	
	public void hideMessage()
	{
		LeanTween.cancel(gameObject);
		messagePanel.anchoredPosition = messagePanelDefaultPosition;
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		PlayerController.playerStateChanged += PlayerStateChange;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerController.playerStateChanged -= PlayerStateChange;
	}
	
	void GameStateChange( GameState newState )
	{
		if( newState != GameState.Normal )
		{
			hideMessage();
		}
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			hideMessage();
		}
	}

	IEnumerator loadVoiceOvers ()
	{
		string languageName; //needs to be in lower case because of asset bundle loading
		SystemLanguage language = Application.systemLanguage;
		switch (language)
		{
			case SystemLanguage.English:
				languageName = "english";
				break;
				
			case SystemLanguage.French:
				languageName = "french";
				break;
	
			default:
				//The language is not supported. Default to English.
				languageName = "english";
				break;
		}

		AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(Application.streamingAssetsPath, "Voice Overs/voice overs." + languageName ));
		yield return bundleLoadRequest;
		
		AssetBundle voiceOverBundle = bundleLoadRequest.assetBundle;
		if (voiceOverBundle == null)
		{
			Debug.LogError("DialogManager-loadVoiceOvers error: Failed to load AssetBundle: " + "Voice Overs/voice overs." + languageName );
			yield break;
		}

		AssetBundleRequest assetLoadAllRequest = voiceOverBundle.LoadAllAssetsAsync<AudioClip>();
		yield return assetLoadAllRequest;
		Object[] prefabs = assetLoadAllRequest.allAssets;

		for( int i = 0; i < prefabs.Length; i++ )
		{
			voiceOvers.Add( prefabs[i].name, Instantiate<AudioClip>(prefabs[i] as AudioClip ) );
		}

		voiceOverBundle.Unload(false);

		/*AssetBundleRequest assetLoadRequest = voiceOverBundle.LoadAssetAsync<AudioClip>("VO_FA_OH_NO");
		//yield return assetLoadRequest;

		//AudioClip prefab = assetLoadRequest.asset as AudioClip;
		//AudioClip testAudioClip = Instantiate<AudioClip>(prefab);
		//print("test Audio Clip Name " + testAudioClip.name + " " + languageName );*/
		
	}
	
	public AudioClip getVoiceOver( string voiceOverName )
	{
		AudioClip voiceOverClip;
		if( voiceOvers.TryGetValue( voiceOverName, out voiceOverClip ) )
		{
			return voiceOverClip;
		}
		else
		{
			Debug.LogWarning("DialogManager-getVoiceOver error. Can't find " + voiceOverName + " in voice over dictionary." );
			return null;
		}
	}
}
