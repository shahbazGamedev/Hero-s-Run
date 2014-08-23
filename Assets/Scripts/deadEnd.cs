using UnityEngine;
using System.Collections;

public enum DeadEndType {
		Left = 0,
		Right = 1,
		LeftRight = 2,
		RightStraight = 3,
		None = 4
	}

public class deadEnd : MonoBehaviour {

	public DeadEndType deadEndType = DeadEndType.Left;

}
