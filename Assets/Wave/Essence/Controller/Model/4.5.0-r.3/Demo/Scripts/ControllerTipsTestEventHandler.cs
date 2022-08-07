// "Wave SDK
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

//#define DEBUG

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wave.Native;
using Wave.Essence.Controller.Model;

namespace Wave.Essence.Samples.WaveController
{
	public class ControllerTipsTestEventHandler : MonoBehaviour
	{
		private static string LOG_TAG = "ControllerTipsTestEventHandler";

		private GameObject rGO = null;
		private GameObject lGO = null;
		private ControllerTips rController, lController;
		private Text rCT, rAL, rPL, rFE, lCT, lAL, lPL, lFE;
		private GameObject rTitle, rEnablePL, rDisablePL, lTitle, lEnablePL, lDisablePL;
		void OnEnable()
		{
			GameObject bs = GameObject.Find("BackButton");
			if (bs != null)
			{
				bs.SetActive(false);
			}

			rGO = GameObject.Find("WaveRightController");
			lGO = GameObject.Find("WaveLeftController");

			if (rGO == null)
				Log.i(LOG_TAG, "WaveRightController can't be found.");
			else
			{
				rController = rGO.GetComponentInChildren<ControllerTips>();
				if (rController == null)
				{
					Log.i(LOG_TAG, "ControllerTips can't be found in WaveRightController.");
				}
			}

			if (lGO == null)
				Log.i(LOG_TAG, "WaveLeftController can't be found.");
			else
			{
				lController = lGO.GetComponentInChildren<ControllerTips>();
				if (lController == null)
				{
					Log.i(LOG_TAG, "ControllerTips can't be found in WaveLeftController.");
				}
			}

			rCT = GameObject.Find("ControllerTipsInfo_Blue").GetComponent<Text>();
			if(rCT == null)
				Log.i(LOG_TAG, "ControllerTipsInfo_Blue can't be found.");
			rFE = GameObject.Find("FadingEffectInfo_Blue").GetComponent<Text>();
			if (rFE == null)
				Log.i(LOG_TAG, "FadingEffectInfo_Blue can't be found.");
			rAL = GameObject.Find("AutoLayoutInfo_Blue").GetComponent<Text>();
			if (rAL == null)
				Log.i(LOG_TAG, "AutoLayoutInfo_Blue can't be found.");
			rPL = GameObject.Find("PolylineInfo_Blue").GetComponent<Text>();
			if (rPL == null)
				Log.i(LOG_TAG, "PolylineInfo_Blue can't be found.");
			lCT = GameObject.Find("ControllerTipsInfo_Red").GetComponent<Text>();
			if (lCT == null)
				Log.i(LOG_TAG, "ControllerTipsInfo_Red can't be found.");
			lFE = GameObject.Find("FadingEffectInfo_Red").GetComponent<Text>();
			if (lFE == null)
				Log.i(LOG_TAG, "FadingEffectInfo_Red can't be found.");
			lAL = GameObject.Find("AutoLayoutInfo_Red").GetComponent<Text>();
			if (lAL == null)
				Log.i(LOG_TAG, "AutoLayoutInfo_Red can't be found.");
			lPL = GameObject.Find("PolylineInfo_Red").GetComponent<Text>();
			if (lPL == null)
				Log.i(LOG_TAG, "PolylineInfo_Red can't be found.");

			rTitle = GameObject.Find("Polyline_Blue");
			if (rTitle == null)
				Log.i(LOG_TAG, "Polyline_Blue can't be found.");
			rEnablePL = GameObject.Find("EnablePolyline_Blue");
			if (rEnablePL == null)
				Log.i(LOG_TAG, "EnablePolyline_Blue can't be found.");
			rDisablePL = GameObject.Find("DisablePolyline_Blue");
			if (rDisablePL == null)
				Log.i(LOG_TAG, "DisablePolyline_Blue can't be found.");
			lTitle = GameObject.Find("Polyline_Red");
			if (lTitle == null)
				Log.i(LOG_TAG, "Polyline_Red can't be found.");
			lEnablePL = GameObject.Find("EnablePolyline_Red");
			if (lEnablePL == null)
				Log.i(LOG_TAG, "EnablePolyline_Red can't be found.");
			lDisablePL = GameObject.Find("DisablePolyline_Red");
			if (lDisablePL == null)
				Log.i(LOG_TAG, "DisablePolyline_Red can't be found.");
		}

		// Update is called once per frame
		void Update()
		{
			if (rCT != null)
			{
				rCT.text = "ControllerTips : ";
				if (rController.enableControllerTips)
					rCT.text += "Enable";
				else
				{
					rCT.text += "Disable";
				}
			}

			if (rAL != null)
			{
				rAL.text = "AutoLayout : ";
				if (rController.enableControllerTips && rController.autoLayout)
					rAL.text += "Enable";
				else
				{
					rAL.text += "Disable";
				}
			}

			if (rPL != null)
			{
				rPL.text = "Polyline : ";
				if (rController.enableControllerTips && rController.autoLayout && rController.polyline)
					rPL.text += "Enable";
				else
				{
					rPL.text += "Disable";
				}
			}

			if (rFE != null)
			{
				rFE.text = "FadingEffect : ";
				if (rController.enableControllerTips && rController.fadingEffect)
					rFE.text += "Enable";
				else
				{
					rFE.text += "Disable";
				}
			}

			if (lCT != null)
			{
				lCT.text = "ControllerTips : ";
				if (lController.enableControllerTips)
					lCT.text += "Enable";
				else
				{
					lCT.text += "Disable";
				}
			}

			if (lAL != null)
			{
				lAL.text = "AutoLayout : ";
				if (lController.enableControllerTips && lController.autoLayout)
					lAL.text += "Enable";
				else
				{
					lAL.text += "Disable";
				}
			}

			if (lPL != null)
			{
				lPL.text = "Polyline : ";
				if (lController.enableControllerTips && lController.autoLayout && lController.polyline)
					lPL.text += "Enable";
				else
				{
					lPL.text += "Disable";
				}
			}

			if (lFE != null)
			{
				lFE.text = "FadingEffect : ";
				if (lController.enableControllerTips && lController.fadingEffect)
					lFE.text += "Enable";
				else
				{
					lFE.text += "Disable";
				}
			}
		}

		public void BackToUpLayer()
		{
			SceneManager.LoadScene(0);
		}

		public void ExitGame()
		{
			Application.Quit();
		}

		public void EnableControllerTipsR()
		{
			if ( rController != null)
			{
				rController.enableControllerTips = true;
			}
		}

		public void DisableControllerTipsR()
		{
			if (rController != null)
			{
				rController.enableControllerTips = false;
			}
		}

		public void EnableFadingEffectR()
		{
			if (rController != null)
			{
				rController.fadingEffect = true;
			}
		}

		public void DisableFadingEffectR()
		{
			if (rController != null)
			{
				rController.fadingEffect = false;
			}
		}

		public void EnableAutoLayoutR()
		{
			if (rController != null)
			{
				rController.autoLayout = true;
				rTitle.SetActive(true);
				rEnablePL.SetActive(true);
				rDisablePL.SetActive(true);
			}
		}

		public void DisableAutoLayoutR()
		{
			if (rController != null)
			{
				rController.autoLayout = false;
				rTitle.SetActive(false);
				rEnablePL.SetActive(false);
				rDisablePL.SetActive(false);
			}
		}

		public void EnablePolylineR()
		{
			if (rController != null)
			{
				rController.polyline = true;
			}
		}

		public void DisablePolylineR()
		{
			if (rController != null)
			{
				rController.polyline = false;
			}
		}

		public void EnableControllerTipsL()
		{
			if (lController != null)
			{
				lController.enableControllerTips = true;
			}
		}

		public void DisableControllerTipsL()
		{
			if (lController != null)
			{
				lController.enableControllerTips = false;
			}
		}

		public void EnableFadingEffectL()
		{
			if (lController != null)
			{
				lController.fadingEffect = true;
			}
		}

		public void DisableFadingEffectL()
		{
			if (lController != null)
			{
				lController.fadingEffect = false;
			}
		}

		public void EnableAutoLayoutL()
		{
			if (lController != null)
			{
				lController.autoLayout = true;
				lTitle.SetActive(true);
				lEnablePL.SetActive(true);
				lDisablePL.SetActive(true);
			}
		}

		public void DisableAutoLayoutL()
		{
			if (lController != null)
			{
				lController.autoLayout = false;
				lTitle.SetActive(false);
				lEnablePL.SetActive(false);
				lDisablePL.SetActive(false);
			}
		}

		public void EnablePolylineL()
		{
			if (lController != null)
			{
				lController.polyline = true;
			}
		}

		public void DisablePolylineL()
		{
			if (lController != null)
			{
				lController.polyline = false;
			}
		}
	}
}
