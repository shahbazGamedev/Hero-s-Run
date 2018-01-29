using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RaceTrackUI : MonoBehaviour {

	[SerializeField] Image textBackground;
	[SerializeField] Text raceTrackName;
	[SerializeField] Image raceTrackImage;
	[SerializeField] Text raceTrackNumber;

	public void configure ( int index, LevelData.MultiplayerInfo info )
	{
		raceTrackNumber.text = (index + 1).ToString();
		string sectorName = LocalizationManager.Instance.getText( "MAP_" + info.circuitInfo.mapNumber.ToString() );
		raceTrackName.text = sectorName;
		raceTrackImage.sprite = info.circuitInfo.circuitImage;
		textBackground.color = info.circuitInfo.backgroundColor;
	}
}
