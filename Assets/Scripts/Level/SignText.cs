using UnityEngine;
using System.Collections;

public class SignText : MonoBehaviour {

	public TextMesh textLine1;
	public TextMesh textLine2;
	public string textID1;
	public string textID2;

	void Awake()
	{
		textLine1.text = LocalizationManager.Instance.getText(textID1);
		textLine2.text = LocalizationManager.Instance.getText(textID2);
	}
}
