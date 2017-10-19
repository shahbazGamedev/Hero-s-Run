using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultEntry : MonoBehaviour {

	[SerializeField] TextMeshProUGUI racePositionText; //1<size=58><sup>st</sup></size>
	[SerializeField] TextMeshProUGUI winStreakText;
	[SerializeField] TextMeshProUGUI levelText;
	[SerializeField] TextMeshProUGUI playerNameText;
	//[SerializeField] Image playerFrame; //Not implemented
	[SerializeField] Image playerIcon;
	[SerializeField] TextMeshProUGUI raceDurationText;

	public void configureEntry( int racePosition, int winStreak, int level, string playerName, Sprite playerIconSprite, string raceDuration  )
	{
		racePositionText.text = racePosition.ToString() + "<size=58><sup>st</sup></size>";
		winStreakText.text = winStreak.ToString();
		levelText.text = level.ToString();
		playerNameText.text = playerName;
		playerIcon.sprite = playerIconSprite;
		raceDurationText.text = raceDuration;
	}
}
