using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
public class FoldGroup : PropertyAttribute
{
	public string GroupName { get; private set; }
	public bool IsDirty { get; set; }
	public float RangFloatMin { get; private set; }
	public float RangFloatMax { get; private set; }
	public int RangIntMin { get; private set; }
	public int RangIntMax { get; private set; }
	public bool RangType { get; private set; }

	public FoldGroup(string groupName)
	{
		this.GroupName = groupName;
	}

	public FoldGroup(string groupName, int min, int max)
	{
		this.GroupName = groupName;
		this.RangType = true;
		this.RangIntMin = min;
		this.RangIntMax = max;
	}

	public FoldGroup(string groupName, float min, float max)
	{
		this.GroupName = groupName;
		this.RangType = true;
		this.RangFloatMin = min;
		this.RangFloatMax = max;
	}
}