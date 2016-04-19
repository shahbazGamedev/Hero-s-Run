using UnityEngine;
using System.Collections;

public class ScrollingTextures : MonoBehaviour {


    public float horizontalScrollSpeed = 0.25f;

    public float verticalScrollSpeed = 0.25f;

 

    private bool scroll = true;

 	public void Awake()
    {
		gameObject.SetActive( false );
	}

    public void Update()

    {

        if (scroll)

        {

            float verticalOffset = Time.time * verticalScrollSpeed;

            float horizontalOffset = Time.time * horizontalScrollSpeed;

            GetComponent<Renderer>().material.mainTextureOffset = new Vector2(horizontalOffset, verticalOffset);

        }

    }

 

    public void DoActivateTrigger()

    {

        scroll = !scroll;

    }

 

}
 
