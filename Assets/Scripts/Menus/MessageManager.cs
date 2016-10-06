using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour {

	[Header("Menu Prefabs")]
	public Text titleText;
	public RectTransform content;
	public GameObject lifeMessageEntryPrefab;
	public GameObject challengeMessageEntryPrefab;
	public GameObject mailInformationPanel;
	public Text mailInformationText;
	public Text numberOfMessages; //See also NewWorldMapHandler
	public ChallengeBoard challengeBoard;
	public NewWorldMapHandler newWorldMapHandler;

	void Start()
	{
		titleText.text = LocalizationManager.Instance.getText("POPUP_TITLE_MESSAGE_CENTER");
		if( PlayerStatsManager.Instance.getChallenges() != string.Empty )
		{
			 challengeBoard = JsonUtility.FromJson<ChallengeBoard>(PlayerStatsManager.Instance.getChallenges());
		}
		else
		{
			challengeBoard = new ChallengeBoard();
		}
		//For testing
		challengeBoard.addChallenge( "Suzie", "90", 2000, 1, System.DateTime.Now );
		challengeBoard.addChallenge( "Bob", "50", 1000, 1, System.DateTime.Now );
		challengeBoard.addChallenge( "Suzie", "90", 3000, 1, System.DateTime.Now );
		challengeBoard.addChallenge( "Bob", "50", 99900, 1, System.DateTime.Now );

		//Make sure the GameManager has a reference since we need access while running in a level
		GameManager.Instance.challengeBoard = challengeBoard;
	}

	public void refreshMessages ()
	{
		//Remove any previous entries
		clearAllEntries();

		//Player is connected to the Internet
		if( Application.internetReachability != NetworkReachability.NotReachable )
		{
			if( FacebookManager.Instance.AppRequestDataList.Count > 0 )
			{
				GameObject go;
				//Player has mail and is connected to the Internet.
				//Do not add the entry if the RequestDataType is Unknown
				mailInformationPanel.SetActive( false );
				for( int i = 0; i < FacebookManager.Instance.AppRequestDataList.Count; i++ )
				{
					switch (FacebookManager.Instance.AppRequestDataList[i].dataType)
					{
						case RequestDataType.Ask_Give_Life:
						case RequestDataType.Accept_Give_Life:
							go = (GameObject)Instantiate(lifeMessageEntryPrefab);
							go.transform.SetParent(content.transform,false);
							go.name = "Message number " + i.ToString();
							go.GetComponent<MessageEntry>().initializeMessage( FacebookManager.Instance.AppRequestDataList[i] );
							break;
						case RequestDataType.Challenge:
							go = (GameObject)Instantiate(challengeMessageEntryPrefab);
							go.transform.SetParent(content.transform,false);
							go.name = "Message number " + i.ToString();
							go.GetComponent<MessageEntry>().initializeMessage( FacebookManager.Instance.AppRequestDataList[i] );
							break;
						default:
							Debug.LogWarning("MessageManager-refreshMessages: unknown data type specified: " + FacebookManager.Instance.AppRequestDataList[i].dataType );
							break;
					}
				}
			}
			else
			{
				//Player has no mail but is connected to the Internet.
				mailInformationText.text = LocalizationManager.Instance.getText("POPUP_MESSAGE_CENTER_NO_MAIL");
				mailInformationPanel.SetActive( true );
			}
		}
		else
		{
			//Player is not connected to the Internet.
			mailInformationText.text = LocalizationManager.Instance.getText("MENU_CONNECTION_FAILED_TEXT");
			mailInformationPanel.SetActive( true );
		}
	}
	
	void clearAllEntries()
	{
		foreach (Transform child in content.transform)
		{
			GameObject.Destroy(child.gameObject);
		}	
	}

	public void hideMessageCenter()
	{
		SoundManager.soundManager.playButtonClick();
		//deleteProcessedEntries();
		//Force a refresh of the App Requests
		FacebookManager.Instance.getAllAppRequests();
		//numberOfMessages.text = FacebookManager.Instance.AppRequestDataList.Count.ToString();
		GetComponent<Animator>().Play("Panel Slide Out");
	}


	void deleteProcessedEntries()
	{
		AppRequestData appRequestData;
		for ( int i = FacebookManager.Instance.AppRequestDataList.Count-1; i >=0; i--)
		{
			appRequestData = (AppRequestData) FacebookManager.Instance.AppRequestDataList[i];
			if( appRequestData.hasBeenProcessed )
			{
				//Delete from main Facebook list
				FacebookManager.Instance.AppRequestDataList.RemoveAt(i);
			}
		}
	}
}
