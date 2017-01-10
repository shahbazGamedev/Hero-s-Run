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

    void Awake()
    {
		loadPlayerSkin();
    }

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

	void loadPlayerSkin()
	{
		if( GameManager.Instance.isMultiplayer() )
		{
			GameObject hero;
			if(PlayerStatsManager.Instance.getAvatar() == Avatar.Hero )
			{
				hero = (GameObject)Instantiate(Hero_Prefab, Vector3.zero, Quaternion.identity ) ;
			}
			else
			{
				hero = (GameObject)Instantiate(Heroine_Prefab, Vector3.zero, Quaternion.identity ) ;
			}
			hero.transform.parent = transform;
			hero.transform.localPosition = Vector3.zero;
			hero.transform.localRotation = Quaternion.identity;
	
			hero.name = "Hero";
			GetComponent<Animator>().avatar = hero.GetComponent<PlayerSkinInfo>().animatorAvatar;
			hero.SetActive( true );
		}
	}
}