using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MPCarouselManager : MonoBehaviour {

	public GameObject lobbyManager;

	StoreManager storeManager;
	bool levelLoading = false;

	void Awake ()
	{
		SceneManager.LoadScene( (int)GameScenes.Store, LoadSceneMode.Additive );
		//The default multiplayer level is 0.
		//This value is changed when the player swipes to change circuit.
		LevelManager.Instance.setCurrentMultiplayerLevel( 0 );
	}

	// Use this for initialization
	void Start ()
	{
		GameObject storeManagerObject = GameObject.FindGameObjectWithTag("Store");
		storeManager = storeManagerObject.GetComponent<StoreManager>();

		Handheld.StopActivityIndicator();
	}

	public void OnClickShowShop()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		storeManager.showStore( StoreTab.Shop, StoreReason.None );
	}

	public void OnClickShowMatchmakingScreen()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		gameObject.SetActive( false );
	}

	public void OnClickReturnToWorldMap()
	{
		StartCoroutine( close() );
	}

	IEnumerator close()
	{
		if( !levelLoading )
		{
			Debug.Log("MPCarouselManager - returning to world map.");
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			GameManager.Instance.setMultiplayerMode( false );
			GameManager.Instance.setGameState(GameState.WorldMapNoPopup);
			MPNetworkLobbyManager.mpNetworkLobbyManager.cleanUpOnExit();
			//Some components of the Lobby Manager game object are DontDestroyOnLoad.
			//Since we are going back to the world map, detroy the Lobby Manager.
			GameObject.Destroy( lobbyManager );
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.WorldMap );
		}
	}
}
