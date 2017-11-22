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

	public void configureEntry( int level, string playerName, Sprite playerIconSprite, int score, int kills, int downs, int revives )
	{
		levelText.text = level.ToString();
		playerNameText.text = playerName;
		playerIcon.sprite = playerIconSprite;
		scoreText.text = score.ToString("N0");
		killsText.text = kills.ToString("N0");
		downsText.text = downs.ToString("N0");
		revivesText.text = revives.ToString("N0");
	}
}
