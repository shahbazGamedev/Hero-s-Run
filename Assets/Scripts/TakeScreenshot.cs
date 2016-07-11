using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public enum PictureRatio {
	
	POLAROID_4_5 = 0,
	WIDE_SCREEN_16_9 = 1
}

public class TakeScreenshot : MonoBehaviour {

	int pictureWidth;
	const int MAX_PICTURE_WIDTH = 1536;
	const int MAX_PICTURE_HEIGHT = 1536;
	int pictureHeight;
	int borderWidth;
	Camera screenShotCamera;
	public Light pointLight;
	Texture2D screenShot;
	public Button cameraButton;
	public Button flipCameraButton;
	public Image picturePreview;
	public static bool selfieTaken = false;
	bool saveToFile = false; //for testing in Editor

	bool isFacingPlayer = true;
	Vector3 frontLocation = new Vector3( 0, 2f, 4.5f ); 	//Facing player
	Quaternion frontRotation = Quaternion.Euler( 9.36f, 180f, 0 );

	Vector3 backLocation = new Vector3( 0, 2f, -4.5f );	//Facing player's back
	Quaternion backRotation = Quaternion.Euler( 9.36f, 0, 0 );
	public PictureRatio pictureRatio = PictureRatio.POLAROID_4_5;

	void Awake()
	{
		screenShotCamera = GetComponent<Camera>();

		//Default value is facing player
		isFacingPlayer = true;
		screenShotCamera.transform.position = frontLocation;
		screenShotCamera.transform.rotation = frontRotation;

		screenShotCamera.enabled = false;
		pointLight.gameObject.SetActive( false );

		calculatePictureSize();

		for( int i = 0; i < screenShot.width; i++ )
		{
			for( int j = 0; j < screenShot.height; j++ )
			{
				screenShot.SetPixel(i,j,Color.white); //Sets the pixel to be white
			}
		}
        screenShot.Apply(); //Applies all the changes made
	}

	void calculatePictureSize()
	{
		if( pictureRatio == PictureRatio.POLAROID_4_5 )
		{
			pictureHeight = Screen.height;
			pictureHeight = Mathf.Min( pictureHeight, MAX_PICTURE_HEIGHT);
			pictureWidth = (int) ( pictureHeight * 4f/5f );
			borderWidth = (int) (pictureWidth * 0.05f);
		}
		else if ( pictureRatio == PictureRatio.WIDE_SCREEN_16_9 )
		{
			pictureWidth = Screen.width * 2;
			pictureWidth = Mathf.Min( pictureWidth, MAX_PICTURE_WIDTH );
			pictureHeight = (int) ( pictureWidth * 9f/16f );
			borderWidth = (int) (pictureHeight * 0.05f);
		}
		screenShot = new Texture2D(pictureWidth + 2 * borderWidth, pictureHeight + 2 * borderWidth, TextureFormat.RGB24, false);

	}

	string ScreenShotName(int width, int height)
	{
       return string.Format("{0}/screen_{1}x{2}_{3}.png", Application.dataPath, width, height, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}
    
	public void takeSelfieNow( )
	{
		StartCoroutine( takeSelfie( screenShotCamera ) );
	}

	public void flipCamera()
	{
		SoundManager.soundManager.playButtonClick();
		isFacingPlayer = !isFacingPlayer;
		if( isFacingPlayer )
		{
			screenShotCamera.transform.localPosition = frontLocation;
			screenShotCamera.transform.localRotation = frontRotation;
			Debug.Log("Flip Camera - Front");
		}
		else
		{
			screenShotCamera.transform.localPosition = backLocation;
			screenShotCamera.transform.localRotation = backRotation;
			Debug.Log("Flip Camera - Back");
		}
	}
	
	IEnumerator takeSelfie( Camera pictureCamera )
	{
		LeanTween.cancel( gameObject );
        yield return new WaitForEndOfFrame();
		GetComponent<AudioSource>().Play();
		Debug.Log("TakeScreenshot-selfie." );
		pictureCamera.enabled = true;
		pointLight.gameObject.SetActive( true );
		RenderTexture rt = new RenderTexture(pictureWidth, pictureHeight, 24);
		rt.antiAliasing = 4;
		rt.format = RenderTextureFormat.Default;
		pictureCamera.targetTexture = rt;
		pictureCamera.Render();
		RenderTexture.active = rt;
		screenShot.ReadPixels(new Rect(0, 0, pictureWidth, pictureHeight), borderWidth, borderWidth);
		screenShot.Apply();
		picturePreview.sprite = Sprite.Create( screenShot, new Rect(0, 0, screenShot.width, screenShot.height ), new Vector2( 0.5f, 0.5f ) );
		byte[] selfieBytes = screenShot.EncodeToPNG();

		//Because we will show the selfie in the social media popup which is in a different scene, we need to save the sprite in a location accessible to both.
		GameManager.Instance.selfie = picturePreview.sprite;
		GameManager.Instance.selfieBytes = selfieBytes;

		pictureCamera.targetTexture = null;
		RenderTexture.active = null; 
		Destroy(rt);
		pictureCamera.enabled = false;
		pointLight.gameObject.SetActive( false );
		selfieTaken = true;
		fadeInPicturePreview();
		
		if( saveToFile )
		{
			#if UNITY_EDITOR
			string filename = ScreenShotName(pictureWidth, pictureHeight);	
			System.IO.File.WriteAllBytes(filename, selfieBytes);
			Debug.Log(string.Format("Saved selfie to: {0}", filename));
			Application.OpenURL(filename);
			#endif
		}
	}
	
	void fadeInPicturePreview()
	{
		picturePreview.color = new Color( picturePreview.color.r, picturePreview.color.g, picturePreview.color.b, 0f );
		picturePreview.rectTransform.localScale = Vector3.zero;
		picturePreview.gameObject.SetActive( true );
		LeanTween.color( picturePreview.rectTransform, new Color( picturePreview.color.r, picturePreview.color.g, picturePreview.color.b, 1f ), 0.6f ).setOnComplete(fadeOutPicturePreview).setOnCompleteParam(gameObject);
		LeanTween.scale ( picturePreview.rectTransform, Vector3.one, 0.6f ).setEase(LeanTweenType.easeOutQuad);
	}

	void fadeOutPicturePreview()
	{
		LeanTween.color( picturePreview.rectTransform, new Color( picturePreview.color.r, picturePreview.color.g, picturePreview.color.b, 0 ), 0.6f ).setOnComplete(hidePicturePreview).setOnCompleteParam(gameObject).setDelay(3.5f);
		LeanTween.scale ( picturePreview.rectTransform, Vector3.zero, 0.6f ).setEase(LeanTweenType.easeOutQuad).setDelay(3.5f);
	}

	void hidePicturePreview()
	{
		picturePreview.gameObject.SetActive( false );
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		TakePictureTrigger.takePictureNowTrigger += TakePictureNowTrigger;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		TakePictureTrigger.takePictureNowTrigger -= TakePictureNowTrigger;
	}


	void GameStateChange( GameState newState )
	{
	
		if( newState == GameState.Normal )
		{
			cameraButton.gameObject.SetActive( true );
			flipCameraButton.gameObject.SetActive( true );
		}
		else
		{
			LeanTween.cancel( gameObject );
			cameraButton.gameObject.SetActive( false );
			flipCameraButton.gameObject.SetActive( false );
			hidePicturePreview();
		}
	}

	void TakePictureNowTrigger( Camera pictureCamera )
	{
		//We test against selfieTaken because we do not want to overide a picture taken by the player.
		if( !selfieTaken )
		{
			StartCoroutine( takeSelfie( pictureCamera ) );
		}
	}

}
