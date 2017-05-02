using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum StoreTab {
	Store = 1,
	Shop = 2
}

public enum StoreReason {
	None = 0,
	Need_Coins = 1,
	Need_Lives = 2
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
	[SerializeField]  Text coinsReason;
	[SerializeField]  Text livesTitle;
	[SerializeField]  Text livesReason;

	// Use this for initialization
	void Awake ()
	{
		if(Instance)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
	}

	// Use this for initialization
	void Start ()
	{
		coinsTitle.text = LocalizationManager.Instance.getText("STORE_COINS_TITLE");
		coinsReason.text = LocalizationManager.Instance.getText("STORE_COINS_REASON");
		livesTitle.text = LocalizationManager.Instance.getText("STORE_LIVES_TITLE");
		livesReason.text = LocalizationManager.Instance.getText("STORE_LIVES_REASON");
	}

	public void showStore(StoreTab selectedTab, StoreReason reason )
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
		if( reason == StoreReason.Need_Lives )
		{
			coinsReason.gameObject.SetActive( false );
			livesReason.gameObject.SetActive( true );
			//Lives are at the middle of the display
			storeScrollRect.verticalNormalizedPosition = 0.0673f;
		}
		else if ( reason == StoreReason.Need_Coins )
		{
			coinsReason.gameObject.SetActive( true );
			livesReason.gameObject.SetActive( false );
			//Coins are at the top of the display
		}
		else if ( reason == StoreReason.None )
		{
			coinsReason.gameObject.SetActive( false );
			livesReason.gameObject.SetActive( false );
		}
	}

	public void closeStore()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		storeParent.SetActive( false );
	}

}
