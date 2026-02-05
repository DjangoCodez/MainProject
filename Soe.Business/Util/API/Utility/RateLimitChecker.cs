using System;

namespace SoftOne.Soe.Business.Util.API.Utility
{
    class RateLimitChecker
    {
        //Fortnox allows 
        private DateTime _timerCompleted;
        private int _windowLengthMs; //The interval for which a certain amount of requests are allowed in ms
        private int _rateLimit; //The amount of requests allowed in the interval
        private int _requestCount = 0;

        public RateLimitChecker(int windowLengthSeconds, int requestCountWithinWindow)
        {
            if (windowLengthSeconds < 1)
            {
                throw new ArgumentException("Window length must be at least 1 second");
            }

            if (requestCountWithinWindow < 1)
            {
                throw new ArgumentException("Request count must be at least 1");
            }

            _windowLengthMs = windowLengthSeconds * 1000;
            _rateLimit = requestCountWithinWindow;
        }

        public bool WaitBeforeRequest(out int waitTime)
        {
            waitTime = 0;
            _requestCount++;

            if (_timerCompleted == null || DateTime.UtcNow > _timerCompleted)
            {
                Reset();
            }
            else if (_requestCount > _rateLimit)
            {
                TimeSpan span = _timerCompleted - DateTime.UtcNow;
                waitTime = span.Milliseconds + 100;
                return true;
            }

            return false;
        }
        private void Reset()
        {
            _timerCompleted = DateTime.UtcNow.AddMilliseconds(_windowLengthMs);
            _requestCount = 0;
        }
    }
}
