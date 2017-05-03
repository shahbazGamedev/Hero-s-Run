using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum StoreTab {
	Store = 1,
	Shop = 2
}

public enum PurchaseStatus {
	Success = 0,
	Cancelled = 1,
	Error = 2
}

public class StoreManager : MonoBehaviour {

	[Header("General")]
	public static StoreManager Instance = null;
	[SerializeField]  GameObject storeParent;
	[SerializeField]  ScrollRect entireStoreScrollRect;
	[Header("Store")]
	[SerializeField]  ScrollRect storeScrollRect;
	[SerializeField]  Text coinsTitle;
	[SerializeField]  Text livesTitle;

	// Use this for initialization
	void Awake ()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start ()
	{
		coinsTitle.text = LocalizationManager.Instance.getText("STORE_COINS_TITLE");
		livesTitle.text = LocalizationManager.Instance.getText("STORE_LIVES_TITLE");
	}

	public void showStore( StoreTab selectedTab )
	{
		storeParent.SetActive( true );
		if( selectedTab == StoreTab.Store )
		{
			//to be re-implemented
		}
		else if( selectedTab == StoreTab.Shop )
		{	
			//to be re-implemented
		}
	}

	public void closeStore()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		storeParent.SetActive( false );
	}

}
