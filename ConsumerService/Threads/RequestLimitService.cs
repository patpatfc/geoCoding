
namespace ConsumerService.Threads
{
    public class RequestLimitService
    {
        private readonly Object numOfRequestsLock = new();
        private int numOfRequests = 0;

        public RequestLimitService()
        {
            Task.Run(() => ResetNumOfRequests());
        }

        public bool RequestLimitAvailable(int _threadId)
        {
            var boolValue = false;
            if (numOfRequests < 10)
            {
                lock (numOfRequestsLock)
                {
                    if (numOfRequests < 10)
                    {
                        numOfRequests++;
                        boolValue = true;
                    }
                }
            }

            return boolValue;
        }

        private void ResetNumOfRequests()
        {
            while (true)
            {
                Task.Delay(1000).Wait();
                lock (numOfRequestsLock)
                {
                    numOfRequests = 0;
                }
            }
        }
    }
}