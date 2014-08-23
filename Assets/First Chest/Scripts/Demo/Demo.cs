// Copyright (c) 2014 Gold Experience TeamDev
//
// First Chest version: 1.1
// Author: Gold Experience TeamDev (http://www.ge-team.com/pages/)
// Support: geteamdev@gmail.com
// Please direct any bugs/comments/suggestions to geteamdev@gmail.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

/*
TERMS OF USE - EASING EQUATIONS#
Open source under the BSD License.
Copyright (c)2001 Robert Penner
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion

/**************
* Demo Scene event handler.
* This component should be attached with the Main Camera in Demo Scene.
**************/

public class Demo : MonoBehaviour
{
  #region Variables

	// Target to look at
	public Transform TargetLookAt;
 
	// Camera distance variables
	public float Distance = 5.0f;
	public float DistanceMin = 3.0f;
	public float DistanceMax = 10.0f;  
	float startingDistance = 0.0f;
	float desiredDistance = 0.0f;

	// Mouse variables
	float mouseX = 0.0f;
	float mouseY = 0.0f;
	public float X_MouseSensitivity = 5.0f;
	public float Y_MouseSensitivity = 5.0f;
	public float MouseWheelSensitivity = 5.0f;

	// Axis limit variables
	public float Y_MinLimit = -40.0f;
	public float Y_MaxLimit = 80.0f;
	public float DistanceSmooth = 0.05f;
	float velocityDistance = 0.0f; 
	Vector3 desiredPosition = Vector3.zero;   
	public float X_Smooth = 0.05f;
	public float Y_Smooth = 0.1f;

	// Velocity variables
	float velX = 0.0f;
	float velY = 0.0f;
	float velZ = 0.0f;
	Vector3 position = Vector3.zero;

	// GUI variables
	Rect WindowRect = new Rect( 2, 2, 185, 115);

  #endregion

  // ######################################################################
  // MonoBehaviour Functions
  // ######################################################################

  #region Component Segments

	void Start() 
	{
		//Distance = Mathf.Clamp(Distance, DistanceMin, DistanceMax);
		Distance = Vector3.Distance(TargetLookAt.transform.position, gameObject.transform.position);
		if (Distance > DistanceMax)
		  Distance = DistanceMax;
		startingDistance = Distance;
		Reset();
	}

	// Update is called once per frame
	void Update()
	{
	  // User pressed the left mouse up
	  if (Input.GetMouseButtonUp(0))
	  {
		MouseButtonUp(0);
	  }
	  // User pressed the right mouse up
	  else if (Input.GetMouseButtonUp(1))
	  {
		MouseButtonUp(1);
	  }
	}

	// LateUpdate is called after all Update functions have been called.
	void LateUpdate()
	{
		if (TargetLookAt == null)
		   return;
 
		HandlePlayerInput();
 
		CalculateDesiredPosition();
 
		UpdatePosition();
	}

	// OnGUI is called for rendering and handling GUI events.
	void OnGUI()
	{
	  WindowRect = GUI.Window(0, WindowRect, WindowFunction, "Help");
	  GUI.Label(new Rect(Screen.width-120, 2, 120, 20), "First Chest v.1.1");
	}

	//Here are the window functions:
	void WindowFunction(int windowID)
	{
	  GUI.Label(new Rect(10, 25, 85, 20), "Mouse Drag:");
	  GUI.Label(new Rect(95, 25, 90, 20), "Orbit Camera");

	  GUI.Label(new Rect(10, 45, 85, 20), "Mouse Scroll:");
	  GUI.Label(new Rect(95, 45, 90, 20), "Zoom In/Out");

	  GUI.Label(new Rect(10, 65, 85, 20), "Left Click:");
	  GUI.Label(new Rect(95, 65, 90, 20), "Open/Close");

	  GUI.Label(new Rect(10, 85, 85, 20), "Right Click:");
	  GUI.Label(new Rect(95, 85, 90, 20), "Lock/Unlock");

	  //GUI.DragWindow();
	}

  #endregion

  // ######################################################################
  // Player Input Functions
  // ######################################################################

  #region Component Segments

	void HandlePlayerInput()
	{
		// mousewheel deadZone
		float deadZone = 0.01f; 
 
		if (Input.GetMouseButton(0))
		{
		   mouseX += Input.GetAxis("Mouse X") * X_MouseSensitivity;
		   mouseY -= Input.GetAxis("Mouse Y") * Y_MouseSensitivity;
		}
	 
		// this is where the mouseY is limited - Helper script
		mouseY = ClampAngle(mouseY, Y_MinLimit, Y_MaxLimit);
 
		// get Mouse Wheel Input
		if (Input.GetAxis("Mouse ScrollWheel") < -deadZone || Input.GetAxis("Mouse ScrollWheel") > deadZone)
		{
		   desiredDistance = Mathf.Clamp(Distance - (Input.GetAxis("Mouse ScrollWheel") * MouseWheelSensitivity), 
													 DistanceMin, DistanceMax);
		}
	}

	void MouseButtonUp(int Button)
	{
	  FCMain pMain = GetHitChest();
	  if (pMain != null)
	  {
		if (Button == 0)
		{
		  pMain.ToggleOpen();
		}
		else if (Button == 1)
		{
		  pMain.ToggleLock();
		}
	  }
	}

	FCMain GetHitChest()
	{
	  Camera CurrentCamera = null;
	  if (camera)
		CurrentCamera = camera;
	  else
		CurrentCamera = this.GetComponent<Camera>();

	  // We need to actually hit an object
	  RaycastHit hitt;
	  if (Physics.Raycast(CurrentCamera.ScreenPointToRay(Input.mousePosition), out hitt, 1000))
	  {
		if (hitt.collider)
		{
		  return hitt.collider.gameObject.GetComponent<FCMain>();
		}
	  }

	  return null;
	}

  #endregion

  // ######################################################################
  // Calculation Functions
  // ######################################################################

  #region Component Segments

	void CalculateDesiredPosition()
	{
		// Evaluate distance
		Distance = Mathf.SmoothDamp(Distance, desiredDistance, ref velocityDistance, DistanceSmooth);
 
		// Calculate desired position -> Note : mouse inputs reversed to align to WorldSpace Axis
		desiredPosition = CalculatePosition(mouseY, mouseX, Distance);
	}

	Vector3 CalculatePosition(float rotationX, float rotationY, float distance)
	{
		Vector3 direction = new Vector3(0, 0, -distance);
		Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
		return TargetLookAt.position + (rotation * direction);
	}

  #endregion

  // ######################################################################
  // Utilities Functions
  // ######################################################################

  #region Component Segments

	// update camera position
	void UpdatePosition()
	{
		float posX = Mathf.SmoothDamp(position.x, desiredPosition.x, ref velX, X_Smooth);
		float posY = Mathf.SmoothDamp(position.y, desiredPosition.y, ref velY, Y_Smooth);
		float posZ = Mathf.SmoothDamp(position.z, desiredPosition.z, ref velZ, X_Smooth);
		position = new Vector3(posX, posY, posZ);
 
		transform.position = position;
 
		transform.LookAt(TargetLookAt);
	}

	// Reset Mouse variables
	void Reset() 
	{
		mouseX = 15;
		mouseY = 30;
		Distance = startingDistance;
		desiredDistance = Distance;
	}

	// Clamps angle between a minimum float and maximum float value
	float ClampAngle(float angle, float min, float max)
	{
		while (angle < -360.0f || angle > 360.0f)
		{
		   if (angle < -360.0f)
			 angle += 360.0f;
		   if (angle > 360.0f)
			 angle -= 360.0f;
		}
 
		return Mathf.Clamp(angle, min, max);
	}

  #endregion
}

