using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CareerProfileManager : Menu {

	[SerializeField] GameObject statisticsPanel;
	[SerializeField] GameObject playerIconsPanel;
	[Header("Name and Icon")]
	[SerializeField] Image playerIcon;
	[SerializeField] TextMeshProUGUI playerNameText;

	public void Start()
	{
		Handheld.StopActivityIndicator();
		playerIcon.sprite = ProgressionManager.Instance.getPlayerIconSpriteByUniqueId( GameManager.Instance.playerProfile.getPlayerIconId() ).icon;
		playerNameText.text = GameManager.Instance.playerProfile.getUserName();
	}

	public void OnClickShowStatistics()
	{
		statisticsPanel.SetActive( true );
		playerIconsPanel.SetActive( false );
	}

	public void OnClickShowPlayerIcons()
	{
		statisticsPanel.SetActive( false );
		playerIconsPanel.SetActive( true );
	}
}
