using UnityEngine;
using UnityEngine.UI;
 using Photon;
using ExitGames.Client.Photon;
 
using System.Collections;
 
 
namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// Player name input field. Let the user input his name, will appear above the player in the game.
    /// </summary>
    [RequireComponent(typeof(InputField))]
    public class PlayerNameInputField : PunBehaviour
    {
        #region Private Variables
 
 
        // Store the PlayerPref Key to avoid typos
        static string playerNamePrefKey = "PlayerName";
		public InputField _nameField;
		public InputField _skinField;
		public Launcher launcher;
 
        #endregion
 
 
        #region MonoBehaviour CallBacks
 
 
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start () {
  
            string defaultName = "";
            if (_nameField!=null)
            {
                if (PlayerPrefs.HasKey(playerNamePrefKey))
                {
                    defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                    _nameField.text = defaultName;
                }
            }
             string defaultSkinName = "";
          	if (PlayerPrefs.HasKey("Skin"))
            {
                defaultSkinName = PlayerPrefs.GetString("Skin", "Girl");
                _skinField.text = defaultSkinName;
           }
             int defaultIconId = 0;
                // defaultIconId = PlayerPrefs.GetInt("Icon", 0);
			#if UNITY_EDITOR
				defaultIconId = 0;
			#else
				defaultIconId = 1;
			#endif
				launcher.setIconLocalPlayer(defaultIconId);
			print("defaultSkinName " + defaultSkinName + " " +launcher.someCustomPropertiesToSet.Count + " " + defaultSkinName);
			launcher.someCustomPropertiesToSet.Clear();
			if( launcher.someCustomPropertiesToSet.ContainsKey("Skin") ) Debug.LogError("Shout");
			launcher.someCustomPropertiesToSet.Add("Skin", defaultSkinName );
			launcher.someCustomPropertiesToSet.Add("Icon", defaultIconId );
			PhotonNetwork.player.SetCustomProperties(launcher.someCustomPropertiesToSet); 

 
            PhotonNetwork.playerName =  defaultName;
       }
 
 
        #endregion
 
 
        #region Public Methods
 
 
        /// <summary>
        /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
        /// </summary>
        /// <param name="value">The name of the Player</param>
        public void SetPlayerName(string value)
        {
            // #Important
            PhotonNetwork.playerName = value + " "; // force a trailing space string in case value is an empty string, else playerName would not be updated.
			launcher.localPlayerName.text = value;
 
            PlayerPrefs.SetString(playerNamePrefKey,value);
        }
 
        public void SetSkinName(string value)
        {
			
            PlayerPrefs.SetString("Skin",value);
			launcher.someCustomPropertiesToSet["Skin"] = value;
			PhotonNetwork.player.SetCustomProperties(launcher.someCustomPropertiesToSet); 
       }

 
        #endregion
    }
}
