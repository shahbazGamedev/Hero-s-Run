using UnityEngine;
using System.Collections;

/// <summary>
/// RandomActivator is used on tile game objects to make sure that all tile instances don't look the same.
/// For example, you can have a tree cluster on a Straight tile. With a random activator, the cluster will only appear on a Straight tile when the random number generated 
/// is less or equal than the chanceDisplayed number.
/// RamdomActivator works in multiplayer. If an object is active on one device, the same object is guaranteed to be active on all other devices.
/// </summary>
public class RandomActivator : MonoBehaviour {
	
	public float chanceDisplayed = 0.3f;
	
	
}
