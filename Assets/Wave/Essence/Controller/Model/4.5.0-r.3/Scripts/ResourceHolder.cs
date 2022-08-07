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
using UnityEngine;
using Wave.Native;
using System.Threading;
using System.Runtime.InteropServices;
using System;

namespace Wave.Essence.Controller.Model
{
	[System.Serializable]
	public class BatteryIndicator
	{
		public int level;
		public float min;
		public float max;
		public string texturePath;
		public bool textureLoaded;
		public Texture2D batteryTexture;
		public TextureInfo batteryTextureInfo;
	}

	[System.Serializable]
	public class TouchSetting
	{
		public Vector3 touchForward;
		public Vector3 touchCenter;
		public Vector3 touchRight;
		public Vector3 touchPtU;
		public Vector3 touchPtW;
		public Vector3 touchPtV;
		public float raidus;
		public float touchptHeight;
	}

	[System.Serializable]
	public class TextureInfo
	{
		public byte[] modelTextureData;
		public int width;
		public int height;
		public int stride;
		public int size;
		public int format;
	}

	[System.Serializable]
	public class ModelResource
	{
		public string renderModelName;
		public bool loadFromAsset;
		public bool mergeToOne;
		public XR_Hand hand;

		public uint sectionCount;
		public FBXInfo_t[] FBXInfo;
		public MeshInfo_t[] SectionInfo;
		public bool parserReady;

		public int modelTextureCount;
		public Texture2D[] modelTexture;
		public TextureInfo[] modelTextureInfo;

		public bool isTouchSetting;
		public TouchSetting TouchSetting;

		public bool isBatterySetting;
		public List<BatteryIndicator> batteryTextureList;
	}

	public class ResourceHolder
	{
		private static string LOG_TAG = "ResourceHolder";
		private Thread mthread;

		private static ResourceHolder instance = null;
		public static ResourceHolder Instance
		{
			get
			{
				if (instance == null)
				{
					Log.i(LOG_TAG, "create ResourceHolder instance");

					instance = new ResourceHolder();
				}
				return instance;
			}
		}

		private void PrintDebugLog(string msg)
		{
			Log.d(LOG_TAG, msg);
		}

		private void PrintInfoLog(string msg)
		{
			Log.i(LOG_TAG, msg);
		}

		private void PrintWarningLog(string msg)
		{
			Log.w(LOG_TAG, msg);
		}

		public List<ModelResource> renderModelList = new List<ModelResource>();

		public bool isRenderModelExist(string renderModel, XR_Hand hand, bool merge)
		{
			foreach (ModelResource t in renderModelList)
			{
				if ((t.renderModelName == renderModel) && (t.mergeToOne == merge) && (t.hand == hand))
				{
					return true;
				}
			}

			return false;
		}

		public ModelResource getRenderModelResource(string renderModel, XR_Hand hand, bool merge)
		{
			foreach (ModelResource t in renderModelList)
			{
				if ((t.renderModelName == renderModel) && (t.mergeToOne == merge) && (t.hand == hand))
				{
					return t;
				}
			}

			return null;
		}

		private void getNativeControllerModel(XR_Hand hand, ModelResource curr, bool isOneBone)
		{
			PrintInfoLog("getNativeControllerModel start, IntPtr size = " + IntPtr.Size + ", isOneBone = " + isOneBone);

			IntPtr ctrlModel = IntPtr.Zero;
			int IntBits = IntPtr.Size;

			WVR_DeviceType deviceType = (hand == XR_Hand.Dominant) ? WVR_DeviceType.WVR_DeviceType_Controller_Right : WVR_DeviceType.WVR_DeviceType_Controller_Left;

			WVR_Result r = Interop.WVR_GetCurrentControllerModel(deviceType, ref ctrlModel, isOneBone);

			PrintInfoLog("WVR_GetCurrentControllerModel, ctrlModel IntPtr = " + ctrlModel.ToInt32());

			PrintInfoLog("sizeof(WVR_CtrlerModel) = " + Marshal.SizeOf(typeof(WVR_CtrlerModel)));

			if (r == WVR_Result.WVR_Success)
			{
				if (ctrlModel != IntPtr.Zero)
				{
					WVR_CtrlerModel ctrl = (WVR_CtrlerModel)Marshal.PtrToStructure(ctrlModel, typeof(WVR_CtrlerModel));

					PrintInfoLog("render model name = " + ctrl.name + " , load from asset = " + ctrl.loadFromAsset);

					WVR_CtrlerCompInfoTable cit = ctrl.compInfos;

					int szStruct = Marshal.SizeOf(typeof(WVR_CtrlerCompInfo));

					PrintInfoLog("Controller component size = " + cit.size);

					curr.FBXInfo = new FBXInfo_t[cit.size];
					curr.sectionCount = cit.size;
					curr.SectionInfo = new MeshInfo_t[cit.size];
					curr.loadFromAsset = ctrl.loadFromAsset;

					for (int i = 0; i < cit.size; i++)
					{
						WVR_CtrlerCompInfo wcci;

						if (IntBits == 4)
							wcci = (WVR_CtrlerCompInfo)Marshal.PtrToStructure(new IntPtr(cit.table.ToInt32() + (szStruct * i)), typeof(WVR_CtrlerCompInfo));
						else
							wcci = (WVR_CtrlerCompInfo)Marshal.PtrToStructure(new IntPtr(cit.table.ToInt64() + (szStruct * i)), typeof(WVR_CtrlerCompInfo));

						curr.FBXInfo[i] = new FBXInfo_t();
						curr.SectionInfo[i] = new MeshInfo_t();

						curr.FBXInfo[i].meshName = Marshal.StringToHGlobalAnsi(wcci.name);
						curr.SectionInfo[i]._active = wcci.defaultDraw;

						PrintInfoLog("Controller component name = " + wcci.name + ", tex index = " + wcci.texIndex + ", active= " + curr.SectionInfo[i]._active);

						// local matrix
						Matrix4x4 lt = TransformConverter.RigidTransform.toMatrix44(wcci.localMat, false);
						Matrix4x4 t = TransformConverter.RigidTransform.RowColumnInverse(lt);
						PrintInfoLog(" matrix = (" + t.m00 + ", " + t.m01 + ", " + t.m02 + ", " + t.m03 + ")");
						PrintInfoLog(" matrix = (" + t.m10 + ", " + t.m11 + ", " + t.m12 + ", " + t.m13 + ")");
						PrintInfoLog(" matrix = (" + t.m20 + ", " + t.m21 + ", " + t.m22 + ", " + t.m23 + ")");
						PrintInfoLog(" matrix = (" + t.m30 + ", " + t.m31 + ", " + t.m32 + ", " + t.m33 + ")");

						curr.FBXInfo[i].matrix = TransformConverter.RigidTransform.ToWVRMatrix(t, false);

						WVR_VertexBuffer vertices = wcci.vertices;

						if (vertices.dimension == 3)
						{
							uint verticesCount = (vertices.size / vertices.dimension);

							PrintInfoLog(" vertices size = " + vertices.size + ", dimension = " + vertices.dimension + ", count = " + verticesCount);

							curr.SectionInfo[i]._vectice = new Vector3[verticesCount];
							float[] verticeArray = new float[vertices.size];

							Marshal.Copy(vertices.buffer, verticeArray, 0, verticeArray.Length);

							int verticeIndex = 0;
							int floatIndex = 0;

							while (verticeIndex < verticesCount)
							{
								curr.SectionInfo[i]._vectice[verticeIndex] = new Vector3();
								curr.SectionInfo[i]._vectice[verticeIndex].x = verticeArray[floatIndex++];
								curr.SectionInfo[i]._vectice[verticeIndex].y = verticeArray[floatIndex++];
								curr.SectionInfo[i]._vectice[verticeIndex].z = verticeArray[floatIndex++] * -1.0f;

								verticeIndex++;
							}
						}
						else
						{
							PrintWarningLog("vertices buffer's dimension incorrect!");
						}

						// normals
						WVR_VertexBuffer normals = wcci.normals;

						if (normals.dimension == 3)
						{
							uint normalsCount = (normals.size / normals.dimension);
							PrintInfoLog(" normals size = " + normals.size + ", dimension = " + normals.dimension + ", count = " + normalsCount);
							curr.SectionInfo[i]._normal = new Vector3[normalsCount];
							float[] normalArray = new float[normals.size];

							Marshal.Copy(normals.buffer, normalArray, 0, normalArray.Length);

							int normalsIndex = 0;
							int floatIndex = 0;

							while (normalsIndex < normalsCount)
							{
								curr.SectionInfo[i]._normal[normalsIndex] = new Vector3();
								curr.SectionInfo[i]._normal[normalsIndex].x = normalArray[floatIndex++];
								curr.SectionInfo[i]._normal[normalsIndex].y = normalArray[floatIndex++];
								curr.SectionInfo[i]._normal[normalsIndex].z = normalArray[floatIndex++] * -1.0f;

								normalsIndex++;
							}

							PrintInfoLog(" normals size = " + normals.size + ", dimension = " + normals.dimension + ", count = " + normalsCount);
						}
						else
						{
							PrintWarningLog("normals buffer's dimension incorrect!");
						}

						// texCoord
						WVR_VertexBuffer texCoord = wcci.texCoords;

						if (texCoord.dimension == 2)
						{
							uint uvCount = (texCoord.size / texCoord.dimension);
							PrintInfoLog(" texCoord size = " + texCoord.size + ", dimension = " + texCoord.dimension + ", count = " + uvCount);
							curr.SectionInfo[i]._uv = new Vector2[uvCount];
							float[] texCoordArray = new float[texCoord.size];

							Marshal.Copy(texCoord.buffer, texCoordArray, 0, texCoordArray.Length);

							int uvIndex = 0;
							int floatIndex = 0;

							while (uvIndex < uvCount)
							{
								curr.SectionInfo[i]._uv[uvIndex] = new Vector2();
								curr.SectionInfo[i]._uv[uvIndex].x = texCoordArray[floatIndex++];
								curr.SectionInfo[i]._uv[uvIndex].y = texCoordArray[floatIndex++];

								uvIndex++;
							}
						}
						else
						{
							PrintWarningLog("normals buffer's dimension incorrect!");
						}

						// indices
						WVR_IndexBuffer indices = wcci.indices;
						PrintInfoLog(" indices size = " + indices.size);

						curr.SectionInfo[i]._indice = new int[indices.size];
						Marshal.Copy(indices.buffer, curr.SectionInfo[i]._indice, 0, curr.SectionInfo[i]._indice.Length);

						uint indiceIndex = 0;

						while (indiceIndex < indices.size)
						{
							int tmp = curr.SectionInfo[i]._indice[indiceIndex];
							curr.SectionInfo[i]._indice[indiceIndex] = curr.SectionInfo[i]._indice[indiceIndex + 2];
							curr.SectionInfo[i]._indice[indiceIndex + 2] = tmp;
							indiceIndex += 3;
						}
					}

					// Controller texture section
					WVR_CtrlerTexBitmapTable wctbt = ctrl.bitmapInfos;
					PrintInfoLog("Controller textures = " + wctbt.size);
					int bmStruct = Marshal.SizeOf(typeof(WVR_CtrlerTexBitmap));
					curr.modelTextureCount = (int)wctbt.size;
					curr.modelTextureInfo = new TextureInfo[wctbt.size];
					curr.modelTexture = new Texture2D[wctbt.size];

					for (int mt = 0; mt < wctbt.size; mt++)
					{
						TextureInfo ct = new TextureInfo();

						WVR_CtrlerTexBitmap wctb;

						if (IntBits == 4)
							wctb = (WVR_CtrlerTexBitmap)Marshal.PtrToStructure(new IntPtr(wctbt.table.ToInt32() + (bmStruct * mt)), typeof(WVR_CtrlerTexBitmap));
						else
							wctb = (WVR_CtrlerTexBitmap)Marshal.PtrToStructure(new IntPtr(wctbt.table.ToInt64() + (bmStruct * mt)), typeof(WVR_CtrlerTexBitmap));

						PrintInfoLog(" [" + mt + "] bitmap width = " + wctb.width);
						PrintInfoLog(" [" + mt + "] bitmap height = " + wctb.height);
						PrintInfoLog(" [" + mt + "] bitmap stride = " + wctb.stride);
						PrintInfoLog(" [" + mt + "] bitmap format = " + wctb.format);
						// bitmap size
						var rawImageSize = wctb.height * wctb.stride;

						ct.modelTextureData = new byte[rawImageSize];
						Marshal.Copy(wctb.bitmap, ct.modelTextureData, 0, ct.modelTextureData.Length);
						ct.width = (int)wctb.width;
						ct.height = (int)wctb.height;
						ct.stride = (int)wctb.stride;
						ct.format = (int)wctb.format;
						ct.size = (int)rawImageSize;

						curr.modelTextureInfo[mt] = ct;
					}

					// Touchpad section
					PrintDebugLog("---  Get touch info from runtime  ---");
					WVR_TouchPadPlane wtpp = ctrl.touchpadPlane;

					curr.TouchSetting = new TouchSetting();
					curr.TouchSetting.touchCenter.x = wtpp.center.v0 * 100f;
					curr.TouchSetting.touchCenter.y = wtpp.center.v1 * 100f;
					curr.TouchSetting.touchCenter.z = (-1.0f * wtpp.center.v2) * 100f;
					PrintInfoLog(" touchCenter! x: " + curr.TouchSetting.touchCenter.x + " ,y: " + curr.TouchSetting.touchCenter.y + " ,z: " + curr.TouchSetting.touchCenter.z);

					curr.TouchSetting.raidus = wtpp.radius * 100;

					curr.TouchSetting.touchptHeight = wtpp.floatingDistance * 100;

					curr.isTouchSetting = wtpp.valid;

					curr.TouchSetting.touchPtU.x = wtpp.u.v0;
					curr.TouchSetting.touchPtU.y = wtpp.u.v1;
					curr.TouchSetting.touchPtU.z = wtpp.u.v2;

					curr.TouchSetting.touchPtV.x = wtpp.v.v0;
					curr.TouchSetting.touchPtV.y = wtpp.v.v1;
					curr.TouchSetting.touchPtV.z = wtpp.v.v2;

					curr.TouchSetting.touchPtW.x = wtpp.w.v0;
					curr.TouchSetting.touchPtW.y = wtpp.w.v1;
					curr.TouchSetting.touchPtW.z = -1.0f * wtpp.w.v2;
					PrintInfoLog(" Floating distance : " + curr.TouchSetting.touchptHeight);

					PrintInfoLog(" touchPtW! x: " + curr.TouchSetting.touchPtW.x + " ,y: " + curr.TouchSetting.touchPtW.y + " ,z: " + curr.TouchSetting.touchPtW.z);
					PrintInfoLog(" touchPtU! x: " + curr.TouchSetting.touchPtU.x + " ,y: " + curr.TouchSetting.touchPtU.y + " ,z: " + curr.TouchSetting.touchPtU.z);
					PrintInfoLog(" touchPtV! x: " + curr.TouchSetting.touchPtV.x + " ,y: " + curr.TouchSetting.touchPtV.y + " ,z: " + curr.TouchSetting.touchPtV.z);
					PrintInfoLog(" raidus: " + curr.TouchSetting.raidus);
					PrintInfoLog(" isTouchSetting: " + curr.isTouchSetting);

					// Battery section
					PrintDebugLog("---  Get battery info from runtime  ---");
					WVR_BatteryLevelTable wblt = ctrl.batteryLevels;

					List<BatteryIndicator> batteryTextureList = new List<BatteryIndicator>();
					curr.batteryTextureList = batteryTextureList;

					PrintInfoLog("Battery levels = " + wblt.size);

					int btStruct = Marshal.SizeOf(typeof(WVR_CtrlerTexBitmap));
					int sizeInt = Marshal.SizeOf(typeof(int));

					for (int b = 0; b < wblt.size; b++)
					{
						WVR_CtrlerTexBitmap batteryImage;
						int batteryMin = 0;
						int batteryMax = 0;

						if (IntBits == 4)
						{
							batteryImage = (WVR_CtrlerTexBitmap)Marshal.PtrToStructure(new IntPtr(wblt.texTable.ToInt32() + (btStruct * b)), typeof(WVR_CtrlerTexBitmap));
							batteryMin = (int)Marshal.PtrToStructure(new IntPtr(wblt.minTable.ToInt32() + (sizeInt * b)), typeof(int));
							batteryMax = (int)Marshal.PtrToStructure(new IntPtr(wblt.maxTable.ToInt32() + (sizeInt * b)), typeof(int));
						}
						else
						{
							batteryImage = (WVR_CtrlerTexBitmap)Marshal.PtrToStructure(new IntPtr(wblt.texTable.ToInt64() + (btStruct * b)), typeof(WVR_CtrlerTexBitmap));
							batteryMin = (int)Marshal.PtrToStructure(new IntPtr(wblt.minTable.ToInt64() + (sizeInt * b)), typeof(int));
							batteryMax = (int)Marshal.PtrToStructure(new IntPtr(wblt.maxTable.ToInt64() + (sizeInt * b)), typeof(int));
						}

						BatteryIndicator tmpBI = new BatteryIndicator();
						tmpBI.level = b;
						tmpBI.min = (float)batteryMin;
						tmpBI.max = (float)batteryMax;

						var batteryImageSize = batteryImage.height * batteryImage.stride;

						tmpBI.batteryTextureInfo = new TextureInfo();
						tmpBI.batteryTextureInfo.modelTextureData = new byte[batteryImageSize];
						Marshal.Copy(batteryImage.bitmap, tmpBI.batteryTextureInfo.modelTextureData, 0, tmpBI.batteryTextureInfo.modelTextureData.Length);
						tmpBI.batteryTextureInfo.width = (int)batteryImage.width;
						tmpBI.batteryTextureInfo.height = (int)batteryImage.height;
						tmpBI.batteryTextureInfo.stride = (int)batteryImage.stride;
						tmpBI.batteryTextureInfo.format = (int)batteryImage.format;
						tmpBI.batteryTextureInfo.size = (int)batteryImageSize;
						tmpBI.textureLoaded = true;
						PrintInfoLog(" Battery Level[" + tmpBI.level + "] min: " + tmpBI.min + " max: " + tmpBI.max + " loaded: " + tmpBI.textureLoaded + " w: " + batteryImage.width + " h: " + batteryImage.height + " size: " + batteryImageSize);

						batteryTextureList.Add(tmpBI);
					}
					curr.isBatterySetting = true;

					PrintInfoLog("WVR_ReleaseControllerModel, ctrlModel IntPtr = " + ctrlModel.ToInt32());

					PrintInfoLog("Call WVR_ReleaseControllerModel");
					Interop.WVR_ReleaseControllerModel(ref ctrlModel);
				}
				else
				{
					PrintWarningLog("WVR_GetCurrentControllerModel return model is null");
				}
			}
			else
			{
				PrintWarningLog("WVR_GetCurrentControllerModel fail!");
			}

			curr.parserReady = true;
			PrintInfoLog("Call WVR_GetCurrentControllerModel end");
		}


		public bool addRenderModel(string renderModel, XR_Hand hand, bool merge)
		{
			if (isRenderModelExist(renderModel, hand, merge))
			{
				PrintInfoLog(hand + " " + renderModel + " is already added, skip it.");
				return false;
			}

			ModelResource newMR = new ModelResource();
			newMR.renderModelName = renderModel;
			newMR.mergeToOne = merge;
			newMR.parserReady = false;
			newMR.hand = hand;
			renderModelList.Add(newMR);

			PrintDebugLog("Initial a thread to load current controller model assets");
			mthread = new Thread(() => getNativeControllerModel(hand, newMR, merge));
			mthread.Name = "srpResourceHolder";
			mthread.Start();

			return true;
		}
	}
}
