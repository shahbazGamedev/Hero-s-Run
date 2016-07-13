using UnityEngine;
using System.Collections;

//A torch carried by the hero.
//It is materialised by the fairy (for example when reaching a dark area).
//The player drops it when he dies.
public class Torch : MonoBehaviour {

	public GameObject Torch_Prefab; //Carried by the player. Used in some levels to light up the scene.
	GameObject torch; //Carried by the player. Used in some levels to light up the scene.
	bool isUsingTorch = false;

	void createTorch()
	{
		Transform torchHolder = transform.Find("Hero/BASE_Master_Root/BASE_Root/BASE_Spine1/BASE_Spine2/BASE_Spine3/BASE_Right_Clavicle/BASE_Right_Shoulder/BASE_Right_Elbow/BASE_Right_Hand");
		torch = (GameObject)Instantiate(Torch_Prefab, Vector3.zero, Quaternion.identity );
		torch.name = "Torch";
		torch.transform.SetParent( torchHolder, false );
		torch.transform.localPosition = new Vector3( 0.08f,0.051f,0.161f );
		torch.transform.localRotation = Quaternion.Euler( -84.7f, 177f, -540f );

	}

	public void enableTorch( bool enable )
	{
		if( enable && torch == null )
		{
			createTorch();
		}

		if( torch != null )
		{
			//Show the torch in the hero's hand.
			torch.SetActive( enable );
			//Enable or disable to torch fire particle system
			torch.transform.FindChild("Torch fire").gameObject.SetActive( enable );
			//Get a refence to the light attached to the torch
			GameObject torchLight = torch.transform.FindChild("Torch light").gameObject;
			if( enable )
			{
				isUsingTorch = true;
				//Play a short lighting torch sound
				torch.GetComponent<AudioSource>().Play();
				//Fade in light after activating it
				torchLight.SetActive( true );
				Light light = torchLight.GetComponent<Light>();
				StartCoroutine( Utilities.fadeInLight( light, 0.8f, light.intensity ) );
			}
			else
			{
				torchLight.SetActive( false );
				isUsingTorch = false;
			}
		}
	}

	void dropTorch()
	{
		if( torch != null )
		{
			torch.transform.SetParent( null );
			Rigidbody rb = torch.GetComponent<Rigidbody>();
			rb.isKinematic = false;
			rb.AddForce( 0, 30f, 150f );
			rb.AddTorque( 23f,15f,20f );
			GameObject.Destroy( torch, 1.9f );
		}
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		PlayerController.playerStateChanged += PlayerStateChange;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerController.playerStateChanged -= PlayerStateChange;
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			dropTorch();
		}
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Resurrect )
		{
			if( isUsingTorch )
			{
				enableTorch( true );
			}
		}
	}

}
