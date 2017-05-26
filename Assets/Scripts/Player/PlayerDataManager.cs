using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour {

	public static PlayerDataManager Instance;
	public PlayerProfile playerProfile;
	public PlayerStatistics playerStatistics;
	public PlayerDeck playerDeck;
	public PlayerFriends playerFriends;
	public RecentPlayers recentPlayers;
	public PlayerInventory playerInventory;
	public PlayerIcons playerIcons;

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

	void Start ()
	{
		initialise();
	}

	void initialise()
	{
		if( PlayerStatsManager.Instance.getPlayerProfile() != string.Empty )
		{
			 playerProfile = JsonUtility.FromJson<PlayerProfile>(PlayerStatsManager.Instance.getPlayerProfile());
		}
		else
		{
			playerProfile = new PlayerProfile();
		}
		GameManager.Instance.playerProfile = playerProfile;

		if( PlayerStatsManager.Instance.getPlayerStatistics() != string.Empty )
		{
			 playerStatistics = JsonUtility.FromJson<PlayerStatistics>(PlayerStatsManager.Instance.getPlayerStatistics());
		}
		else
		{
			playerStatistics = new PlayerStatistics();
			playerStatistics.initialize();
		}
		GameManager.Instance.playerStatistics = playerStatistics;

		if( PlayerStatsManager.Instance.getPlayerDeck() != string.Empty )
		{
			 playerDeck = JsonUtility.FromJson<PlayerDeck>(PlayerStatsManager.Instance.getPlayerDeck());
		}
		else
		{
			playerDeck = new PlayerDeck();
			playerDeck.createNewPlayerDeck();

		}
		GameManager.Instance.playerDeck = playerDeck;

		if( PlayerStatsManager.Instance.getPlayerFriends() != string.Empty )
		{
			 playerFriends = JsonUtility.FromJson<PlayerFriends>(PlayerStatsManager.Instance.getPlayerFriends());
		}
		else
		{
			playerFriends = new PlayerFriends();
		}
		GameManager.Instance.playerFriends = playerFriends;

		if( PlayerStatsManager.Instance.getRecentPlayers() != string.Empty )
		{
			 recentPlayers = JsonUtility.FromJson<RecentPlayers>(PlayerStatsManager.Instance.getRecentPlayers());
		}
		else
		{
			recentPlayers = new RecentPlayers();
		}
		GameManager.Instance.recentPlayers = recentPlayers;

		if( PlayerStatsManager.Instance.getPlayerInventory() != string.Empty )
		{
			 playerInventory = JsonUtility.FromJson<PlayerInventory>(PlayerStatsManager.Instance.getPlayerInventory());
		}
		else
		{
			playerInventory = new PlayerInventory();
		}
		GameManager.Instance.playerInventory = playerInventory;

		if( PlayerStatsManager.Instance.getPlayerIcons() != string.Empty )
		{
			 playerIcons = JsonUtility.FromJson<PlayerIcons>(PlayerStatsManager.Instance.getPlayerIcons());
		}
		else
		{
			playerIcons = new PlayerIcons();
			playerIcons.createNewPlayerIcons();
			
		}
		GameManager.Instance.playerIcons = playerIcons;

	}
}
