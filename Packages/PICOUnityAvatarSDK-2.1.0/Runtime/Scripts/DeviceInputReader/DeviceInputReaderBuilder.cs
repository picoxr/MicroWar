namespace Pico
{
	namespace Avatar
	{
		public partial class DeviceInputReaderBuilder
		{
			public DeviceInputReaderBuilderInputType type;
			public string userId;

			public DeviceInputReaderBuilder SetType(DeviceInputReaderBuilderInputType type)
			{
				this.type = type;
				return this;
			}

			public DeviceInputReaderBuilder SetUserId(string userId)
			{
				this.userId = userId;
				return this;
			}

			public static IDeviceInputReader CreateFrom(DeviceInputReaderBuilder builder)
			{
				if (builder == null)
					return null;
#if UNITY_EDITOR
                if (EditorInputDevice.GetDevice(builder.userId) != null && builder.type != DeviceInputReaderBuilderInputType.BodyTracking && builder.type != DeviceInputReaderBuilderInputType.RemotePackage)
                {
                    builder.type = DeviceInputReaderBuilderInputType.Editor;
                }
#endif

				IDeviceInputReader deviceInputReader = null;
				switch (builder.type)
				{
					case DeviceInputReaderBuilderInputType.RemotePackage:
					{
						deviceInputReader = new RemoteDeviceInputReader();
					}
						break;
					case DeviceInputReaderBuilderInputType.BodyTracking:
					{
						deviceInputReader = new BodyTrackingDeviceInputReader();
						deviceInputReader.InitInputFeatureUsage();
					}
						break;
					case DeviceInputReaderBuilderInputType.PicoXR:
					{
						deviceInputReader = new PXRDeviceInputReader();
						deviceInputReader.InitInputFeatureUsage();
					}
						break;
					case DeviceInputReaderBuilderInputType.Editor:
					{
						deviceInputReader = new EditorDeviceInputReader();
					}
						break;
					default:
						break;
				}

				return deviceInputReader;
			}
		}

		public static class DeviceInputReaderBuilderExtra
		{
			public static IDeviceInputReader Build(this DeviceInputReaderBuilder builder) =>
				DeviceInputReaderBuilder.CreateFrom(builder);
		}
	}
}