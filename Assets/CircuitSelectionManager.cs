using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CircuitSelectionManager : MonoBehaviour {

	const float DELAY_BEFORE_SHOWING_HERO_SELECTION = 5f;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
		// we don't join the lobby. There is no need to join a lobby to get the list of rooms.
		PhotonNetwork.autoJoinLobby = false;
		//Are we playing online or doing an offline PvE match?
		if( GameManager.Instance.getPlayMode() == PlayMode.PlayAgainstEnemy )
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
		Invoke("showHeroSelection", DELAY_BEFORE_SHOWING_HERO_SELECTION );
	}

	void showHeroSelection()
	{
		LevelManager.Instance.selectedRaceDetails = GetComponentInChildren<CircuitDetails>();
		StartCoroutine( loadHeroSelection() );
	}

	IEnumerator loadHeroSelection()
	{
		Handheld.StartActivityIndicator();
		yield return new WaitForSeconds(0);
		SceneManager.LoadScene( (int)GameScenes.HeroSelection );
	}

}
