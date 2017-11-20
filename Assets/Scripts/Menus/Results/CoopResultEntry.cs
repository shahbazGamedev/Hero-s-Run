using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoopResultEntry : MonoBehaviour {

	[SerializeField] Image playerIcon;
	[SerializeField] TextMeshProUGUI playerNameText;
	[SerializeField] TextMeshProUGUI levelText;
	//[SerializeField] Image playerFrame; //Not implemented
	[SerializeField] TextMeshProUGUI scoreText;
	[SerializeField] TextMeshProUGUI killsText;
	[SerializeField] TextMeshProUGUI downsText;
	[SerializeField] TextMeshProUGUI revivesText;
	public GameObject emoteGameObject;

	public void configureEntry( int level, string playerName, Sprite playerIconSprite, string score  )
	{
		levelText.text = level.ToString();
		playerNameText.text = playerName;
		playerIcon.sprite = playerIconSprite;
		scoreText.text = score;
	}
}
