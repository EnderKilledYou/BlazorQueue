﻿using ServiceStack.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace BlazorQueue.ServiceInterface
{
    public class BlazorTag
    {
        public string Name { get; set; }
    }
    //the me instance

    
    public class BlazorInstance
    {

        //"https://localhost:7278/StreamHub"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">The parent hub of this</param>
        /// <param name="blazorInstances">If set, will set child tags if child tags is not set</param>
        /// <param name="childTags">Either the flattened distinct list of instances or will be </param>

        public BlazorInstance(BlazorInstanceFacade parent, List<BlazorInstanceFacade> blazorInstances = null, List<BlazorTag> childTags = null)
        {
            this.parent = parent;
            this.blazorInstances = blazorInstances ?? new();
            this.childTags = childTags ?? blazorInstances.SelectMany(a=>a.BlazorTags).GroupBy(a=>a.Name).Select(a=>a.First()).ToList();
        }

        private readonly BlazorInstanceFacade parent;
        private readonly List<BlazorInstanceFacade> blazorInstances;
        private readonly List<BlazorTag> childTags;
        public ConcurrentDictionary<string, CancellationTokenSource> LockTokens { get; set; } = new();

        public BlazorInstanceFacade Parent => parent;

        /// <summary>
        /// 
        ///
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async Task<BlazorInstanceFacade> GetTopBlaztor(BlazorTag tag)
        {


            BlazorInstanceFacade instance = parent;
            BlazorInstanceFacade tmp = parent;
            while (tmp != null)
            {
                instance = tmp;
                tmp = await tmp.GetParent();
                if (tmp == null)
                {
                    break;
                }            
            }
            return instance;
        }
        /// <summary>
        ///  Create a lock around a named resource for anyone who has this one who is accessing this facade. 
        /// 
        /// </summary>
        /// <param name="name">Lock Name</param>
        /// <param name="time">time out to auto release incase of system failure.</param>
        /// <returns></returns>
        public bool LockResource(string name, TimeSpan time = default)
        {
            if(LockTokens.TryGetValue(name,out var token))
            {
              
                if (!token.IsCancellationRequested )
                {
                    return false;
                }

                CancellationTokenSource cancellationTokenSource = new();
                var updated = LockTokens.TryUpdate(name, cancellationTokenSource,token);
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

        /// <summary>
        ///  Do we at this time have this lock
        /// 
        /// </summary>
        /// <param name="name">Lock Name</param>
 
        /// <returns> </returns>
        public bool ContainsLock(string name)
        {
            return LockTokens.ContainsKey(name) ;
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
        /// <summary>
        ///  Gets the blazor instance that has a queue and the first by order of amount in that queue desc.
        ///  If it can't find the tag, it traverses upwards until it goes to top blazor.
        /// 
        /// </summary>
        /// <param name="tag">Returns null if no tags are on this tree</param>
        /// <returns></returns>
        public async Task<BlazorInstanceFacade> GetFreestBlazorInstance(BlazorTag tag)
        {
            if (!childTags.All(a => a.Name != tag.Name)) return null; //short cut search

            var instance = blazorInstances.Where(a => a.BlazorTags.Any(a => a.Name == tag.Name)).OrderBy(a=>a.GetTagQueueCountSync(tag).Count).FirstOrDefault();
            if (instance != null)
            {
                return instance;
            }
            
            instance = await parent.GetByTag(tag);
            var tmp = instance;
            while (instance == null)
            {
                tmp = await tmp.GetParent();
                if(tmp == null)
                {
                    break;
                }
                instance = await tmp.GetByTag(tag);
            }
            return instance;
        }

        public BlazorInstanceFacade GetParent()
        {
            return parent;
        }
    }
    public interface IHasBlazorQueueGuid
    {
        public Guid Guid { get; set; }
    }
    public class HubHelper
    {

  
    }
}
