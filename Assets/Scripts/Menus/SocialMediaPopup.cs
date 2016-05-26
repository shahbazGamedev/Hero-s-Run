using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Facebook.Unity;

public class SocialMediaPopup : MonoBehaviour {

	public GameObject postLevelPopupPanel;
	public Image picturePreview;

	// Use this for initialization
	void Start () {
	
	}
	
	public void showSocialMediaPopup()
	{
		//Reset value
		TakeScreenshot.selfieTaken = false;
		loadSocialMediaData();
		GetComponent<Animator>().Play("Panel Slide In");
	}

	private void loadSocialMediaData()
	{
		Debug.Log("loadSocialMediaData " );
		if( GameManager.Instance.selfie != null ) picturePreview.sprite = GameManager.Instance.selfie;
	}

	public void closeSocialMediaPopup()
	{
		Debug.Log("closeSocialMediaPopup");
		SoundManager.playButtonClick();
		GetComponent<Animator>().Play("Panel Slide Out");
		Invoke("showPostLevelPopup", 1f );
	}

	void showPostLevelPopup()
	{
		postLevelPopupPanel.GetComponent<PostLevelPopup>().showPostLevelPopup(LevelManager.Instance.getLevelData());

	}

	void shareOnFacebook()
	{
		WWWForm wwwForm = new WWWForm();
        wwwForm.AddBinaryData("image", GameManager.Instance.selfieBytes, "Hello!");
        wwwForm.AddField("message", "This is awesome.");
		FB.API("me/photos", HttpMethod.POST, TakeScreenshotCallback, wwwForm);
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
