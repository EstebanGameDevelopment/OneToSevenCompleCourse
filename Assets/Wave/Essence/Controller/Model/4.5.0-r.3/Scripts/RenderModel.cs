// "Wave SDK
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;
using System.Runtime.InteropServices;
using UnityEngine.XR;
using Wave.Essence.Extra;
using UnityEditor;
using UnityEngine.Profiling;

namespace Wave.Essence.Controller.Model
{
	public class RenderModel : MonoBehaviour
	{
		private static string LOG_TAG = "RenderModel";
		private void PrintDebugLog(string msg)
		{
			Log.d(LOG_TAG, "Hand: " + WhichHand + ", " + msg);
		}

		private void PrintInfoLog(string msg)
		{
			Log.i(LOG_TAG, "Hand: " + WhichHand + ", " + msg);
		}

		private void PrintWarningLog(string msg)
		{
			Log.w(LOG_TAG, "Hand: " + WhichHand + ", " + msg);
		}

		public enum LoadingState
		{
			LoadingState_NOT_LOADED,
			LoadingState_LOADING,
			LoadingState_LOADED
		}

		public XR_Hand WhichHand = XR_Hand.Dominant;
		public GameObject defaultModel = null;
		public bool updateDynamically = true;
		public bool mergeToOneBone = false;

		public delegate void RenderModelDelegate(XR_Hand hand);
		public static event RenderModelDelegate onRenderModelReady = null;
		public static event RenderModelDelegate onRenderModelRemoved = null;

		private GameObject controllerSpawned = null;
		private XRNode node;

		private bool connected = false;
		private string renderModelNamePath = "";
		private string renderModelName = "";

		private List<Color32> colors = new List<Color32>();
		private Component[] childArray = null;
		private Material ImgMaterial = null;
		private WaitForEndOfFrame wfef = new WaitForEndOfFrame();
		private WaitForSeconds wfs = new WaitForSeconds(1.0f);
		private bool showBatterIndicator = true;
		private bool isBatteryIndicatorReady = false;
		private BatteryIndicator currentBattery;
		private int batteryIdx = -1;

		private ModelResource modelResource = null;
		private LoadingState mLoadingState = LoadingState.LoadingState_NOT_LOADED;

		[HideInInspector]
		public bool checkInteractionMode = false;

		// Default is not to show model.  When all model is completed loaded. Check pose valid first.
		private bool showModel = false;

		private bool EnableDirectPreview = false;

		private InputDevice inputDevice;

		class Component
		{
			private GameObject obj = null;
			private MeshRenderer renderer = null;
			private bool isVisible = false;
			private bool showState = false;

			public Component(GameObject obj, MeshRenderer renderer, bool isVisible) {
				this.obj = obj;
				this.renderer = renderer;
				this.isVisible = isVisible;
				if (obj == null || renderer == null)
					throw new System.ArgumentNullException("Controller's component didn't exist.");
			}

			public void Update(GameObject obj, MeshRenderer renderer, bool isVisible)
			{
				if (this.obj != null) { Destroy(this.obj); }
				this.obj = obj;
				this.renderer = renderer;
				this.isVisible = isVisible;
			}
			public void Clear()
			{
				if (renderer != null && renderer.material != null)
				{
					if (renderer.material.mainTexture != null)
					{
						Log.i(LOG_TAG, "Component.Clear() texture: " + renderer.material.mainTexture.name, true);
						//Destroy(renderer.material.mainTexture);  // The texture is shared and keeped in ResourceHolder
						renderer.material.mainTexture = null;
					}
					Log.i(LOG_TAG, "Component.Clear() material: " + renderer.material.name, true);
					//Destroy(renderer.material);  // The material is shared and keeped in this gameobject
					renderer.material = null;
				}

				if (obj != null)
				{
					Log.i(LOG_TAG, "Component.Clear(): " + obj.name, true);
					Destroy(obj);
					obj = null;
				}
			}

			// If visible but not show, keep hide.
			public void SetVisibility(bool isVisible)
			{
				this.isVisible = isVisible;
				if (renderer != null) { renderer.enabled = showState && isVisible; }
			}

			public void SetShowState(bool show)
			{
				this.showState = show;
				if (renderer != null) { renderer.enabled = showState && isVisible; }
			}

			public GameObject GetObject() { return obj; }
			public MeshRenderer GetRenderer() { return renderer; }
			public bool IsVisibility() { return isVisible; }
			public bool IsShow() { return showState && isVisible; }
		}

		void OnEnable()
		{
			PrintDebugLog("OnEnable");
#if UNITY_EDITOR
			EnableDirectPreview = EditorPrefs.GetBool("Wave/DirectPreview/EnableDirectPreview", false);
			PrintDebugLog("OnEnterPlayModeMethod: " + EnableDirectPreview);
#endif

			ImgMaterial = new Material(Shader.Find("Unlit/Texture"));

			if (mLoadingState == LoadingState.LoadingState_LOADING)
			{
				DestroyRenderModel("RenderModel doesn't expect model is in loading, delete all children");
			}

			if (WhichHand == XR_Hand.Dominant)
			{
				node = XRNode.RightHand;
			}
			else
			{
				node = XRNode.LeftHand;
			}

			connected = CheckConnection();

			if (connected)
			{
				WVR_DeviceType type = CheckDeviceType();

				if (mLoadingState == LoadingState.LoadingState_LOADED)
				{
					if (isRenderModelNameSameAsPrevious())
					{
						PrintDebugLog("OnEnable - Controller connected, model was loaded!");
					}
					else
					{
						DestroyRenderModel("Controller load when OnEnable, render model is different!");
						onLoadController(type);
					}
				}
				else
				{
					PrintDebugLog("Controller load when OnEnable!");
					onLoadController(type);
				}
			}

			OEMConfig.onOEMConfigChanged += onOEMConfigChanged;
		}

		void OnDisable()
		{
			PrintDebugLog("OnDisable");

			if (ImgMaterial != null)
			{
				Destroy(ImgMaterial);
				ImgMaterial = null;
			}

			if (mLoadingState == LoadingState.LoadingState_LOADING)
			{
				DestroyRenderModel("RenderModel doesn't complete creating meshes before OnDisable, delete all children");
			}

			OEMConfig.onOEMConfigChanged -= onOEMConfigChanged;
		}

		void OnDestroy()
		{
			PrintDebugLog("OnDestroy");
		}

		private void onOEMConfigChanged()
		{
			PrintDebugLog("onOEMConfigChanged");
			ReadJsonValues();
		}

		private void ReadJsonValues()
		{
#if UNITY_EDITOR
			if (EnableDirectPreview)
				showBatterIndicator = true;
#else
			showBatterIndicator = false;
#endif
			JSON_BatteryPolicy batteryP = OEMConfig.getBatteryPolicy();

			if (batteryP != null)
			{
				if (batteryP.show == 2)
					showBatterIndicator = true;
			} else
			{
				PrintDebugLog("There is no system policy!");
			}

			PrintDebugLog("showBatterIndicator: " + showBatterIndicator);
		}

		private bool isRenderModelNameSameAsPrevious()
		{
			bool _connected = CheckConnection();
			bool _same = false;

			if (!_connected)
				return _same;

			WVR_DeviceType type = CheckDeviceType();

			string tmprenderModelName = ClientInterface.GetCurrentRenderModelName(type);

			PrintDebugLog("previous render model: " + renderModelName + ", current " + type + " render model name: " + tmprenderModelName);

			if (tmprenderModelName == renderModelName)
			{
				_same = true;
			}

			return _same;
		}

		void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus) // pause
			{
				PrintInfoLog("Pause(" + pauseStatus + ") and check loading");
				if (mLoadingState == LoadingState.LoadingState_LOADING)
				{
					DestroyRenderModel("Destroy controller prefeb because of spawn process is not completed and app is going to pause.");
				}
			} else
			{
				PrintDebugLog("Resume");
			}
		}

		// Use this for initialization
		void Start()
		{
			PrintDebugLog("start() connect: " + connected + " Which hand: " + WhichHand);
			ReadJsonValues();

			if (updateDynamically)
			{
				PrintDebugLog("updateDynamically, start a coroutine to check connection and render model name periodly");
				StartCoroutine(checkRenderModelAndDelete());
			}

			if (this.transform.parent != null)
			{
				PrintDebugLog("start() parent is " + this.transform.parent.name);
			}
		}

		int t = 0;

		bool checkShowModel()
		{
			if (Interop.WVR_IsInputFocusCapturedBySystem() || !inputDevice.isValid)
				return false;

			if (checkInteractionMode && (ClientInterface.InteractionMode != XR_InteractionMode.Controller))
				return false;

			inputDevice.TryGetFeatureValue(CommonUsages.isTracked, out bool validPose);
			
			return validPose;
		}

		// Update is called once per frame
		void Update()
		{
			if (mLoadingState == LoadingState.LoadingState_NOT_LOADED)
			{
				InputDevice device = InputDevices.GetDeviceAtXRNode(node);

				if (device.isValid && device.TryGetFeatureValue(CommonUsages.isTracked, out bool validPoseState)
					&& validPoseState)
				{
					WVR_DeviceType type = CheckDeviceType();

					this.connected = true;
					PrintDebugLog("spawn render model");
					inputDevice = device;
					onLoadController(type);
				}
			}

			if (mLoadingState == LoadingState.LoadingState_LOADED)
			{
				bool preShowModel = showModel;
				showModel = checkShowModel();

				if (showModel != preShowModel)
				{
					Profiler.BeginSample("ShowHide");
					PrintDebugLog("show model change");

					if (showModel)
					{
						PrintDebugLog("Show render model to previous state");
						if (childArray != null)
						{
							for (int i = 0; i < childArray.Length; i++)
								childArray[i].SetShowState(true);
							Profiler.BeginSample("UpdateBatteryLevel");
							updateBatteryLevel();
							Profiler.EndSample();
						}
					}
					else
					{
						PrintDebugLog("Save render model state and force show to false");

						if (childArray != null)
						{
							for (int i = 0; i < childArray.Length; i++)
								childArray[i].SetShowState(false);
						}
					}
					Profiler.EndSample();
				}

				if (showModel && (t-- < 0))
				{
					Profiler.BeginSample("UpdateBatteryLevel");
					updateBatteryLevel();
					Profiler.EndSample();
					t = 200;
				}
			}

			if (Log.gpl.Print)
			{
				var p = transform.position;
				var sb = Log.CSB
					.Append("Update() hand=").Append(WhichHand)
					.Append(", connect=").Append(connected)
					.Append(", child=").Append(transform.childCount)
					.Append(", showBattery=").Append(showBatterIndicator)
					.Append(", hasBattery=").Append(isBatteryIndicatorReady)
					.Append(", ShowModel=").Append(showModel)
					.Append(", state=").Append(mLoadingState)
					.Append(", position (").Append(p.x).Append(", ").Append(p.y).Append(", ").Append(p.z).Append(")");
				Log.d(LOG_TAG, sb.ToString(), true);

				if (showModel)
				{
					if (childArray != null)
					{
						for (int i = 0; i < childArray.Length; i++)
						{
							if (childArray[i] != null)
							{
								var obj = childArray[i].GetObject();
								if (obj.name.Equals("__CM__Body"))
								{
									var p2 = obj.transform.position;
									var sb2= Log.CSB
										.Append("Update() render model ").Append(WhichHand)
										.Append(", name=").Append(obj.name)
										.Append(", visible=").Append(childArray[i].IsVisibility())
										.Append(", position (").Append(p2.x).Append(", ").Append(p2.y).Append(", ").Append(p2.z).Append(")");
									Log.d(LOG_TAG, sb2.ToString());
								}
							}
						}
					}
				}
			}
		}

		public void applyChange()
		{
			DestroyRenderModel("Setting is changed.");
			WVR_DeviceType type = CheckDeviceType();
			onLoadController(type);
		}

		private void onLoadController(WVR_DeviceType type)
		{
			mLoadingState = LoadingState.LoadingState_LOADING;
			PrintDebugLog("Pos: " + this.transform.localPosition.x + " " + this.transform.localPosition.y + " " + this.transform.localPosition.z);
			PrintDebugLog("Rot: " + this.transform.localEulerAngles);
			PrintDebugLog("MergeToOneBone: " + mergeToOneBone);
			PrintDebugLog("type: " + type);

			if (Interop.WVR_GetWaveRuntimeVersion() < 2 && !EnableDirectPreview)
			{
				PrintDebugLog("onLoadController in old service");
				if (defaultModel != null)
				{
					DestroySpawnedController();
					controllerSpawned = Instantiate(defaultModel, this.transform);
					controllerSpawned.transform.parent = this.transform;
				}
				else
				{
					PrintDebugLog("Can't load controller model from DS, default model is null and load WaveFinchController");
					var prefab = Resources.Load("DefaultController/WaveFinchController") as GameObject;
					if (prefab != null)
					{
						DestroySpawnedController();
						controllerSpawned = Instantiate(prefab, this.transform);
						controllerSpawned.transform.parent = this.transform;
					}
					mLoadingState = LoadingState.LoadingState_LOADED;
				}
				mLoadingState = LoadingState.LoadingState_LOADED;
				return;
			}

			renderModelName = ClientInterface.GetCurrentRenderModelName(type);

			if (renderModelName.Equals(""))
			{
				PrintDebugLog("Can not find " + type + " render model.");
				if (defaultModel != null)
				{
					PrintDebugLog("Can't load controller model from DS, load default model");
					DestroySpawnedController();
					controllerSpawned = Instantiate(defaultModel, this.transform);
					controllerSpawned.transform.parent = this.transform;
					mLoadingState = LoadingState.LoadingState_LOADED;
				}
				else
				{
					PrintDebugLog("Can't load controller model from DS, default model is null and load WaveFinchController");
					var prefab = Resources.Load("DefaultController/WaveFinchController") as GameObject;
					if (prefab != null)
					{
						DestroySpawnedController();
						controllerSpawned = Instantiate(prefab, this.transform);
						controllerSpawned.transform.parent = this.transform;
					}
					mLoadingState = LoadingState.LoadingState_LOADED;
				}
				return;
			}

			bool retModel = false;
			modelResource = null;

			retModel = ResourceHolder.Instance.addRenderModel(renderModelName, WhichHand, mergeToOneBone);
			if (retModel)
			{
				PrintDebugLog("Add " + renderModelName + " model sucessfully!");
			}

			modelResource = ResourceHolder.Instance.getRenderModelResource(renderModelName, WhichHand, mergeToOneBone);

			if (modelResource != null)
			{
				mLoadingState = LoadingState.LoadingState_LOADING;

				PrintDebugLog("Starting load " + renderModelName + " model!");

				StartCoroutine(SpawnRenderModel());
			}
			else
			{
				PrintDebugLog("Model is null!");

				if (defaultModel != null)
				{
					PrintDebugLog("Can't load controller model from DS, load default model");
					DestroySpawnedController();
					controllerSpawned = Instantiate(defaultModel, this.transform);
					controllerSpawned.transform.parent = this.transform;
					mLoadingState = LoadingState.LoadingState_LOADED;
				} else
				{
					PrintDebugLog("Can't load controller model from DS, default model is null and load WaveFinchController");
					var prefab = Resources.Load("DefaultController/WaveFinchController") as GameObject;
					if (prefab != null)
					{
						DestroySpawnedController();
						controllerSpawned = Instantiate(prefab, this.transform);
						controllerSpawned.transform.parent = this.transform;
					}
					mLoadingState = LoadingState.LoadingState_LOADED;
				}
			}
		}

		string emitterMeshName = "__CM__Emitter";
		string textureContent = "";

		IEnumerator SpawnRenderModel()
		{
			while (true)
			{
				if (modelResource != null)
				{
					if (modelResource.parserReady) break;
				}
				PrintDebugLog("SpawnRenderModel is waiting");
				yield return wfef;
			}

			PrintDebugLog("Start to spawn all meshes!");

			if (modelResource == null)
			{
				PrintDebugLog("modelResource is null, skipping spawn objects");
				mLoadingState = LoadingState.LoadingState_NOT_LOADED;
				yield return null;
			}

			PrintDebugLog("modelResource texture count = " + modelResource.modelTextureCount);

			Profiler.BeginSample("Create Texture");
			for (int t = 0; t < modelResource.modelTextureCount; t++)
			{
				TextureInfo mainTexture = modelResource.modelTextureInfo[t];
				if (modelResource.modelTexture[t] == null)
				{
					Texture2D modelpng = new Texture2D((int)mainTexture.width, (int)mainTexture.height, TextureFormat.RGBA32, false);
					modelpng.LoadRawTextureData(mainTexture.modelTextureData);
					modelpng.Apply();

					// The texture is shared and keeped in ResourceHolder.  And will not create again.
					//if (modelResource.modelTexture[t] != null) { Destroy(modelResource.modelTexture[t]); }
					modelResource.modelTexture[t] = modelpng;
				}

				for (int q = 0; q < 10240; q+=1024) {
					textureContent = "";

					for (int c = 0; c < 64; c++)
					{
						if ((q * 64 + c) >= mainTexture.modelTextureData.Length)
							break;
						textureContent += mainTexture.modelTextureData.GetValue(q*64+c).ToString();
						textureContent += " ";
					}
					PrintDebugLog("T(" + t + ") L(" + q + ")=" + textureContent);
				}

				PrintDebugLog("Add [" + t + "] to texture2D");
			}
			Profiler.EndSample();

			if (childArray == null || childArray.Length != modelResource.sectionCount)
			{
				if (childArray == null)
				{
					childArray = new Component[modelResource.sectionCount];
					PrintDebugLog("SpawnRenderModel() initialize the childArray size: " + childArray.Length);
				}
				else // childArray.Length != modelResource.sectionCount
				{
					DestroyRenderModel("SpawnRenderModel() Creates a new child array.");
					childArray = new Component[modelResource.sectionCount];
					PrintDebugLog("SpawnRenderModel() realloc the childArray size: " + childArray.Length);
				}
				for (int i = 0; i < childArray.Length; i++)
				{
					childArray[i] = null;
				}
			}

			string meshName = "";
			for (int i = 0; i < modelResource.sectionCount; i++)
			{
				Profiler.BeginSample("Create Mesh");
				meshName = Marshal.PtrToStringAnsi(modelResource.FBXInfo[i].meshName);

				for (uint j = 0; j < i; j++)
				{
					string tmp = Marshal.PtrToStringAnsi(modelResource.FBXInfo[j].meshName);

					if (tmp.Equals(meshName))
					{
						PrintDebugLog(meshName + " is created! skip.");
						Profiler.EndSample();
						continue;
					}
				}

				Mesh updateMesh = new Mesh();
				GameObject meshGO = new GameObject();
				MeshRenderer meshRenderer = meshGO.AddComponent<MeshRenderer>();
				MeshFilter meshfilter = meshGO.AddComponent<MeshFilter>();
				meshGO.transform.parent = this.transform;
				meshGO.name = meshName;
				Matrix4x4 t = TransformConverter.RigidTransform.toMatrix44(modelResource.FBXInfo[i].matrix, false);

				Vector3 pos = TransformConverter.GetPosition(t);
				pos.z = -pos.z;
				meshGO.transform.localPosition = pos;

				meshGO.transform.localRotation = TransformConverter.GetRotation(t);
				Vector3 angle = meshGO.transform.localEulerAngles;
				angle.x = -angle.x;
				meshGO.transform.localEulerAngles = angle;
				meshGO.transform.localScale = TransformConverter.GetScale(t);

				PrintDebugLog("i = " + i + " MeshGO = " + meshName + ", localPosition: " + meshGO.transform.localPosition.x + ", " + meshGO.transform.localPosition.y + ", " + meshGO.transform.localPosition.z);
				PrintDebugLog("i = " + i + " MeshGO = " + meshName + ", localRotation: " + meshGO.transform.localEulerAngles);
				PrintDebugLog("i = " + i + " MeshGO = " + meshName + ", localScale: " + meshGO.transform.localScale);

				updateMesh.Clear();
				updateMesh.vertices = modelResource.SectionInfo[i]._vectice;
				updateMesh.uv = modelResource.SectionInfo[i]._uv;
				updateMesh.uv2 = modelResource.SectionInfo[i]._uv;
				updateMesh.colors32 = colors.ToArray();
				updateMesh.normals = modelResource.SectionInfo[i]._normal;
				updateMesh.SetIndices(modelResource.SectionInfo[i]._indice, MeshTopology.Triangles, 0);
				updateMesh.name = meshName;
				if (meshfilter != null)
				{
					//We just create it.  should not have mesh inside
					//if (meshfilter.mesh != null) { Destroy(meshfilter.mesh); }
					meshfilter.mesh = updateMesh;
				}
				if (meshRenderer != null)
				{
					if (ImgMaterial == null)
					{
						PrintDebugLog("ImgMaterial is null");
					}
					meshRenderer.material = ImgMaterial;
					// The texture is shared and keeped in ResourceHolder
					//if (meshRenderer.material.mainTexture != null) { Destroy(meshRenderer.material.mainTexture); }
					meshRenderer.material.mainTexture = modelResource.modelTexture[0];
					meshRenderer.enabled = false;  // Wait all component is loaded.
				}

				if (meshName.Equals(emitterMeshName))
				{
					PrintDebugLog(meshName + " is found, set " + meshName + " visible: true");
					if (childArray[i] == null)
						childArray[i] = new Component(meshGO, meshRenderer, true);
					else
						childArray[i].Update(meshGO, meshRenderer, true);
				}
				else if (meshName.Equals("__CM__Battery"))
				{
					isBatteryIndicatorReady = false;
					if (modelResource.isBatterySetting)
					{
						if (modelResource.batteryTextureList != null)
						{
							Material mat = null;

							if (modelResource.hand == XR_Hand.Dominant)
							{
								PrintDebugLog(modelResource.hand + " loaded Materials/WaveBatteryMatR");
								mat = Resources.Load("Materials/WaveBatteryMatR") as Material;
							}
							else
							{
								PrintDebugLog(modelResource.hand + " loaded Materials/WaveBatteryMatL");
								mat = Resources.Load("Materials/WaveBatteryMatL") as Material;
							}

							if (mat != null)
							{
								//Should not destroy this material.
								//if (meshRenderer.material != null) { Destroy(meshRenderer.material); }
								meshRenderer.material = Instantiate(mat);
							}

							foreach (BatteryIndicator bi in modelResource.batteryTextureList)
							{
								TextureInfo ti = bi.batteryTextureInfo;

								if (bi.batteryTexture == null)
								{
									// The texture is shared and keeped in ResourceHolder and will not create it again.
									//if (bi.batteryTexture != null) { Destroy(bi.batteryTexture); }
									bi.batteryTexture = new Texture2D((int)ti.width, (int)ti.height, TextureFormat.RGBA32, false);
									bi.batteryTexture.LoadRawTextureData(ti.modelTextureData);
									bi.batteryTexture.Apply();
								}
								PrintInfoLog(" min: " + bi.min + " max: " + bi.max + " loaded: " + bi.textureLoaded + " w: " + ti.width + " h: " + ti.height + " size: " + ti.size + " array length: " + ti.modelTextureData.Length);
							}

							// The texture is shared and keeped in ResourceHolder and will not create it again.
							//if (meshRenderer.material.mainTexture != null) { Destroy(meshRenderer.material.mainTexture); }
							meshRenderer.material.mainTexture = modelResource.batteryTextureList[0].batteryTexture;
							isBatteryIndicatorReady = true;
						}
					}
					batteryIdx = i;

					if (childArray[i] == null)
						childArray[i] = new Component(meshGO, meshRenderer, false);
					else
						childArray[i].Update(meshGO, meshRenderer, false);

					PrintDebugLog(meshName + " is found, set " + meshName + " visible: False (waiting for update");
				}
				else if (meshName == "__CM__TouchPad_Touch")
				{
					if (childArray[i] == null)
						childArray[i] = new Component(meshGO, meshRenderer, false);
					else
						childArray[i].Update(meshGO, meshRenderer, false);

					PrintDebugLog(meshName + " is found, set " + meshName + " visible: False");
				}
				else
				{
					if (childArray[i] == null)
						childArray[i] = new Component(meshGO, meshRenderer, modelResource.SectionInfo[i]._active);
					else
						childArray[i].Update(meshGO, meshRenderer, modelResource.SectionInfo[i]._active);

					PrintDebugLog("set " + meshName + " visible: " + modelResource.SectionInfo[i]._active);
				}

				Profiler.EndSample();
				yield return wfef;
			}
			PrintDebugLog("send " + WhichHand + " RENDER_MODEL_READY ");

			Profiler.BeginSample("onRenderModelReady");
			onRenderModelReady?.Invoke(WhichHand);
			Profiler.EndSample();

			// This will cause significant GC event in a scene with complex design.
			//Resources.UnloadUnusedAssets();
			mLoadingState = LoadingState.LoadingState_LOADED;
		}

		void updateBatteryLevel()
		{
			if (batteryIdx >= 0 && childArray[batteryIdx] != null)
			{
				if (showBatterIndicator && isBatteryIndicatorReady && showModel)
				{
					if ((modelResource == null) || (modelResource.batteryTextureList == null))
						return;

					bool found = false;
					WVR_DeviceType type = CheckDeviceType();
					float batteryP = Interop.WVR_GetDeviceBatteryPercentage(type);
					if (batteryP < 0)
					{
						PrintDebugLog("updateBatteryLevel BatteryPercentage is negative, return");
						childArray[batteryIdx].SetVisibility(false);
						return;
					}
					foreach (BatteryIndicator bi in modelResource.batteryTextureList)
					{
						if (batteryP >= bi.min / 100 && batteryP <= bi.max / 100)
						{
							currentBattery = bi;
							found = true;
							break;
						}
					}
					if (found)
					{
						childArray[batteryIdx].GetRenderer().material.mainTexture = currentBattery.batteryTexture;
						childArray[batteryIdx].SetVisibility(true);
						PrintDebugLog("updateBatteryLevel battery level to " + currentBattery.level + ", battery percent: " + batteryP);
					}
					else
					{
						childArray[batteryIdx].SetVisibility(false);
					}
				}
				else
				{
					childArray[batteryIdx].SetVisibility(false);
				}
			}
		}

		IEnumerator checkRenderModelAndDelete()
		{
			while (true)
			{
				DeleteControllerWhenDisconnect();
				yield return wfs;
			}
		}

		public void showRenderModel(bool isControllerMode)
		{
			Profiler.BeginSample("ShowRenderModel");
			if (childArray != null)
			{
				for (int i = 0; i < childArray.Length; i++)
				{
					if (childArray[i] != null)
					{
						childArray[i].SetShowState(isControllerMode);
					}
				}
			}
			if (controllerSpawned != null)
				ShowSpawnedController(isControllerMode);
			Profiler.EndSample();
		}

		private void ShowSpawnedController(bool isShow)
		{
			if (controllerSpawned != null)
				controllerSpawned.SetActive(isShow);
		}

		private void DestroyRenderModel(string reason)
		{
			Profiler.BeginSample("DestroyRenderModel");
			PrintDebugLog("DestroyRenderModel() " + reason);

			DeleteChild();
			DestroySpawnedController();

			this.connected = false;
			mLoadingState = LoadingState.LoadingState_NOT_LOADED;
			onRenderModelRemoved?.Invoke(WhichHand);

			Profiler.EndSample();
		}

		private void DestroySpawnedController()
		{
			if (controllerSpawned != null)
			{
				Destroy(controllerSpawned);
				controllerSpawned = null;
			}
		}

		private void DeleteChild()
		{
			Profiler.BeginSample("DeleteChild");

			if (childArray == null || childArray.Length == 0)
				return;

			int ca = childArray.Length;
			PrintInfoLog("deleteChild count: " + ca);

			for (int i = 0; i < ca; i++)
			{
				if (childArray[i] != null)
				{
					childArray[i].Clear();
					childArray[i] = null;
				}
			}
			childArray = null;

			Profiler.EndSample();
		}

		private void DeleteControllerWhenDisconnect()
		{
			if (mLoadingState != LoadingState.LoadingState_LOADED)
				return;

			this.connected = CheckConnection();

			if (this.connected)
			{
				WVR_DeviceType type = CheckDeviceType();

				string tmprenderModelName = ClientInterface.GetCurrentRenderModelName(type);

				if (tmprenderModelName != renderModelName)
				{
					DestroyRenderModel("Destroy controller prefeb because " + type + " render model is different");
				}
			}
			else
			{
				DestroyRenderModel("Destroy controller prefeb because it is disconnect");
			}
			return;
		}

		private bool CheckConnection()
		{
#if UNITY_EDITOR
			if (!EnableDirectPreview)
				return true;
#endif
			// InputDevice is a struct.  Therefore GetDeviceAtXRNode will never return null.
			if (!inputDevice.isValid)
				inputDevice = InputDevices.GetDeviceAtXRNode(node);

			return inputDevice.isValid;
		}

		private WVR_DeviceType CheckDeviceType()
		{
			return (WhichHand == XR_Hand.Right ? WVR_DeviceType.WVR_DeviceType_Controller_Right : WVR_DeviceType.WVR_DeviceType_Controller_Left);
		}
	}
}
