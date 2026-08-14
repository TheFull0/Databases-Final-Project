using System;
using Events;
using UnityEngine;

namespace Game_Logic
{
    public class StopWatch : MonoBehaviour
    {
        private float _maxTime = 60f; // Maximum time in seconds
        private float _currentTime = 0f;
        
        private bool _isRunning;
        public bool IsRunning => _isRunning;

        public void Update()
        {
            if (_isRunning)
            {
                _currentTime += Time.deltaTime;

                if (_currentTime >= _maxTime)
                {
                    _currentTime = _maxTime;
                    _isRunning = false;
                    EventBus.Raise(new TimerFinishedEvent());
                    Stop();
                }
            }
        }

        public void StartClock(float timeToTime)
        {
            _maxTime = timeToTime;
            _currentTime = 0f;
            _isRunning = true;
        }

        private void Stop()
        {
            _isRunning = false;
        }
        
        public float StopAndPeek()
        {
            Stop();
            return Peek();
        }

        private float Peek()
        {
            return _currentTime;
        }
    }
}