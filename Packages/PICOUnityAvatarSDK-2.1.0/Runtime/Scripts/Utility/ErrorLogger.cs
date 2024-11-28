using UnityEngine;

namespace Pico
{
    namespace Avatar
    {
        internal class ErrorLogger
        {
            private float _errorInterval = 1.0f;
            private float _timer = -1.0f;

            internal ErrorLogger(float errorInterval = 1.0f)
            {
                _errorInterval = errorInterval;
                _timer = -errorInterval;
            }

            internal void LogErrorWithInterval(object message)
            {
                if (Time.time - _timer > _errorInterval)
                {
                    Debug.LogError(message);
                    _timer = Time.time;
                }
            }
        }
    }
}