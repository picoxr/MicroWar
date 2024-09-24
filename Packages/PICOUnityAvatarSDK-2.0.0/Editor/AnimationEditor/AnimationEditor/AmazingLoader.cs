using System.Runtime.InteropServices;
using System;
using UnityEngine;

public class AmazingLoader
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct AmazingAnimazTrack
	{
		public IntPtr name;
		public IntPtr guid;
		public int keyCount;
		public IntPtr keyTimes;
		public int keyTimesCount;
		public IntPtr keyValues;
		public int keyValuesCount;
		public IntPtr keyInValues;
		public int keyInValuesCount;
		public IntPtr keyOutValues;
		public int keyOutValuesCount;
		public int interpolationType;
		public int trackDataType;
		public IntPtr targetName;
		public IntPtr propertyName;

		public AmazingAnimazTrack(int keyCount, string targetName, bool transformCurve, string blendShapeName)
		{
			byte[] guid = Guid.NewGuid().ToByteArray();
			IntPtr guidPtr = Marshal.AllocHGlobal(16);
			Marshal.Copy(guid, 0, guidPtr, 16);

			int valueCountMul = transformCurve ? 10 : 1;

			this.name = StringAlloc("");
			this.guid = guidPtr;
			this.keyCount = keyCount;
			keyTimes = Marshal.AllocHGlobal(Marshal.SizeOf<float>() * keyCount);
			keyTimesCount = keyCount;
			keyValues = Marshal.AllocHGlobal(Marshal.SizeOf<float>() * keyCount * valueCountMul);
			keyValuesCount = keyCount * valueCountMul;
			keyInValues = IntPtr.Zero;
			keyInValuesCount = 0;
			keyOutValues = IntPtr.Zero;
			keyOutValuesCount = 0;
			interpolationType = 7; // LINEAR
			trackDataType = transformCurve ? 5 : 0; // SRT : FLOAT
			this.targetName = StringAlloc(targetName);
			if (transformCurve)
			{
				propertyName = StringAlloc("Transform.localMatrix");
			}
			else
			{
				propertyName = StringAlloc(string.Format("MorpherComponent.channelWeights[{0}]", blendShapeName));
			}
		}

		public void SetTimes(float[] times)
		{
			Marshal.Copy(times, 0, keyTimes, times.Length);
		}

		public void SetPosition(int index, Vector3 pos)
		{
			float[] vs = new float[] { pos.x, pos.y, pos.z };
			int offset = Marshal.SizeOf<float>() * (10 * index + 7);
			IntPtr ptr = keyValues + offset;
			Marshal.Copy(vs, 0, ptr, vs.Length);
		}

		public void SetRotation(int index, Quaternion rot)
		{
			float[] vs = new float[] { rot.x, rot.y, rot.z, rot.w };
			int offset = Marshal.SizeOf<float>() * (10 * index + 3);
			IntPtr ptr = keyValues + offset;
			Marshal.Copy(vs, 0, ptr, vs.Length);
		}

		public void SetScale(int index, Vector3 scale)
		{
			float[] vs = new float[] { scale.x, scale.y, scale.z };
			int offset = Marshal.SizeOf<float>() * (10 * index + 0);
			IntPtr ptr = keyValues + offset;
			Marshal.Copy(vs, 0, ptr, vs.Length);
		}

		public void SetBlendShapeWeight(int index, float weight)
		{
			float[] vs = new float[] { weight };
			int offset = Marshal.SizeOf<float>() * (1 * index + 0);
			IntPtr ptr = keyValues + offset;
			Marshal.Copy(vs, 0, ptr, vs.Length);
		}

		public void Destroy()
		{
			Marshal.FreeHGlobal(name);
			name = IntPtr.Zero;
			Marshal.FreeHGlobal(guid);
			guid = IntPtr.Zero;
			Marshal.FreeHGlobal(keyTimes);
			keyTimes = IntPtr.Zero;
			Marshal.FreeHGlobal(keyValues);
			keyValues = IntPtr.Zero;
			Marshal.FreeHGlobal(targetName);
			targetName = IntPtr.Zero;
			Marshal.FreeHGlobal(propertyName);
			propertyName = IntPtr.Zero;
		}
	};

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct AmazingAnimaz
	{
		public IntPtr name;
		public IntPtr guid;
		public float duration;
		public float startTime;
		public float endTime;
		public int timeType;
		public IntPtr tracks;
		public int tracksCount;

		public AmazingAnimaz(string name, int trackCount)
		{
			byte[] guid = Guid.NewGuid().ToByteArray();
			IntPtr guidPtr = Marshal.AllocHGlobal(16);
			Marshal.Copy(guid, 0, guidPtr, 16);

			this.name = StringAlloc(name);
			this.guid = guidPtr;
			duration = 0;
			startTime = 0;
			endTime = 0;
			timeType = 0;
			tracks = IntPtr.Zero;
			tracksCount = trackCount;
			tracks = Marshal.AllocHGlobal(Marshal.SizeOf<AmazingAnimazTrack>() * trackCount);
		}

		public void Destroy()
		{
			Marshal.FreeHGlobal(name);
			name = IntPtr.Zero;
			Marshal.FreeHGlobal(guid);
			guid = IntPtr.Zero;
			Marshal.FreeHGlobal(tracks);
			tracks = IntPtr.Zero;
		}

		public void SetTrack(int index, ref AmazingAnimazTrack track)
		{
			int offset = Marshal.SizeOf<AmazingAnimazTrack>() * index;
			IntPtr ptr = tracks + offset;
			Marshal.StructureToPtr(track, ptr, false);
		}
	}

	public static void SaveAnimaz(string rootDir, string localPath, ref AmazingAnimaz animaz)
	{
		AmazingSaveAnimaz(rootDir, localPath, ref animaz);
	}

	static IntPtr StringAlloc(string str)
	{
		var bytes = System.Text.Encoding.UTF8.GetBytes(str);
		var ptr = Marshal.AllocHGlobal(bytes.Length + 1);
		Marshal.Copy(bytes, 0, ptr, bytes.Length);
		Marshal.WriteByte(ptr, bytes.Length, 0);
		return ptr;
	}

#if UNITY_IPHONE && !UNITY_EDITOR
	const string DLLName = "__Internal";
#else
	const string DLLName = "AmazingLoader";
#endif

	[DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void AmazingSaveAnimaz(string rootDir, string localPath, ref AmazingAnimaz animaz);
}