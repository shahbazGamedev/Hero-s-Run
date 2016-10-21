using UnityEngine;
using System.Timers;

public enum CounterType {
	//The number of required events must be obtained in the specified duration.
	//Validation happens when the timer expires.
	Duration = 0,
	//The number of required events must be obtained in a single run which the player must complete successfully.
	//Validation happens when the player reaches an end-of-level checkpoint. Counter is reset if he dies.
	Level_completed = 1,
	//The number of required events must be obtained in a single run. The player does not need to successfully complete the level.
	//Validation happens when the player dies or reaches an end-of-level checkpoint.
	Level_in_progress = 2,
	//The number of required events can be obtained in multiple runs. The player can take several days to achieve it.
	//Validation happens each time a new event is registered.
	Total_any_level = 2,
}

public class EventCounter {

	//Number of events needed to be successfull.
	int nbrEventsRequired;
	//Current number of events
	int currentNbrEvents = 0;
	//Allocated time to get nbrEventsRequired
	//For example, 3000 is 3 seconds
	double duration;
	//The achievement ID used to report this achievement to GameCenter
	//See GameCenterManager for list of valid IDs.
	string achievementID; //See GameCenterManager for list
	//Used to validate if events were completed in the allocated time
	Timer timer;
	//Used to prevent tracking this event once it has been completed.
	bool eventCompleted = false;
	//The counter type
	CounterType counterType;
	//For example, novice runner requires you to run 500 meters or
	//coin hoarder requires you to pick-up 50,000 coins.
	public int valueNeededToSucceed = 0; 
	
	//Use for duration-based event counting
	public EventCounter( string achievementID, int nbrEventsRequired, int duration )
	{
		this.nbrEventsRequired = nbrEventsRequired;
		this.duration = (double)duration;
		this.achievementID = achievementID;
		this.counterType = CounterType.Duration;
		timer = new Timer( duration );
		timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
	}

	//Use for other types of event counting
	public EventCounter( string achievementID, int nbrEventsRequired, CounterType counterType )
	{
		this.nbrEventsRequired = nbrEventsRequired;
		this.achievementID = achievementID;
		this.counterType = counterType;
	}
	
	//Use for other types of event counting where you need to specify a goal. For example, for novice_runner achievement
	//you need to run 500 meters, so you would set valueNeededToSucceed to 500.
	public EventCounter( string achievementID, int nbrEventsRequired, CounterType counterType, int valueNeededToSucceed )
	{
		this.nbrEventsRequired = nbrEventsRequired;
		this.achievementID = achievementID;
		this.counterType = counterType;
		this.valueNeededToSucceed = valueNeededToSucceed;
	}

	public void incrementCounter()
	{
		//Don't bother if the event was previously completed succesfully
		if( !eventCompleted && !GameCenterManager.isAchievementCompleted( achievementID ) ) 
		{
			currentNbrEvents++;
			Debug.LogWarning ("EventCounter-incrementCounter: " + currentNbrEvents + "/" + nbrEventsRequired );
			if( counterType == CounterType.Duration && currentNbrEvents == 1 )
			{
				//Start the timer
				timer.Start();
			}
			validateResult();
		}
	}

	void timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		Debug.LogWarning ("EventCounter-timer_Elapsed");
		//Verify results
		validateResult();
		//Reset
		reset();
	}

	void validateResult()
	{
		if( currentNbrEvents >= nbrEventsRequired )
		{
			//Success!
			eventCompleted = true;
			Debug.LogWarning ("EventCounter-validateResult: Event " + achievementID + " was completed successfully with: " + currentNbrEvents + "/" + nbrEventsRequired );
			//Game Center does not work in the Unity Editor, so don't bother to report it if that is the case.
			#if !UNITY_EDITOR
			//Don't report achievements if the player is not authenticated with Game Center
			if( GameCenterManager.isAuthenticated )
			{
				GameCenterManager.addAchievement( achievementID );
				//Display a message to the user
				string achievementDescription = GameCenterManager.getDescription(achievementID);
				//Only proceed if we have a valid description.
				if( achievementDescription != "DESCRIPTION NOT FOUND" )
				{
					Texture2D achievementImage = GameCenterManager.getImage(achievementID);
					//Only proceed if we have a valid image.
					if( achievementImage != null )
					{
						DialogManager.dialogManager.activateDisplay( achievementDescription, achievementImage );
					}
				}
			}
			#endif

		}
	}

	void reset()
	{
		currentNbrEvents = 0;
		if( counterType == CounterType.Duration )
		{
			timer.Stop();
		}
	}
}
