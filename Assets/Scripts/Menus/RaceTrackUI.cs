using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RaceTrackUI : MonoBehaviour {

	[SerializeField] Text raceTrackName;
	[SerializeField] Text raceTrackDescription;
	[SerializeField] Image raceTrackImage;
	[SerializeField] Text raceTrackNumber;
	[SerializeField] Text trophiesNeeded;

	public void configure ( int index, LevelData.MultiplayerInfo info )
	{
		raceTrackNumber.text = (index + 1).ToString();
		raceTrackName.text = LocalizationManager.Instance.getText( info.circuitInfo.circuitTextID );
		raceTrackDescription.text = "Lopsum Irum.";
		raceTrackImage.sprite = info.circuitInfo.circuitImage;
		trophiesNeeded.text = info.trophiesNeededToUnlock.ToString("N0");
	}
}
