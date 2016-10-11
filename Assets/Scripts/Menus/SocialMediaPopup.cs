using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Facebook.Unity;

public class SocialMediaPopup : MonoBehaviour {

	public GameObject postLevelPopupPanel;
	public GameObject endlessPostLevelPopupPanel;
	public Image picturePreview;
	public Text episodeNameText;
	public Text messageText;
	public Button shareButton;
	public Text shareButtonText;
	public int numberOfLivesAsReward = 3;
	string facebookMessage;

	int ellipisCounter = 0;

	// Use this for initialization
	void Start ()
 	{
		int episodeNumber = LevelManager.Instance.getCurrentEpisodeNumber();
		string levelNumberString = (episodeNumber + 1).ToString();
		episodeNameText.text = LocalizationManager.Instance.getText("EPISODE_NAME_" + levelNumberString );
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
		shareButton.interactable = true;
		if( GameManager.Instance.selfie != null ) picturePreview.sprite = GameManager.Instance.selfie;
		if( PlayerStatsManager.Instance.getSharedOnFacebook() )
		{
			messageText.text = LocalizationManager.Instance.getText("SOCIAL_MEDIA_MESSAGE");
		}
		else
		{
			messageText.text = LocalizationManager.Instance.getText("SOCIAL_MEDIA_MESSAGE_NOT_POSTED_YET");
			messageText.text = messageText.text.Replace("<quantity>", numberOfLivesAsReward.ToString() );
		}
	}

	public void closeSocialMediaPopup()
	{
		Debug.Log("closeSocialMediaPopup");
		SoundManager.soundManager.playButtonClick();
		freeUpPictureMemory();
		GetComponent<Animator>().Play("Panel Slide Out");
		Invoke("showPostLevelPopup", 1f );
		CancelInvoke("animateEllipsis");
	}

	void showPostLevelPopup()
	{
		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			postLevelPopupPanel.GetComponent<PostLevelPopup>().showPostLevelPopup(LevelManager.Instance.getLevelData());
		}
		else
		{
			endlessPostLevelPopupPanel.GetComponent<EndlessPostLevelPopup>().showEndlessPostLevelPopup(LevelManager.Instance.getLevelData());
		}
	}

	void freeUpPictureMemory()
	{
		GameManager.Instance.selfie = null;
		GameManager.Instance.selfieBytes = null;
	}

	public void shareOnFacebook()
	{
		shareButton.interactable = false;
		SoundManager.soundManager.playButtonClick();
		WWWForm wwwForm = new WWWForm();
        wwwForm.AddBinaryData("image", GameManager.Instance.selfieBytes, "Hello!");
        wwwForm.AddField("message", facebookMessage );
		FB.API("me/photos", HttpMethod.POST, TakeScreenshotCallback, wwwForm);
		freeUpPictureMemory();
		InvokeRepeating("animateEllipsis", 0, 0.6f );

	}

	void animateEllipsis()
	{
		string ellipis;
		if( ellipisCounter == 0 )
		{
			ellipis = string.Empty;
		}
		else if( ellipisCounter == 1 )
		{
			ellipis = " .";
		}
		else if( ellipisCounter == 2 )
		{
			ellipis = " ..";
		}
		else
		{
			ellipis = " ...";
		}
		messageText.text = LocalizationManager.Instance.getText("SOCIAL_MEDIA_SENDING_MESSAGE") + ellipis;
		
		ellipisCounter++;
		if( ellipisCounter == 4 ) ellipisCounter = 0;
	}

	void TakeScreenshotCallback(IGraphResult result)
	{
		CancelInvoke("animateEllipsis");
		if (result.Error != null)
		{
			Debug.LogWarning("TakeScreenshot-TakeScreenshotCallback: error: " + result.Error );
			messageText.text = LocalizationManager.Instance.getText("SOCIAL_MEDIA_ERROR_OCCURRED");
		}
		else
		{
			Debug.Log("TakeScreenshot-TakeScreenshotCallback: success: " + result.RawResult );
			//Give a reward to the player the first time he posts an image on Facebook
			if( PlayerStatsManager.Instance.getSharedOnFacebook() )
			{
				messageText.text = LocalizationManager.Instance.getText("SOCIAL_MEDIA_POSTED_SUCCESSFULLY");
			}
			else
			{
				PlayerStatsManager.Instance.setSharedOnFacebook( true );
				PlayerStatsManager.Instance.increaseLives( numberOfLivesAsReward );
				PlayerStatsManager.Instance.savePlayerStats();
				messageText.text = LocalizationManager.Instance.getText("SOCIAL_MEDIA_POSTED_FIRST_TIME");
				messageText.text = messageText.text.Replace("<quantity>", numberOfLivesAsReward.ToString() );
			}
		}
	}

}
