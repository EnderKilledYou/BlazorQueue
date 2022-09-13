using System;
using System.Collections.Concurrent;
using System.Threading;


namespace BlazorQueue.ServiceInterface
{
    public class LockManager
    {
        public ConcurrentDictionary<string, CancellationTokenSource> LockTokens { get; init; } = new();
        public ConcurrentDictionary<string, (int max, ConcurrentDictionary<int, CancellationTokenSource> intPool)> LockPools { get; init; } = new();
        public ConcurrentDictionary<BlazorTag, BlazorInstanceFacade[]> InstanceTags { get; init; } = new();
        /// <summary>
        ///  Create a lock around a named resource for anyone who has this one who is accessing this facade. 
        /// 
        /// </summary>
        /// <param name="name">Lock Name</param>
        /// <param name="time">time out to auto release incase of system failure.</param>
        /// <returns></returns>
        public bool LockResource(string name, TimeSpan time = default)
        {
            if (LockTokens.TryGetValue(name, out var token))
            {

                if (!token.IsCancellationRequested)
                {
                    return false;
                }

                CancellationTokenSource cancellationTokenSource = new();
                var updated = LockTokens.TryUpdate(name, cancellationTokenSource, token);
                if (updated)
                {
                    token.Dispose();
                    if (time != default)
                    {
                        cancellationTokenSource.CancelAfter(time);
                    }
                }
                return updated;


            }
            else
            {

                CancellationTokenSource cancellationTokenSource = new();
                var added = LockTokens.TryAdd(name, cancellationTokenSource);
                if (added)
                {
                    if (time != default)
                    {
                        cancellationTokenSource.CancelAfter(time);
                    }
                }
                return added;
            }
        }

        public void SetupDbPool()
        {
            LockPools.TryAdd("Db", (5, new ConcurrentDictionary<int, CancellationTokenSource>()));
        }
        /// <summary>
        /// Attempts to get a 5 minute max db connection
        /// </summary>
        /// <returns></returns>
        public int LockDbPool()
        {
            return LockPool("Db", new TimeSpan(0, 5, 0));
        }

        public int LockPool(string name, TimeSpan time = default)
        {
            if (!LockPools.ContainsKey(name))
            {
                return -2; // no such queue
            }
            if (LockPools.TryGetValue(name, out var pool))
            {

                CancellationTokenSource item = new();
                int idnex = pool.intPool.Count;
                if (pool.max < idnex && pool.intPool.TryAdd(idnex, item))
                {
                    if (time.Milliseconds > 0)
                        item.CancelAfter(time);
                    return idnex;
                }

            }
            return -1;
        }

        public bool UnlockPool(string name, int index)
        {
            if (!LockPools.ContainsKey(name))
            {
                return false; // no such queue
            }
            if (LockPools.TryGetValue(name, out var pool) && pool.intPool.TryGetValue(index, out var tokensource))
            {
                tokensource.Dispose();
                return true;
            }
            return false;

        }
        /// <summary>
        ///  Do we at this time have this lock
        /// 
        /// </summary>
        /// <param name="name">Lock Name</param>

        /// <returns> </returns>
        public bool ContainsLock(string name)
        {
            return LockTokens.ContainsKey(name);
        }

        /// <summary>
        ///  unlock a named resource if it exists.
        /// 
        /// </summary>
        /// <param name="name">Lock Name</param> 
        /// <returns>true if it unlocked it. false if it didn't exist or it didn't unlock it (soem other thred did)</returns>
        public bool UnLockResource(string name)
        {
            return LockTokens.ContainsKey(name) && LockTokens.TryRemove(name, out _);
        }
    }
    
}
