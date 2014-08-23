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
* First Chest Sound Handler.
* Play/Stop sounds.
***************/

public class FCSound : MonoBehaviour
{

  #region Variables

	public string m_tempString = "";

	// Close sound variables
	public bool m_EnableSoundClose = true;
	public AudioClip m_SoundClose;
	[Range(0, 2)]
	public float m_SoundCloseDelay = 0.25f;

	// Open sound variables
	public bool m_EnableSoundOpen = true;
	public AudioClip m_SoundOpen;
	[Range(0, 2)]
	public float m_SoundOpenDelay = 0.0f;

	// Open faile sound variables
	public bool m_EnableSoundOpenFailed = true;
	public AudioClip m_SoundOpenFailed;
	[Range(0, 2)]
	public float m_SoundOpenFailedDelay = 0.0f;

	// Lock sound variables
	public bool m_EnableSoundSetLock = true;
	public AudioClip m_SoundSetLock;
	[Range(0, 2)]
	public float m_SoundSetLockDelay = 0.0f;

	// Unlock sound variables
	public bool m_EnableSoundSetUnlock = true;
	public AudioClip m_SoundSetUnlock;
	[Range(0, 2)]
	public float m_SoundSetUnlockDelay = 0.0f;

	// Prop sound variables
	public bool m_EnableSoundProp = true;
	public AudioClip m_SoundProp;
	[Range(0, 2)]
	public float m_SoundPropDelay = 0.25f;
  
  #endregion

  // ######################################################################
  // MonoBehaviour Functions
  // ######################################################################

  #region Component Segments

	// Use this for initialization
	void Start()
	{
	  // Create AudioSounce
	  CreateAudioSource();
	}

	// Create AudioSource
	public void CreateAudioSource()
	{
	  AudioSource pAudioSource = GetComponent<AudioSource>();
	  if (pAudioSource == null)
	  {
		pAudioSource = this.gameObject.AddComponent<AudioSource>();
		pAudioSource.rolloffMode = AudioRolloffMode.Linear;
	  }
	}

	// Update is called once per frame
	void Update()
	{

	}

  #endregion

  // ######################################################################
  // Play/Stop sound Functions
  // ######################################################################

  #region Play each sound

	// Play close sound
	public void PlaySoundClose()
	{
	  if (m_EnableSoundClose == true && m_SoundClose != null)
	  {
		// Stop Prop sound
		StopSoundProp(m_SoundCloseDelay*0.5f);

		// Play Close sound
		PlaySound(m_SoundClose, m_SoundCloseDelay);
	  }
	}

	// Play Open sound
	public void PlaySoundOpen()
	{
	  if (m_EnableSoundOpen == true && m_SoundOpen != null)
		PlaySound(m_SoundOpen, m_SoundOpenDelay);
	}

	// Play Open failed sound
	public void PlaySoundOpenFailed()
	{
	  if (m_EnableSoundOpenFailed == true && m_SoundOpenFailed != null)
		PlaySound(m_SoundOpenFailed, m_SoundOpenFailedDelay);
	}

	// Play Lock sound
	public void PlaySoundSetLock()
	{
	  if (m_EnableSoundSetLock == true && m_SoundSetLock != null)
		PlaySound(m_SoundSetLock, m_SoundSetLockDelay);
	}

	// Play Unlock sound
	public void PlaySoundSetUnlock()
	{
	  if (m_EnableSoundSetUnlock == true && m_SoundSetUnlock != null)
		PlaySound(m_SoundSetUnlock, m_SoundSetUnlockDelay);
	}

	// Play Propshow sound
	public void PlaySoundProp()
	{
	  if (m_EnableSoundProp == true && m_SoundProp != null)
		PlaySound(m_SoundProp, m_SoundPropDelay);
	}

	// Stop Propshow sound
	public void StopSoundProp()
	{
	  if (m_EnableSoundProp == true && m_SoundProp != null)
	  {
		if (!audio.isPlaying)
		{
		  audio.Stop();
		}
	  }
	}

	// Stop Prop sound after any delay
	public void StopSoundProp(float Delay)
	{
	  // Play sound after any delay
	  if (Delay > 0)
	  {
		StartCoroutine(StopSoundDelay(Delay));
	  }
	  // Play sound immediately
	  else
	  {
		audio.Stop();
	  }
	}

  #endregion

  // ######################################################################
  // Common Functions
  // ######################################################################

  #region Play/Stop Common Functions

	// Play specific AudioClip sound with delay
	void PlaySound(AudioClip pAudioClip, float Delay)
	{
	  // Nothing to do if pAudioClip is null
	  if (pAudioClip == null)
	  {
		return;
	  }

	  // Play sound after any delay
	  if (Delay > 0)
	  {
		StartCoroutine(PlaySoundDelay(pAudioClip, Delay));
	  }
	  // Play sound immediately
	  else
	  {
		audio.PlayOneShot(pAudioClip);
	  }
	}

	// Play sound after any delay
	IEnumerator PlaySoundDelay(AudioClip pAudioClip, float Delay)
	{
	  yield return new WaitForSeconds(Delay);
	  audio.PlayOneShot(pAudioClip);
	  yield break;
	}

	// Stop sound after any delay
	IEnumerator StopSoundDelay(float Delay)
	{
	  yield return new WaitForSeconds(Delay);
	  audio.Stop();
	  yield break;
	}

  #endregion
}
