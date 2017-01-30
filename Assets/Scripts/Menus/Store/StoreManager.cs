﻿using UnityEngine;
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
	public static StoreManager Instance = null;
	[SerializeField]  GameObject storeParent;
	[SerializeField]  ScrollRect entireStoreScrollRect;
	[SerializeField]  GameObject storeTab;
	[SerializeField]  GameObject shopTab;
	[Header("Store")]
	[SerializeField]  ScrollRect storeScrollRect;
	[SerializeField]  Text starsTitle;
	[SerializeField]  Text starsReason;
	[SerializeField]  Text livesTitle;
	[SerializeField]  Text livesReason;
	[Header("Shop")]
	[SerializeField]  ScrollRect shopScrollRect;
	[SerializeField]  Text upgradeTitle;
	[SerializeField]  Text consumableTitle;

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
		starsTitle.text = LocalizationManager.Instance.getText("STORE_STARS_TITLE");
		starsReason.text = LocalizationManager.Instance.getText("STORE_STARS_REASON");
		livesTitle.text = LocalizationManager.Instance.getText("STORE_LIVES_TITLE");
		livesReason.text = LocalizationManager.Instance.getText("STORE_LIVES_REASON");

		upgradeTitle.text = LocalizationManager.Instance.getText("STORE_UPGRADE_TITLE");
		consumableTitle.text = LocalizationManager.Instance.getText("STORE_CONSUMABLE_TITLE");

	}

	public void showStore(StoreTab selectedTab, StoreReason reason )
	{
		storeParent.SetActive( true );
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
			storeScrollRect.verticalNormalizedPosition = 0.0673f;
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

	void showStoreTab()
	{
		//Show the store
		entireStoreScrollRect.horizontalNormalizedPosition = 0f;
		//Move to the top of the scroll view
		storeScrollRect.verticalNormalizedPosition = 1f;
		shopScrollRect.verticalNormalizedPosition = 1f;
	}

	void showShopTab()
	{
		//Show the shop
		entireStoreScrollRect.horizontalNormalizedPosition = 1f;
		//Move to the top of the scroll view
		storeScrollRect.verticalNormalizedPosition = 1f;
		shopScrollRect.verticalNormalizedPosition = 1f;
	}

	public void closeStore()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		storeParent.SetActive( false );
	}

}
