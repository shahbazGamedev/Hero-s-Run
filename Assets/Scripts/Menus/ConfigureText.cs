using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfigureText : MonoBehaviour {

	[SerializeField] string textID;
	void Awake ()
	{
		if( GetComponent<TextMeshProUGUI>() != null )
		{
			GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.getText( textID );
		}
		else if( GetComponent<Text>() !=null )
		{
			GetComponent<Text>().text = LocalizationManager.Instance.getText( textID );
		}
		else
		{
			Debug.LogWarning("The game object " + gameObject.name + " has a ConfigureText component but no Text or TextMeshProUGUI component.");
		}
	}
}
