using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum StoreTab {
	Store = 1,
	Shop = 2
}

public enum StoreReason {
	None = 0,
	Need_Stars = 1,
	Need_Lives = 2
}

public enum PurchaseStatus {
	Success = 0,
	Cancelled = 1,
	Error = 2
}

public class StoreManager : MonoBehaviour {

	[Header("General")]
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
	public Text consumableTitle;

	// Use this for initialization
	void Awake ()
	{
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif
		starsTitle.text = LocalizationManager.Instance.getText("STORE_STARS_TITLE");
		starsReason.text = LocalizationManager.Instance.getText("STORE_STARS_REASON");
		livesTitle.text = LocalizationManager.Instance.getText("STORE_LIVES_TITLE");
		livesReason.text = LocalizationManager.Instance.getText("STORE_LIVES_REASON");

		upgradeTitle.text = LocalizationManager.Instance.getText("STORE_UPGRADE_TITLE");
		consumableTitle.text = LocalizationManager.Instance.getText("STORE_CONSUMABLE_TITLE");

	}

	public void showStore(StoreTab selectedTab, StoreReason reason )
	{
		storeCanvas.gameObject.SetActive( true );
		if( selectedTab == StoreTab.Store )
		{	
			showStoreTab();
		}
		else if( selectedTab == StoreTab.Shop )
		{	
			showShopTab();
		}
		if( reason == StoreReason.Need_Lives )
		{
			starsReason.gameObject.SetActive( false );
			livesReason.gameObject.SetActive( true );
			//Lives are at the middle of the display
			storeVerticalScrollbar.value = 0.2096281f;
		}
		else if ( reason == StoreReason.Need_Stars )
		{
			starsReason.gameObject.SetActive( true );
			livesReason.gameObject.SetActive( false );
			//Stars are at the top of the display
		}
		else if ( reason == StoreReason.None )
		{
			starsReason.gameObject.SetActive( false );
			livesReason.gameObject.SetActive( false );
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
