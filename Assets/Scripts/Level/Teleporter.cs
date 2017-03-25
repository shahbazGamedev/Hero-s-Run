using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour {

	enum TeleporterType {
		Transmitter = 0,
		Receiver = 1
	}

	[SerializeField] TeleporterType type = TeleporterType.Transmitter;
	[Tooltip("The name of the matching teleporter game object. This is mandatory if the teleporter type is set to transmitter.")]
	[SerializeField] string receiverName;
	[Tooltip("The name of the move-to-center-lane game object. This game object is disabled when the teleporter type is set to Receiver.")]
	[SerializeField] GameObject moveToCenterLaneTrigger;
	Transform receiverTransform;

	void Start ()
	{
		if( type == TeleporterType.Transmitter )
		{
			if( string.IsNullOrEmpty( receiverName ) )
			{
				Debug.LogError("Teleporter-The receiver for this teleporter has not been set.");
			}
			else
			{
				GameObject receiverGameObject = GameObject.Find( receiverName );
				if( receiverGameObject == null )
				{
					Debug.LogError("Teleporter-The receiver for this teleporter could not be found.");
				}
				else
				{
					receiverTransform = receiverGameObject.transform;
				}
			}
		}
		else
		{
			//The move to center lane trigger for bots is not needed for the receiver
			moveToCenterLaneTrigger.SetActive( false );
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if( type == TeleporterType.Transmitter && receiverTransform != null )
		{
			if( other.gameObject.CompareTag("Player") )
			{
				GetComponent<AudioSource>().Play();
				other.gameObject.GetComponent<PlayerInput>().teleport( receiverTransform.position, receiverTransform.eulerAngles.y );
			}
		}
	}
}
