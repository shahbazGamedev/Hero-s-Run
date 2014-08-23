using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;



public enum FacebookState {
	NotInitialised = 0,
	Initialised = 1,
	LoggedIn = 2,
	Error = 3,
	Canceled = 4
}

public class FacebookManager
{
	private const string FACEBOOK_NAMESPACE = "dragonrunsaga";

	private FacebookState facebookState = FacebookState.NotInitialised;
	private Action<FacebookState> myCallback;
	private Action<FBResult, string > directRequestCallback;
	private string appRequestIDToDelete;
	private const int NUMBER_OF_FRIENDS = 20;
	public string  Username = null;
	//Facebook ID of player
	string FBUserId = "";
	public Texture UserPortrait = null;
	public Texture FriendTexture = null;
	//The following Dictionary has a string ID as the Key and the friend's data as the Value
	public Dictionary<string, FriendData> friendsList = new Dictionary<string, FriendData>(NUMBER_OF_FRIENDS);
	private Dictionary<string, string> profile = null;
	//The following Dictionary has a string ID as the Key and the friend's score as the Value
	//The Facebook score is used to track the highest level achieved by that friend.
	Dictionary<string, int> scores = new Dictionary<string, int>(NUMBER_OF_FRIENDS);
	//The following Dictionary has a string ID as the Key and the friend's picture as the Value
	public Dictionary<string, Texture>  friendImages = new Dictionary<string, Texture>(NUMBER_OF_FRIENDS);
	//The following List holds the IDs for which a picture was requested but not yet received
	public List<string> friendImagesRequested = new List<string>(NUMBER_OF_FRIENDS);

	//List of AppRequestData objects
	public List<AppRequestData> AppRequestDataList = new List<AppRequestData>();

	//Event management used to notify other classes when a map section is unlocked
	public delegate void NextSectionNowUnlocked();
	public static event NextSectionNowUnlocked nextSectionNowUnlocked;

	private static FacebookManager facebookManager = null;

	public static FacebookManager Instance
	{
		get
		{
			if (facebookManager == null)
			{
				
				facebookManager = new FacebookManager();
				
			}
			return facebookManager;
		}
	} 

	//Called by LoadScreenHandler
	public void CallFBInit()
	{
		FB.Init(OnInitComplete, OnHideUnity);
		myCallback = null;
	}

	//Called by MainMenuHandler
	public void CallFBInit(Action<FacebookState> updateState)
    {
        FB.Init(OnInitComplete, OnHideUnity);
		myCallback = updateState;
	}

    private void OnInitComplete()
    {
		if( facebookState != FacebookState.LoggedIn )
		{
			Debug.Log("FacebookManager-OnInitComplete: IsLoggedIn: " + FB.IsLoggedIn + " ID: " + FB.UserId);
	 		facebookState = FacebookState.Initialised;
			if( !FB.IsLoggedIn )
			{
				CallFBLogin();
			}
			else
			{
				facebookState = FacebookState.LoggedIn;
				if( myCallback != null )
				{
					myCallback( facebookState );
				}
				OnLoggedIn();
			}
		}
	}
	
	private void OnHideUnity(bool isGameShown)
    {
		Debug.Log("FacebookManager-OnHideUnity: Is game showing? " + isGameShown);
		if (!isGameShown)
		{
			// pause the game - we will need to hide
			Time.timeScale = 0;
		}
		else
		{
			// start the game back up - we're getting focus again
			Time.timeScale = 1;
		}
	}
	
    private void CallFBLogin()
    {
		//Note: In order to publish a photo to a user’s album, you must have the publish_stream permission. To do - remove this permission.
		//In order to publish an Open Graph story to Facebook on the user's behalf using API calls, you will need the user to grant the publish_actions permission to your app. 
		FB.Login("email,publish_actions", LoginCallback);
	}

    void LoginCallback(FBResult result)
    {
		if (FB.IsLoggedIn)
		{
			Debug.Log ("FacebookManager-LoginCallback: user is successfully logged in.");
			facebookState = FacebookState.LoggedIn;
			//Very important - COPPA related
			//By default, we will use the safer, opt-out option in order to be COPPA compliant.
			//If the user successfully connects to Facebook, we will assume the player is 13 years old or older
			//and set the opt-out to false.
			MyUpsightManager.setUpsightOptOutOption( false );
			Debug.Log ( "FacebookManager: COPPA opt-out status is set to false since player successfully logged into Facebook." );

			OnLoggedIn();
		}

        if (result.Error != null)
		{
			Debug.LogWarning("FacebookManager-LoginCallback: Facebook error: " + result.Error );
 			facebookState = FacebookState.Error;
			MyUpsightManager.setUpsightOptOutOption( true );
		}
        else if (!FB.IsLoggedIn)
		{
            //Login canceled by Player
			facebookState = FacebookState.Canceled;
			MyUpsightManager.setUpsightOptOutOption( true );
		}
		myCallback( facebookState );
	}

    public void CallFBLogout()
    {
        FB.Logout();
		facebookState = FacebookState.Initialised;
	}

	public bool isLoggedIn()
	{
		return FB.IsLoggedIn;
	}
	
    private void CallFBPublishInstall()
    {
        FB.PublishInstall(PublishComplete);
    }

    private void PublishComplete(FBResult result)
    {
        Debug.Log("publish response: " + result.Text);
    }

	//Title: 	The title for the Dialog. Maximum length is 50 characters. For example, "App Requests". Currently this parameter does not change anything. It appears to be a Facebook bug.
	//Message:	For example: "Help me unlock the next episode!"
	//Data:		Custom data identifying what type of app requests this is. The format is <type,number>. The maximum length is 255 characters.
	//Note: 	The excludeIds, maxRecipients and filters AppRequest parameters are currently not supported for mobile devices by Facebook.
	public void CallAppRequestAsFriendSelector( string title, string message, string filters, string data, string excludeIds, string friendSelectorMax )
    {
		try
		{
			string[] excludeIdsList = (excludeIds == "") ? null : excludeIds.Split(',');
			// If there's a Max Recipients specified, include it
			int? maxRecipients = null;
			if (friendSelectorMax != "")
			{
				try
				{
					maxRecipients = Int32.Parse(friendSelectorMax);
				}
				catch (Exception e)
				{
					Debug.LogWarning("FacebookManager-CallAppRequestAsFriendSelector parse error: " + e.Message);
				}
			}
			//filters = "[\"all\",\"app_users\",\"app_non_users\"]";
			FB.AppRequest(
				message,
				null,
				"[\"all\"]",
				excludeIdsList,
				maxRecipients,
				data,
				title,
				appRequestCallback
				);
			Debug.Log("FacebookManager-CallAppRequestAsFriendSelector called successfully." );
		}
		catch (Exception e)
		{
			Debug.LogWarning("FacebookManager-CallAppRequestAsFriendSelector error: " + e.Message);
		}
	}
	
	void appRequestCallback(FBResult result)
	{
		if (result.Error != null)
		{
			Debug.LogWarning("FacebookManager-Callback: Error response: " + result.Error );
		}
		else
		{
			//Message when user cancels: {"error_code":"4201","error_message":"User+canceled+the+Dialog+flow"}
			Dictionary<string, object> appRequestResult = Json.Deserialize(result.Text) as Dictionary<string, object>;

			object obj = 0;
			if (appRequestResult.TryGetValue ("error_code", out obj))
			{
				if( obj.ToString () == "4201" )
				{
					Debug.Log("FacebookManager-appRequestCallback: request cancelled by user." );
				}
			}
			else
			{
				object recipients;
				List<object> recipientsList = new List<object>();
				if (appRequestResult.TryGetValue ("to", out recipients)) 
				{
					recipientsList = (List<object>)recipients;

					foreach(object recipient in recipientsList) 
					{
						Debug.Log ("Recipient ID is : " + recipient.ToString() );
					}
				}
			}
		}
		
	}

	public void getAllAppRequests()
	{
		if( FB.IsLoggedIn )
		{
			FB.API("/me/apprequests", Facebook.HttpMethod.GET, allAppRequestsDataCallback );
		}
	}

	void allAppRequestsDataCallback( FBResult result )
	{
		if (result.Error != null)
		{
			Debug.LogWarning("FacebookManager-allAppRequestsDataCallback: error: " + result.Error + "\n" );
		}
		else
		{
			//Debug.Log("FacebookManager-allAppRequestsDataCallback: success: " + result.Text + "\n" );

			//We are refreshing the list, so remove whatever is there
			AppRequestDataList.Clear();

			Dictionary<string, object> appRequestsData = Json.Deserialize(result.Text) as Dictionary<string, object>;
			object appRequests;
			List<object> appRequestsList = new List<object>();
			if (appRequestsData.TryGetValue ("data", out appRequests)) 
			{
				appRequestsList = (List<object>)appRequests;
				
				foreach(object appRequest in appRequestsList) 
				{
					//Create a AppRequestData object to store the info
					AppRequestData appRequestData = new AppRequestData();

					Dictionary<string, object> appRequestDictionary = (Dictionary<string, object>)appRequest;
					
					object appRequestID;
					if (appRequestDictionary.TryGetValue ("id", out appRequestID)) 
					{
						appRequestData.appRequestID = appRequestID.ToString();
					}
					object fromData;
					if (appRequestDictionary.TryGetValue ("from", out fromData )) 
					{
						Dictionary<string, object> appRequestFromData = (Dictionary<string, object>)fromData;
						
						object nameData;
						if (appRequestFromData.TryGetValue ("name", out nameData )) 
						{
							string[] nameDetails = nameData.ToString().Split(' ');
							if( nameDetails != null && nameDetails.Length > 1 )
							{
								appRequestData.fromFirstName = nameDetails[0];
								appRequestData.fromLastName  = nameDetails[1];
							}

						}
						object idData;
						if (appRequestFromData.TryGetValue ("id", out idData )) 
						{
							appRequestData.fromID = idData.ToString();
						}
					}
					object data;
					if (appRequestDictionary.TryGetValue ("data", out data )) 
					{
						//Format is <type,number> so we need to parse it
						if( data.ToString().Contains(",") )
						{
							string[] dataDetails = data.ToString().Split(',');
							appRequestData.setRequestDataType( dataDetails[0] );
							int.TryParse(dataDetails[1], out appRequestData.dataNumber);
						}
					}
					object created_time;
					if (appRequestDictionary.TryGetValue ("created_time", out created_time )) 
					{
						try
						{
							appRequestData.created_time = DateTime.Parse( created_time.ToString() );
						}
						catch( Exception e )
						{
							Debug.LogWarning("FacebookManager-allAppRequestsDataCallback: unable to parse date : " + created_time.ToString() + ". Error is: " + e.Message );
						}
					}
					appRequestData.printAppRequestData();
					if( appRequestData.dataType != RequestDataType.Unknown )
					{
						//We have a valid data type, add it
						AppRequestDataList.Add( appRequestData );
					}
					else
					{
						//The request data type is unknown. Ask Facebook to delete the app request as it is unusable.
						Debug.Log("FacebookManager-allAppRequestsDataCallback: deleting unknown request: " + appRequestData.appRequestID );
						deleteAppRequest( appRequestData.appRequestID );
					}

				}
			}
			Debug.Log("FacebookManager-allAppRequestsDataCallback: Added " + AppRequestDataList.Count + " app requests to list.");
			//Get any missing player pictures
			fetchAppRequestPictures();
			//See if any friends helped us unlock the next section.
			verifyWhichFriendsAccepted();
		}
	}
	
	//Get any missing pictures related to app requests.
	void fetchAppRequestPictures()
	{
		foreach( AppRequestData appRequestData in AppRequestDataList) 
		{
			//Note that getFriendPicture verifies that we do not already have the picture and that it has not been requested yet.
			getFriendPicture( appRequestData.fromID );
		}
	}

	//Verifies which friends have accepted to unlock the next section of the map then saves the information.
	//If 3 friends have accepted, the next section is unlocked and an event is sent.
	void verifyWhichFriendsAccepted()
	{
		foreach( AppRequestData appRequestData in FacebookManager.Instance.AppRequestDataList ) 
		{
			if( appRequestData.dataType == RequestDataType.Accept_Section_Unlock && appRequestData.dataNumber == LevelManager.Instance.getNextSectionToUnlock() )
			{
				//Yes, we have a valid app request corresponding to the section to unlock.
				//We need to display the pictures of each friend who has accepted to unlock the next section
				//so store his FB id.
				PlayerStatsManager.Instance.addUnlockRequest(appRequestData.fromID);
				//Note that getFriendPicture verifies that we do not already have the picture and that it has not been requested yet.
				getFriendPicture( appRequestData.fromID );
			}
		}
		if( PlayerStatsManager.Instance.getSaveUnlockRequests().Count == 3 )
		{
			//Yes! The next section is unlocked.
			//Send an event to interested classes.
			if(nextSectionNowUnlocked != null) nextSectionNowUnlocked();
			//Empty the current list since no longer needed
			PlayerStatsManager.Instance.ClearSaveUnlockRequests();
		}
		PlayerStatsManager.Instance.savePlayerStats();
		
	}

	void Callback(FBResult result)
	{

		Texture lastResponseTexture = null;
		string lastResponse;
		
		if (result.Error != null)
		{
			lastResponse = "Error Response:\n" + result.Error;
		}
		else if (!ApiQuery.Contains("/picture"))
		{
			lastResponse = "Success Response:\n" + result.Text;
		}
		else
		{
			lastResponseTexture = result.Texture;
			lastResponse = "Success Response:\n";
		}
		Debug.Log("FacebookManager-Callback: response " + lastResponse );
	}

	void DirectAppRequestCallback(FBResult result)
	{
		//Inform the MessageCenterHandler
		if( directRequestCallback != null )
		{
			directRequestCallback(result, appRequestIDToDelete);
		}
	}

	public void DeleteAllScores()
	{
		if (FB.IsLoggedIn)
		{
			FB.API("/app/scores", Facebook.HttpMethod.DELETE, delegate(FBResult r) { FbDebug.Log("DeleteAllScoresResult: " + r.Text); });
		}
	}

	public void DeleteMyScores()
	{
		if (FB.IsLoggedIn)
		{
			FB.API("/me/scores",  Facebook.HttpMethod.DELETE, delegate(FBResult r) { FbDebug.Log("DeleteMyScoresResult: " + r.Text); });
		}
	}

	private void QueryScores()
	{
		FB.API("/app/scores", Facebook.HttpMethod.GET, ScoresCallback);
	}

	private int getScoreFromEntry(object obj)
	{
		Dictionary<string,object> entry = (Dictionary<string,object>) obj;
		return Convert.ToInt32(entry["score"]);
	}
	
	private void ScoresCallback(FBResult result) 
	{
		if (result.Error != null)
		{
			FbDebug.Error(result.Error);
			return;
		}
		
		List<object> scoresList = Util.DeserializeScores(result.Text);
		
		foreach(object score in scoresList) 
		{
			var entry = (Dictionary<string,object>) score;
			var user = (Dictionary<string,object>) entry["user"];
			
			string userId = (string)user["id"];
			if ( userId == FBUserId )
			{
				// This entry is the current player
				int playerHighScore = getScoreFromEntry(entry);
				Debug.Log("Local players score on server is " + playerHighScore);
				int nextLevelToComplete = LevelManager.Instance.getNextLevelToComplete();
				if( nextLevelToComplete > playerHighScore )
				{
					//Update our Facebook score. We were probably not online when we finished the last level.
					postHighScore( LevelManager.Instance.getNextLevelToComplete() );
				}
			}
			else
			{
				//Do not add the player to the scores list. Only friends.
				scores.Add(userId, getScoreFromEntry( score ));
				getFriendPicture( userId );
				Debug.Log("Received friend score for " + userId + " score " + getScoreFromEntry( score ));
			}
		}
		
	}

	public string getFriendPictureForLevel( int level )
	{
		foreach(KeyValuePair<string, int> pair in scores ) 
		{
			if( pair.Value == level )
			{
				//Yes, we have a friend who has reached the specified level
				//return his userID.
				return pair.Key;
			}
		}
		return null;
	}

	public void postHighScore( int highScore )
	{
		if (FB.IsLoggedIn)
		{
			Dictionary<string, string> query = new Dictionary<string, string>();
			query["score"] = highScore.ToString();
			FB.API("/me/scores", Facebook.HttpMethod.POST, delegate(FBResult r) { FbDebug.Log("postHighScore Result: " + r.Text); }, query);
		}
	}

	public void CallAppRequestAsDirectRequest( string DirectRequestTitle, string DirectRequestMessage, string DirectRequestTo, string data, Action<FBResult, string> mchCallback, string mchAppRequestIDToDelete )
    {
        if (DirectRequestTo == "")
        {
            throw new ArgumentException("\"To Comma Ids\" must be specificed", "to");
        }
		directRequestCallback = mchCallback;
		appRequestIDToDelete = mchAppRequestIDToDelete;
        FB.AppRequest(
			DirectRequestMessage,
			DirectRequestTo.Split(','),
			null,
			null,
			null,
			data,
			DirectRequestTitle,
			DirectAppRequestCallback
        );
    }
	
    public string FeedToId = "";
    public string FeedLink = "";
    public string FeedLinkName = "";
    public string FeedLinkCaption = "";
    public string FeedLinkDescription = "";
    public string FeedPicture = "";
    public string FeedMediaSource = "";
    public string FeedActionName = "";
    public string FeedActionLink = "";
    public string FeedReference = "";
    public bool IncludeFeedProperties = false;
    private Dictionary<string, string[]> FeedProperties = new Dictionary<string, string[]>();

    private void CallFBFeed()
    {
        Dictionary<string, string[]> feedProperties = null;
        if (IncludeFeedProperties)
        {
            feedProperties = FeedProperties;
        }
        FB.Feed(
            toId: FeedToId,
            link: FeedLink,
            linkName: FeedLinkName,
            linkCaption: FeedLinkCaption,
            linkDescription: FeedLinkDescription,
            picture: FeedPicture,
            mediaSource: FeedMediaSource,
            actionName: FeedActionName,
            actionLink: FeedActionLink,
            reference: FeedReference,
            properties: feedProperties,
            callback: Callback
        );
    }
	
    public string PayProduct = "";

    private void CallFBPay()
    {
        FB.Canvas.Pay(PayProduct);
    }
	
    public string ApiQuery = "";

    private void CallFBAPI()
    {
        FB.API(ApiQuery, Facebook.HttpMethod.GET, Callback);
    }

	//Tells you the URI at which the app was accessed. On Web Player, it's the URL of the page that contains the Web Player;
	//on Android or iOS, it's the URL with which the app was invoked, using the schema that the app is registered to handle.
    private void CallFBGetDeepLink()
    {
        FB.GetDeepLink(DeepLinkCallback);
    }

	void DeepLinkCallback(FBResult result)
	{
		if (result.Error != null)
		{
			Debug.Log("FacebookManager-DeepLinkCallback: error " + result.Error );
		}
		else if(result.Text != null)
		{
			// ...have the user interact with the friend who sent the request,
			// perhaps by showing them the gift they were given, taking them
			// to their turn in the game with that friend, etc.
			//Debug.Log("FacebookManager-DeepLinkCallback: response " + query["id"] );
			Debug.Log("FacebookManager-DeepLinkCallback: response " + result.Text );
		}
	}
	
    public float PlayerLevel = 1.0f;

    public void CallAppEventLogEvent()
    {
        var parameters = new Dictionary<string, object>();
        parameters[Facebook.FBAppEventParameterName.Level] = "Player Level";
        FB.AppEvents.LogEvent(Facebook.FBAppEventName.AchievedLevel, PlayerLevel, parameters);
        PlayerLevel++;
    }
	
	//fullRequestID needs to be the full individual request id, <REQUEST_OBJECT_ID>_<USER_ID>.
	//For example: 563117760441295_100001079952187
	public void deleteAppRequest( string fullRequestID )
	{
		FB.API( fullRequestID, Facebook.HttpMethod.DELETE, DeleteAppRequestCallback);
	}

	void DeleteAppRequestCallback(FBResult result)
	{
		if (result.Error != null)
		{
			//result.Error if the request is not found is: "error 404 not found"
			Debug.Log("FacebookManager-DeleteAppRequestCallback: error: " + result.Error );
		}
		else if(result.Text != null)
		{
			//result.Text if the request has been deleted correctly is: "true"
			Debug.Log("FacebookManager-DeleteAppRequestCallback: response: " + result.Text );
		}
	}

	public IEnumerator deleteAllAppRequests()
	{
		foreach(AppRequestData appRequestData in AppRequestDataList ) 
		{
			deleteAppRequest( appRequestData.appRequestID );
			Debug.Log("FacebookManager-deleteAllAppRequests: deleting: " + appRequestData.appRequestID );

		}
		yield return new WaitForSeconds(4);
		//Refresh our list
		getAllAppRequests();
	}

    void Awake()
    {
        FeedProperties.Add("key1", new[] { "valueString1" });
        FeedProperties.Add("key2", new[] { "valueString2", "http://www.facebook.com" });

    }
	
	void OnLoggedIn()
	{
		FbDebug.Log("Logged in. ID: " + FB.UserId);
		FBUserId = FB.UserId;
		// Request player info and profile picture
		FB.API("/me?fields=id,first_name", Facebook.HttpMethod.GET, MyProfileCallback);
		FB.API(Util.GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, MyPictureCallback);
		CallFBGetDeepLink();
		getNRandomFriends( 7 );
		//For debugging
		//publishAction();
		//Get your friends score.
		//The score is the highest level achieved in the game.
		QueryScores();
	}

	void getNRandomFriends( int numberOfFriends )
	{
		string fqlQuery = "/fql?q=" + WWW.EscapeURL("SELECT uid, first_name FROM user WHERE uid IN ( SELECT uid2 FROM friend WHERE uid1 = me() ) ORDER BY rand() limit " + numberOfFriends.ToString() );
		FB.API(fqlQuery, Facebook.HttpMethod.GET, getNRandomFriendsCallback);
	}

	void getNRandomFriendsCallback(FBResult result)
	{
		if (result.Error != null)
		{
			Debug.Log ("getNRandomFriendsCallback-Callback error:\n" + result.Error );
		}
		else
		{
			//{"data":[{"uid":127428615,"first_name":"Herv\u00e9"},{"uid":125864534,"first_name":"Pierre-Alexandre"},{"uid":129893765,"first_name":"Marie-claude"}]}
			Debug.Log ("getNRandomFriendsCallback-Callback success:\n" + result.Text );
			friendsList.Clear();
			
			Dictionary<string, object> friendsData = Json.Deserialize(result.Text) as Dictionary<string, object>;
			object friends;
			List<object> tempList = new List<object>();
			if (friendsData.TryGetValue ("data", out friends)) 
			{
				tempList = (List<object>)friends;
				
				foreach(object friend in tempList) 
				{
					FriendData friendData = new FriendData();
					string uid = "";
					Dictionary<string, object> friendDictionary = (Dictionary<string, object>)friend;
					
					object friendID;
					if (friendDictionary.TryGetValue ("uid", out friendID)) 
					{
						uid = friendID.ToString();
						getFriendPicture( uid );
					}
					object first_name;
					if (friendDictionary.TryGetValue ("first_name", out first_name)) 
					{
						string firstName = first_name.ToString();
						friendData.first_name = firstName;
					}
					//Add to dictionary
					friendsList.Add( uid, friendData );
					friendData.printFriendData();
				}
			}
			Debug.Log ("FacebookManager-added " + friendsList.Count + " friends." );
		}
	}

	void MyProfileCallback(FBResult result)
	{
		if (result.Error != null)
		{
			// Let's just try again if we are not in the Unity Editor
			#if !UNITY_EDITOR
			FbDebug.Error("FacebookManager: MyProfileCallback error: " + result.Error + ". trying again.");
			FB.API("/me?fields=id,first_name", Facebook.HttpMethod.GET, MyProfileCallback);
			#endif
		}
		else
		{
			Debug.Log( "MyProfileCallback " + result.Text );
			profile = Util.DeserializeJSONProfile(result.Text);
			Username = profile["first_name"];
		}
	}

	void MyPictureCallback(FBResult result)
	{
		if (result.Error != null)
		{
			// Let's just try again if we are not in the Unity Editor
			#if !UNITY_EDITOR
			FbDebug.Error("FacebookManager: MyPictureCallback error: " + result.Error + ". trying again.");
			FB.API(Util.GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, MyPictureCallback);
			#endif
		}
		else
		{
			UserPortrait = result.Texture;
		}
	}
	
    public IEnumerator TakeScreenshot() 
    {
        yield return new WaitForEndOfFrame();
		Debug.Log("FacebookManager-TakeScreenshot called." );

        var width = Screen.width;
        var height = Screen.height;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        byte[] screenshot = tex.EncodeToPNG();

        var wwwForm = new WWWForm();
        wwwForm.AddBinaryData("image", screenshot, "Me, riding a dragon!");
        wwwForm.AddField("message", "Fly with me in Dragon Run Saga.");

		FB.API("me/photos", Facebook.HttpMethod.POST, TakeScreenshotCallback, wwwForm);
    }

	void TakeScreenshotCallback(FBResult result)
	{
		if (result.Error != null)
		{
			Debug.LogWarning("FacebookManager-TakeScreenshotCallback: error: " + result.Error );
		}
		else
		{
			Debug.Log("FacebookManager-TakeScreenshotCallback: success: " + result.Text );
		}
	}


	public void getFriendPicture( string userId )
	{
		if (!friendImages.ContainsKey(userId) && !friendImagesRequested.Contains( userId ))
		{
			//Debug.Log("FacebookManager-getFriendPicture: Getting missing picture for: " + userId );
			friendImagesRequested.Add( userId );
			
			// We don't have this friends's image yet, request it now
			FB.API(Util.GetPictureURL(userId, 128, 128), Facebook.HttpMethod.GET, pictureResult =>
			       {
				if (pictureResult.Error != null)
				{
					FbDebug.Error(pictureResult.Error);
				}
				else
				{
					//Add image to picture dictionary
					friendImages.Add(userId, pictureResult.Texture);
					if( friendImagesRequested.Contains( userId ) )
					{
						//We have received the image, so remove the entry in the friendImagesRequested list.
						friendImagesRequested.Remove( userId );
					}
				}
			});
		}
	}

	public void publishAction()
	{
		Debug.Log ("publish action: "  + FB.AppId);
		if (FB.IsLoggedIn)
		{
			Dictionary<string, string> querySmash = new Dictionary<string, string>();
			string testUserID = "1378641987";
			querySmash["profile"] = testUserID;
			FB.API ("/me/" + FACEBOOK_NAMESPACE + ":smash", Facebook.HttpMethod.POST, publishActionCallback, querySmash);
		}
	}

	void publishActionCallback(FBResult result)
	{
		if (result.Error != null)
		{
			Debug.LogWarning("FacebookManager-publishActionCallback: error: " + result.Error );
		}
		else
		{
			Debug.Log("FacebookManager-publishActionCallback: success: " + result.Text );
		}
	}

 }
