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

/***************
* First Chest Easetypes Handler.
* Convert easetype between Hotween, Leantween and iTween tweeners.
**************/

public class FCEaseType : MonoBehaviour
{

  #region Variables

	// Common ease type for iTween, Hotween, LeanTween
	// Hotween EaseType: http://www.holoville.com/hotween/hotweenAPI/namespace_holoville_1_1_h_o_tween.html#ab8f6c428f087160deca07d7d402c4934
	// LeanTween EaseType: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTweenType.html
	// iTween EaseType: http://itween.pixelplacement.com/documentation.php
	// Here's a good reference for what you can expect from each ease type: http://www.robertpenner.com/easing/easing_demo.html
	public enum eEaseType
	{
	  InQuad,
	  OutQuad,
	  InOutQuad,
	  InCubic,
	  OutCubic,
	  InOutCubic,
	  InQuart,
	  OutQuart,
	  InOutQuart,
	  InQuint,
	  OutQuint,
	  InOutQuint,
	  InSine,
	  OutSine,
	  InOutSine,
	  InExpo,
	  OutExpo,
	  InOutExpo,
	  InCirc,
	  OutCirc,
	  InOutCirc,
	  linear,
	  spring,
	  InBounce,
	  OutBounce,
	  InOutBounce,
	  InBack,
	  OutBack,
	  InOutBack,
	  InElastic,
	  OutElastic,
	  InOutElastic
	}

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
	
	  }

	  // Update is called once per frame
	  void Update () {

	  }
  #endregion

  // ######################################################################
  // EaseType Converter Functions
  // ######################################################################

  #region EaseType Converter
#if USE_HOTWEEN	// use HOTween: https://www.assetstore.unity3d.com/#/content/3311 Documentation:  http://www.holoville.com/hotween/documentation.html
  public static Holoville.HOTween.EaseType EaseTypeConvert(eEaseType easeType)
  {
	Holoville.HOTween.EaseType result = Holoville.HOTween.EaseType.Linear;
	switch (easeType)
	{
	  case eEaseType.InQuad:    result = Holoville.HOTween.EaseType.EaseInQuad; break;
	  case eEaseType.OutQuad:    result = Holoville.HOTween.EaseType.EaseInOutQuad; break;
	  case eEaseType.InOutQuad:    result = Holoville.HOTween.EaseType.EaseInOutQuad; break;
	  case eEaseType.InCubic:    result = Holoville.HOTween.EaseType.EaseOutCubic; break;
	  case eEaseType.OutCubic:    result = Holoville.HOTween.EaseType.EaseOutCubic; break;
	  case eEaseType.InOutCubic:    result = Holoville.HOTween.EaseType.EaseInOutCubic; break;
	  case eEaseType.InQuart:    result = Holoville.HOTween.EaseType.EaseInQuart; break;
	  case eEaseType.OutQuart:    result = Holoville.HOTween.EaseType.EaseOutQuart; break;
	  case eEaseType.InOutQuart:    result = Holoville.HOTween.EaseType.EaseInOutQuart; break;
	  case eEaseType.InQuint:    result = Holoville.HOTween.EaseType.EaseInQuint; break;
	  case eEaseType.OutQuint:    result = Holoville.HOTween.EaseType.EaseOutQuint; break;
	  case eEaseType.InOutQuint:    result = Holoville.HOTween.EaseType.EaseInOutQuint; break;
	  case eEaseType.InSine:    result = Holoville.HOTween.EaseType.EaseInSine; break;
	  case eEaseType.OutSine:    result = Holoville.HOTween.EaseType.EaseOutSine; break;
	  case eEaseType.InOutSine:    result = Holoville.HOTween.EaseType.EaseInOutSine; break;
	  case eEaseType.InExpo:    result = Holoville.HOTween.EaseType.EaseInExpo; break;
	  case eEaseType.OutExpo:    result = Holoville.HOTween.EaseType.EaseOutExpo; break;
	  case eEaseType.InOutExpo:    result = Holoville.HOTween.EaseType.EaseInOutExpo; break;
	  case eEaseType.InCirc:    result = Holoville.HOTween.EaseType.EaseInCirc; break;
	  case eEaseType.OutCirc:    result = Holoville.HOTween.EaseType.EaseOutCirc; break;
	  case eEaseType.InOutCirc:    result = Holoville.HOTween.EaseType.EaseInOutCirc; break;
	  case eEaseType.linear:    result = Holoville.HOTween.EaseType.Linear; break;
	  case eEaseType.InBounce:    result = Holoville.HOTween.EaseType.EaseInBounce; break;
	  case eEaseType.OutBounce:    result = Holoville.HOTween.EaseType.EaseOutBounce; break;
	  case eEaseType.InOutBounce:    result = Holoville.HOTween.EaseType.EaseInOutBounce; break;
	  case eEaseType.InBack:    result = Holoville.HOTween.EaseType.EaseInBack; break;
	  case eEaseType.OutBack:    result = Holoville.HOTween.EaseType.EaseInOutBack; break;
	  case eEaseType.InOutBack:    result = Holoville.HOTween.EaseType.EaseInOutBack; break;
	  case eEaseType.InElastic:    result = Holoville.HOTween.EaseType.EaseInElastic; break;
	  case eEaseType.OutElastic:    result = Holoville.HOTween.EaseType.EaseOutElastic; break;
	  case eEaseType.InOutElastic:    result = Holoville.HOTween.EaseType.EaseInOutElastic; break;
	  default:    result = Holoville.HOTween.EaseType.Linear; break;
	}
	return result;
  }
#elif USE_LEANTWEEN // use LeanTween: https://www.assetstore.unity3d.com/#/content/3595 Documentation: http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html
  public static LeanTweenType EaseTypeConvert(eEaseType easeType)
  {
	LeanTweenType result = LeanTweenType.linear;
	switch (easeType)
	{
	  case eEaseType.InQuad:    result = LeanTweenType.easeInQuad; break;
	  case eEaseType.OutQuad:    result = LeanTweenType.easeInOutQuad; break;
	  case eEaseType.InOutQuad:    result = LeanTweenType.easeInOutQuad; break;
	  case eEaseType.InCubic:    result = LeanTweenType.easeOutCubic; break;
	  case eEaseType.OutCubic:    result = LeanTweenType.easeOutCubic; break;
	  case eEaseType.InOutCubic:    result = LeanTweenType.easeInOutCubic; break;
	  case eEaseType.InQuart:    result = LeanTweenType.easeInQuart; break;
	  case eEaseType.OutQuart:    result = LeanTweenType.easeOutQuart; break;
	  case eEaseType.InOutQuart:    result = LeanTweenType.easeInOutQuart; break;
	  case eEaseType.InQuint:    result = LeanTweenType.easeInQuint; break;
	  case eEaseType.OutQuint:    result = LeanTweenType.easeOutQuint; break;
	  case eEaseType.InOutQuint:    result = LeanTweenType.easeInOutQuint; break;
	  case eEaseType.InSine:    result = LeanTweenType.easeInSine; break;
	  case eEaseType.OutSine:    result = LeanTweenType.easeOutSine; break;
	  case eEaseType.InOutSine:    result = LeanTweenType.easeInOutSine; break;
	  case eEaseType.InExpo:    result = LeanTweenType.easeInExpo; break;
	  case eEaseType.OutExpo:    result = LeanTweenType.easeOutExpo; break;
	  case eEaseType.InOutExpo:    result = LeanTweenType.easeInOutExpo; break;
	  case eEaseType.InCirc:    result = LeanTweenType.easeInCirc; break;
	  case eEaseType.OutCirc:    result = LeanTweenType.easeOutCirc; break;
	  case eEaseType.InOutCirc:    result = LeanTweenType.easeInOutCirc; break;
	  case eEaseType.linear:    result = LeanTweenType.linear; break;
	  case eEaseType.InBounce:    result = LeanTweenType.easeInBounce; break;
	  case eEaseType.OutBounce:    result = LeanTweenType.easeOutBounce; break;
	  case eEaseType.InOutBounce:    result = LeanTweenType.easeInOutBounce; break;
	  case eEaseType.InBack:    result = LeanTweenType.easeInBack; break;
	  case eEaseType.OutBack:    result = LeanTweenType.easeInOutBack; break;
	  case eEaseType.InOutBack:    result = LeanTweenType.easeInOutBack; break;
	  case eEaseType.InElastic:    result = LeanTweenType.easeInElastic; break;
	  case eEaseType.OutElastic:    result = LeanTweenType.easeOutElastic; break;
	  case eEaseType.InOutElastic:    result = LeanTweenType.easeInOutElastic; break;
	  default:    result = LeanTweenType.linear; break;
	}
	return result;
  }
#else // use iTween: https://www.assetstore.unity3d.com/#/content/84 Documentation: http://itween.pixelplacement.com/documentation.php
	public static string EaseTypeConvert(eEaseType easeType)
	{
	  string result = "linear";
	  switch (easeType)
	  {
		case eEaseType.InQuad: result = "EaseInQuad"; break;
		case eEaseType.OutQuad: result = "EaseInOutQuad"; break;
		case eEaseType.InOutQuad: result = "EaseInOutQuad"; break;
		case eEaseType.InCubic: result = "EaseOutCubic"; break;
		case eEaseType.OutCubic: result = "EaseOutCubic"; break;
		case eEaseType.InOutCubic: result = "EaseInOutCubic"; break;
		case eEaseType.InQuart: result = "EaseInQuart"; break;
		case eEaseType.OutQuart: result = "EaseOutQuart"; break;
		case eEaseType.InOutQuart: result = "EaseInOutQuart"; break;
		case eEaseType.InQuint: result = "EaseInQuint"; break;
		case eEaseType.OutQuint: result = "EaseOutQuint"; break;
		case eEaseType.InOutQuint: result = "EaseInOutQuint"; break;
		case eEaseType.InSine: result = "EaseInSine"; break;
		case eEaseType.OutSine: result = "EaseOutSine"; break;
		case eEaseType.InOutSine: result = "EaseInOutSine"; break;
		case eEaseType.InExpo: result = "EaseInExpo"; break;
		case eEaseType.OutExpo: result = "EaseOutExpo"; break;
		case eEaseType.InOutExpo: result = "EaseInOutExpo"; break;
		case eEaseType.InCirc: result = "EaseInCirc"; break;
		case eEaseType.OutCirc: result = "EaseOutCirc"; break;
		case eEaseType.InOutCirc: result = "EaseInOutCirc"; break;
		case eEaseType.linear: result = "Linear"; break;
		case eEaseType.InBounce: result = "EaseInBounce"; break;
		case eEaseType.OutBounce: result = "EaseOutBounce"; break;
		case eEaseType.InOutBounce: result = "EaseInOutBounce"; break;
		case eEaseType.InBack: result = "EaseInBack"; break;
		case eEaseType.OutBack: result = "EaseInOutBack"; break;
		case eEaseType.InOutBack: result = "EaseInOutBack"; break;
		case eEaseType.InElastic: result = "EaseInElastic"; break;
		case eEaseType.OutElastic: result = "EaseOutElastic"; break;
		case eEaseType.InOutElastic: result = "EaseInOutElastic"; break;
		default: result = "Linear"; break;
	  }
	  return result;
	}
#endif
  #endregion
}
