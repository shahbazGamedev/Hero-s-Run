using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoopResultEntry : MonoBehaviour {

	[SerializeField] TextMeshProUGUI levelText;
	[SerializeField] TextMeshProUGUI playerNameText;
	//[SerializeField] Image playerFrame; //Not implemented
	[SerializeField] Image playerIcon;
	[SerializeField] TextMeshProUGUI scoreText;
	public GameObject emoteGameObject;

	public void configureEntry( int level, string playerName, Sprite playerIconSprite, string score  )
	{
		levelText.text = level.ToString();
		playerNameText.text = playerName;
		playerIcon.sprite = playerIconSprite;
		scoreText.text = score;
	}
}
