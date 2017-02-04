using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(channel=1,sendInterval=0.1f)]
public class MPPlayerRotationSync : NetworkBehaviour {

	[SyncVar]
	private float syncRot;
	[SerializeField] Transform myTransform;
	[SerializeField] float lerpRate = 15f;

	private float lastRotY;
	[SerializeField] float rotThreshold = 1f;

	// Update is called once per frame
	void Update ()
	{
		LerpRotation();	
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		TransmitRotation();
	}

	void LerpRotation()
	{
		if( !isLocalPlayer )
		{
			Vector3 playerNewRot = new Vector3(0, syncRot, 0 );
			myTransform.rotation = Quaternion.Lerp(myTransform.rotation, Quaternion.Euler(playerNewRot), Time.deltaTime * lerpRate );
		}
	}

	[Command]
	void CmdProvideRotationToServer( float rotY )
	{
		syncRot = rotY;
	}
	
	void TransmitRotation()
	{
		if( isLocalPlayer )
		{
			if( Mathf.Abs( myTransform.localEulerAngles.y - lastRotY ) > rotThreshold )
			{
				CmdProvideRotationToServer( myTransform.localEulerAngles.y );
				lastRotY = myTransform.localEulerAngles.y;
			}
		}
	}

}
