using UnityEngine;
using System.Collections;

//By deriving from this base class, you are able to pause coroutines while the game state is either PAUSED or COUNTDOWN.
//You also need to add: yield return _sync(); instead of: yield return null; in the coroutine.
public class BaseClass : MonoBehaviour {

	
public Coroutine _sync()
{

	return StartCoroutine(PauseRoutine());  

}

public IEnumerator PauseRoutine()
{
    while (GameManager.Instance.getGameState() == GameState.Paused || GameManager.Instance.getGameState() == GameState.Countdown )
	{

        yield return new WaitForFixedUpdate();  

    }

    yield return new WaitForEndOfFrame();   

    }
}
