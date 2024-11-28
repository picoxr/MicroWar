using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// A too class to filter position with time.
		/// </summary>
		public class PlaybackSampleFilter
		{
			/// <summary>
			/// position sample
			/// </summary>
			public struct Sample
			{
				public float time;
				public Vector3 pos;
				public Quaternion quat;
			}


			/// <summary>
			/// Initialize filter.
			/// </summary>
			/// <param name="windowSize"></param>
			/// <param name="avgDelayTime"></param>
			/// <param name="maxPlaybackSpeedRatio"></param>
			/// <param name="minPlaybackSpeedRatio"></param>
			public void Initialize(int windowSize, float avgDelayTime, float maxPlaybackSpeedRatio,
				float minPlaybackSpeedRatio)
			{
				_avgDelayTime = Mathf.Max(0.001f, avgDelayTime);
				_maxPlaybackSpeedRatio = maxPlaybackSpeedRatio;
				_minPlaybackSpeedRatio = minPlaybackSpeedRatio;

				//
				_invAvgDelayTime = 1.0f / _avgDelayTime;
				//
				_samples = new Sample[windowSize];
				//
				_filterWindowSize = windowSize;
				//
				Reset();
			}

			/// <summary>
			/// Adds position.
			/// </summary>
			/// <param name="pos"></param>
			/// <param name="quat"></param>
			/// <param name="timeStamp"></param>
			public void AddSamplePoint(Vector3 pos, Quaternion quat, float timeStamp)
			{
				// if too lagged, remote client must be reset.
				var timeDiff = timeStamp - _latestRemoteTime;
				if (timeDiff < 0.0)
				{
					// if lagged over 5 seconds, should reset.
					if (timeDiff < -5.0)
					{
						Reset();
						_remoteTime = timeStamp;
					}
					else
					{
						// skip 
						return;
					}
				}
				else if (isEmpty)
				{
					_remoteTime = timeStamp;
				}

				// keep the latest remote time.
				_latestRemoteTime = timeStamp;

				//
				var sample = new Sample();
				sample.time = timeStamp;
				sample.pos = pos;
				sample.quat = quat;
				//
				_samples[_end] = sample;

				// if overflow, skip begin one.
				if (_end + 1 == _begin || (_end == _filterWindowSize - 1 && _begin == 0))
				{
					_begin = (_begin++) % _filterWindowSize;
				}

				//
				_end = (_end + 1) % _filterWindowSize;
			}

			/// <summary>
			///  Move time and get position/quaternion.
			/// </summary>
			/// <param name="pos"></param>
			/// <param name="quat"></param>
			/// <param name="time"></param>
			/// <returns>false if can not find value with the time.</returns>
			public bool MoveTime(ref Vector3 pos, ref Quaternion quat, float time)
			{
				if (isEmpty)
				{
					return false;
				}

				var delta = time - _lastPlaybackUpdateTime;
				// calculate frame time.
				if (delta > 1.0f)
				{
					delta = 0.01f; // 100 fps
				}

				//
				_lastPlaybackUpdateTime = time;

				// if time diff more than 1.5 seconds, means run after, should be faster, otherwise should slow it.
				if (_avgDelayTime > 0.001f)
				{
					var timeDiff = _latestRemoteTime - _remoteTime - _avgDelayTime;
					float alpha = (float)(timeDiff * _invAvgDelayTime);
					if (alpha > 0.0)
					{
						alpha = Mathf.Min(1.0f, alpha);
						delta *= Mathf.Lerp(1.0f, _maxPlaybackSpeedRatio, alpha);
					}
					else
					{
						alpha = Mathf.Min(1.0f, -alpha);
						delta *= Mathf.Lerp(1.0f, _minPlaybackSpeedRatio, alpha);
					}

					//
					_remoteTime += delta;
				}
				else
				{
					// do not support delay process.  LocalUser m_intermitentMode need rigid time.
					_remoteTime = time;
				}

				//
				var animTime = _remoteTime;

				int left = -1, right = -1;
				for (int i = _begin; i != _end; i = (i + 1) % _filterWindowSize)
				{
					if (_samples[i].time < animTime)
					{
						left = _begin = i;
					}
					else
					{
						right = i;
						break;
					}
				}

				//
				if (left != -1 && right != -1)
				{
					if (_lastLeft != left || _lastRight != right)
					{
						_lastLeft = left;
						_lastRight = right;
						_invLastSnapFrameTime = 1.0f / (_samples[_lastRight].time - _samples[_lastLeft].time);
					}

					//
					var leftSample = _samples[left];
					var rightSample = _samples[right];
					float alpha = (animTime - _samples[left].time) * _invLastSnapFrameTime;

					pos = Vector3.Lerp(leftSample.pos, rightSample.pos, alpha);
					quat = Quaternion.Lerp(leftSample.quat, rightSample.quat, alpha);
				}
				else
				{
					if (left != -1)
					{
						var sample = _samples[left];
						pos = sample.pos;
						quat = sample.quat;
					}
					else if (right != -1)
					{
						var sample = _samples[right];
						pos = sample.pos;
						quat = sample.quat;
					}
					else
					{
						return false;
					}
				}

				return true;
			}


			#region Private Fields

			//
			private int _begin = 0;

			//
			private int _end = 0;

			// fixed size of positions.
			private Sample[] _samples;

			// length of _positions
			private int _filterWindowSize = 0;

			// time of remote side user.
			private float _remoteTime = 0.0f;

			//  most advance time of all received packets, used to skip obsolete
			private float _latestRemoteTime = -1000.0f;

			//
			private float _lastPlaybackUpdateTime = -1000.0f;

			//
			private int _lastLeft = -1;
			private int _lastRight = -1;

			// 1.0 / (m_shots[m_lastRight].timestamp - m_shots[m_lastLeft].timestamp)
			private float _invLastSnapFrameTime = 0.0f;


			// average net delay time. if is zero, turn of speed adjust.
			private float _avgDelayTime;

			// maximum speed ratio when run after during playback.
			private float _maxPlaybackSpeedRatio;

			// minum speed ratio when run before during playback.
			private float _minPlaybackSpeedRatio;

			// 1.0 / m_avgDelayTime
			private float _invAvgDelayTime;

			#endregion


			#region Private Methods

			// brief Query whether no item added.
			bool isEmpty
			{
				get => _begin == _end;
			}

			// Clear positions.
			private void Reset()
			{
				_begin = _end = 0;
				_lastLeft = _lastRight = -1;
			}

			#endregion
		}
	}
}