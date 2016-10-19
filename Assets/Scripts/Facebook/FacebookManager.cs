using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using Facebook.MiniJSON;
using System.Linq;
using UnityEngine.UI;



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
	private Action<IAppRequestResult, string > directRequestCallback;
	private string appRequestIDToDelete;
	private const int NUMBER_OF_FRIENDS = 20;
	public string  firstName = null;
	//Facebook ID of player
	string FBUserId = string.Empty;
	public Sprite playerPortrait = null;
	//Delegate used to communicate to other classes when a Facebook portrait was received
	public delegate void FacebookPlayerPortraitReceived();
	public static event FacebookPlayerPortraitReceived facebookPlayerPortraitReceived;
	//The following Dictionary has a string ID as the Key and the friend's data as the Value
	public Dictionary<string, FriendData> friendsList = new Dictionary<string, FriendData>(NUMBER_OF_FRIENDS);
	private Dictionary<string, string> profile = null;
	//The following Dictionary has a string ID as the Key and the friend's score as the Value
	//The Facebook score is used to track the episode about to be started by that friend.
	//If the score is equal to LevelData.NUMBER_OF_EPISODES, that means that your friend completed the game.
	Dictionary<string, int> scores = new Dictionary<string, int>(NUMBER_OF_FRIENDS);
	//Delegate used to communicate to other classes when a friends's scores have arrived
	public delegate void FacebookScoresReceived();
	public static event FacebookScoresReceived facebookScoresReceived;

	//The following Dictionary has a string ID as the Key and the friend's picture as the Value
	public Dictionary<string, Sprite>  friendImages = new Dictionary<string, Sprite>(NUMBER_OF_FRIENDS);
	//The following List holds the IDs for which a picture was requested but not yet received
	public List<string> friendImagesRequested = new List<string>(NUMBER_OF_FRIENDS);
	//Delegate used to communicate to other classes when the portrait of a friend was received
	public delegate void FacebookFriendPortraitReceived( string facebookID );
	public static event FacebookFriendPortraitReceived facebookFriendPortraitReceived;

	//The following Dictionary has the AppRequest ID as the Key and the AppRequestData as the Value
	public Dictionary<string, AppRequestData> AppRequestDataList = new Dictionary<string, AppRequestData>();
	//Delegate used to communicate to other classes when we have received App Requests
	public delegate void AppRequestsReceived( int appRequestsCount );
	public static event AppRequestsReceived appRequestsReceived;
	TimeSpan allowedAppRequestLifetime = new TimeSpan(0, 0, 5, 0, 0); //Fives minute. How long do we keep a processed AppRequest before deleting it.

	//Delegate used to communicate to other classes when the player logouts of Facebook
	public delegate void FacebookLogout();
	public static event FacebookLogout facebookLogout;

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

	//Called by TitleScreenHandler
	public void CallFBInit(Action<FacebookState> updateState)
    {
        FB.Init(OnInitComplete, OnHideUnity);
		myCallback = updateState;
	}

    private void OnInitComplete()
    {
		if( facebookState != FacebookState.LoggedIn )
		{
			Debug.Log("FacebookManager-OnInitComplete: IsLoggedIn: " + FB.IsLoggedIn + " IsInitialized: " + FB.IsInitialized );
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
		FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, LoginCallback);
	}

    void LoginCallback(ILoginResult result)
    {
		if (FB.IsLoggedIn)
		{
			Debug.Log ("FacebookManager-LoginCallback: user is successfully logged in.");
			facebookState = FacebookState.LoggedIn;
			OnLoggedIn();
		}

        if (result.Error != null)
		{
			Debug.LogWarning("FacebookManager-LoginCallback: Facebook error: " + result.Error );
 			facebookState = FacebookState.Error;
		}
        else if (!FB.IsLoggedIn)
		{
            //Login canceled by Player
			facebookState = FacebookState.Canceled;
		}
		myCallback( facebookState );
	}

	private void CallFBLoginForPublish()
	{
		FB.LogInWithPublishPermissions(new List<string>() { "publish_actions" }, LoginForPublishCallback);
	}

	void LoginForPublishCallback(ILoginResult result)
    {
		Debug.Log ("FacebookManager-LoginForPublishCallback: " + result.RawResult);
	}

	//You ALSO need to logout from Facebook on the browser on your mobile if you want someone else to sign in.
    public void CallFBLogout()
    {
		FB.LogOut();
		facebookState = FacebookState.Initialised;
		firstName = null;
		FBUserId = string.Empty;
		playerPortrait = null;
		PlayerStatsManager.Instance.setUsesFacebook( false );
		PlayerStatsManager.Instance.savePlayerStats();
		//Inform interested classes
		if( facebookLogout != null ) facebookLogout();
	}

	public bool isLoggedIn()
	{
		return FB.IsLoggedIn;
	}
	
    private void CallFBPublishInstall()
    {
       // FB.PublishInstall(PublishComplete);
    }

    private void PublishComplete(IGraphResult result)
    {
        //Debug.Log("publish response: " + result.RawResult);
    }

	//Title: 	The title for the Dialog. Maximum length is 50 characters. For example, "App Requests".
	//Message:	For example: "Send me lives!"
	//Data:		Custom data identifying what type of app requests this is. The format is <type,number>. The maximum length is 255 characters.
	//Note: 	The excludeIds, maxRecipients and filters AppRequest parameters are currently not supported for mobile devices by Facebook.
	public void CallAppRequestAsFriendSelector( string title, string message, string data, string excludeIds, string friendSelectorMax )
    {
		try
		{
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

			string[] excludeIdsList = (excludeIds == "") ? null : excludeIds.Split(',');

			FB.AppRequest(
				message,
				null,
				new List<object>(){ "app_users" }, //options are: all, app_users, and app_non_users
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
	
	void appRequestCallback(IAppRequestResult result)
	{
		if (result.Error != null)
		{
			Debug.LogWarning("FacebookManager-Callback: Error response: " + result.Error );
		}
		else
		{
			//Message when user cancels: {"error_code":"4201","error_message":"User+canceled+the+Dialog+flow"}
			Dictionary<string, object> appRequestResult = Json.Deserialize(result.RawResult) as Dictionary<string, object>;

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
			FB.API("/me/apprequests", HttpMethod.GET, allAppRequestsDataCallback );
		}
	}

	void allAppRequestsDataCallback( IGraphResult result )
	{
		if (result.Error != null)
		{
			Debug.LogWarning("FacebookManager-allAppRequestsDataCallback: error: " + result.Error + "\n" );
		}
		else
		{
			//Debug.Log("FacebookManager-allAppRequestsDataCallback: success: " + result.RawResult + "\n" );

			Dictionary<string, object> appRequestsData = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
			object appRequests;
			List<object> appRequestsList = new List<object>();
			if (appRequestsData.TryGetValue ("data", out appRequests)) 
			{
				appRequestsList = (List<object>)appRequests;
				
				foreach(object appRequest in appRequestsList) 
				{
					Dictionary<string, object> appRequestDictionary = (Dictionary<string, object>)appRequest;
					
					object appRequestID;
					if (appRequestDictionary.TryGetValue ("id", out appRequestID)) 
					{
					}
					else
					{
						//Invalid data - skip
						Debug.LogWarning("FacebookManager-allAppRequestsDataCallback: unable to parse AppRequest ID." );
						continue;
					}

					//Make sure we don't already have this AppRequest before continuing
					if( !AppRequestDataList.ContainsKey( appRequestID.ToString() ) )
					{
						//No, we do not have it
						//Create a AppRequestData object to store the info
						AppRequestData appRequestData = new AppRequestData();

						//Store the AppRequestID which we obtained earlier
						appRequestData.appRequestID = appRequestID.ToString();

						//Continue processing
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
							//Format is <type,number1,number2> so we need to parse it
							if( data.ToString().Contains(",") )
							{
								string[] dataDetails = data.ToString().Split(',');
								appRequestData.setRequestDataType( dataDetails[0] );
								int.TryParse(dataDetails[1], out appRequestData.dataNumber1);
								//For backward compatibility when there was only one dataNumber, verify array length first
								if( dataDetails.Length == 3 ) int.TryParse(dataDetails[2], out appRequestData.dataNumber2);
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
							AppRequestDataList.Add( appRequestData.appRequestID, appRequestData );
						}
						else
						{
							//The request data type is unknown. Ask Facebook to delete the app request as it is unusable.
							Debug.LogWarning("FacebookManager-allAppRequestsDataCallback: deleting unknown request: " + appRequestData.appRequestID );
							deleteAppRequest( appRequestData.appRequestID );
						}
					}
				}
			}
			//Count the number of AppRequests that we have that have not been processed.
			int activeAppRequestCounter = 0;
			foreach(KeyValuePair<string, AppRequestData> pair in AppRequestDataList) 
			{
				if( !pair.Value.hasBeenProcessed ) activeAppRequestCounter++;
			}
			Debug.Log("FacebookManager-allAppRequestsDataCallback: non-processed: " + activeAppRequestCounter );
			
			if( appRequestsReceived != null ) appRequestsReceived(  activeAppRequestCounter );

			//Get any missing player pictures
			fetchAppRequestPictures();

			deleteExpiredAppRequests();
		}
	}
	
	void deleteExpiredAppRequests()
	{
		foreach(KeyValuePair<string, AppRequestData> pair in AppRequestDataList.ToList()) 
		{
			//Verify if it has already been processed and that it is expired
			if( pair.Value.hasBeenProcessed )
			{
				TimeSpan elapsedTime = DateTime.Now.Subtract(pair.Value.processed_time);
				if( elapsedTime.CompareTo(allowedAppRequestLifetime) >= 0  )
				{
					//It is expired
					AppRequestDataList.Remove(pair.Value.appRequestID);
				}
			}
		}
	}

	//Get any missing pictures related to app requests.
	void fetchAppRequestPictures()
	{
		foreach(KeyValuePair<string, AppRequestData> pair in AppRequestDataList) 
		{
			//Note that getFriendPicture verifies that we do not already have the picture and that it has not been requested yet.
			getFriendPicture( pair.Value.fromID );
		}
	}

	void FeedCallback(IShareResult result)
	{
		string lastResponse;
		
		if (result.Error != null)
		{
			lastResponse = "Error Response:\n" + result.Error;
		}
		else
		{
			lastResponse = "Success Response:\n" + result.RawResult;
		}
		Debug.Log("FacebookManager-FeedCallback: response " + lastResponse );
	}

	void DirectAppRequestCallback(IAppRequestResult result)
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
			FB.API("/app/scores", HttpMethod.DELETE, delegate(IGraphResult r) { Debug.Log("DeleteAllScoresResult: " + r.RawResult); });
		}
	}

	public void DeleteMyScores()
	{
		if (FB.IsLoggedIn)
		{
			FB.API("/me/scores",  HttpMethod.DELETE, delegate(IGraphResult r) { Debug.Log("DeleteMyScoresResult: " + r.RawResult); });
		}
	}

	public void QueryScores()
	{
		if (FB.IsLoggedIn && AccessToken.CurrentAccessToken != null )
		{
			FB.API("/app/scores", HttpMethod.GET, ScoresCallback);
		}
	}

	private int getScoreFromEntry(object obj)
	{
		Dictionary<string,object> entry = (Dictionary<string,object>) obj;
		return Convert.ToInt32(entry["score"]);
	}
	
	private void ScoresCallback(IGraphResult result) 
	{
		//We are refreshing the list, so remove whatever is there
		scores.Clear();
		if (result.Error != null)
		{
			Debug.LogError(result.Error);
		}
		else
		{
			Debug.Log("ScoresCallback: " + result.RawResult);

			List<object> scoresList = Util.DeserializeScores(result.RawResult);
			
			foreach(object score in scoresList) 
			{
				var entry = (Dictionary<string,object>) score;
				var user = (Dictionary<string,object>) entry["user"];
				
				string userId = (string)user["id"];
				if ( userId == FBUserId )
				{
					// This entry is for the current player
					int playerScore = getScoreFromEntry(entry);
					Debug.Log("Local player's score on server is " + playerScore);
				}
				else
				{
					int friendScore = getScoreFromEntry(score);
					scores.Add( userId, friendScore );
					getFriendPicture( userId );
					Debug.Log("Received friend score for " + userId + " score " + friendScore);
				}
			}
			//Communicate to other classes that scores have arrived so that we can, for example, update the friend portraits location on the map
			if( facebookScoresReceived != null ) facebookScoresReceived();

		}
	}

	public string getFriendPictureForEpisode( int episode )
	{
		foreach(KeyValuePair<string, int> pair in scores ) 
		{
			if( pair.Value == episode )
			{
				//Yes, we have a friend who has reached the specified episode
				//return his userID.
				return pair.Key;
			}
		}
		return null;
	}

	//Because we are posting the high score only when the player reaches a cullis gate,
	//the score is only used in story mode. 
	public void postHighScore( int highScore )
	{
		if (FB.IsLoggedIn && AccessToken.CurrentAccessToken != null )
		{
			//posting a score requires the publish_actions permission
			if( !AccessToken.CurrentAccessToken.Permissions.Contains("publish_actions") )
			{
				CallFBLoginForPublish();
			}
			Dictionary<string, string> query = new Dictionary<string, string>();
			query["score"] = highScore.ToString();
			FB.API("/me/scores", HttpMethod.POST, delegate(IGraphResult r) { Debug.Log("postHighScore Result: " + r.RawResult); }, query);
		}
	}

	public void CallAppRequestAsDirectRequest( string DirectRequestTitle, string DirectRequestMessage, string DirectRequestTo, string data, Action<IAppRequestResult, string> mchCallback, string mchAppRequestIDToDelete )
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
        FB.FeedShare(
			string.Empty,
			new Uri("https://developers.facebook.com/"),
			"Test Title",
			"Test caption",
			"Test Description",
			new Uri("http://i.imgur.com/zkYlB.jpg"),
			string.Empty,
            callback: FeedCallback
        );
    }
	
    public string PayProduct = "";

    private void CallFBPay()
    {
        FB.Canvas.Pay(PayProduct);
    }
	
	//Tells you the URI at which the app was accessed. On Web Player, it's the URL of the page that contains the Web Player;
	//on Android or iOS, it's the URL with which the app was invoked, using the schema that the app is registered to handle.
    private void CallFBGetDeepLink()
    {
        FB.GetAppLink(DeepLinkCallback);
    }

	void DeepLinkCallback(IAppLinkResult result)
	{
		if (result.Error != null)
		{
			Debug.Log("FacebookManager-DeepLinkCallback: error " + result.Error );
		}
		else if(result.RawResult != null)
		{
			// ...have the user interact with the friend who sent the request,
			// perhaps by showing them the gift they were given, taking them
			// to their turn in the game with that friend, etc.
			//Debug.Log("FacebookManager-DeepLinkCallback: response " + query["id"] );
			Debug.Log("FacebookManager-DeepLinkCallback: response " + result.RawResult );
		}
	}
	
    public void CallAppEventLogEvent()
    {
        FB.LogAppEvent(
                    AppEventName.UnlockedAchievement,
                    null,
                    new Dictionary<string, object>()
                    {
                        { AppEventParameterName.Description, "Clicked 'Log AppEvent' button" }
                    });
       //"You may see results showing up at https://www.facebook.com/analytics/" + FB.AppId
    }
	
	//fullRequestID needs to be the full individual request id, <REQUEST_OBJECT_ID>_<USER_ID>.
	//For example: 563117760441295_100001079952187
	public void deleteAppRequest( string fullRequestID )
	{
		FB.API( fullRequestID, HttpMethod.DELETE, DeleteAppRequestCallback);
	}

	void DeleteAppRequestCallback(IGraphResult result)
	{
		if (result.Error != null)
		{
			//result.Error if the request is not found is: "error 404 not found"
			Debug.Log("FacebookManager-DeleteAppRequestCallback: error: " + result.Error );
		}
		else if(result.RawResult != null)
		{
			//result.RawResult if the request has been deleted correctly is: "true"
			Debug.Log("FacebookManager-DeleteAppRequestCallback: response: " + result.RawResult );
		}
	}

	public IEnumerator deleteAllAppRequests()
	{
		foreach(KeyValuePair<string, AppRequestData> pair in AppRequestDataList) 
		{
			deleteAppRequest( pair.Value.appRequestID );
			Debug.Log("FacebookManager-deleteAllAppRequests: deleting: " + pair.Value.appRequestID );
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
		FBUserId = AccessToken.CurrentAccessToken.UserId;
		Debug.Log("Logged in. ID: " + FBUserId);
		// Request player info and profile picture
		FB.API("/me?fields=id,first_name", HttpMethod.GET, MyProfileCallback);
		FB.API(Util.GetPictureURL("me", 128, 128), HttpMethod.GET, MyPictureCallback);
		CallFBGetDeepLink();
		getListOfFriendsWhoPlayTheApp();
		//Get your friends's score.
		QueryScores();
	}

	void getListOfFriendsWhoPlayTheApp()
	{
		FB.API("/me?fields=id,first_name,friends.limit(100).fields(first_name,id)", HttpMethod.GET, listOfFriendsCallback);

	}

	void listOfFriendsCallback(IGraphResult result)
	{
		if (result.Error != null)
		{
			Debug.Log ("listOfFriendsCallback-error:\n" + result.Error );
		}
		else
		{
			Debug.Log( "listOfFriendsCallback-success:\n" + result.RawResult );
	
			//data":[{"first_name":"Raphael","id":"2378641987"}]
			friendsList.Clear();
			
			var dict = Json.Deserialize(result.RawResult) as Dictionary<string,object>;
		
			object friendsH;
			var friends = new List<object>();
			string friendName;
			if(dict.TryGetValue ("friends", out friendsH))
			{
		  		friends = (List<object>)(((Dictionary<string, object>)friendsH) ["data"]);
				foreach(object fb_friend in friends) 
				{
					FriendData friendData = new FriendData();
	    			var friendDict = ((Dictionary<string,object>)(fb_friend));
	    			var friend = new Dictionary<string, string>();
	    			friend["id"] = (string)friendDict["id"];
	    			friend["first_name"] = (string)friendDict["first_name"];
					getFriendPicture( friend["id"] );
					friendData.first_name = friend["first_name"];
					//Add to dictionary
					friendsList.Add( friend["id"], friendData );
					friendData.printFriendData();
				}
			}
			Debug.Log ("FacebookManager-added " + friendsList.Count + " friends." );
		}
	}

	void MyProfileCallback(IGraphResult result)
	{
		if (result.Error != null)
		{
			// Let's just try again if we are not in the Unity Editor
			#if !UNITY_EDITOR
			Debug.LogError("FacebookManager: MyProfileCallback error: " + result.Error + ". trying again.");
			FB.API("/me?fields=id,first_name", HttpMethod.GET, MyProfileCallback);
			#endif
		}
		else
		{
			Debug.Log( "MyProfileCallback " + result.RawResult );
			profile = Util.DeserializeJSONProfile(result.RawResult);
			firstName = profile["first_name"];
		}
	}

	void MyPictureCallback(IGraphResult result)
	{
		if (result.Error != null)
		{
			// Let's just try again if we are not in the Unity Editor
			#if !UNITY_EDITOR
			Debug.LogError("FacebookManager: MyPictureCallback error: " + result.Error + ". trying again.");
			FB.API(Util.GetPictureURL("me", 128, 128), HttpMethod.GET, MyPictureCallback);
			#endif
		}
		else
		{
			playerPortrait = Sprite.Create( result.Texture, new Rect(0, 0, result.Texture.width, result.Texture.height ), new Vector2( 0.5f, 0.5f ) );
			if( facebookPlayerPortraitReceived != null ) facebookPlayerPortraitReceived();
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

		FB.API("me/photos", HttpMethod.POST, TakeScreenshotCallback, wwwForm);
    }

	void TakeScreenshotCallback(IGraphResult result)
	{
		if (result.Error != null)
		{
			Debug.LogWarning("FacebookManager-TakeScreenshotCallback: error: " + result.Error );
		}
		else
		{
			Debug.Log("FacebookManager-TakeScreenshotCallback: success: " + result.RawResult );
		}
	}


	public void getFriendPicture( string userId )
	{
		if (!friendImages.ContainsKey(userId) && !friendImagesRequested.Contains( userId ))
		{
			Debug.Log("FacebookManager-getFriendPicture: Getting missing picture for: " + userId );
			friendImagesRequested.Add( userId );
			
			// We don't have this friends's image yet, request it now
			FB.API(Util.GetPictureURL(userId, 128, 128), HttpMethod.GET, pictureResult =>
			       {
				if (pictureResult.Error != null)
				{
					Debug.LogError(pictureResult.Error);
				}
				else
				{
					Debug.Log("FacebookManager-getFriendPicture: success: image received for: " + userId );

					//Add image to picture dictionary
					Sprite friendPortrait = Sprite.Create( pictureResult.Texture, new Rect(0, 0, pictureResult.Texture.width, pictureResult.Texture.height ), new Vector2( 0.5f, 0.5f ) );

					friendImages.Add(userId, friendPortrait);
					if( friendImagesRequested.Contains( userId ) )
					{
						//We have received the image, so remove the entry in the friendImagesRequested list.
						friendImagesRequested.Remove( userId );
						if( facebookFriendPortraitReceived != null ) facebookFriendPortraitReceived( userId );
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
			FB.API ("/me/" + FACEBOOK_NAMESPACE + ":smash", HttpMethod.POST, publishActionCallback, querySmash);
		}
	}

	void publishActionCallback(IGraphResult result)
	{
		if (result.Error != null)
		{
			Debug.LogWarning("FacebookManager-publishActionCallback: error: " + result.Error );
		}
		else
		{
			Debug.Log("FacebookManager-publishActionCallback: success: " + result.RawResult );
		}
	}

	//customImageUri format is: http://i.imgur.com/zkYlB.jpg
	public void inviteFriends( string customImageUri )
	{
		string appLinkUrl = string.Empty;
		#if UNITY_IPHONE
		appLinkUrl = "https://itunes.apple.com/app/apple-store/id797936081";
		#elif UNITY_ANDROID
		//appLinkUrl = "https://fb.me/" + FB.AppId;
		Debug.LogWarning("FacebookManager - inviteFriends - appLinkUrl has not been set for Android.");
		#endif

		if( customImageUri != null )
		{
			Debug.Log ("inviteFriends action: "  + FB.AppId + " with a custom image specified." );
            FB.Mobile.AppInvite(new Uri(appLinkUrl), new Uri(customImageUri), inviteFriendsCallback);
		}
		else
		{
			Debug.Log ("inviteFriends action: "  + FB.AppId + " with no custom image specified." );
		    FB.Mobile.AppInvite(new Uri(appLinkUrl), null, inviteFriendsCallback);
		}
	}

	void inviteFriendsCallback( IAppInviteResult result )
	{
		if( result.Error != null )
		{
			Debug.LogError("inviteFriendsCallback-Error: " + result.Error );
		}
		else
		{
			Debug.Log ("inviteFriendsCallback-Success: " +  result.RawResult );
		}			
 	}


 }
