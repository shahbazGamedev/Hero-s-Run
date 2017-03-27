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
	[SerializeField] GameObject Teleport_VFX1;
	[SerializeField] GameObject Teleport_VFX2;
	Color originalColor;

	void Awake()
	{
		originalColor = Teleport_VFX1.GetComponent<MeshRenderer>().material.GetColor("_TintColor");
	}

	// Use this for initialization
	public void Start () {
		
		changeDeviceState( state );
	}

	void changeDeviceState( DeviceState newState )
	{
		switch ( newState )
		{
			case DeviceState.Broken:
				changeColor( Color.red );
			break;

			case DeviceState.Off:
				Teleport_VFX1.SetActive( false );
				Teleport_VFX2.SetActive( false );
			break;

			case DeviceState.On:
				if( category == DeviceCategory.Jump_Pad )
				{
					changeColor( Color.green );
				}
				else
				{
					changeColor( originalColor );
				}
			break;
		}
	}

	void changeColor( Color newColor )
	{
		Teleport_VFX1.GetComponent<MeshRenderer>().material.SetColor("_TintColor", newColor);
		Teleport_VFX2.GetComponent<MeshRenderer>().material.SetColor("_TintColor", newColor);
	}
}
