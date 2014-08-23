using UnityEngine;
using System.Collections;

public class Roadsign : MonoBehaviour {

	void Start ()
	{
		//We don't want the roadsign to be displayed at every corner
		float rd = Random.Range( 0f, 1f );
		if( rd > 0.6f )
		{
			//We are active, so display the name of the next destination
			GameObject roadLabel = transform.Find("Road Label").gameObject;
			roadLabel.GetComponent<TextMesh>().text = LevelManager.Instance.getNextLevelName();
		}
		else
		{
			gameObject.SetActive( false );
		}

	}
}
