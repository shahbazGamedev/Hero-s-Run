using System.Collections;
using UnityEngine;
using TMPro;

public class TipHandler : MonoBehaviour {

	[Header("Game Tips")]
	[SerializeField] TextMeshProUGUI tipDescription;
	
	[Tooltip("The number of tips contained in GameText. The text ID should be in the form TIP_<number>. The first tip is TIP_1.")]
	[SerializeField] int numberOfTips;

	void Start ()
	{
		displayRandomTip ();
	}

	/// <summary>
	/// Displays a localized random tip.
	/// The GameText should contain the tips.
	/// The text ID should be in the form TIP_<number>.
	/// The first tip is TIP_1.
	/// </summary>
	void displayRandomTip ()
	{
		int randomNumber = Random.Range(1, numberOfTips);
		tipDescription.text = LocalizationManager.Instance.getText( "TIP_" + randomNumber.ToString() );
	}
}
