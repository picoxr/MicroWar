using UnityEngine;
using System.Threading;
using System.Collections;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Yield type of async coroutine.
		/// @warning CAN ONLY be used in first entry of the coroutine, thus the it can not be invoked from
		/// another coroutine.
		/// </summary>
		public static class PicoAsyncCoroutineYieldType
		{
			// yield object to specify go to main thread.
			public static readonly object ToMainThread;

			// yield object to specify go to work thread.
			public static readonly object ToWorkThread;

			// yield object to specify to cancel parent coroutines.
			public static readonly object ToExit;

			// synchronize following coroutines.
			public static readonly object ToMainThreadFollows;

			static PicoAsyncCoroutineYieldType()
			{
				ToMainThread = new object();
				ToWorkThread = new object();
				ToExit = new object();
				ToMainThreadFollows = new object();
			}
		}


		/// <summary>
		/// Asynchronous coroutine.
		/// @note Only top level coroutine entry can use PicoAsyncCoroutineYieldType to marshal steps.
		/// </summary>
		public class AsyncCoroutine : IEnumerator
		{
			#region IEnumerator Interfaces

			/// <summary>
			/// Current yield object.
			/// </summary>
			public object Current { get; private set; }

			/// <summary>
			/// Update coroutine state, go to next yield block.
			/// </summary>
			/// <returns>false to specify exit coroutine.</returns>
			public bool MoveNext()
			{
				return UpdateState();
			}

			public void Reset()
			{
				throw new System.NotSupportedException("AsyncCoroutine DONOT support Reset.");
			}

			#endregion


			#region Public Methods

			/// <summary>
			/// Constructor with actual coroutine.
			/// </summary>
			/// <param name="actualCoutine_"></param>
			public AsyncCoroutine(IEnumerator actualCoutine_)
			{
				//
				_actualCoroutine = actualCoutine_;

				// initially marshaled to work thread.
				_state = State.ToWorkThread;
			}

			/// <summary>
			/// Cancel the coroutine.
			/// </summary>
			public void Cancel()
			{
				SetState(State.Cancelled);
			}

			#endregion


			#region Private Types/Fields

			/// <summary>
			/// State type.
			/// </summary>
			private enum State
			{
				ToWorkThread,
				InWorkThread,
				ToMainThread,
				PendingYieldReturn,
				Finished,
				Cancelled,
			}

			// Current state.
			private State _state;

			// prievous state.
			private State _previousState;

			// wrapped coroutine which holds actual coroutine codes.
			private IEnumerator _actualCoroutine;

			// pending yield type which returned from actual coroutine.
			private object _pendingYieldObject;

			// stats
			//private int _statsUpdateCount = 0;
			//private int _statsUpdateExecuteCount = 0;

			#endregion


			#region Private Methods

			// Sets new state.
			private void SetState(State state)
			{
				lock (this)
				{
					if (_state == state || _state == State.Cancelled)
					{
						return;
					}

					// maintainance the previous state;
					_state = state;
					_previousState = state;
				}
			}
			
            //Update coroutine state. 
            //Invoked from Unity MonoBehaviour.
            //@return false if finished coroutine.
            private bool UpdateState()
			{
				// check actual coroutine.
				if (_actualCoroutine == null)
				{
					return false;
				}

				//++_statsUpdateCount;
				//AvatarEnv.Log("Coroutine.UpdateState Start:" + _statsUpdateCount.ToString());

				// clear current state.
				Current = null;

				// loops until the inner routine yield something to Unity;
				while (true)
				{
					// a simple state machine;
					switch (_state)
					{
						// marshal to work thread.
						case State.ToWorkThread:
							PostToWorkThread();
							break;
						// if is waiting work thread finished, just return true and check next frame.
						case State.InWorkThread:
							return true;
						// to run in main thread
						case State.ToMainThread:
							ExecuteActualCoroutine(null);
							break;
						// to process last yield return value.
						case State.PendingYieldReturn:
						{
							// Only top level coroutine entry can use PicoAsyncCoroutineYieldType to marshal steps.
							if (_pendingYieldObject == PicoAsyncCoroutineYieldType.ToWorkThread)
							{
								PostToWorkThread();
							}
							else if (_pendingYieldObject == PicoAsyncCoroutineYieldType.ToMainThread)
							{
								SetState(State.ToMainThread);
								ExecuteActualCoroutine(null);
							} // if to main thread follows
							else if (_pendingYieldObject == PicoAsyncCoroutineYieldType.ToMainThreadFollows)
							{
								if (!ExecuteMainThreadFollowingCoroutines())
								{
									return true;
								}
							}
							else if (_pendingYieldObject == PicoAsyncCoroutineYieldType.ToExit)
							{
								SetState(State.Cancelled);
								return false;
							}
							else
							{
								// set current yield object.
								Current = _pendingYieldObject;

								// yield from background thread, or main thread?
								if (_previousState == State.InWorkThread)
								{
									// if from background thread, 
									// go back into background in the next loop;
									_pendingYieldObject = PicoAsyncCoroutineYieldType.ToWorkThread;
								}
								else
								{
									// otherwise go back to main thread the next loop;
									_pendingYieldObject = PicoAsyncCoroutineYieldType.ToMainThread;
								}

								// need execute successive coroutine block.
								return true;
							}
						}
							break;
						// done running, pass false to Unity;
						case State.Finished:
						case State.Cancelled:
						default:
							return false;
					}
				}
			}

			// Post work to work thread.
			private void PostToWorkThread()
			{
				SetState(State.InWorkThread);
				ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteActualCoroutine));
			}

			// Execute actual coroutine.
			private void ExecuteActualCoroutine(object state)
			{
				//++_statsUpdateExecuteCount;
				//AvatarEnv.Log("Coroutine.ExecuteActualCoroutine Start: " + _statsUpdateExecuteCount.ToString());

				try
				{
					// if actual coroutine need more yield block.
					if (_actualCoroutine.MoveNext())
					{
						lock (this)
						{
							_pendingYieldObject = _actualCoroutine.Current;
						}

						SetState(State.PendingYieldReturn);
					}
					else
					{
						// set finished state.
						SetState(State.Finished);
					}
				}
				catch (System.Exception ex)
				{
					// exception handling, save & log it;
					//this.Exception = ex;
					Debug.LogError(string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
					// then terminates the task;
					//GotoState(RunningState.Error);
				}

				//AvatarEnv.Log("Coroutine.ExecuteActualCoroutine Exit" + _statsUpdateExecuteCount.ToString());
			}
			
            //Execute main thread coroutine.
            //@return true if all coroutine finished, otherwise return false.
            private bool ExecuteMainThreadFollowingCoroutines()
			{
				//++_statsUpdateExecuteCount;
				//AvatarEnv.Log("Coroutine.ExecuteMainThreadFollowingCoroutines Start: " + _statsUpdateExecuteCount.ToString());

				try
				{
					// if actual coroutine need more yield block.
					while (_actualCoroutine.MoveNext())
					{
						//
						_pendingYieldObject = _actualCoroutine.Current;
						SetState(State.PendingYieldReturn);

						var childCoroutine = _pendingYieldObject as IEnumerator;
						if (childCoroutine != null)
						{
							// if child coroutine return exit, should exit parent.
							var yieldState = RecursivelySyncRunCoroutine(childCoroutine);
							if (yieldState == PicoAsyncCoroutineYieldType.ToExit)
							{
								_pendingYieldObject = null;
								SetState(State.Cancelled);
							}
						}
						else if (_pendingYieldObject == PicoAsyncCoroutineYieldType.ToExit)
						{
							_pendingYieldObject = null;
							SetState(State.Cancelled);
							break;
						}
						else if (_pendingYieldObject == PicoAsyncCoroutineYieldType.ToWorkThread)
						{
							break;
						}
						else if (_pendingYieldObject == PicoAsyncCoroutineYieldType.ToMainThreadFollows)
						{
							return false;
						}
					}

					// set finished state for last.
					SetState(State.Finished);
				}
				catch (System.Exception ex)
				{
					// exception handling, save & log it;
					//this.Exception = ex;
					Debug.LogError(string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
					// then terminates the task;
					//GotoState(RunningState.Error);
				}

				//
				return true;
				//AvatarEnv.Log("Coroutine.ExecuteMainThreadFollowingCoroutines Exit" + _statsUpdateExecuteCount.ToString());
			}

			private object RecursivelySyncRunCoroutine(IEnumerator coroutine)
			{
				//
				while (coroutine != null && coroutine.MoveNext())
				{
					var yieldObject = coroutine.Current;
					//
					if (yieldObject != null)
					{
						var childCoroutine = yieldObject as IEnumerator;
						if (childCoroutine != null)
						{
							RecursivelySyncRunCoroutine(childCoroutine);
						}
						else if (yieldObject == PicoAsyncCoroutineYieldType.ToExit)
						{
							return yieldObject;
						}
					}
				}

				return null;
			}

			#endregion
		}

		/// <summary>
		/// MonoBehaviour Extension for AsyncCoroutine.
		/// </summary>
		public static class AsyncCoroutineMonoBehaviourExtensions
		{
			/// <summary>
			/// Start async-coroutine on MonoBehaviour.
			/// </summary>
			/// <param name="behaviour"></param>
			/// <param name="routine"></param>
			/// <returns></returns>
			public static Coroutine StartAsyncCoroutine(
				this MonoBehaviour behaviour, IEnumerator routine)
			{
				return behaviour.StartCoroutine(new AsyncCoroutine(routine));
			}
		}
	}
}