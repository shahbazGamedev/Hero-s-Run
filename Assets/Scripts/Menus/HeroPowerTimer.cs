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
	Transform localPlayer;

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
		//get a reference to the local player
		for(int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().isMine && PlayerRace.players[i].GetComponent<PlayerAI>() == null )
			{
				localPlayer = PlayerRace.players[i].transform;
				print("HeroPowerTimer-StartRunningEvent " + localPlayer.name  );
				break;
			}
		}
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
			activate();
		}
	}

	void activate()
	{
		object[] data = new object[4];
		data[0] = localPlayer.GetComponent<PhotonView>().viewID;
		data[1] = 10f;
		data[2] = 100f;
		data[3] = 1f;

		PhotonNetwork.InstantiateSceneObject( "Sentry", localPlayer.position, localPlayer.rotation, 0, data );

	}
}