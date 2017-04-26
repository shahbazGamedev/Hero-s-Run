using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SocialMenu : MonoBehaviour {

	[Header("Social Menu")]
	[SerializeField] Transform friendsHolder;
	[SerializeField] GameObject friendEntryPrefab;
	[SerializeField] InputField addFriendInputField;

	bool levelLoading = false;

	// Use this for initialization
	void Start () {

		Handheld.StopActivityIndicator();
		createFriendList();
	}

	void createFriendList()
	{
		List<PlayerFriends.FriendData> friendList = GameManager.Instance.playerFriends.getFriendList();
		for( int i = 0; i < friendList.Count; i++ )
		{
			createFriend( i, friendList[i] );
		}
	}

	void createFriend( int index, PlayerFriends.FriendData fd )
	{
		GameObject go = (GameObject)Instantiate(friendEntryPrefab);
		go.transform.SetParent(friendsHolder,false);
		go.GetComponent<FriendUIDetails>().configureFriend( index, fd );
	}

	/// <summary>
	/// Raises the end edit event when the name of the friend has been entered.
	/// </summary>
	/// <param name="friendName">Friend name.</param>
 	public void OnEndEdit( string friendName )
    {
		if (!string.IsNullOrEmpty(friendName))
		{
			//ChatManager.Instance.sendInvitationToFriend( friendName.Trim() );
			ChatManager.Instance.chatMessageHandler.sendAddFriendMessage( friendName.Trim() );
 		}
		addFriendInputField.text = string.Empty;
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

	void OnEnable()
	{
		ChatManager.onStatusUpdateEvent += OnStatusUpdateEvent;
	}

	void OnDisable()
	{
		ChatManager.onStatusUpdateEvent -= OnStatusUpdateEvent;
	}

	void OnStatusUpdateEvent( string userName, int newStatus )
	{
		print("OnStatusUpdateEvent " + userName + " " + newStatus );
		FriendUIDetails fud;
		for( int i = 0; i < friendsHolder.transform.childCount; i++ )
		{
			fud = friendsHolder.transform.GetChild( i ).GetComponent<FriendUIDetails>();
			if( fud != null && fud.user == userName ) fud.configureStatus( newStatus );
		}
	}
	
}
