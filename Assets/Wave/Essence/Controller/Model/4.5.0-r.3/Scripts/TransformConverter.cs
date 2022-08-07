using UnityEngine;
using Wave.Native;

namespace Wave.Essence.Controller.Model
{
	public static class TransformConverter {
		public static Quaternion GetRotation(WVR_Quatf_t glQuat)
		{
			return new Quaternion(glQuat.x, glQuat.y, -glQuat.z, -glQuat.w);
		}

		public static Vector3 GetPosition(this Matrix4x4 matrix)
		{
			var x = matrix.m03;
			var y = matrix.m13;
			var z = matrix.m23;

			return new Vector3(x, y, z);
		}

		public static Quaternion GetRotation(Matrix4x4 matrix)
		{
			float tr = matrix.m00 + matrix.m11 + matrix.m22;
			float qw, qx, qy, qz;
			if (tr > 0)
			{
				float S = Mathf.Sqrt(tr + 1.0f) * 2; // S=4*qw
				qw = 0.25f * S;
				qx = (matrix.m21 - matrix.m12) / S;
				qy = (matrix.m02 - matrix.m20) / S;
				qz = (matrix.m10 - matrix.m01) / S;
			}
			else if ((matrix.m00 > matrix.m11) & (matrix.m00 > matrix.m22))
			{
				float S = Mathf.Sqrt(1.0f + matrix.m00 - matrix.m11 - matrix.m22) * 2; // S=4*qx
				qw = (matrix.m21 - matrix.m12) / S;
				qx = 0.25f * S;
				qy = (matrix.m01 + matrix.m10) / S;
				qz = (matrix.m02 + matrix.m20) / S;
			}
			else if (matrix.m11 > matrix.m22)
			{
				float S = Mathf.Sqrt(1.0f + matrix.m11 - matrix.m00 - matrix.m22) * 2; // S=4*qy
				qw = (matrix.m02 - matrix.m20) / S;
				qx = (matrix.m01 + matrix.m10) / S;
				qy = 0.25f * S;
				qz = (matrix.m12 + matrix.m21) / S;
			}
			else
			{
				float S = Mathf.Sqrt(1.0f + matrix.m22 - matrix.m00 - matrix.m11) * 2; // S=4*qz
				qw = (matrix.m10 - matrix.m01) / S;
				qx = (matrix.m02 + matrix.m20) / S;
				qy = (matrix.m12 + matrix.m21) / S;
				qz = 0.25f * S;
			}

			return new Quaternion(qx, qy, qz, qw).normalized;
		}

		public static Vector3 GetScale(this Matrix4x4 matrix)
		{
			Vector3 scale;
			scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
			scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
			scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
			return scale;
		}

		[System.Serializable]
		public struct RigidTransform
		{
		public Vector3 pos;
		public Quaternion rot;

		public static RigidTransform identity
		{
			get { return new RigidTransform(Vector3.zero, Quaternion.identity); }
		}

		public RigidTransform(Vector3 pos, Quaternion rot)
		{
			this.pos = pos;
			this.rot = rot;
		}

		public RigidTransform(Transform t)
		{
			this.pos = t.position;
			this.rot = t.rotation;
		}

			public RigidTransform(WVR_Matrix4f_t pose)
			{
				var m = toMatrix44(pose);
				this.pos = GetPosition(m);
				this.rot = GetRotation(m);
			}

			public static Matrix4x4 RowColumnInverse(Matrix4x4 mat)
			{
				Matrix4x4 m = Matrix4x4.identity;

				m[0, 0] = mat[0, 0];
				m[0, 1] = mat[1, 0];
				m[0, 2] = mat[2, 0];
				m[0, 3] = mat[3, 0];

				m[1, 0] = mat[0, 1];
				m[1, 1] = mat[1, 1];
				m[1, 2] = mat[2, 1];
				m[1, 3] = mat[3, 1];

				m[2, 0] = mat[0, 2];
				m[2, 1] = mat[1, 2];
				m[2, 2] = mat[2, 2];
				m[2, 3] = mat[3, 2];

				m[3, 0] = mat[0, 3];
				m[3, 1] = mat[1, 3];
				m[3, 2] = mat[2, 3];
				m[3, 3] = mat[3, 3];

				return m;
			}

			//public static Matrix4x4 ROT_FROM_RH_TO_LF(Matrix4x4 mat)
			//{
			//	Matrix4x4 m = Matrix4x4.identity;
			//	Matrix4x4 g_matLHToRH = Matrix4x4.identity;

			//	g_matLHToRH[0, 0] = 1.0f;
			//	g_matLHToRH[0, 1] = 0.0f;
			//	g_matLHToRH[0, 2] = 0.0f;
			//	g_matLHToRH[0, 3] = 0.0f;

			//	g_matLHToRH[1, 0] = 0.0f;
			//	g_matLHToRH[1, 1] = 1.0f;
			//	g_matLHToRH[1, 2] = 0.0f;
			//	g_matLHToRH[1, 3] = 0.0f;

			//	g_matLHToRH[2, 0] = 0.0f;
			//	g_matLHToRH[2, 1] = 0.0f;
			//	g_matLHToRH[2, 2] = -1.0f;
			//	g_matLHToRH[2, 3] = 0.0f;

			//	g_matLHToRH[3, 0] = 0.0f;
			//	g_matLHToRH[3, 1] = 0.0f;
			//	g_matLHToRH[3, 2] = 0.0f;
			//	g_matLHToRH[3, 3] = 1.0f;

			//	m = g_matLHToRH * mat * g_matLHToRH;

			//	return m;
			//}

			public static Matrix4x4 toMatrix44(WVR_Matrix4f_t pose, bool glToUnity = true)
			{
				var m = Matrix4x4.identity;
				int sign = glToUnity ? -1 : 1;

				m[0, 0] = pose.m0;
				m[0, 1] = pose.m1;
				m[0, 2] = pose.m2 * sign;
				m[0, 3] = pose.m3;

				m[1, 0] = pose.m4;
				m[1, 1] = pose.m5;
				m[1, 2] = pose.m6 * sign;
				m[1, 3] = pose.m7;

				m[2, 0] = pose.m8 * sign;
				m[2, 1] = pose.m9 * sign;
				m[2, 2] = pose.m10;
				m[2, 3] = pose.m11 * sign;

				m[3, 0] = pose.m12;
				m[3, 1] = pose.m13;
				m[3, 2] = pose.m14;
				m[3, 3] = pose.m15;

				return m;
			}

			public static WVR_Matrix4f_t ToWVRMatrix(Matrix4x4 m, bool unityToGL = true)
			{
				WVR_Matrix4f_t pose;
				int sign = unityToGL ? -1 : 1;

				pose.m0 = m[0, 0];
				pose.m1 = m[0, 1];
				pose.m2 = m[0, 2] * sign;
				pose.m3 = m[0, 3];

				pose.m4 = m[1, 0];
				pose.m5 = m[1, 1];
				pose.m6 = m[1, 2] * sign;
				pose.m7 = m[1, 3];

				pose.m8 = m[2, 0] * sign;
				pose.m9 = m[2, 1] * sign;
				pose.m10 = m[2, 2];
				pose.m11 = m[2, 3] * sign;

				pose.m12 = m[3, 0];
				pose.m13 = m[3, 1];
				pose.m14 = m[3, 2];
				pose.m15 = m[3, 3];

				return pose;
			}

			public static Vector3 ToUnityPos(Vector3 glPos)
			{
				glPos.z *= -1;
				return glPos;
			}

			public void update(WVR_Matrix4f_t pose)
			{
				var m = toMatrix44(pose);
				this.pos = GetPosition(m);
				this.rot = GetRotation(m);
			}

			public void update(Vector3 position, Quaternion orientation)
			{
				this.pos = position;
				this.rot = orientation;
			}

			public override bool Equals(object o)
			{
				if (o is RigidTransform)
				{
					RigidTransform t = (RigidTransform)o;
					return pos == t.pos && rot == t.rot;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return pos.GetHashCode() ^ rot.GetHashCode();
			}

			public static bool operator ==(RigidTransform a, RigidTransform b)
			{
				return a.pos == b.pos && a.rot == b.rot;
			}

			public static bool operator !=(RigidTransform a, RigidTransform b)
			{
				return a.pos != b.pos || a.rot != b.rot;
			}

			public static RigidTransform operator *(RigidTransform a, RigidTransform b)
			{
				return new RigidTransform
				{
					rot = a.rot * b.rot,
					pos = a.pos + a.rot * b.pos
				};
			}

			public void Inverse()
			{
				rot = Quaternion.Inverse(rot);
				pos = -(rot * pos);
			}

			public RigidTransform GetInverse()
			{
				var t = new RigidTransform(pos, rot);
				t.Inverse();
				return t;
			}

			public Vector3 TransformPoint(Vector3 point)
			{
				return pos + (rot * point);
			}

			public static Vector3 operator *(RigidTransform t, Vector3 v)
			{
				return t.TransformPoint(v);
			}
		}
	}
}
