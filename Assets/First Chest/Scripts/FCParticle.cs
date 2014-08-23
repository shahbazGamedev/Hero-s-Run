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

/***************
* First Chest Particles Handler. This is base class for FCPropParticle and FCChestParticle.
* Create, Remove and Update ParticleSystem.
***************/

public class FCParticle : MonoBehaviour
{

  #region Variables

	// Repository of FCMain component
	FCMain m_Main = null;

	// Prefab and Particle objects
	public GameObject m_Prefab;
	GameObject m_ParticleGameObject;

	// ParticleSystem objects
	ParticleSystem m_ParticleSystem;

	// Offset position to relate to FCProp or FCMain position
	public Vector3 m_OffSetLocalPosition;
	Vector3 m_OffSetLocalPositionOld;

	// ParticleSystem variables
	public bool m_isLoop = true;
	public ParticleSystemSimulationSpace m_SimulationSpace = ParticleSystemSimulationSpace.World;

	// ParticleSystem duration to use when call FCGameObjectUtil.SelfRemoveParticle
	float m_ParticleDuration = 1.0f;

	// Remove variable
	[Range(0, 5)]
	public float m_RemoveDelay = 1.0f;

	// Counter of how many time CreateParticle function is called
	public int m_CreateCount = 0;

  #endregion

  // ######################################################################
  // MonoBehaviour Functions
  // ######################################################################

  #region Component Segments

	// Use this for initialization
	void Start()
	{
	  // Get FCMain compoment objects
	  m_Main = this.GetComponent<FCMain>();
	}
	
	// Update is called once per frame
	void Update()
	{
	}

  #endregion

  #region Create

	// Create Particle
	public void CreateParticle()
	{
	  if (m_Prefab != null && m_ParticleGameObject == null && this.gameObject != null)
	  {
		// Make sure there is FCMain compoment
		m_Main = this.GetComponent<FCMain>();
		if (m_Main == null)
		{
		  return;
		}

		// Create Particle from m_Prefab
		m_ParticleGameObject = Instantiate(m_Prefab, this.gameObject.transform.position + m_Prefab.transform.position + m_OffSetLocalPosition, transform.rotation) as GameObject;

		// Update Particle position
		UpdateParticlePosition();
		m_OffSetLocalPositionOld = m_OffSetLocalPosition;

		// Set up ParticleSystem variables
		if (m_ParticleSystem == null)
		{
		  m_ParticleSystem = m_ParticleGameObject.GetComponent<ParticleSystem>();
		  m_ParticleDuration = m_ParticleSystem.duration;
		  SetParticleLoop(m_ParticleGameObject.transform, m_isLoop);
		}

		// Increase counter of how many time this function is called
		m_CreateCount++;
	  }
	}

	// Set Loop to children Particle
	void SetParticleLoop(Transform tran, bool isLoop)
	{
	  ParticleSystem pParticleSystem = tran.gameObject.GetComponent<ParticleSystem>();

	  if (pParticleSystem != null)
	  {
		pParticleSystem.loop = m_isLoop;
		pParticleSystem.simulationSpace = ParticleSystemSimulationSpace.World;
	  }

	  // continue find Lid in children GameObject
	  foreach (Transform child in tran)
	  {
		SetParticleLoop(child, isLoop);
	  }
	}

  #endregion
  
  // ######################################################################
  // Remove Functions
  // ######################################################################

  #region Remove

	// Remove Particle
	public void Remove()
	{
	  Remove(m_RemoveDelay);
	}

	// Remove particle after any delay
	public void Remove(float Delay)
	{
	  // Stop particle system
	  if (m_ParticleSystem != null)
	  {
		m_ParticleSystem.loop = false;
		m_ParticleSystem.Stop();
	  }

	  // Update delay time
	  m_RemoveDelay = Delay;

	  // Destroy m_ParticleGameObject with FCGameObjectUtil
	  if (m_RemoveDelay > 0)
	  {
		// Attach FCGameObjectUtil to m_ParticleGameObject
		FCGameObjectUtil pFCGameObjectUtil = m_ParticleGameObject.GetComponent<FCGameObjectUtil>();
		if (pFCGameObjectUtil == null)
		{
		  pFCGameObjectUtil = m_ParticleGameObject.AddComponent<FCGameObjectUtil>();
		}
		// Call FCGameObjectUtil.SelfRemoveParticle
		pFCGameObjectUtil.SelfRemoveParticle(m_RemoveDelay, m_ParticleDuration);
		m_ParticleGameObject = null;
	  }
	  // Destroy m_ParticleGameObject immediately
	  else
	  {
		Destroy(m_ParticleGameObject);
		m_ParticleGameObject = null;
	  }
	}

  #endregion

  // ######################################################################
  // Update Functions
  // ######################################################################

  #region Update
  
	// update particle position
	public void UpdateParticlePosition()
	{
	  if (m_Main!=null)
	  {
		if (m_OffSetLocalPositionOld != m_OffSetLocalPosition)
		{
		  // Particle was not created as a child of this Chest
		  if (m_Main.m_Elastic == true)
		  {
			m_ParticleGameObject.transform.position = transform.position + m_Prefab.transform.position + m_OffSetLocalPosition;
		  }
		  // Particle was created as a child of this Chest
		  else
		  {
			m_ParticleGameObject.transform.parent = this.gameObject.transform;
			m_ParticleGameObject.transform.position = m_ParticleGameObject.transform.position + m_Prefab.transform.position + m_OffSetLocalPosition;
		  }
		  m_OffSetLocalPositionOld = m_OffSetLocalPosition;
		}
	  }
	}

  #endregion

  // ######################################################################
  // Get Functions
  // ######################################################################

  #region Get

	// Get Prefab
	public GameObject getPrefab()
	{
	  return m_Prefab;
	}

	// Get Particle GameObject
	public GameObject getParticleGameObject()
	{
	  return m_ParticleGameObject;
	}

	// Get FCMain component
	public FCMain getMain()
	{
	  return m_Main;
	}

	// Get Counter
	public int getCounter()
	{
	  return m_CreateCount;
	}

  #endregion

  // ######################################################################
  // Set Functions
  // ######################################################################

  #region Set

	// Set RemoveDelay
	public void setRemoveDelay(float RemoveDelay)
	{
	  m_RemoveDelay = RemoveDelay;
	}

  #endregion
}
