#pragma strict

var style : GUIStyle;

function Start(){
   style.normal.textColor = Color.white;
    }

function OnGUI() {



GUI.Box (Rect (10, 10, 200, 20), "Mouse : Move around",style);
GUI.Box (Rect (10, 30, 200, 20), "Up : Move Forward",style);
GUI.Box (Rect (10, 50, 200, 20), "Down : Move Back",style);

}