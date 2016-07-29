using UnityEngine;
using System.Collections;

public class Roadsign : MonoBehaviour {
	
	public enum SignType {
			ShowLevelName = 0,
			NormalText = 1
	}

	public SignType signType = SignType.ShowLevelName;
	public float percentageDisplayed = 0.4f;
	public string signText;

	void Start ()
	{
		//We don't want the roadsign to be displayed at every corner
		float rd = Random.Range( 0f, 1f );
		if( rd <= percentageDisplayed )
		{
			//We are active
			GameObject roadLabel = transform.Find("Road Label").gameObject;
			if( signType == SignType.ShowLevelName )
			{
				//Display the level name
				roadLabel.GetComponent<TextMesh>().text = LevelManager.Instance.getNextLevelName();
			}
			else if( signType == SignType.NormalText )
			{
				//Display the specified text
				roadLabel.GetComponent<TextMesh>().text = LocalizationManager.Instance.getText(signText);
			}
		}
		else
		{
			gameObject.SetActive( false );
		}

	}
}
