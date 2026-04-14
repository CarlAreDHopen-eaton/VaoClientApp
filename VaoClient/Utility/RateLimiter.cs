using System;
using System.Collections.Generic;
using System.Threading;

namespace Vao.Client.Utility
{
   /// <summary>
   /// A rate limiter that limits the number of requests per second.
   /// NOTE: 
   /// This is a very simple limiter, in production code you should consider using a more proven limiter such as the 
   /// one in the Polly library
   /// </summary>
   internal sealed class RateLimiter
   {
      private readonly int mMaxRequestsPerSecond;
      private readonly Queue<DateTime> mRequestTimestamps;
      private readonly object mLock = new object();

      /// <summary>
      /// Initializes a new instance of the <see cref="RateLimiter"/> class.
      /// </summary>
      /// <param name="maxRequestsPerSecond">Maximum number of requests allowed per second.</param>
      public RateLimiter(int maxRequestsPerSecond)
      {
         if (maxRequestsPerSecond <= 0)
         {
            throw new ArgumentOutOfRangeException(nameof(maxRequestsPerSecond), "Must be greater than zero.");
         }

         mMaxRequestsPerSecond = maxRequestsPerSecond;
         mRequestTimestamps = new Queue<DateTime>();
      }

      /// <summary>
      /// Waits until a request can be made without exceeding the rate limit.
      /// This method blocks the calling thread if the rate limit has been reached.
      /// </summary>
      public void WaitForSlot()
      {
         while (true)
         {
            lock (mLock)
            {
               DateTime now = DateTime.UtcNow;
               DateTime windowStart = now.AddSeconds(-1);

               // Remove timestamps older than 1 second
               while (mRequestTimestamps.Count > 0 && mRequestTimestamps.Peek() < windowStart)
               {
                  mRequestTimestamps.Dequeue();
               }

               // Check if we can make a request
               if (mRequestTimestamps.Count < mMaxRequestsPerSecond)
               {
                  mRequestTimestamps.Enqueue(now);
                  return;
               }

               // Calculate how long to wait until the oldest request falls outside the window
               DateTime oldestTimestamp = mRequestTimestamps.Peek();
               TimeSpan waitTime = oldestTimestamp.AddSeconds(1) - now;

               if (waitTime > TimeSpan.Zero)
               {
                  // Release lock before sleeping
                  Monitor.Exit(mLock);
                  try
                  {
                     Thread.Sleep(waitTime);
                  }
                  finally
                  {
                     Monitor.Enter(mLock);
                  }
               }
            }
         }
      }
   }
}
