using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultEntry : MonoBehaviour {

	[SerializeField] TextMeshProUGUI racePositionText; //1<size=58><sup>st</sup></size>
	[SerializeField] TextMeshProUGUI levelText;
	[SerializeField] TextMeshProUGUI playerNameText;
	//[SerializeField] Image playerFrame; //Not implemented
	[SerializeField] Image playerIcon;
	[SerializeField] TextMeshProUGUI raceDurationText;
	public GameObject emoteGameObject;

	public void configureEntry( RacePosition racePosition, int level, string playerName, Sprite playerIconSprite, string raceDuration  )
	{
		string racePositionString = LocalizationManager.Instance.getText( "RACE_" + racePosition.ToString() );
		racePositionString = string.Format( racePositionString, 58 );
		racePositionText.text = racePositionString;
		levelText.text = level.ToString();
		playerNameText.text = playerName;
		playerIcon.sprite = playerIconSprite;
		raceDurationText.text = raceDuration;
	}
}
