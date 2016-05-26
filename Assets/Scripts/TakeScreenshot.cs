using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class TakeScreenshot : MonoBehaviour {

	int pictureWidth;
	const int MAX_PICTURE_WIDTH = 1024;
	int pictureHeight;
	int borderWidth;
	Camera screenShotCamera;
	public Light pointLight;
	Texture2D screenShot;
	public Button cameraButton;
	public Image picturePreview;
	public static bool selfieTaken = false;
	bool saveToFile = false; //for testing in Editor

	bool isFacingPlayer = true;
	Vector3 frontLocation = new Vector3( 0, 2f, 4.5f ); 	//Facing player
	Quaternion frontRotation = Quaternion.Euler( 9.36f, 180f, 0 );

	Vector3 backLocation = new Vector3( 0, 2f, -4.5f );	//Facing player's back
	Quaternion backRotation = Quaternion.Euler( 9.36f, 0, 0 );


	void Awake()
	{
		screenShotCamera = GetComponent<Camera>();

		//Default value is facing player
		isFacingPlayer = true;
		screenShotCamera.transform.position = frontLocation;
		screenShotCamera.transform.rotation = frontRotation;

		screenShotCamera.enabled = false;
		pictureWidth = Screen.width * 2;
		pictureWidth = Mathf.Min( pictureWidth, MAX_PICTURE_WIDTH );
		pictureHeight = (int) ( pictureWidth * 9f/16f );
		borderWidth = (int) (pictureHeight * 0.06f);
		screenShot = new Texture2D(pictureWidth + 2 * borderWidth, pictureHeight + 2 * borderWidth, TextureFormat.RGB24, false);
		for( int i = 0; i < screenShot.width; i++ )
		{
			for( int j = 0; j < screenShot.height; j++ )
			{
				screenShot.SetPixel(i,j,Color.white); //Sets the pixel to be white
			}
		}
        screenShot.Apply(); //Applies all the changes made
		pointLight.gameObject.SetActive( false );
	}
           
	string ScreenShotName(int width, int height)
	{
       return string.Format("{0}/screen_{1}x{2}_{3}.png", Application.dataPath, width, height, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}
    
	public void takeSelfieNow()
	{
		StartCoroutine( takeSelfie() );
	}

	public void flipCamera()
	{
		SoundManager.playButtonClick();
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
	
	IEnumerator takeSelfie()
	{
        yield return new WaitForEndOfFrame();
		GetComponent<AudioSource>().Play();
		Debug.Log("TakeScreenshot-selfie." );
		screenShotCamera.enabled = true;
		pointLight.gameObject.SetActive( true );
		RenderTexture rt = new RenderTexture(pictureWidth, pictureHeight, 24);
		rt.antiAliasing = 4;
		rt.format = RenderTextureFormat.Default;
		screenShotCamera.targetTexture = rt;
		screenShotCamera.Render();
		RenderTexture.active = rt;
		screenShot.ReadPixels(new Rect(0, 0, pictureWidth, pictureHeight), borderWidth, borderWidth);
		screenShot.Apply();
		picturePreview.sprite = Sprite.Create( screenShot, new Rect(0, 0, screenShot.width, screenShot.height ), new Vector2( 0.5f, 0.5f ) );
		byte[] selfieBytes = screenShot.EncodeToPNG();

		//Because we will show the selfie in the social media popup which is in a different scene, we need to save the sprite in a location accessible to both.
		GameManager.Instance.selfie = picturePreview.sprite;
		GameManager.Instance.selfieBytes = selfieBytes;

		screenShotCamera.targetTexture = null;
		RenderTexture.active = null; 
		Destroy(rt);
		screenShotCamera.enabled = false;
		pointLight.gameObject.SetActive( false );
		selfieTaken = true;
		Invoke( "showPicturePreview", 1f );
		
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
	
	void showPicturePreview()
	{
		picturePreview.gameObject.SetActive( true );
		Invoke( "hidePicturePreview", 5f );
	}

	void hidePicturePreview()
	{
		picturePreview.gameObject.SetActive( false );
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
	}


	void GameStateChange( GameState newState )
	{
	
		if( newState == GameState.Normal )
		{
			cameraButton.gameObject.SetActive( true );
		}
		else
		{
			CancelInvoke();
			cameraButton.gameObject.SetActive( false );
			picturePreview.gameObject.SetActive( false );
		}
	}

}
