using UnityEngine;
using System.Collections;
using Facebook.Unity;
using UnityEngine.UI;

public class TakeScreenshot : MonoBehaviour {

	int pictureWidth; 
	int pictureHeight;
	int borderWidth;
	Camera screenShotCamera;
	public Light pointLight;
	Texture2D screenShot;
	public Image picturePreview;

	void Awake()
	{
		screenShotCamera = GetComponent<Camera>();
		screenShotCamera.enabled = false;
		pictureWidth = Screen.width * 2;
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
		screenShotCamera.targetTexture = null;
		RenderTexture.active = null; 
		Destroy(rt);
		screenShotCamera.enabled = false;
		pointLight.gameObject.SetActive( false );
		Invoke( "showPicturePreview", 1f );

		/*byte[] bytes = screenShot.EncodeToPNG();

		#if !UNITY_EDITOR
        WWWForm wwwForm = new WWWForm();
        wwwForm.AddBinaryData("image", bytes, "Hello!");
        wwwForm.AddField("message", "This is awesome.");
		FB.API("me/photos", HttpMethod.POST, TakeScreenshotCallback, wwwForm);
		#endif

		#if UNITY_EDITOR
		string filename = ScreenShotName(pictureWidth, pictureHeight);	
		System.IO.File.WriteAllBytes(filename, bytes);
		Debug.Log(string.Format("Saved selfie to: {0}", filename));
		Application.OpenURL(filename);
		#endif
		*/
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

	void TakeScreenshotCallback(IGraphResult result)
	{
		if (result.Error != null)
		{
			Debug.LogWarning("TakeScreenshot-TakeScreenshotCallback: error: " + result.Error );
		}
		else
		{
			Debug.Log("TakeScreenshot-TakeScreenshotCallback: success: " + result.RawResult );
		}
	}

}
