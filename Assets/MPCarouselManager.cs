using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MPCarouselManager : MonoBehaviour {

	public GameObject lobbyManager;
	[SerializeField] MPLobbyMenu mpLobbyMenu;

	bool levelLoading = false;
	public List<CarouselEntry> carouselEntryList = new List<CarouselEntry>(2);
	[SerializeField] Scrollbar scrollbar;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
	}

	public void OnClickShowStore()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		StoreManager.Instance.showStore( StoreTab.Store, StoreReason.None );
	}

	public void OnClickShowMatchmakingScreen()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		LevelManager.Instance.setCurrentMultiplayerLevel( (int) scrollbar.value );
		CarouselEntry selected = carouselEntryList[LevelManager.Instance.getCurrentMultiplayerLevel() ];
		mpLobbyMenu.configureCircuitData( selected.circuitImage.sprite, selected.circuitName.text, selected.entryFee.text );
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
			//Some components of the Lobby Manager game object are DontDestroyOnLoad.
			//Since we are going back to the world map, detroy the Lobby Manager.
			GameObject.Destroy( lobbyManager );
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)GameScenes.WorldMap );
		}
	}
}
