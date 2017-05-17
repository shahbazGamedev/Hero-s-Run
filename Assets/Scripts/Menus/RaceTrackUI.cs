using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RaceTrackUI : MonoBehaviour {

	[SerializeField] Text raceTrackName;
	[SerializeField] Image raceTrackImage;
	[SerializeField] Text raceTrackNumber;
	[SerializeField] GameObject trophyIcon;
	[SerializeField] Text trophiesNeeded;
	[SerializeField] Material grayscale;

	public void configure ( int index, LevelData.MultiplayerInfo info )
	{
		raceTrackNumber.text = (index + 1).ToString();
		raceTrackName.text = LocalizationManager.Instance.getText( info.circuitInfo.circuitTextID );
		raceTrackImage.sprite = info.circuitInfo.circuitImage;
		if( info.trophiesNeededToUnlock == 0 )
		{
			//If it does not require trophies, hide the trophy icon and trophies needed text
			trophyIcon.SetActive( false );
		}
		else
		{
			trophiesNeeded.text = info.trophiesNeededToUnlock.ToString("N0") + "+";
		}
		if( GameManager.Instance.playerProfile.getTrophies() < info.trophiesNeededToUnlock )
		{
			raceTrackImage.material = grayscale;
			GetComponent<Button>().interactable = false;
		}
	}
}
