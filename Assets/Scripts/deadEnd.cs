using UnityEngine;
using System.Collections;

public enum DeadEndType {
		Left = 0,
		Right = 1,
		None = 3
	}

public class deadEnd : MonoBehaviour {

	public DeadEndType deadEndType = DeadEndType.Left;

}
