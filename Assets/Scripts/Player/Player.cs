using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

[System.Serializable]
public class ToggleEvent : UnityEvent<bool>{}

public class Player : NetworkBehaviour 
{
    [SerializeField] ToggleEvent onToggleShared;
    [SerializeField] ToggleEvent onToggleLocal;
    [SerializeField] ToggleEvent onToggleRemote;
    [SerializeField] float respawnTime = 5f;
	public GameObject Hero_Prefab;
	public GameObject Heroine_Prefab;
	public int skinIndexTest;
	public string playerNameTest;

	[SyncVar (hook = "OnSkinIndexChanged" ) ] int skinIndex;
	[SyncVar (hook = "OnPlayerNameChanged" ) ] string playerName;

   	void Start()
    {
        EnablePlayer ();
    }

    void DisablePlayer()
    {
        onToggleShared.Invoke (false);

        if (isLocalPlayer)
            onToggleLocal.Invoke (false);
        else
            onToggleRemote.Invoke (false);
    }

    void EnablePlayer()
    {
        onToggleShared.Invoke (true);

        if (isLocalPlayer)
            onToggleLocal.Invoke (true);
        else
            onToggleRemote.Invoke (true);
    }

	public void setSkin( int value )
    {
		skinIndex = value;
    }

	void OnSkinIndexChanged( int value )
    {
		skinIndex = value;
		skinIndexTest = skinIndex;
		loadPlayerSkin();
    }

	public void setPlayerName( string value )
    {
		playerName = value;
    }

	void OnPlayerNameChanged( string value )
    {
		playerName = value;
		playerNameTest = playerName;
    }
	
	void loadPlayerSkin()
	{
		if( GameManager.Instance.isMultiplayer() )
		{
			GameObject hero = null;
			if(skinIndex == (int)Avatar.Hero )
			{
				
				Debug.Log("loadPlayerSkin-Loading MALE prefab." );
				hero = (GameObject)Instantiate(Hero_Prefab, Vector3.zero, Quaternion.identity ) ;
			}
			else if(skinIndex == (int)Avatar.Heroine )
			{
				Debug.Log("loadPlayerSkin-Loading FEMALE prefab." );
				hero = (GameObject)Instantiate(Heroine_Prefab, Vector3.zero, Quaternion.identity ) ;
			}
			else
			{
				Debug.LogError("loadPlayerSkin-Unknown skin index: " + skinIndex );
			}
			if( hero != null )
			{
				hero.transform.parent = transform;
				hero.transform.localPosition = Vector3.zero;
				hero.transform.localRotation = Quaternion.identity;
		
				hero.name = "Hero";
				GetComponent<Animator>().avatar = hero.GetComponent<PlayerSkinInfo>().animatorAvatar;
				hero.SetActive( true );
			}
		}
	}

    public void Die()
    {
        DisablePlayer ();

        Invoke ("Respawn", respawnTime);
    }

    void Respawn()
    {
        if (isLocalPlayer) 
        {
            Transform spawn = NetworkManager.singleton.GetStartPosition ();
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
        }

        EnablePlayer ();
    }
}