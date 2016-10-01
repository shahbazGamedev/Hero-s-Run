using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour {

	/*[Header("General")]
	public Canvas storeCanvas;
	public GameObject storeTab;
	public GameObject shopTab;
	[Header("Store")]
	public Text starsTitle;
	public Text starsReason;
	public Text livesTitle;
	public Text livesReason;
	public Scrollbar storeVerticalScrollbar;
	[Header("Shop")]
	public Text upgradeTitle;
	public Text consumableTitle;*/

	[Header("Menu Prefabs")]
	public RectTransform content;
	public GameObject messageEntryPrefab;
	public GameObject mailInformationPanel;
	public Text mailInformationText;

	public void refreshMessages ()
	{
		//Player is connected to the Internet
		if( Application.internetReachability != NetworkReachability.NotReachable )
		{
			if( FacebookManager.Instance.AppRequestDataList.Count > 0 )
			{
				//Player has mail and is connected to the Internet.
				mailInformationPanel.SetActive( false );
				for( int i = 0; i < FacebookManager.Instance.AppRequestDataList.Count; i++ )
				{
					GameObject go = (GameObject)Instantiate(messageEntryPrefab);
					go.transform.SetParent(content.transform,false);
					go.name = "Message number " + i.ToString();
					go.GetComponent<MessageEntry>().initializeMessage( FacebookManager.Instance.AppRequestDataList[i] );
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
	
}
