namespace Pico
{
	namespace Avatar
	{
		// The class is used to track waiting count to helpe test whether waiting asynchronous works have all finished.
		public class WaitCounter
		{
			private volatile int _waitingCount = 1;
			private System.Action _finishedCallback;
#if DEBUG
			private bool _WaitAllInvoked = false;
			public int waitingCount
			{
				get { return _waitingCount; }
			}
#endif

			// 在GameInit.Shutdown时会清除所有的Coroutine，所以这里要检查清理callback
			~WaitCounter()
			{
#if DEBUG
				if (!_WaitAllInvoked)
				{
					throw new System.InvalidOperationException("WaitCounter.WaitAll MUST be invoked.");
				}
#endif
			}

			// Add new work
			public void AddWaitCount(int count = 1)
			{
				_waitingCount += count;
			}

			/// <summary>
			/// Remove wait count.
			/// </summary>
			/// <param name="count"></param>
			/// <returns>true if all waiting work finished</returns>
			public bool RemoveWaitCount(int count = 1)
			{
				_waitingCount -= count;

				// 如果计数为0，发送通知.
				if (_waitingCount == 0 && _finishedCallback != null)
				{
					var callback = _finishedCallback;
					_finishedCallback = null;
					callback();
				}

				return _waitingCount == 0;
			}

			/// <summary>
			/// Invoke when all async work emitted. And can ONLY be invoked once.
			/// </summary>
			/// <returns>true if all waiting work finished</returns>
			/// <exception cref="InvalidOperationException"></exception>
			public bool WaitAll()
			{
#if DEBUG
				if (_WaitAllInvoked)
				{
					throw new System.InvalidOperationException("WaitCounter.WaitAll CAN ONLY be invoked once.");
				}

				_WaitAllInvoked = true;
#endif
				return --_waitingCount == 0;
			}

			public bool isReady
			{
				get { return _waitingCount == 0; }
			}

			/// <summary>
			/// Wait in coroutine and invoke callback.
			/// </summary>
			/// <param name="callback"></param>
			public void WaitAll(System.Action callback)
			{
				if (WaitAll())
				{
					if (callback != null)
					{
						callback();
					}

					return;
				}

				//
				_finishedCallback = callback;
			}
		}
	}
}