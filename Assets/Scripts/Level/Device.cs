using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DeviceState
{
	Broken = 0,
	Off = 1,
	On = 2
}

public enum DeviceCategory
{
	Teleporter = 0,
	Jump_Pad = 1
}

public class Device : MonoBehaviour {

	public DeviceState state = DeviceState.On;
	public DeviceCategory category = DeviceCategory.Teleporter;
	[SerializeField] Sprite  minimapIcon;
	[SerializeField] GameObject Teleport_VFX1;
	[SerializeField] ParticleSystem brokenVFX;
	[SerializeField] AudioClip brokenAudioClip;
	Color originalColor;

	void Awake()
	{
		originalColor = Teleport_VFX1.GetComponent<MeshRenderer>().material.GetColor("_TintColor");
	}

	// Use this for initialization
	public void Start () {
		
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );
		changeDeviceState( state );
	}

	public virtual void changeDeviceState( DeviceState newState )
	{
		state = newState;
		switch ( state )
		{
			case DeviceState.Broken:
				changeColor( Color.red );
				brokenVFX.Play();
				GetComponent<AudioSource>().PlayOneShot( brokenAudioClip );
				MiniMap.Instance.changeColorOfRadarObject( gameObject, Color.red );
			break;

			case DeviceState.Off:
				Teleport_VFX1.SetActive( false );
				MiniMap.Instance.removeRadarObject( gameObject );
			break;

			case DeviceState.On:
				if( category == DeviceCategory.Jump_Pad )
				{
					changeColor( Color.green );
					MiniMap.Instance.changeColorOfRadarObject( gameObject, Color.green );
				}
				else
				{
					changeColor( originalColor );
					MiniMap.Instance.changeColorOfRadarObject( gameObject, originalColor );
				}
			break;
		}
	}

	void changeColor( Color newColor )
	{
		Teleport_VFX1.GetComponent<MeshRenderer>().material.SetColor("_TintColor", newColor);
	}

}
