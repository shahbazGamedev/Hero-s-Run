using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletFire : MonoBehaviour 
{
	public float fireTime = .05f;
	public GameObject bullet;
 
	public int pooledAmount = 20;
	List<GameObject> bullets;
	AudioSource myAudio;

	void Awake()
	{
		myAudio = GetComponent<AudioSource> ();
	}
 
	void Start () 
	{
		
		bullets = new List <GameObject> ();
		for (int i = 0; i < pooledAmount; i++)
		{
			GameObject obj = (GameObject)Instantiate (bullet);
			obj.SetActive (false);
			bullets.Add (obj);
		}
		 
	}

	 
	void Update()
	{
		 
		if (Input.GetKeyDown(KeyCode.Space)) 
		{
			myAudio.Play ();
			Fire ();
			 
		}

	}
		
	 
	void Fire ()
	{

		for (int i = 0; i < bullets.Count; i++)
		{
			if (!bullets [i].activeInHierarchy)
			{
				bullets [i].transform.position = transform.position;
				bullets [i].transform.rotation = transform.rotation;
				bullets [i].SetActive (true);
				break;
			}
		}
	}
}
