using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorInputDevice : MonoBehaviour
{
	public static EditorInputDevice GetDevice(string userId)
	{
		if (_devices.TryGetValue(userId, out EditorInputDevice device))
			return device;
		else
			return null;
	}

	private static Dictionary<string, EditorInputDevice> _devices = new Dictionary<string, EditorInputDevice>();

	[Obsolete("Deprecated, use transLeftHand instead.")]
	public Transform transLeft
	{
		get => transLeftHand;
	}

	[Obsolete("Deprecated, use transRightHand instead.")]
	public Transform transRight
	{
		get => transRightHand;
	}


	public string UserId;
	public bool useEditorTarget = false;
	public Transform transRoot;
	public Transform transHead;
	public Transform transLeftHand;
	public Transform transRightHand;
	public Transform transHips;
	public Transform transLeftFoot;
	public Transform transRightFoot;

	public bool isWorking { get; private set; } = false;

	public void Init()
	{
		if (isWorking)
		{
			return;
		}

		if (_devices.ContainsKey(UserId))
		{
			Debug.LogErrorFormat("EditorInputDevice with userId {0} is allready exist.", UserId);
		}
		else
		{
			_devices.Add(UserId, this);
			isWorking = true;
		}
	}

	public void DeInit()
	{
		if (isWorking)
		{
			_devices.Remove(UserId);
			isWorking = false;
		}
	}

	void Start()
	{
		Init();
	}

	void OnDestroy()
	{
		DeInit();
	}
}