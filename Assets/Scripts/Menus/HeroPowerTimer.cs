using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class HeroPowerTimer : MonoBehaviour {

	[SerializeField] GameObject holder;
	[SerializeField] Image radialImage;
	[SerializeField] Image radialImageMask;
	[SerializeField] Text nameText;
	[SerializeField] Text centerText;
	bool ready = false;

	void OnEnable()
	{
		PlayerRace.crossedFinishLine += CrossedFinishLine;
		HUDMultiplayer.startRunningEvent += StartRunningEvent;
	}

	void OnDisable()
	{
		PlayerRace.crossedFinishLine -= CrossedFinishLine;
		HUDMultiplayer.startRunningEvent -= StartRunningEvent;
	}

	void StartRunningEvent()
	{
		startAnimation ( "Sentry", 30, Color.gray );
	}

	void CrossedFinishLine( Transform player, int officialRacePosition, bool isBot )
	{
		if( !isBot )
		{
			print("HeroPowerTimer-CrossedFinishLine " + player.name + " officialRacePosition " +  officialRacePosition );
			holder.SetActive( false );
		}
	}

	public void startAnimation ( string name, float duration, Color color )
	{
		holder.SetActive( true );
		nameText.text = name;
		StartCoroutine( animate( duration ) );
		radialImage.color = color;
	}
	
	IEnumerator animate( float duration  )
	{
		float startTime = Time.time;
		float elapsedTime = 0;

		float fromValue = 0;
	
		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			radialImageMask.fillAmount =  Mathf.Lerp( fromValue, 1f, elapsedTime/duration );
			centerText.text = string.Format( "{0:P0}", radialImageMask.fillAmount );
			yield return new WaitForEndOfFrame();  
	    }
		centerText.text = "Ready";
		ready = true;
	}

	public void activatePower ()
	{
		if( ready )
		{
			ready = false;
			print("Gros prout");
			startAnimation ( "Sentry", 30, Color.gray );
		}
	}

}