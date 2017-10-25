using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Facebook.Unity;
using System;

public class MessageEntry : MonoBehaviour {

	[Header("Shared")]
	public MessageManager messageManager;
	public Image portrait; //requires a FacebookPortraitHandler component
	public Text message;
	[Header("Life Message Entry Only")]
	public Text actionButtonText; //either Send or Accept
	[Header("Challenge Message Entry Only")]
	public Image challengeImage;
	public Text acceptButtonText;
	public Text dismissButtonText;

	AppRequestData requestData;

	public void initializeMessage( AppRequestData requestData )
	{
		this.requestData = requestData;
		portrait.GetComponent<FacebookPortraitHandler>().setPortrait( requestData.fromID );
		string levelNumberString = string.Empty;
		string episodeName = string.Empty;
		switch (requestData.dataType)
		{
			case RequestDataType.Ask_Give_Life:
				actionButtonText.text =  LocalizationManager.Instance.getText("POPUP_BUTTON_SEND"); 
				message.text = LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TEXT_ASK_LIFE" );		//Mary asks you to send a life!
				message.text = message.text.Replace("<first name>", requestData.fromFirstName );
				break;
			case RequestDataType.Accept_Give_Life:
				actionButtonText.text =  LocalizationManager.Instance.getText("POPUP_BUTTON_ACCEPT"); 
				message.text = LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TEXT_RECEIVED_GIFT" );	//Bob offered you a life!
				message.text = message.text.Replace("<first name>", requestData.fromFirstName );
				break;
			case RequestDataType.Challenge:
				acceptButtonText.text =  LocalizationManager.Instance.getText("POPUP_BUTTON_ACCEPT"); 
				dismissButtonText.text =  LocalizationManager.Instance.getText("POPUP_BUTTON_DISMISS"); 
				message.text = LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TEXT_IS_CHALLENGING" );	//Bob is challenging you to beat his score of <score> in the <episode name> level.
				message.text = message.text.Replace("<first name>", requestData.fromFirstName );
				message.text = message.text.Replace("<score>", requestData.dataNumber1.ToString() );
				//dataNumber2 contains the episode number
				levelNumberString = (requestData.dataNumber2 + 1).ToString();
				episodeName = LocalizationManager.Instance.getText("EPISODE_NAME_" + levelNumberString );
				message.text = message.text.Replace("<episode name>", episodeName );
				break;
			case RequestDataType.ChallengeBeaten:
				acceptButtonText.text =  LocalizationManager.Instance.getText("POPUP_BUTTON_REVENGE"); 
				dismissButtonText.text =  LocalizationManager.Instance.getText("POPUP_BUTTON_DISMISS"); 
				message.text = LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TEXT_CHALLENGE_BEATEN" );	//<first name> beat your challenge with a score of <score> in <episode name>. Defend your honor!
				message.text = message.text.Replace("<first name>", requestData.fromFirstName );
				message.text = message.text.Replace("<score>", requestData.dataNumber1.ToString() );
				//dataNumber2 contains the episode number
				levelNumberString = (requestData.dataNumber2 + 1).ToString();
				episodeName = LocalizationManager.Instance.getText("EPISODE_NAME_" + levelNumberString );
				message.text = message.text.Replace("<episode name>", episodeName );
				break;
		}
	}

	public void buttonPressed()
	{
		//In all cases, do the following:
		FacebookManager.Instance.deleteAppRequest( requestData.appRequestID );
		requestData.hasBeenProcessed = true;
		requestData.processed_time = DateTime.Now;
		messageManager.decrementNumberOfMessages();
		
		switch (requestData.dataType)
		{
			case RequestDataType.Ask_Give_Life:
				FacebookManager.Instance.CallAppRequestAsDirectRequest(LocalizationManager.Instance.getText("MESSAGE_ENTRY_TITLE_EXTRA_LIFE"), LocalizationManager.Instance.getText("FB_HAVE_A_LIFE_MESSAGE"), requestData.fromID, "Accept_Give_Life," + requestData.dataNumber1.ToString() + ",-1" + ",N/A", MCHCallback, requestData.appRequestID );
				GameObject.Destroy( gameObject );
				break;
			case RequestDataType.Accept_Give_Life:
				PlayerStatsManager.Instance.increaseLives(1);
				PlayerStatsManager.Instance.savePlayerStats();
				GameObject.Destroy( gameObject );
				break;
			case RequestDataType.Challenge:
				//Save challenge
				messageManager.challengeBoard.addChallenge( requestData.fromFirstName, requestData.fromID, requestData.dataString1, requestData.dataNumber1, requestData.dataNumber2, requestData.created_time );
				//Update the challenge details below the episode station
				messageManager.newWorldMapHandler.updateChallengeDetails();
				GameObject.Destroy( gameObject );
				break;
			case RequestDataType.ChallengeBeaten:
				//Save challenge
				messageManager.challengeBoard.addChallenge( requestData.fromFirstName, requestData.fromID, requestData.dataString1, requestData.dataNumber1, requestData.dataNumber2, requestData.created_time );
				//Update the challenge details below the episode station
				messageManager.newWorldMapHandler.updateChallengeDetails();
				GameObject.Destroy( gameObject );
				break;
		}
	}

	//Called by the dismiss button on a message entry
	public void dismiss()
	{
		FacebookManager.Instance.deleteAppRequest( requestData.appRequestID );
		requestData.hasBeenProcessed = true;
		messageManager.decrementNumberOfMessages();
		GameObject.Destroy( gameObject );
	}

	public void MCHCallback(IAppRequestResult result, string appRequestIDToDelete )
	{
		if (result.Error != null)
		{
			Debug.Log ("MessageEntry-MCHCallback error:\n" + result.Error );
		}
		else
		{
			Debug.Log ("MessageEntry-MCHCallback success:\n" + result.RawResult );
			if( !result.RawResult.Contains("cancelled") )
			{
				//Now that it is successfully processed, delete the app request on Facebook
				FacebookManager.Instance.deleteAppRequest( appRequestIDToDelete );
			}
			else
			{
				Debug.Log ("MessageEntry-MCHCallback user cancelled." );
			}
		}
	}

}
