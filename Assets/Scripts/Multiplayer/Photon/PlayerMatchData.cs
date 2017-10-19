using System.Collections;
using UnityEngine;

//This data is used by Results Screen. It is populated during matchmaking.
public class PlayerMatchData {

	public string playerName;
	public int playerIcon;
	public int level;
	public int currentWinStreak;

	public PlayerMatchData ( string playerName, int playerIcon, int level, int currentWinStreak )
	{
		this.playerName = playerName;
		this.playerIcon = playerIcon;
		this.level = level;
		this.currentWinStreak = currentWinStreak;
	}
}
