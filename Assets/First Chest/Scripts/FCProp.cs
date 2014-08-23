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

#if USE_HOTWEEN	// use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://www.holoville.com/hotween/documentation.html
using Holoville.HOTween;
#elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html
#else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php
#endif

#endregion

/**************
* First Chest Prop handler.
* Create/Remove and show the Prop in Treasure Chest.
**************/

public class FCProp : MonoBehaviour
{

  #region Variables

	// Repository of FCMain component
	FCMain m_Main = null;

	// Prefab and Prop objects
	public GameObject m_Prefab = null;
	GameObject m_PropGameObject = null;

	// The type of Transformation used for position, rotation and scale of Prop
	public enum eTransformState
	{
	  Begin,
	  Changing,
	  End
	};

	// Position variables
	eTransformState m_PositionState = eTransformState.Begin;
	//eTransformState m_PositionStateOld = eTransformState.Begin;
	public FCEaseType.eEaseType m_PosEaseType = FCEaseType.eEaseType.OutElastic;
	public Vector3 m_PosBegin = new Vector3(0, 0, 0);
	public Vector3 m_PosEnd = new Vector3(0, 2.0f, 0);
	[Range(0.0f, 1.0f)]
	public float m_PosValue = 0.0f;
	[Range(0.0f, 2.0f)]
	public float m_PosDelay = 0.25f;
	[Range(0.0f, 2.0f)]
	public float m_PosDuration = 1.0f;

	// Rotation variables
	public enum eRotationType
	{
	  Disable,
	  Endless,
	  LimitedRound
	};
	public eRotationType m_RotationType = eRotationType.Endless;
	//eTransformState m_RotationState = eTransformState.Begin;
	//eTransformState m_RotationStateOld = eTransformState.Begin;
	public FCEaseType.eEaseType m_RotationEaseType = FCEaseType.eEaseType.InOutQuad;
	public Vector3 m_Rotation = new Vector3(0, -1, 0);
	public float m_RotationValue = 0.0f;
	//int m_RotationRoundCount = 0;
	public int m_MaxRotationRound = 5;
	[Range(0.0f, 2.0f)]
	public float m_RotationDelay = 0;
	[Range(0.0f, 5.0f)]
	public float m_RotationDurationPerRound = 2.0f;

	// Scale variables
	public enum eScaleType
	{
	  Disable,
	  Enable
	};
	public eScaleType m_ScaleType = eScaleType.Disable;
	eTransformState m_ScaleState = eTransformState.Begin;
	//eTransformState m_ScaleStateOld = eTransformState.Begin;
	public FCEaseType.eEaseType m_ScaleEaseType = FCEaseType.eEaseType.OutElastic;
	public Vector3 m_ScaleBegin = new Vector3(1, 1, 1);
	public Vector3 m_ScaleEnd = new Vector3(2, 2, 2);
	public float m_ScaleValue = 0.0f;
	[Range(0.0f, 2.0f)]
	public float m_ScaleDelay = 0.5f;
	[Range(0.0f, 5.0f)]
	public float m_ScaleDuration = 1.0f;

	// Remove Prop variables
	public bool m_RemovedWhenChestClose = true;
	public bool m_FadeOut = true;
	[Range(0.0f, 2.0f)]
	public float m_FadeOutDuration = 1.0f;
	[Range(0, 5)]
	public float m_RemoveDelay = 1.0f;

  #endregion

  // ######################################################################
  // MonoBehaviour Functions
  // ######################################################################

  #region Component Segments

	void Awake()
	{
  #if USE_HOTWEEN	// use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://www.holoville.com/hotween/documentation.html
  #elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html
	  // LeanTween.init(3200); // This line is optional. Here you can specify the maximum number of tweens you will use (the default is 400).  This must be called before any use of LeanTween is made for it to be effective.
  #else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php
  #endif
	}

	// Use this for initialization
	void Start()
	{
  #if USE_HOTWEEN	// use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://www.holoville.com/hotween/documentation.html
		// HOTWEEN INITIALIZATION
		// Must be done only once, before the creation of your first tween
		// (you can skip this if you want, and HOTween will be initialized automatically
		// when you create your first tween - using default values)
		HOTween.Init(true, true, true);
  #elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html
  #else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php
  #endif

	  // Get FCMain compoment
	  m_Main = this.GetComponent<FCMain>();
	}

	// Update is called once per frame
	void Update()
	{
	}

  #endregion

  // ######################################################################
  // Prop Position
  // ######################################################################

  #region Prop Position

	// Start transform position
	void UpdateTransformPos_Start()
	{
	  m_PositionState = FCProp.eTransformState.Changing;

	  if (m_PropGameObject != null)
	  {
		m_PropGameObject.transform.localPosition = transform.position + m_PosBegin;
	  }

	  m_PosValue = 0.0f;
	}

	// Update position
	void UpdateTransformPos_Transforming()
	{
	  UpdateTransformPos_TransformingWithValue(m_PosValue);
	}

	// Update position by value (0.0 to 1.0)
	void UpdateTransformPos_TransformingWithValue(float Value)
	{
	  m_PosValue = Value;
	  if (m_PropGameObject != null)
	  {
		if (m_Main.m_Elastic == true)
		{
		  m_PropGameObject.transform.position = transform.position + m_PosBegin + (m_PosEnd * Value);
		}
		else
		{
		  m_PropGameObject.transform.localPosition = m_PosBegin + (m_PosEnd * Value);
		}		
	  }
	}

	// End position
	void UpdateTransformPos_Finished()
	{
	  m_PosValue = 1;
	  m_PositionState = FCProp.eTransformState.End;
	  //m_PositionStateOld = m_PositionState;
	}

  #endregion

  // ######################################################################
  // Prop Rotation
  // ######################################################################

  #region Prop Rotation

	// Start rotation
	void UpdateRotation_Start()
	{
	  //m_RotationState = FCProp.eTransformState.Changing;

	  if (m_PropGameObject != null)
	  {
		m_PropGameObject.transform.Rotate(new Vector3(0, 0, 0));
	  }

	  m_RotationValue = 0.0f;
	}

	// Update rotation
	void UpdateRotation_Transforming()
	{
	  UpdateRotation_TransformingWithValue(m_RotationValue);
	}

	// Update rotation by value (0.0 to 1.0)
	void UpdateRotation_TransformingWithValue(float Value)
	{
	  m_RotationValue = Value;

	  if (m_PropGameObject != null)
	  {
		m_PropGameObject.transform.localRotation = Quaternion.AngleAxis((360 * Value), m_Rotation);
	  }

	  // update m_RotationValue
	  //m_RotationRoundCount = (int)m_RotationValue;
	}

	// End rotation
	void UpdateRotation_Finished()
	{
	  if (m_PropGameObject)
	  {
		m_PropGameObject.transform.parent = null;
	  }
	  
	  //m_RotationState = FCProp.eTransformState.End;
	  //m_RotationStateOld = m_RotationState;
	}

  #endregion

  // ######################################################################
  // Prop Scaling
  // ######################################################################

  #region Prop Scale

	// start scaling
	void UpdateScale_Start()
	{
	  m_ScaleState = FCProp.eTransformState.Changing;

	  if (m_PropGameObject != null)
	  {
		m_PropGameObject.transform.localScale = m_ScaleBegin;
	  }

	  m_ScaleValue = 0.0f;
	}

	// update scaling
	void UpdateScale_Transforming()
	{
	  UpdateScale_TransformingWithValue(m_ScaleValue);
	}

	// update scaling by value (0.0 to 1.0)
	void UpdateScale_TransformingWithValue(float Value)
	{
	  m_ScaleValue = Value;
	  if (m_PropGameObject != null)
	  {
		m_PropGameObject.transform.localScale = m_ScaleBegin + (m_ScaleEnd * Value);
	  }
	}

	// End scaling
	void UpdateScale_Finished()
	{
	  m_ScaleValue = 1;
	  m_ScaleState = FCProp.eTransformState.End;
	  //m_ScaleStateOld = m_ScaleState;
	}

  #endregion

  // ######################################################################
  // Create/Remove Prop
  // ######################################################################

  #region Create/Remove Prop

	// Create prop
	public void CreateProp()
	{
	  if (m_PropGameObject == null && m_Prefab != null)
	  {
		// Create Particle from m_Prefab
		m_PropGameObject = Instantiate(m_Prefab, transform.position + m_Prefab.transform.position + m_PosBegin, transform.rotation) as GameObject;
		
		// Get FCMain compoment
		m_Main = this.GetComponent<FCMain>();
		m_PropGameObject.transform.parent = transform;

		// Set position variables
		m_PositionState = FCProp.eTransformState.Begin;
		//m_PositionStateOld = FCProp.eTransformState.Begin;
		m_PosValue = 0;

		// Set rotation variables
		//m_RotationState = FCProp.eTransformState.Begin;
		//m_RotationStateOld = FCProp.eTransformState.Begin;
		m_RotationValue = 0;
		//m_RotationRoundCount = 0;

		// Set scale variables
		m_ScaleState = FCProp.eTransformState.Begin;
		//m_ScaleStateOld = FCProp.eTransformState.Begin;
		m_ScaleValue = 0;

	  }
	}

	// Remove Prop
	public void Remove()
	{
	  Remove(m_RemoveDelay);
	}

	// Remove Prop after any delay
	public void Remove(float Delay)
	{
	  m_RemoveDelay = Delay;

	  // Destroy m_PropGameObject with FCGameObjectUtil
	  if (m_RemoveDelay > 0)
	  {
		// Attach FCGameObjectUtil to m_PropGameObject
		FCGameObjectUtil pFCGameObjectUtil = m_PropGameObject.GetComponent<FCGameObjectUtil>();
		if (pFCGameObjectUtil == null)
		{
		  pFCGameObjectUtil = m_PropGameObject.AddComponent<FCGameObjectUtil>();
		}
		// Call FCGameObjectUtil.SelfRemoveParticle
		pFCGameObjectUtil.SelfRemoveGameObject(m_RemoveDelay, m_FadeOut, m_FadeOutDuration);
		m_PropGameObject = null;
	  }
	  // Destroy m_PropGameObject immediately
	  else
	  {
		Destroy(m_PropGameObject);
		m_PropGameObject = null;
	  }
	}

  #endregion

  // ######################################################################
  // Showing Prop Functions
  // ######################################################################

  #region Showing Prop

	// show prop
	public void Show()
	{
	  Show(m_PosDelay, m_RotationDelay, m_ScaleDelay);
	}

	public void Show(float PosDelay, float RotationDelay, float ScaleDelay)
	{
  #if USE_HOTWEEN	// use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://www.holoville.com/hotween/documentation.html
	  
		// tween position
		  m_PosValue = 0;
		  HOTween.To(this, m_PosDuration, new TweenParms()
			.Prop("m_PosValue", 1.0f, false)
			.Delay(PosDelay)
			.Ease(EaseType.EaseOutElastic)
			.OnStart(UpdateTransformPos_Start)
			.OnUpdate(UpdateTransformPos_Transforming)
			.OnStepComplete(UpdateTransformPos_Finished)
		  );
	  
		// tween rotation
		  m_RotationValue = 0;
		  if (m_RotationType == FCProp.eRotationType.Endless)
		  {
			FCGameObjectUtil pFCGameObjectUtil = m_PropGameObject.AddComponent<FCGameObjectUtil>();
			pFCGameObjectUtil.InitRotation(m_Rotation);
		  }
		  else if (m_RotationType == FCProp.eRotationType.LimitedRound)
		  {
			HOTween.To(this, m_RotationDurationPerRound * m_MaxRotationRound, new TweenParms()
			  .Prop("m_RotationValue", (float) m_MaxRotationRound, false)
			  .Delay(RotationDelay)
			  .Ease(EaseType.EaseInOutQuad)
			  .OnStart(UpdateRotation_Start)
			  .OnUpdate(UpdateRotation_Transforming)
			  .OnStepComplete(UpdateRotation_Finished)
			);
		  }
	  
		// tween scale
		  if(m_ScaleType == eScaleType.Enable)
		  {
			HOTween.To(this, m_ScaleDuration, new TweenParms()
			  .Prop("m_ScaleValue", 1.0f, false)
			  .Delay(ScaleDelay)
			  .Ease(EaseType.EaseOutElastic)
			  .OnStart(UpdateScale_Start)
			  .OnUpdate(UpdateScale_Transforming)
			  .OnStepComplete(UpdateScale_Finished)
			);
		  }

#elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html
	  
		// tween position
		  UpdateTransformPos_Start();
		  LeanTween.value(m_PropGameObject, UpdateTransformPos_TransformingWithValue, 0.0f, 1.0f, m_PosDuration)
			.setDelay(PosDelay)
			.setEase(LeanTweenType.easeOutElastic)
			.setOnComplete(UpdateTransformPos_Finished);
	  
		// tween rotation
		  if (m_RotationType == FCProp.eRotationType.Endless)
		  {
			FCGameObjectUtil pFCGameObjectUtil = m_PropGameObject.AddComponent<FCGameObjectUtil>();
			pFCGameObjectUtil.InitRotation(m_Rotation);
		  }
		  else if (m_RotationType == FCProp.eRotationType.LimitedRound)
		  {
			UpdateRotation_Start();
			LeanTween.value(m_PropGameObject, UpdateRotation_TransformingWithValue, 0.0f, (float) m_MaxRotationRound, m_RotationDurationPerRound * m_MaxRotationRound)
			  .setDelay(RotationDelay)
			  .setEase(LeanTweenType.easeInOutQuad)
			  .setOnComplete(UpdateRotation_Finished);
		  }
	  
		// tween scale
		  if(m_ScaleType == eScaleType.Enable)
		  {
			UpdateScale_Start();
			LeanTween.value(m_PropGameObject, UpdateScale_TransformingWithValue, 0.0f, 1.0f, m_ScaleDuration)
			  .setDelay(ScaleDelay)
			  .setEase(LeanTweenType.easeOutElastic)
			  .setOnComplete(UpdateScale_Finished);
		  }

#else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php

	  // tween position
		iTween.ValueTo(m_PropGameObject, iTween.Hash("from", 0, "to", 1,
		  "time", m_PosDuration,
		  "delay", PosDelay,
		  "easeType", FCEaseType.EaseTypeConvert(m_PosEaseType),
		  "onstart", "UpdateTransformPos_Start",
		  "onupdate", "UpdateTransformPos_TransformingWithValue",
		  "onupdatetarget", this.gameObject,
		  "oncomplete", "UpdateTransformPos_Finished"));

	  // tween rotation
		if (m_RotationType == FCProp.eRotationType.Endless)
		{
		  FCGameObjectUtil pFCGameObjectUtil = m_PropGameObject.AddComponent<FCGameObjectUtil>();
		  pFCGameObjectUtil.InitRotation(m_Rotation);
		  pFCGameObjectUtil.StartRotation();
		}
		else if (m_RotationType == FCProp.eRotationType.LimitedRound)
		{
		  iTween.ValueTo(m_PropGameObject, iTween.Hash("from", 0, "to", m_MaxRotationRound,
			"time", m_RotationDurationPerRound * m_MaxRotationRound,
			"delay", RotationDelay,
			"easeType", FCEaseType.EaseTypeConvert(m_RotationEaseType),
			"onstart", "UpdateRotation_Start",
			"onupdate", "UpdateRotation_TransformingWithValue",
			"onupdatetarget", this.gameObject,
			"oncomplete", "UpdateRotation_Finished"));
		}

	  // tween scale
		if (m_ScaleType == eScaleType.Enable)
		{
		  iTween.ValueTo(m_PropGameObject, iTween.Hash("from", 0, "to", 1,
			"time", m_ScaleDuration,
			"delay", ScaleDelay,
			"easeType", FCEaseType.EaseTypeConvert(m_ScaleEaseType),
			"onstart", "UpdateScale_Start",
			"onupdate", "UpdateScale_TransformingWithValue",
			"onupdatetarget", this.gameObject,
			"oncomplete", "UpdateScale_Finished"));
		}

  #endif

	}
  #endregion

  // ######################################################################
  // Get Functions
  // ######################################################################

  #region Get and Check

	// Prop showing
	public bool isShowing()
	{
	  if (m_PositionState == FCProp.eTransformState.Changing /*|| m_RotationState == FCProp.eTransformState.Changing*/ || m_ScaleState == FCProp.eTransformState.Changing)
		return true;

	  return false;
	}

	// Get Prefab
	public GameObject getPrefab()
	{
	  return m_Prefab;
	}

	// Get Prop GameObject
	public GameObject getPropGameObject()
	{
	  return m_PropGameObject;
	}

  #endregion
}
