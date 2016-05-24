using UnityEngine;
using System.Collections;

public class TakeScreenshot : MonoBehaviour {

	int resWidth; 
	int resHeight;
	int borderWidth;
	Camera screenShotCamera;
	public Light pointLight;
	Texture2D screenShot;
                
	bool takeHiResShot = false;
    
	void Awake()
	{
		screenShotCamera = GetComponent<Camera>();
		screenShotCamera.enabled = false;
		resWidth = Screen.width * 2;
		resHeight = (int) ( resWidth * 9f/16f );
		borderWidth = (int) (resHeight * 0.06f);
		screenShot = new Texture2D(resWidth + 2 * borderWidth, resHeight + 2 * borderWidth, TextureFormat.RGB24, false);
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
           
	public static string ScreenShotName(int width, int height)
	{
       return string.Format("{0}/screen_{1}x{2}_{3}.png", Application.dataPath, width, height, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}
    
	public void takeHiResShotNow()
	{
		takeHiResShot = true;
	}
    
	void LateUpdate()
	{
		takeHiResShot |= Input.GetKeyDown("l");
		if (takeHiResShot) 
		{
			screenShotCamera.enabled = true;
			pointLight.gameObject.SetActive( true );
			RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
			rt.antiAliasing = 4;
			rt.format = RenderTextureFormat.Default;
			screenShotCamera.targetTexture = rt;
			screenShotCamera.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), borderWidth, borderWidth);
			screenShotCamera.targetTexture = null;
			RenderTexture.active = null; 
			Destroy(rt);
			byte[] bytes = screenShot.EncodeToPNG();
			string filename = ScreenShotName(resWidth, resHeight);
			
			System.IO.File.WriteAllBytes(filename, bytes);
			Debug.Log(string.Format("Took screenshot to: {0}", filename));
			Application.OpenURL(filename);
			takeHiResShot = false;
			screenShotCamera.enabled = false;
			pointLight.gameObject.SetActive( false );
			GetComponent<AudioSource>().Play();
		}
	}

}
