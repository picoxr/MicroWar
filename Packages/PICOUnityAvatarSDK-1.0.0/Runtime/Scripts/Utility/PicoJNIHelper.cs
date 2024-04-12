using System;
using UnityEngine;

public class PicoJNIHelper
{
	#region AndroidJNIHelper.CreateJNIArgArray 源码,取消AndroidJavaProxy

	public static bool IsPrimitive(System.Type t)
	{
		return t.IsPrimitive;
	}

	//stirng 变成char[],不支持取消AndroidJavaProxy
	public static jvalue[] CreateJNIArgArray(object[] args)
	{
		jvalue[] ret = new jvalue[args.GetLength(0)];
		int i = 0;
		foreach (object obj in args)
		{
			if (obj == null)
				ret[i].l = System.IntPtr.Zero;
			else if (IsPrimitive(obj.GetType()))
			{
				if (obj is System.Int32)
					ret[i].i = (System.Int32)obj;
				else if (obj is System.Boolean)
					ret[i].z = (System.Boolean)obj;
				else if (obj is System.Byte)
				{
					Debug.LogWarning(
						"Passing Byte arguments to Java methods is obsolete, pass SByte parameters instead");
					ret[i].b = (System.SByte)(System.Byte)obj;
				}
				else if (obj is System.SByte)
					ret[i].b = (System.SByte)obj;
				else if (obj is System.Int16)
					ret[i].s = (System.Int16)obj;
				else if (obj is System.Int64)
					ret[i].j = (System.Int64)obj;
				else if (obj is System.Single)
					ret[i].f = (System.Single)obj;
				else if (obj is System.Double)
					ret[i].d = (System.Double)obj;
				else if (obj is System.Char)
					ret[i].c = (System.Char)obj;
			}
			else if (obj is System.String)
			{
				//转成Char数组
				char[] charData = ((String)obj).ToCharArray();
				try
				{
					ret[i].l = AndroidJNIHelper.ConvertToJNIArray((System.Array)charData);
				}
				catch (Exception ex)
				{
					ret[i].l = System.IntPtr.Zero;
					Debug.LogError(string.Format("AndroidJNIHelper.ConvertToJNIArray is error: {0} msg : {1}",
						obj.ToString(), ex.Message));
				}
			}
			else if (obj is AndroidJavaClass)
			{
				ret[i].l = ((AndroidJavaClass)obj).GetRawClass();
			}
			else if (obj is AndroidJavaObject)
			{
				ret[i].l = ((AndroidJavaObject)obj).GetRawObject();
			}
			else if (obj is System.Array)
			{
				ret[i].l = AndroidJNIHelper.ConvertToJNIArray((System.Array)obj);
			}

			else if (obj is AndroidJavaRunnable)
			{
				ret[i].l = AndroidJNIHelper.CreateJavaRunnable((AndroidJavaRunnable)obj);
			}
			else
			{
				throw new Exception("JNI; Unknown argument type '" + obj.GetType() + "'");
			}

			++i;
		}

		return ret;
	}

	#endregion
}