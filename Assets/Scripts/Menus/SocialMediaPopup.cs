using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Facebook.Unity;

public class SocialMediaPopup : MonoBehaviour {

	public GameObject postLevelPopupPanel;
	public Image picturePreview;
	public Text episodeNameText;
	public Text messageText;
	public Text shareButtonText;
	string facebookMessage;

	// Use this for initialization
	void Start ()
 	{
		int episodeNumber = LevelManager.Instance.getCurrentEpisodeNumber();
		string levelNumberString = (episodeNumber + 1).ToString();
		episodeNameText.text = LocalizationManager.Instance.getText("EPISODE_NAME_" + levelNumberString );
		messageText.text = LocalizationManager.Instance.getText("SOCIAL_MEDIA_MESSAGE");
		shareButtonText.text = LocalizationManager.Instance.getText("SOCIAL_MEDIA_SHARE_BUTTON");
		facebookMessage = LocalizationManager.Instance.getText("SOCIAL_MEDIA_FACEBOOK_MESSAGE");
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
		freeUpPictureMemory();
		GetComponent<Animator>().Play("Panel Slide Out");
		Invoke("showPostLevelPopup", 1f );
	}

	void showPostLevelPopup()
	{
		postLevelPopupPanel.GetComponent<PostLevelPopup>().showPostLevelPopup(LevelManager.Instance.getLevelData());

	}

	void freeUpPictureMemory()
	{
		GameManager.Instance.selfie = null;
		GameManager.Instance.selfieBytes = null;
	}

	public void shareOnFacebook()
	{
		SoundManager.playButtonClick();
		WWWForm wwwForm = new WWWForm();
        wwwForm.AddBinaryData("image", GameManager.Instance.selfieBytes, "Hello!");
        wwwForm.AddField("message", facebookMessage );
		FB.API("me/photos", HttpMethod.POST, TakeScreenshotCallback, wwwForm);
		freeUpPictureMemory();
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
