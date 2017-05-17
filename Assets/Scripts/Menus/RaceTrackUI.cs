using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RaceTrackUI : MonoBehaviour {

	[SerializeField] Text raceTrackName;
	[SerializeField] Image raceTrackImage;

	public void configure ( LevelData.MultiplayerInfo info )
	{
		raceTrackName.text = LocalizationManager.Instance.getText( info.circuitInfo.circuitTextID );
		raceTrackImage.sprite = info.circuitInfo.circuitImage;
		
	}
}
