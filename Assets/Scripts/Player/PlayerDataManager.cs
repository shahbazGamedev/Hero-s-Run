using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour {

	public static PlayerDataManager Instance;
	public PlayerProfile playerProfile;
	public PlayerStatistics playerStatistics;

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
			initialise();
		}
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
		}
		GameManager.Instance.playerStatistics = playerStatistics;
	}
}
