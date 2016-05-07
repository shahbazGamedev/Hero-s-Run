using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum StoreTab {
	Store = 1,
	Shop = 2
}

public class StoreManager : MonoBehaviour {

	[Header("Powerup Shop")]
	public Text storeTitle;
	public Text upgradeTitle;
	public Text consumableTitle;
	public GameObject storeTab;
	public GameObject shopTab;

	[Header("Store")]
	public Canvas storeCanvas;

	bool levelLoading = false;

	// Use this for initialization
	void Awake ()
	{
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif

		storeTitle.text = LocalizationManager.Instance.getText("STORE_TITLE");
		upgradeTitle.text = LocalizationManager.Instance.getText("STORE_UPGRADE_TITLE");
		consumableTitle.text = LocalizationManager.Instance.getText("STORE_CONSUMABLE_TITLE");

	}

	public void showStore(StoreTab selectedTab )
	{
		storeCanvas.gameObject.SetActive( true );
		if( selectedTab == StoreTab.Store )
		{	
			storeTab.gameObject.SetActive( true );
			shopTab.gameObject.SetActive( false );
		}
		else if( selectedTab == StoreTab.Shop )
		{	
			storeTab.gameObject.SetActive( false );
			shopTab.gameObject.SetActive( true );
		}
	}

	public void showStoreTab()
	{
		storeTab.gameObject.SetActive( true );
		shopTab.gameObject.SetActive( false );
	}

	public void showShopTab()
	{
		storeTab.gameObject.SetActive( false );
		shopTab.gameObject.SetActive( true );
	}

	public void closeStore()
	{
		storeCanvas.gameObject.SetActive( false );
	}

}
