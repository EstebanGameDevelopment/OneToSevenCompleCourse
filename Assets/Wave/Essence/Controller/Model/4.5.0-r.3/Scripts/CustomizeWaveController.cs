// "Wave SDK
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.Controller.Model
{
	public class CustomizeWaveController : MonoBehaviour
	{
		private static string LOG_TAG = "CustomizeWaveController";

		private bool mPerformSetup = false;
		private bool ControllerTipsChanged = false;

		public bool mergeToOne = true;
		public bool updateDynamical = true;

		public bool enableButtonEffect = true;
		public bool useSystemDefinedColor = true;
		public Color buttonEffectColor = new Color(0, 179, 227, 255);

		//for controller tips
		public bool enableControllerTips = true;
		public bool autoLayout = true;
		public bool polyline = true;
		public bool fadingEffect = true;
		[Range(0, 45.0f)]
		public float displayAngle = 30.0f;
		public bool hideWhenRolling = true;
		public bool basedOnEmitter = true;
		public DisplayPlane displayPlane = DisplayPlane.Normal;

		[Header("Line Customization")]
		[Range(0.0001f, 0.001f)]
		public float lineWidth = 0.0004f;
		public Color lineColor = Color.white;

		[Header("Text Customization")]
		[Range(1, 15)]
		public int textFontSize = 8;
		public Color textColor = Color.white;

		[Header("Controller Tips Information")]
		public bool useSystemConfig = true;
		[HideInInspector]
		public List<TipInfo> tipInfoList = new List<TipInfo>();

		void OnEnable()
		{
		}

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			ControllerTips ct = this.gameObject.GetComponentInChildren<ControllerTips>();

			if (!mPerformSetup)
			{
				// render model
				RenderModel rm = this.gameObject.GetComponentInChildren<RenderModel>();
				if (rm == null)
				{
					return;
				}

				ButtonEffect be = this.gameObject.GetComponentInChildren<ButtonEffect>();
				if (be == null)
				{
					return;
				}

				StringBuilder sb = new StringBuilder();
				sb.Append("Customize Wave controller model - ");
				sb.Append(rm.WhichHand);
				sb.AppendLine();

				sb.Append("RenderModel: ");
				sb.AppendLine();
				sb.Append("  mergeToOne from ");
				sb.Append(rm.mergeToOneBone);
				sb.Append("  to ");
				sb.Append(this.mergeToOne);
				sb.AppendLine();
				sb.Append("  updateDynamical from ");
				sb.Append(rm.updateDynamically);
				sb.Append("  to ");
				sb.Append(this.updateDynamical);
				sb.AppendLine();

				sb.Append("ButtonEffect: ");
				sb.AppendLine();
				sb.Append("  enableButtonEffect from ");
				sb.Append(be.enableButtonEffect);
				sb.Append("  to ");
				sb.Append(this.enableButtonEffect);
				sb.AppendLine();
				sb.Append("  useSystemDefinedColor from ");
				sb.Append(be.useSystemConfig);
				sb.Append("  to ");
				sb.Append(this.useSystemDefinedColor);
				sb.AppendLine();
				sb.Append("  Color from ");
				sb.Append(be.buttonEffectColor);
				sb.Append("  to ");
				sb.Append(this.buttonEffectColor);
				sb.AppendLine();

				Log.i(LOG_TAG, sb.ToString(), true);

				bool changed = ((rm.mergeToOneBone != this.mergeToOne) ||
								(rm.updateDynamically != this.updateDynamical) ||
								(be.enableButtonEffect != this.enableButtonEffect) ||
								(be.useSystemConfig != this.useSystemDefinedColor) ||
								(be.buttonEffectColor != this.buttonEffectColor) ||
								this.enableControllerTips);

				rm.mergeToOneBone = this.mergeToOne;
				rm.updateDynamically = this.updateDynamical;

				be.enableButtonEffect = this.enableButtonEffect;
				be.useSystemConfig = this.useSystemDefinedColor;
				be.buttonEffectColor = this.buttonEffectColor;

				if (ControllerTipsChanged)
				{
					if (ct != null)
					{
						ct.enableControllerTips = true;
						ApplyControllerTipsParameters(ct);
					}
				}

				if (changed) {
					Log.i(LOG_TAG, "Wave Controller Setting is changed, re-create model", true);
					rm.applyChange();
				}
				mPerformSetup = true;
			}

			if (ct != null)
			{
				if (ct.enableControllerTips != this.enableControllerTips || ct.autoLayout != this.autoLayout || ct.polyline != this.polyline || ct.fadingEffect != this.fadingEffect)
					ControllerTipsChanged = true;
				if (ControllerTipsChanged)
				{
					Log.i(LOG_TAG, "ControllerTipsChanged :" + ControllerTipsChanged + " enableControllerTips: " + this.enableControllerTips + " autoLayout: " + autoLayout + " polyline: " + polyline + " fadingEffect: " + fadingEffect);
					ApplyControllerTipsParameters(ct);
					ControllerTipsChanged = false;
				}
			}
		}

		private void PrintInfoLog(string msg)
		{
			Log.i(LOG_TAG, msg, true);
		}

		private void ApplyControllerTipsParameters(ControllerTips ct)
		{

			ct.enableControllerTips = enableControllerTips;
			ct.autoLayout = autoLayout;
			ct.polyline = polyline;
			ct.fadingEffect = fadingEffect;
			ct.displayAngle = displayAngle;
			ct.hideWhenRolling = hideWhenRolling;
			ct.basedOnEmitter = basedOnEmitter;
			ct.displayPlane = displayPlane;
			ct.lineWidth = lineWidth;
			ct.lineColor = lineColor;
			ct.textFontSize = textFontSize;
			ct.textColor = textColor;
			ct.tipInfoList.Clear();

			if (useSystemConfig)
			{
				PrintInfoLog("CustomizeWaveController uses system default controller tips!");
				AddControllerTipsList();
			}
			else
			{
				PrintInfoLog("CustomizeWaveController uses customized controller tips!");
				if (tipInfoList.Count == 0)
				{
					PrintInfoLog("CustomizeWaveController doesn't setup the customized controller tips!");
					return;
				}
			}

			foreach (TipInfo ti in tipInfoList)
			{
				PrintInfoLog("inputType: " + ti.inputType);
				PrintInfoLog("alignment: " + ti.alignment);
				PrintInfoLog("buttonAndTextDistance: " + ti.buttonAndTextDistance);
				PrintInfoLog("buttonAndLineDistance: " + ti.buttonAndLineDistance);
				PrintInfoLog("lineLengthAdjustment: " + ti.lineLengthAdjustment);
				PrintInfoLog("multiLanguage: " + ti.multiLanguage);
				PrintInfoLog("inputText: " + ti.inputText);

				ct.tipInfoList.Add(ti);
			}
			ct.CreateControllerTips();
		}

		private void AddControllerTipsList()
		{
			tipInfoList.Clear();

			TipInfo home = new TipInfo();
			home.inputType = InputType.Home;
			home.alignment = Alignment.Balance;
			home.buttonAndTextDistance = 0.035f;
			home.buttonAndLineDistance = 0.0f;
			home.lineLengthAdjustment = -0.003f;
			home.multiLanguage = true;
			home.inputText = "system";

			tipInfoList.Add(home);

			TipInfo app = new TipInfo();
			app.inputType = InputType.App;
			app.alignment = Alignment.Balance;
			app.buttonAndTextDistance = 0.035f;
			app.buttonAndLineDistance = 0.0f;
			app.lineLengthAdjustment = -0.003f;
			app.multiLanguage = true;
			app.inputText = "system";

			tipInfoList.Add(app);

			TipInfo grip = new TipInfo();
			grip.inputType = InputType.Grip;
			grip.alignment = Alignment.Balance;
			grip.buttonAndTextDistance = 0.035f;
			grip.buttonAndLineDistance = 0.0f;
			grip.lineLengthAdjustment = -0.003f;
			grip.multiLanguage = true;
			grip.inputText = "system";

			tipInfoList.Add(grip);

			TipInfo trigger = new TipInfo();
			trigger.inputType = InputType.Trigger;
			trigger.alignment = Alignment.Balance;
			trigger.buttonAndTextDistance = 0.035f;
			trigger.buttonAndLineDistance = 0.0f;
			trigger.lineLengthAdjustment = -0.003f;
			trigger.multiLanguage = true;
			trigger.inputText = "system";

			tipInfoList.Add(trigger);

			TipInfo dt = new TipInfo();
			dt.inputType = InputType.DigitalTrigger;
			dt.alignment = Alignment.Balance;
			dt.buttonAndTextDistance = 0.035f;
			dt.buttonAndLineDistance = 0.0f;
			dt.lineLengthAdjustment = -0.003f;
			dt.multiLanguage = true;
			dt.inputText = "system";

			tipInfoList.Add(dt);

			TipInfo touchpad = new TipInfo();
			touchpad.inputType = InputType.TouchPad;
			touchpad.alignment = Alignment.Balance;
			touchpad.buttonAndTextDistance = 0.035f;
			touchpad.buttonAndLineDistance = 0.0f;
			touchpad.lineLengthAdjustment = -0.003f;
			touchpad.multiLanguage = true;
			touchpad.inputText = "system";

			tipInfoList.Add(touchpad);

			TipInfo vol = new TipInfo();
			vol.inputType = InputType.Volume;
			vol.alignment = Alignment.Balance;
			vol.buttonAndTextDistance = 0.035f;
			vol.buttonAndLineDistance = 0.0f;
			vol.lineLengthAdjustment = -0.003f;
			vol.multiLanguage = true;
			vol.inputText = "system";

			tipInfoList.Add(vol);

			TipInfo thumbstick = new TipInfo();
			thumbstick.inputType = InputType.Thumbstick;
			thumbstick.alignment = Alignment.Balance;
			thumbstick.buttonAndTextDistance = 0.035f;
			thumbstick.buttonAndLineDistance = 0.0f;
			thumbstick.lineLengthAdjustment = -0.003f;
			thumbstick.multiLanguage = true;
			thumbstick.inputText = "system";

			tipInfoList.Add(thumbstick);

			TipInfo buttonA = new TipInfo();
			buttonA.inputType = InputType.ButtonA;
			buttonA.alignment = Alignment.Balance;
			buttonA.buttonAndTextDistance = 0.035f;
			buttonA.buttonAndLineDistance = 0.0f;
			buttonA.lineLengthAdjustment = -0.003f;
			buttonA.multiLanguage = true;
			buttonA.inputText = "system";

			tipInfoList.Add(buttonA);

			TipInfo buttonB = new TipInfo();
			buttonB.inputType = InputType.ButtonB;
			buttonB.alignment = Alignment.Balance;
			buttonB.buttonAndTextDistance = 0.035f;
			buttonB.buttonAndLineDistance = 0.0f;
			buttonB.lineLengthAdjustment = -0.003f;
			buttonB.multiLanguage = true;
			buttonB.inputText = "system";

			tipInfoList.Add(buttonB);

			TipInfo buttonX = new TipInfo();
			buttonX.inputType = InputType.ButtonX;
			buttonX.alignment = Alignment.Balance;
			buttonX.buttonAndTextDistance = 0.035f;
			buttonX.buttonAndLineDistance = 0.0f;
			buttonX.lineLengthAdjustment = -0.003f;
			buttonX.multiLanguage = true;
			buttonX.inputText = "system";

			tipInfoList.Add(buttonX);

			TipInfo buttonY = new TipInfo();
			buttonY.inputType = InputType.ButtonY;
			buttonY.alignment = Alignment.Balance;
			buttonY.buttonAndTextDistance = 0.035f;
			buttonY.buttonAndLineDistance = 0.0f;
			buttonY.lineLengthAdjustment = -0.003f;
			buttonY.multiLanguage = true;
			buttonY.inputText = "system";

			tipInfoList.Add(buttonY);
		}
	}
}
