using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SocialMenu : MonoBehaviour {

	[Header("Social Menu")]
	[SerializeField] Transform friendsHolder;
	[SerializeField] GameObject friendEntryPrefab;

	bool levelLoading = false;

	// Use this for initialization
	void Start () {

		Handheld.StopActivityIndicator();
		createFriendList();
	}

	void createFriendList()
	{
		List<PlayerFriends.OnlineFriendData> friendList = GameManager.Instance.playerFriends.getFriendList();
		for( int i = 0; i < friendList.Count; i++ )
		{
			createFriend( i, friendList[i] );
		}
	}

	void createFriend( int index, PlayerFriends.OnlineFriendData fd )
	{
		print("createFriend " + index + " " + fd.userName);
		GameObject go = (GameObject)Instantiate(friendEntryPrefab);
		go.transform.SetParent(friendsHolder,false);
		go.GetComponent<FriendUIDetails>().configureFriend( index, fd );
	}


	public void OnClickReturnToMainMenu()
	{
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
