using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Facebook.Unity;

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
				string levelNumberString = (requestData.dataNumber2 + 1).ToString();
				string episodeName = LocalizationManager.Instance.getText("EPISODE_NAME_" + levelNumberString );
				//Use rich text
				episodeName = "<color=#D22626FF>" + episodeName + "</color>"; //Red color
				message.text = message.text.Replace("<episode name>", episodeName );
				break;
		}
	}

	public void buttonPressed()
	{
		Debug.Log("MessageEntry-buttonPressed: " + requestData.dataType );
		switch (requestData.dataType)
		{
			case RequestDataType.Ask_Give_Life:
				FacebookManager.Instance.CallAppRequestAsDirectRequest("App Requests", LocalizationManager.Instance.getText("FB_HAVE_A_LIFE_MESSAGE"), requestData.fromID, "Accept_Give_Life," + requestData.dataNumber1.ToString() + ",-1", MCHCallback, requestData.appRequestID );
				requestData.hasBeenProcessed = true;
				GameObject.Destroy( gameObject );
				break;
			case RequestDataType.Accept_Give_Life:
				Debug.Log("MessageEntry-buttonPressed: Accept_Give_Life" );
				PlayerStatsManager.Instance.increaseLives(1);
				PlayerStatsManager.Instance.savePlayerStats();
				//Now that it is successfully processed, delete the app request on Facebook
				FacebookManager.Instance.deleteAppRequest( requestData.appRequestID );
				requestData.hasBeenProcessed = true;
				GameObject.Destroy( gameObject );
				break;
			case RequestDataType.Challenge:
				Debug.Log("MessageEntry-buttonPressed: Challenge" );
				FacebookManager.Instance.deleteAppRequest( requestData.appRequestID );
				requestData.hasBeenProcessed = true;
				//Save challenge
				messageManager.challengeBoard.addChallenge( requestData.fromFirstName, requestData.fromID, requestData.dataNumber1, requestData.dataNumber2, requestData.created_time );
				//We don't have time to slide it out, so simply hide it
				messageManager.gameObject.SetActive( false );
				//Make sure we are in the endless running mode
				if( GameManager.Instance.getGameMode() != GameMode.Endless ) messageManager.newWorldMapHandler.toggleGameMode();
				messageManager.newWorldMapHandler.play( requestData.dataNumber2, LevelManager.Instance.getLevelNumberFromEpisodeNumber( requestData.dataNumber2 ) );
				break;
			case RequestDataType.Unknown:
				Debug.LogWarning("MessageEntry-buttonPressed: unknown data type specified." );
				break;
		}

	}

	public void dismiss()
	{
		FacebookManager.Instance.deleteAppRequest( requestData.appRequestID );
		requestData.hasBeenProcessed = true;
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
