using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pico.Avatar
{
	/// <summary>
	/// avatar ik inputType
	/// </summary>
	public enum DeviceInputReaderBuilderInputType
	{
		/// <summary>
		/// Invalid, internal use
		/// </summary>
		Invalid = 0,

		/// <summary>
		/// In the Editor, the motion of the gameobject is used as the input source data of the ik driver 
		/// </summary>
		Editor = 1,

		/// <summary>
		/// PicoVR device as IK driver input source data 
		/// </summary>
		PicoXR = 2,

		/// <summary>
		/// Network data as IK driver input source data 
		/// </summary>
		RemotePackage = 3,

		/// <summary>
		/// Body tracking data as swift device input source data
		/// </summary>
		BodyTracking = 4
	}

	public partial class DeviceInputReaderBuilder
	{
	}
}