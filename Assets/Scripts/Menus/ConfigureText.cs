using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ConfigureText : MonoBehaviour {

	[SerializeField] string textID;
	void Awake ()
	{
		GetComponent<Text>().text = LocalizationManager.Instance.getText( textID );
	}
}
