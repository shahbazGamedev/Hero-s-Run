using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultEntry : MonoBehaviour {

	[SerializeField] TextMeshProUGUI racePosition; //1<size=58><sup>st</sup></size>
	[SerializeField] TextMeshProUGUI winStreak;
	[SerializeField] TextMeshProUGUI level;
	[SerializeField] TextMeshProUGUI playerName;
	//[SerializeField] Image playerFrame; //Not implemented
	[SerializeField] Image playerIcon;
	[SerializeField] TextMeshProUGUI raceDuration;

	public void configureEntry( int racePosition, int winStreak, int level, string playerName, Sprite playerIcon, string raceDuration  )
	{

	}
}
