using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendsOnlineBadge : MonoBehaviour {

	[SerializeField] Text numberFriendsOnline;

	// Use this for initialization
	void Start ()
	{
		numberFriendsOnline.text = GameManager.Instance.playerFriends.getNumberOfFriendsOnline().ToString();
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
		numberFriendsOnline.text = GameManager.Instance.playerFriends.getNumberOfFriendsOnline().ToString();
	}

}	
	
