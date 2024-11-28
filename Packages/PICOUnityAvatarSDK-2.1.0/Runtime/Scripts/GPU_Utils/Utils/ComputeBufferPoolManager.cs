using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class ComputeBufferPoolManager 
{
	static Dictionary<string, (ComputeBuffer, ComputeBufferType)> s_BufferPool = new Dictionary<string, (ComputeBuffer, ComputeBufferType)>();

	public static ComputeBuffer Fetch(string name)
	{
		if (s_BufferPool.TryGetValue(name, out (ComputeBuffer, ComputeBufferType) data))
		{
			return data.Item1;
		}
		return null;
	}
	public static ComputeBuffer Get(string name, int count, int stride, ComputeBufferType type) 
	{
		if (!s_BufferPool.TryGetValue(name, out (ComputeBuffer, ComputeBufferType) data))
		{
			var cpuBuff = new ComputeBuffer(count, stride, type);
			cpuBuff.name = name;
			s_BufferPool.Add(name, (cpuBuff, type));
			return cpuBuff;
		}
		else
		{
			if (data.Item1.count != count || data.Item1.stride != stride || data.Item2 != type)
			{
				Debug.LogError("You must change another name for this Compute Buffer");
			}
		}
		return data.Item1;
	}

	public static void Remove(string name)
	{
#if UNITY_2021_1_OR_NEWER
		if (s_BufferPool.Remove(name, out (ComputeBuffer, ComputeBufferType) cpuBuff))
		{
			cpuBuff.Item1.Dispose();
		}
#else
        if (s_BufferPool.ContainsKey(name))
		{
			var pair = s_BufferPool[name];
			pair.Item1.Dispose();
			s_BufferPool.Remove(name);
		}
#endif
	}

	public static void RemoveAll() 
	{
		foreach (var cpuBuff in s_BufferPool.Values)
		{
			cpuBuff.Item1.Dispose();
		}
		s_BufferPool.Clear();
	}
}
