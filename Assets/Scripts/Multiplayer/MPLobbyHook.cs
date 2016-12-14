using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MPLobbyHook : MonoBehaviour 
{
    public void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
		Debug.Log("MPLobbyHook - OnLobbyServerSceneLoadedForPlayer");
    }
}

