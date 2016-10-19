using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Facebook.Unity;

public class MessageManager : MonoBehaviour {

	[Header("Menu Prefabs")]
	public Text titleText;
	public RectTransform content;
	public GameObject lifeMessageEntryPrefab;
	public GameObject challengeMessageEntryPrefab;
	public GameObject mailInformationPanel;
	public Text mailInformationText;
	public int numberOfMessage = 0;
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
		/*challengeBoard.addChallenge( "Suzie", "90", 200, 4, System.DateTime.Now );
		challengeBoard.addChallenge( "Bob", "50", 100, 4, System.DateTime.Now );
		challengeBoard.addChallenge( "Suzie", "90", 400, 4, System.DateTime.Now );
		challengeBoard.addChallenge( "Bob", "50", 800, 4, System.DateTime.Now );
		challengeBoard.addChallenge( "Martha", "60", 300, 4, System.DateTime.Now );
		challengeBoard.addChallenge( "John", "70", 500, 4, System.DateTime.Now );*/

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
			if( FB.IsLoggedIn )
			{
				if( numberOfMessage > 0 )
				{
					GameObject go = null;
					//Player has mail and is connected to the Internet.
					//Do not add the entry if the RequestDataType is Unknown
					mailInformationPanel.SetActive( false );
					foreach(KeyValuePair<string, AppRequestData> pair in FacebookManager.Instance.AppRequestDataList) 
					{
						//Only add entries which have not been processed
						if( !pair.Value.hasBeenProcessed )
						{
							switch (pair.Value.dataType)
							{
								case RequestDataType.Ask_Give_Life:
								case RequestDataType.Accept_Give_Life:
									go = (GameObject)Instantiate(lifeMessageEntryPrefab);
									break;
								case RequestDataType.Challenge:
								case RequestDataType.ChallengeBeaten:
									go = (GameObject)Instantiate(challengeMessageEntryPrefab);
									break;
							}
							go.transform.SetParent(content.transform,false);
							go.GetComponent<MessageEntry>().initializeMessage( pair.Value );
						}
					}
				}
				else
				{
					//Player has no mail but is connected to the Internet and his connected to Facebook.
					mailInformationText.text = LocalizationManager.Instance.getText("MESSAGE_CENTER_NO_MAIL");
					mailInformationPanel.SetActive( true );
				}
			}
			else
			{
				//Player is not connected to Facebook.
				mailInformationText.text = LocalizationManager.Instance.getText("MESSAGE_CENTER_NOT_LOGGED_IN_TO_FACEBOOK");
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

	void OnEnable()
	{
		FacebookManager.appRequestsReceived += AppRequestsReceived;
	}

	void OnDisable()
	{
		FacebookManager.appRequestsReceived -= AppRequestsReceived;
	}

	public void decrementNumberOfMessages()
	{
		numberOfMessage--;
		newWorldMapHandler.numberOfMessages.text = numberOfMessage.ToString();
		if( numberOfMessage == 0 ) 
		{
			mailInformationText.text = LocalizationManager.Instance.getText("MESSAGE_CENTER_NO_MAIL");
			mailInformationPanel.SetActive( true );
		}
	}

	void AppRequestsReceived( int numberOfActiveAppRequests )
	{
		numberOfMessage = numberOfActiveAppRequests;
		newWorldMapHandler.numberOfMessages.text = numberOfMessage.ToString();
	}

	public void hideMessageCenter()
	{
		SoundManager.soundManager.playButtonClick();
		GetComponent<Animator>().Play("Panel Slide Out");
	}
}
