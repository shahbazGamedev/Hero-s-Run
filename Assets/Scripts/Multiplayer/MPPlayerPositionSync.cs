using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(channel=1,sendInterval=0.1f)]
public class MPPlayerPositionSync : NetworkBehaviour {

	[SyncVar (hook = "SyncPositionValues")]
	private Vector3 syncPos;
	[SerializeField] Transform myTransform;
	private float lerpRate;
	[SerializeField] float normalLerpRate = 16f;
	[SerializeField] float fasterLerpRate = 27f;
	
	private Vector3 lastPos;
	[SerializeField] float positionThreshold = 0.1f;

	List<Vector3> syncPosList = new List<Vector3>();
	[SerializeField] bool useHistoricalLerping = false;
	[SerializeField] float closeEnough = 0.1f;
	[SerializeField] float snapThreshold = 2f;
	private float previousZ = -1f;
	void Start()
	{
		lerpRate = normalLerpRate;
	}

	// Update is called once per frame
	void Update ()
	{
		LerpPosition();
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		TransmitPosition();
	}

	void LerpPosition()
	{
		if( !isLocalPlayer )
		{
			if( useHistoricalLerping )
			{
				historicalLerping();
			}
			else
			{
				ordinaryLerping();
			}
		}
	}

	[Client]
	void SyncPositionValues( Vector3 latestPos )
	{
		if ( latestPos.z <= previousZ ) return;
		previousZ = latestPos.z;
		syncPos = latestPos;
		if( useHistoricalLerping) syncPosList.Add(syncPos);
	}

	void ordinaryLerping()
	{
		if( Vector3.Distance( myTransform.position, lastPos ) > snapThreshold )
		{
			//Remote player is very desynchronised. Do not interpolate. Snap the remote player into the correct position.
			myTransform.position = syncPos;
		}
		else
		{
			//Remote player is a little desynchronised. Interpolate to the correct position.
			myTransform.position = Vector3.Lerp(myTransform.position, syncPos, Time.deltaTime * lerpRate );
		}
	}

	void historicalLerping()
	{
		if( syncPosList.Count > 0 )
		{
			myTransform.position = Vector3.Lerp(myTransform.position, syncPosList[0], Time.deltaTime * lerpRate );
			if( Vector3.Distance( myTransform.position, syncPosList[0] ) < closeEnough )
			{
				syncPosList.RemoveAt(0);
			}

			if( syncPosList.Count > 10 )
			{
				lerpRate = fasterLerpRate;
			}
			else
			{
				lerpRate = normalLerpRate;
			}
		}
	}

	[Command]
	void CmdProvidePositionToServer( Vector3 pos )
	{
		syncPos = pos;
	}
	
	void TransmitPosition()
	{
		if( isLocalPlayer && Vector3.Distance( myTransform.position, lastPos ) > positionThreshold )
		{
			CmdProvidePositionToServer( myTransform.position);
			lastPos = myTransform.position;
		}
	}

}
