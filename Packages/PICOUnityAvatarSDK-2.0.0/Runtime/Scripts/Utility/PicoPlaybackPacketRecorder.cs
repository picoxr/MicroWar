using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Used to record playback packets.
		/// </summary>
		public class PicoPlaybackPacketRecorder
		{
			// playback record/sample interval. It is usually set by sdk application.
			public static float recordInterval = 0.2f;

			// Start recourds.
			public void StartRecord(AvatarEntity avatarEntity)
			{
				try
				{
					var dirName = AvatarEnv.avatarCachePath + "/records";
					//
					System.IO.Directory.CreateDirectory(dirName);

					string fileNameBase;
					if (avatarEntity.owner.capabilities.controlSourceType == ControlSourceType.OtherPlayer)
					{
						fileNameBase = "/remote_";
					}
					else
					{
						fileNameBase = "/local_";
					}

					//
					var dt = System.DateTime.Now;
					var timeText = string.Format("{0}d{1}h{2}m{3}s_", dt.Day, dt.Hour, dt.Minute, dt.Second);
					//
					var recordFilePathName = dirName + fileNameBase + timeText + avatarEntity.owner.userId + ".bytes";
					UnityEngine.Debug.Log("recordPacketFileName: " + recordFilePathName);

					//
					var recordPacketFile = new System.IO.FileStream(recordFilePathName, System.IO.FileMode.Create
						, System.IO.FileAccess.Write);
					//
					_recordPacketFileWriter = new System.IO.BinaryWriter(recordPacketFile);

					// recordInterval
					_recordPacketFileWriter.Write(recordInterval);
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogException(ex);
				}
			}

			public void StopRecord()
			{
				if (_recordPacketFileWriter != null)
				{
					_recordPacketFileWriter.Close();
					_recordPacketFileWriter = null;
				}
			}

			public void RecordPacket(MemoryView packetMemoryView)
			{
				if (_recordPacketFileWriter == null)
				{
					return;
				}

				try
				{
					//
					{
						var dataBytes = packetMemoryView.getData();

						// time
						_recordPacketFileWriter.Write(Time.realtimeSinceStartup);

						// write packet body.
						var len = dataBytes.Length;
						// write length.
						_recordPacketFileWriter.Write(len);
						_recordPacketFileWriter.Write(dataBytes, 0, dataBytes.Length);
					}
					//
					if (++_packetCountFromLastFlush > FlushPacketCount)
					{
						_packetCountFromLastFlush = 0;
						_recordPacketFileWriter.Flush();
					}
				}
				catch (System.Exception ex)
				{
					_recordPacketFileWriter = null;
					UnityEngine.Debug.LogException(ex);
				}
			}


			#region Private Fields

			// file to save playback packets.
			private System.IO.BinaryWriter _recordPacketFileWriter = null;

			private const int FlushPacketCount = 100;

			// count of packet from last flush. if added and arrives FlushPacketCount, will invoke Flush for _recordPacketFileWriter.
			private int _packetCountFromLastFlush = 0;

			#endregion
		}
	}
}