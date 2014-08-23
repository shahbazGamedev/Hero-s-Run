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
* First Chest GameObject Utility class.
* This class handles endless rotations, self remove GameObject/ParticleSystem and MeshRenderer fade out.
***************/

public class FCGameObjectUtil : MonoBehaviour
{

  #region Variables

	// Rotation
	public Vector3 m_rotation;
	public bool m_isRotate = false;

	// Remove variables
	public float m_RemoveDelay = 1.0f;
	public bool m_RemoveFadeout = false;

	// Fade variables
	public float m_DurationCount = 1.0f;
	public float m_FadeOutDuration = 1.0f;
	public float m_AlphaFadeValue = 1.0f;

  #endregion


  // ######################################################################
  // MonoBehaviour Functions
  // ######################################################################

  #region Component Segments

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	  // Update rotation
	  if (m_isRotate==true)
	  {
		transform.Rotate(m_rotation, Space.Self);
	  }

	  // Update GameObject fading out
	  if (m_RemoveFadeout==true)
	  {
		m_DurationCount -= Time.deltaTime / m_FadeOutDuration;
		m_AlphaFadeValue = m_DurationCount / m_FadeOutDuration;
		recursiveFade(this.transform, m_AlphaFadeValue);
		if(m_AlphaFadeValue<=0)
		{
		  RemoveGameObject();
		}
	  }
	}
  #endregion

  // ######################################################################
  // Rotation Functions
  // ######################################################################

  #region Rotation

	// Initial rotation
	public void InitRotation(Vector3 rotation)
	{
	  m_rotation = rotation;
	}

	// Start rotation
	public void StartRotation()
	{
	  m_isRotate = true;
	}

	// End rotation
	public void StopRotation()
	{
	  m_isRotate = false;
	}

  #endregion

  // ######################################################################
  // Remove GameObject Functions
  // ######################################################################

  #region Remove GameObject

	// Remove this GameObject
	public void SelfRemoveGameObject(float RemoveDelay, bool RemoveFadeout, float FadeOutDuration)
	{
	  m_RemoveDelay = RemoveDelay;
	  if (m_RemoveDelay > 0)
	  {
		StartCoroutine(RemoveGameObjectDelay(m_RemoveDelay, RemoveFadeout, FadeOutDuration));
	  }
	  else
	  {
		RemoveGameObjectStart(RemoveFadeout, FadeOutDuration);
	  }
	}

	// Coroutine function
	IEnumerator RemoveGameObjectDelay(float RemoveDelay, bool RemoveFadeout, float FadeOutDuration)
	{
	  yield return new WaitForSeconds(RemoveDelay);
	  RemoveGameObjectStart(RemoveFadeout, FadeOutDuration);
	  yield break;
	}

	// Start removing with fade out parameters
	void RemoveGameObjectStart(bool RemoveFadeout, float FadeOutDuration)
	{
	  m_RemoveFadeout = RemoveFadeout;
	  m_FadeOutDuration = FadeOutDuration;
	}

	// Remove this GameObject
	void RemoveGameObject()
	{
	  Destroy(this.gameObject);
	}

  #endregion

  // ######################################################################
  // Remove Particle Functions
  // ######################################################################

  #region Remove Particle

	// Remove Particle
	public void SelfRemoveParticle(float RemoveDelay, float ParticleLifeTime)
	{
	  m_RemoveDelay = RemoveDelay;

	  // Destroy m_ParticleGameObject after any delay
	  if (m_RemoveDelay > 0)
	  {
		StartCoroutine(SelfClearParticleDelay(ParticleLifeTime));
		StartCoroutine(SelfRemoveParticleDelay(m_RemoveDelay + ParticleLifeTime));
	  }
	  // Destroy m_ParticleGameObject immediately
	  else
	  {
		SelfRemoveParticle();
	  }
	}

	// Remove particle after any delay
	IEnumerator SelfRemoveParticleDelay(float Delay)
	{
	  yield return new WaitForSeconds(Delay);
	  SelfRemoveParticle();
	  yield break;
	}

	// Clear ParticleSystem after any delay
	IEnumerator SelfClearParticleDelay(float Delay)
	{
	  yield return new WaitForSeconds(Delay);
	  ParticleSystem pParticleSystem = this.GetComponent<ParticleSystem>();
	  if (pParticleSystem != null)
	  {
		pParticleSystem.Clear();
	  }
	  yield break;
	}

	// Destroy ParticleSystem and GameObject
	void SelfRemoveParticle()
	{
	  ParticleSystem pParticleSystem = this.GetComponent<ParticleSystem>();
	  if (pParticleSystem != null)
	  {
		//pParticleSystem.Clear();
	  }
	  Destroy(this.gameObject);
	}

  #endregion

  // ######################################################################
  // Utilities Functions
  // ######################################################################

  #region Utilities

	// Fade children GameObjects
	void recursiveFade(Transform tran, float alpha)
	{
	  if (this.gameObject.renderer)
	  {
		// Fade only if there is Material that supports alpha color
		if (tran.gameObject.renderer.material.HasProperty("_Color"))
		{
		  Color color = this.gameObject.renderer.material.GetColor("_Color");
		  tran.gameObject.renderer.material.SetColor("_Color", new Color(color.r,
																		color.g,
																		color.b,
																		alpha));
		}

		foreach (Transform child in tran)
		{
		  recursiveFade(child, alpha);
		}
	  }
	}

  #endregion
}
