// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerManager.cs" company="Regis Geoffrion"></copyright>
// <summary>
//  Used in multiplayer
// </summary>
// <author>Régis Geoffrion</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using Photon;
using ExitGames.Client.Photon;

public class PlayerManager : Photon.PunBehaviour, IPunObservable
{
	[Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
	public static GameObject LocalPlayerInstance;
	LevelNetworkingManager levelNetworkingManager;
	
	public void Awake()
	{
	    // #Important
	    // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
	    if (photonView.isMine)
	    {
	        LocalPlayerInstance = gameObject;
	    }
	
	    // #Critical
	    // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
	    DontDestroyOnLoad(gameObject);
	}
	
	public void Start()
	{
		levelNetworkingManager = GameObject.FindGameObjectWithTag("Level Networking Manager").GetComponent<LevelNetworkingManager>();
	
		//Tell the MasterClient that we are ready to go. Our level has been loaded and our player created.
		//The MasterClient will initiate the countdown
		if( this.photonView.isMine ) this.photonView.RPC("readyToGo", PhotonTargets.MasterClient, null );
	}

	[PunRPC]
	void readyToGo()
	{
	   levelNetworkingManager.playerReady();
		Debug.Log("A new player is ready to go " + gameObject.name );
	}
	
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		gameObject.name = info.sender.NickName;
		Debug.Log("PlayerManager-OnPhotonInstantiate-Skin: " + info.sender.CustomProperties["Skin"] + " isMasterClient: " + PhotonNetwork.isMasterClient + " Name: " + info.sender.NickName );
		if ( PhotonNetwork.isMasterClient )
		{
			object[] stuff = new object[1];
			stuff[0] = info.sender.NickName;
		    GameObject heroSkin = (GameObject)PhotonNetwork.InstantiateSceneObject (info.sender.CustomProperties["Skin"].ToString(), Vector3.zero, Quaternion.identity, 0, stuff);
			Animator anim = gameObject.GetComponent<Animator>();
			heroSkin.transform.SetParent( transform, false );
			heroSkin.transform.localPosition = Vector3.zero;
			heroSkin.transform.localRotation = Quaternion.identity;
			anim.avatar = heroSkin.GetComponent<PlayerSkinInfo>().animatorAvatar;
			anim.Rebind(); //Important
		}			
	}
}
