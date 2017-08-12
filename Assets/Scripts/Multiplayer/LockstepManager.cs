using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LockstepActionType {

	NONE = 0,
	RACE_START = 1,
	RESURRECT = 2
}

public class LockstepManager : MonoBehaviour {

	[SerializeField] double lockedStepTime = 0.1; //100 ms time per frame. Note: 62.5 ms is the value used in Hero of the Storm
	double lastActionTime = 0;
	public static LockstepManager Instance;
	public Queue<LockstepAction> lockstepCurrentActionQueue = new Queue<LockstepAction>();
	public Queue<LockstepAction> lockstepNextActionQueue = new Queue<LockstepAction>();

	void Awake ()
	{
		if( Instance == null ) Instance = this;
	}

	public void initiateFirstAction ( double timeWhenFirstActionShouldBeProcessed )
	{
		Invoke("firstAction", (float)timeWhenFirstActionShouldBeProcessed );
	}
	
	void firstAction()
	{
		addActionToQueue( new LockstepAction( LockstepActionType.RACE_START ) );
		lastActionTime = PhotonNetwork.time;
	}

	// Update is called once per frame
	void Update ()
	{
		if( PhotonNetwork.time - lastActionTime >= lockedStepTime)
		{
			lastActionTime = PhotonNetwork.time;
			processQueue();
		}
	}

	void processQueue()
	{
		while( lockstepCurrentActionQueue.Count > 0 )
		{
			process( lockstepCurrentActionQueue.Dequeue() );
		}
		//Now add the actions from the next action queue
		while( lockstepNextActionQueue.Count > 0 )
		{
			lockstepCurrentActionQueue.Enqueue( lockstepNextActionQueue.Dequeue() );
		}
	}

	void process( LockstepAction lockstepAction )
	{
		switch( lockstepAction.type )
		{
			case LockstepActionType.RACE_START:
				HUDMultiplayer.hudMultiplayer.startCountdown();
			break;

			case LockstepActionType.RESURRECT:
				lockstepAction.actor.GetComponent<PlayerControl>().resurrectBegin();
			break;
		}
	}

	public void addActionToQueue( LockstepAction lockstepAction )
	{
		lockstepNextActionQueue.Enqueue( lockstepAction );
		print("adding action to queue " + lockstepAction.type );
	}

	public class LockstepAction
	{
		public LockstepActionType type; 
		public GameObject actor;
		public CardName cardName;

		public LockstepAction( LockstepActionType type, GameObject actor = null, CardName cardName = CardName.None )
		{
			this.type = type;
			this.actor = actor;
			this.cardName = cardName;
		}
	}

}
