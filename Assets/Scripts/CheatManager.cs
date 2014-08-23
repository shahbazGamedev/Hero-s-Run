using UnityEngine;
using System.Collections;


//This class is a Singleton
public class CheatManager {


	private static CheatManager cheatManager = null;

	public static CheatManager Instance
	{
        get
		{
			if (cheatManager == null)
			{

				cheatManager = new CheatManager();
            }
			return cheatManager;
        }
    }

	public bool getIsInvincible()
	{
		#if UNITY_EDITOR
		return false;
		#else
		return false;
		#endif
	}
}
