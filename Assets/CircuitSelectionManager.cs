using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CircuitSelectionManager : MonoBehaviour {

	[SerializeField] ScrollRect carouselScrollRect;
	bool levelLoading = false;
	public List<CarouselEntry> carouselEntryList = new List<CarouselEntry>(2);
	[SerializeField] Scrollbar scrollbar;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
		GameManager.Instance.setMultiplayerMode( true );
		carouselScrollRect.horizontalNormalizedPosition = 0; //Make sure it is on the far left completely or the dot won't light up
		// we don't join the lobby. There is no need to join a lobby to get the list of rooms.
		PhotonNetwork.autoJoinLobby = false;
		//Are we playing online or doing an offline PvE/solo match?
		if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstEnemy || GameManager.Instance.getPlayMode() == PlayMode.PlayAlone )
		{
			//PvE is an offline mode. We will not connect. We will also set Photon to offline.
			PhotonNetwork.offlineMode = true;
		}
		else
		{
			//All other play modes are online.
			PhotonNetwork.offlineMode = false;
			//In order to display the number of online players, we need to be connected to the master server.
			//Users are separated from each other by game version (which allows you to make breaking changes).
			//Don't attempt to connect if you are already connected.
			if( !PhotonNetwork.connected ) PhotonNetwork.ConnectUsingSettings(GameManager.Instance.getVersionNumber());
			Debug.Log("CircuitSelectionManager-PhotonNetwork.versionPUN is " + PhotonNetwork.versionPUN );
		}
	}

	public void OnClickShowStore()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		StoreManager.Instance.showStore( StoreTab.Store, StoreReason.None );
	}

	public void OnClickShowHeroSelection()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		LevelManager.Instance.setCurrentMultiplayerLevel( (int) scrollbar.value );
		CarouselEntry selected = carouselEntryList[LevelManager.Instance.getCurrentMultiplayerLevel() ];
		LevelManager.Instance.selectedRaceDetails = selected;
		StartCoroutine( loadScene(GameScenes.HeroSelection) );
	}

	public void OnClickReturnToMainMenu()
	{
		PhotonNetwork.Disconnect();
		StartCoroutine( loadScene(GameScenes.MainMenu) );
	}

	IEnumerator loadScene(GameScenes value)
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)value );
		}
	}

}
