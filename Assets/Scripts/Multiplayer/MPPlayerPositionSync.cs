using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MPPlayerPositionSync : NetworkBehaviour {

	[SyncVar]
	private Vector3 syncPos;
	[SerializeField] Transform myTransform;
	[SerializeField] float lerpRate = 15f;
	[SerializeField] float transmitRate = 0.25f;
	
	void OnEnable()
	{
		InvokeRepeating( "TransmitPosition", 0, transmitRate );
	}

	void OnDisable()
	{
		CancelInvoke();
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		LerpPosition();
	}

	void LerpPosition()
	{
		if( !isLocalPlayer )
		{
			myTransform.position = Vector3.Lerp(myTransform.position, syncPos, Time.deltaTime * lerpRate );
		}
	}

	[Command]
	void CmdProvidePositionToServer( Vector3 pos )
	{
		syncPos = pos;
	}
	
	void TransmitPosition()
	{
		if( isLocalPlayer )
		{
			CmdProvidePositionToServer( myTransform.position);
		}
	}

}
