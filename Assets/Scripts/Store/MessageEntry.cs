using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Facebook.Unity;

public class MessageEntry : MonoBehaviour {

	[Header("Message Entry")]
	public Image icon;
	public Text message;
	AppRequestData requestData;

	public void initializeMessage( AppRequestData requestData )
	{
		this.requestData = requestData;
		message.text = requestData.dataType.ToString() + " " + requestData.fromFirstName;
		icon.GetComponent<FacebookPortraitHandler>().setPortrait( requestData.fromID );
	}

	public void buttonPressed()
	{
		Debug.Log("MessageEntry-buttonPressed: " + requestData.dataType );
		switch (requestData.dataType)
		{
			case RequestDataType.Ask_Give_Life:
				FacebookManager.Instance.CallAppRequestAsDirectRequest("App Requests", LocalizationManager.Instance.getText("FB_HAVE_A_LIFE_MESSAGE"), requestData.fromID, "Accept_Give_Life," + requestData.dataNumber.ToString(), MCHCallback, requestData.appRequestID );
				break;
			case RequestDataType.Accept_Give_Life:
				Debug.Log("MessageEntry-buttonPressed: Accept_Give_Life" );
				PlayerStatsManager.Instance.increaseLives(1);
				PlayerStatsManager.Instance.savePlayerStats();
				//Now that it is successfully processed, delete the app request on Facebook
				//FacebookManager.Instance.deleteAppRequest( appRequestData.appRequestID );
				//Slide out the message entry and then remove it from the Content RectTransform
				GameObject.Destroy( gameObject );
				break;
			case RequestDataType.Unknown:
				Debug.LogWarning("MessageEntry-processNextAppRequest: unknown data type specified." );
				break;
		}

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
				//FacebookManager.Instance.deleteAppRequest( appRequestIDToDelete );
			}
			else
			{
				Debug.Log ("MessageEntry-MCHCallback user cancelled." );
			}
		}
	}

}
