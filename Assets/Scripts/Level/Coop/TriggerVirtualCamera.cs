using System.Collections;
using UnityEngine;
using Cinemachine;

public class TriggerVirtualCamera : MonoBehaviour {

	[SerializeField] Transform lookAtCamera;
	[SerializeField] CinemachineVirtualCamera cmvc;
	[SerializeField] float duration = 2.5f;

	bool hasBeenActivated = false;

	void OnTriggerEnter(Collider other)
	{
		if( other.CompareTag("Player") && !hasBeenActivated )
		{
			hasBeenActivated = true;
			if( lookAtCamera != null ) cmvc.m_LookAt = lookAtCamera;
			cmvc.enabled = true;
			Invoke( "deactivateCamera", duration );
			for( int i = 0; i < PlayerRace.players.Count; i++ )
			{
				PlayerRace.players[i].GetComponent<PlayerControl>().pausePlayer( true );
			}
		}
	}

	void deactivateCamera()
	{
		cmvc.enabled = false;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			PlayerRace.players[i].GetComponent<PlayerControl>().pausePlayer( false );
		}
	}
}
