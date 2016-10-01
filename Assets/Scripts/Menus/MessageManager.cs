using UnityEngine;
using System.Collections;

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

	// Use this for initialization
	public void refreshMessages ()
	{
		for( int i = 0; i < FacebookManager.Instance.AppRequestDataList.Count; i++ )
		{
			GameObject go = (GameObject)Instantiate(messageEntryPrefab);
			go.transform.SetParent(content.transform,false);
			go.name = "Message number " + i.ToString();
			go.GetComponent<MessageEntry>().initializeMessage( FacebookManager.Instance.AppRequestDataList[i] );
		}
	}
	
}
