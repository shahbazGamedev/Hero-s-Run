using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour {

	[Header("Menu Prefabs")]
	public Text titleText;
	public RectTransform content;
	public GameObject messageEntryPrefab;
	public GameObject mailInformationPanel;
	public Text mailInformationText;
	public Text numberOfMessages; //See also NewWorldMapHandler

	void Start()
	{
		titleText.text = LocalizationManager.Instance.getText("POPUP_TITLE_MESSAGE_CENTER");
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
				//Player has mail and is connected to the Internet.
				mailInformationPanel.SetActive( false );
				for( int i = 0; i < FacebookManager.Instance.AppRequestDataList.Count; i++ )
				{
					if( !FacebookManager.Instance.AppRequestDataList[i].hasBeenProcessed )
					{
						GameObject go = (GameObject)Instantiate(messageEntryPrefab);
						go.transform.SetParent(content.transform,false);
						go.name = "Message number " + i.ToString();
						go.GetComponent<MessageEntry>().initializeMessage( FacebookManager.Instance.AppRequestDataList[i] );
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
		int counter = 0;
		foreach(AppRequestData appRequestData in FacebookManager.Instance.AppRequestDataList) 
		{
			if( !appRequestData.hasBeenProcessed ) counter++;
		}
		numberOfMessages.text = counter.ToString();
		GetComponent<Animator>().Play("Panel Slide Out");
	}

}
