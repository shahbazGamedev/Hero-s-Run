using System.Collections;
using UnityEngine;
using Photon;

public class FinishLine :PunBehaviour {

    public void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.isMasterClient && other.CompareTag("Player"))
        {
			Debug.Log(other.name + " crossed the finish line. Hurrah!" );
			this.photonView.RPC("crossedFinishLine", PhotonTargets.AllBufferedViaServer, null );
        }
    }

	[PunRPC]
	void crossedFinishLine()
	{
		Debug.Log("RPC crossedFinishLine " + gameObject.name );
		//load podium
	}

}
