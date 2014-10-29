using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StoreEntry : MonoBehaviour {

	[Header("Store Entry")]

	public Text title;
	public Text description;
	public Text buttonLabel;

	public string titleID;
	public string descriptionID;
	public string buttonLabelID;

	// Use this for initialization
	void Awake () {
	
		title.text = "Coin Magnet";
		description.text = "The Coin Magnet pickup lasts 10 seconds longer. This is really wonderful news.";
		buttonLabel.text = "5,000";

	}
	
}
