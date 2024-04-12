using System;
using System.Threading;

namespace Pico
{
	namespace Avatar
	{
		// Avatar stats.
		public class PicoAvatarStats
		{
			/// <summary>
			/// Statistics type.
			/// </summary>
			public enum StatsType
			{
				AppBoot = 0, // 
				AvatarManagerLogin,
				FirstAvatarVisible,
				AverageAvatarLoad,
				AvatarTotalUpdate,
				AvatarPreUpdate,
				AvatarCoreUpdate,
				AvatarRenderUpdate,
				Num,
			}

			/// <summary>
			/// Statistics type.
			/// </summary>
			public enum InstanceType
			{
				AvatarEnity = 0,
				AvatarLod,
				AvatarRenderMesh,
				Num,
			}

			// instance that managed by PicoAvatarApp.
			public static PicoAvatarStats instance { get; set; }

			#region Public Methods

			/// <summary>
			/// Gets cost time.
			/// </summary>
			/// <param name="statsType"></param>
			/// <returns></returns>
			public float GetCostTime(StatsType statsType)
			{
				return _statCostTimes[(int)statsType];
			}

			/// <summary>
			///  emit start of a task.
			/// </summary>
			/// <param name="statType"></param>
			public void EmitStart(StatsType statType)
			{
				_statStartTimes[(int)statType] = AvatarEnv.realtimeSinceStartup;
			}

			/// <summary>
			/// emit finishe of previously started task.
			/// </summary>
			/// <param name="statType"></param>
			/// <param name="logStat">logStat whether add cost time to log.</param>
			public void EmitFinish(StatsType statType, bool logStat = false)
			{
				var costTime = AvatarEnv.realtimeSinceStartup - _statStartTimes[(int)statType];
				_statCostTimes[(int)statType] = costTime;
				//
				// 项目名称[空格][项名][空格]Metrics_1: value_1[空格]Metrics_2: value_2[空格]
				if (logStat)
				{
					AvatarEnv.Log(DebugLogMask.ForceLog,
						string.Format("{0} {1} cost:{2}s", "AvatarSDK", statType.ToString(), costTime));
				}
			}

			/// <summary>
			/// Application boot started.
			/// </summary>
			public void AppBootStart()
			{
				EmitStart(StatsType.AppBoot);
				EmitStart(StatsType.FirstAvatarVisible);
			}

			/// <summary>
			/// Application boot finished.
			/// </summary>
			public void AppBootFinished()
			{
				EmitFinish(StatsType.AppBoot, true);
			}

			/// <summary>
			/// Avatar manager login started.
			/// </summary>
			public void AvatarManagerLoginStart()
			{
				AvatrManagerLoginStart();
			}

			[Obsolete("AvatrManagerLoginStart has been deprecated. Use AvatarManagerLoginStart instead", false)]
			public void AvatrManagerLoginStart()
			{
				EmitStart(StatsType.AvatarManagerLogin);
			}

			/// <summary>
			/// Avatar manager login finished.
			/// </summary>
			public void AvatarManagerLoginFinished()
			{
				EmitFinish(StatsType.AvatarManagerLogin, true);
			}

			/// <summary>
			/// Emit that first avatar visible.
			/// </summary>
			public void FirstAvatarVisible()
			{
				EmitStart(StatsType.FirstAvatarVisible);
			}

			/// <summary>
			/// Emit that an avatar lod ready.
			/// </summary>
			/// <param name="avatarId"></param>
			/// <param name="lodLevel"></param>
			/// <param name="costTime"></param>
			public void AvatarLodReady(string avatarId, AvatarLodLevel lodLevel, float costTime)
			{
				if (_statCostTimes[(int)StatsType.FirstAvatarVisible] == 0.0f)
				{
					EmitFinish(StatsType.FirstAvatarVisible, true);
				}

				// 项目名称[空格][项名][空格]Metrics_1: value_1[空格]Metrics_2: value_2[空格]
				AvatarEnv.Log(DebugLogMask.ForceLog, string.Format("{0} {1} avatarId:{2} lod:{3} cost:{4}"
					, "AvatarSDK", "AvatarLoad", avatarId, lodLevel.ToString(), costTime));
			}

			#endregion


			#region Instance Count

			/// <summary>
			/// Add instance count.
			/// </summary>
			/// <param name="instanceType"></param>
			public void IncreaseInstanceCount(InstanceType instanceType)
			{
				Interlocked.Increment(ref _instanceCounts[(int)instanceType]);
			}

			/// <summary>
			/// Dec instance count.
			/// </summary>
			/// <param name="instanceType"></param>
			public void DecreaseInstanceCount(InstanceType instanceType)
			{
				Interlocked.Decrement(ref _instanceCounts[(int)instanceType]);
			}


			/// <summary>
			/// log statics
			/// </summary>
			public void LogStats()
			{
				var sb = new System.Text.StringBuilder();
				sb.Append("AvatarStats C# instances:");
				for (int i = 0; i < (int)InstanceType.Num; i++)
				{
					sb.Append(String.Format("{0}={1} | ", ((InstanceType)i).ToString(), _instanceCounts[i]));
				}

				// use unity log to avoid avatar main log performance burden.
				UnityEngine.Debug.Log(sb.ToString());
			}

			#endregion

			//

			#region Private Fields

			private float[] _statStartTimes = new float[(int)StatsType.Num];
			private float[] _statCostTimes = new float[(int)StatsType.Num];

			private int[] _instanceCounts = new int[(int)InstanceType.Num];

			#endregion

			#region Private Methods

			#endregion
		}
	}
}