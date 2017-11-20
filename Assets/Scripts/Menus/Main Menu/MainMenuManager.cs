using System.Collections;
using UnityEngine;

public class MainMenuManager : MonoBehaviour {

	[SerializeField] GameObject mainMenuPanel;
	[SerializeField] GameObject cardCollectionPanel;
	[SerializeField] GameObject storePanel;

	public void OnClickShowMainMenu()
	{
		mainMenuPanel.SetActive( true );
		cardCollectionPanel.SetActive( false );
		storePanel.SetActive( false );
	}

	public void OnClickShowCardCollection()
	{
		mainMenuPanel.SetActive( false );
		cardCollectionPanel.SetActive( true );
		storePanel.SetActive( false );
	}

	public void OnClickShowStore()
	{
		mainMenuPanel.SetActive( false );
		cardCollectionPanel.SetActive( false );
		storePanel.SetActive( true );
	}
}
