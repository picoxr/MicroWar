using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Pico
{
	namespace Avatar
	{
		// Helper execute coroutine for class that not derived from MonoBehaviour.
		// The object created by PicoAvatarApp and Started when PicoAvatarApp Started. 
		public class CoroutineExecutor
		{
			#region Framework

			/// <summary>
			/// Invoked by PicoAvatarApp to start the coroutine executor.
			/// </summary>
			/// <param name="holder"></param>
			public static void Start(MonoBehaviour holder)
			{
				_coroutineHolder = holder;
			}

			public static void Stop(MonoBehaviour holder)
			{
				if (_coroutineHolder == holder)
				{
					_coroutineHolder = null;
				}
			}

			#endregion


			#region Delayed work

			public static Coroutine DoDelayedWork(System.Action callback, float time = 1.0f)
			{
				if (_coroutineHolder == null)
				{
					throw new System.Exception("_coroutineHolder not set yet.");
				}

				return _coroutineHolder.StartCoroutine(CoroutineDelayedWork(callback, time));
			}

			/// <summary>
			/// condition wait, target len must 1, only can access target[0]
			/// </summary>
			public static Coroutine DoConditionDelayedWork<T>(T[] target, T value, System.Action callback)
				where T : System.IComparable
			{
				if (_coroutineHolder == null)
				{
					throw new System.Exception("_coroutineHolder not set yet.");
				}

				return _coroutineHolder.StartCoroutine(CoroutineConditionDelayedWork<T>(target, value, callback));
			}

			private static IEnumerator CoroutineConditionDelayedWork<T>(T[] target, T value, System.Action callback)
				where T : System.IComparable
			{
				while (target[0].CompareTo(value) != 0)
				{
					yield return null;
				}

				if (callback != null)
				{
					try
					{
						callback();
					}
					catch (System.Exception e)
					{
						//.
						//ReportError.
					}
				}
			}

			private static IEnumerator CoroutineDelayedWork(System.Action callback, float time)
			{
				yield return new WaitForSeconds(time);

				try
				{
					callback();
				}
				catch (System.Exception e)
				{
					Debug.LogException(e);
					//ReportError();
				}
			}

			/** Queue work item which invoked during main frame update.*/
			public static void QueueWorkDuringUpdate(System.Action callback)
			{
				bool lockTaken = false;
				_Lock.Enter(ref lockTaken);

				if (lockTaken)
				{
					_TaskQueueDuringUpdate.Add(callback);
					//
					_Lock.Exit();
				}
				else
				{
					throw new System.Exception("QueueWorkDuringUpdate wrong lock!");
				}
			}

			// Invoked during Update.
			private static void DispatchWorkQueueDuringUpdate()
			{
				List<System.Action> queue;
				// get queue.
				{
					bool lockTaken = false;
					_Lock.Enter(ref lockTaken);

					if (lockTaken)
					{
						queue = _TaskQueueDuringUpdateShadow;
						_TaskQueueDuringUpdateShadow = _TaskQueueDuringUpdate;
						_TaskQueueDuringUpdate = queue;
						//
						_Lock.Exit();
					}
					else
					{
						throw new System.Exception("DispatchWorkQueueDuringUpdate wrong lock!");
					}
				}

				//
				foreach (var x in _TaskQueueDuringUpdateShadow)
				{
					x();
				}

				_TaskQueueDuringUpdateShadow.Clear();
			}

			#endregion


			#region Private Fields

			// Global coroutine holder. Should be PicoAvatarApp
			private static MonoBehaviour _coroutineHolder;

			//
			private static List<System.Action>
				_TaskQueueDuringUpdate = new List<System.Action>(); // work queue done during MonoBeaviour.update.

			//
			private static List<System.Action>
				_TaskQueueDuringUpdateShadow = new List<System.Action>(); // work queue done during MonoBeaviour.update.

			//
			private static System.Threading.SpinLock _Lock = new System.Threading.SpinLock(true);

			#endregion
		}
	}
}