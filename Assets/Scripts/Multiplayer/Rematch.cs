using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Rematch : Photon.PunBehaviour {

	[SerializeField] Color rematchTextColor;
	[SerializeField] TextMeshProUGUI rematchText;
	[SerializeField] TextMeshProUGUI rematchAcceptedCounterText;
	[SerializeField] Button okayButton;
	int rematchAcceptedCounter = 0;
	bool rematchAcceptedLocally = false;
	string localPlayerName;
	string partnerName;

	void Awake()
	{
		updateRematchCounter();
	}

	public void setPlayerNames( string localPlayerName, string partnerName )
	{
		this.localPlayerName = localPlayerName;
		this.partnerName = partnerName;
	}

	public void OnClickRematch()
	{
		rematchAcceptedLocally = !rematchAcceptedLocally;
		if( rematchAcceptedLocally )
		{
			rematchText.color = rematchTextColor;
			rematchText.GetComponent<UIBounceEffect>().scaleUp();
			this.photonView.RPC( "rematchAcceptedRPC", PhotonTargets.All );
		}
		else
		{
			rematchText.color = Color.white;
			rematchText.GetComponent<UIBounceEffect>().scaleUp();
			this.photonView.RPC( "rematchCanceledRPC", PhotonTargets.All );
		}
	}

	[PunRPC]
	void rematchAcceptedRPC()
	{
		rematchAcceptedCounter++;

		updateRematchCounter();

		Debug.Log("rematchAcceptedRPC " + rematchAcceptedCounter );

		if( rematchAcceptedCounter == LevelManager.Instance.getNumberOfPlayersRequired() && PhotonNetwork.isMasterClient )
		{
			//All players accepted to rematch.
			Debug.Log("Master-All players accepted rematch " );
			//The Master decides on the room name.
			//The room name is made up of the local player's name and the partner's name.
			this.photonView.RPC( "rematchRPC", PhotonTargets.All, localPlayerName + partnerName );
		}
	}

	[PunRPC]
	void rematchCanceledRPC()
	{
		rematchAcceptedCounter--;

		updateRematchCounter();

		Debug.Log("rematchCanceledRPC " + rematchAcceptedCounter );
	}

	[PunRPC]
	void rematchRPC( string roomName )
	{
		//Since all players accepted the rematch, disable the buttons.
		GetComponent<Button>().interactable = false;
		okayButton.interactable = false;

		//Return to the matchmaking screen but set the game state to Rematch.
		//The small delay is to give time to the player to notice that the counter has changed value.
		StartCoroutine( HUDMultiplayer.hudMultiplayer.returnToMatchmakingAfterDelay( 1.25f, GameState.Rematch ) );
		GameManager.Instance.setGameState(GameState.Rematch);

		//Store the room name that we are going to use for the rematch.
		LevelManager.Instance.rematchRoomName = roomName;
	}

	private void updateRematchCounter()
	{
		string rematchCounterString = LocalizationManager.Instance.getText( "RESULTS_REMATCH_COUNTER" );
		rematchAcceptedCounterText.text = string.Format( rematchCounterString, rematchAcceptedCounter, LevelManager.Instance.getNumberOfPlayersRequired() );
	}
	
}
