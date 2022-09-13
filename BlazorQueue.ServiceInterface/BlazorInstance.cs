using Newtonsoft.Json.Serialization;
using ServiceStack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace BlazorQueue.ServiceInterface
{

    //the me instance


    public class BlazorInstance : IAmABlazor, IHaveBlazorTags
    {

        //"https://localhost:7278/StreamHub"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">The parent hub of this</param>
        /// <param name="blazorInstances">If set, will set child tags if child tags is not set</param>
        /// <param name="childTags">Either the flattened distinct list of instances or will be </param>

        public BlazorInstance(BlazorInstanceFacade parent,LockManager lockManager)
        {
            this.parent = parent;
            this.lockManager = lockManager;
            this.blazorInstances = new();
            this.childTags = new();
            this.tags = new();
        }
        private readonly List<object> taskQueue;
        private readonly BlazorInstanceFacade parent;
        private readonly LockManager lockManager;
        private readonly List<BlazorInstanceFacade> blazorInstances;
        private readonly List<BlazorTag> childTags;
        private readonly List<BlazorTag> tags;
 
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
        ///  Gets the blazor instance that has a queue and the first by order of amount in that queue desc.
        ///  If it can't find the tag, it traverses upwards until it goes to top blazor.
        /// 
        /// </summary>
        /// <param name="tag">Returns null if no tags are on this tree</param>
        /// <returns></returns>
        public async Task<BlazorInstanceFacade> GetFreestBlazorInstance(BlazorTag tag)
        {
            if (!childTags.All(a => a.Name != tag.Name)) return null; //short cut search

            var instance = blazorInstances.Where(a => a.BlazorTags.Contains(tag)).OrderBy(a=>a.GetTagQueueCountSync(tag).Count).FirstOrDefault();
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

        public Task<BlazorInstanceFacade> GetParent(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task AddChild(BlazorInstanceFacade child, CancellationToken cancellationToken = default)
        {
            this.blazorInstances.Add(child);
            var tags = await child.BlazorTags();
            this.childTags.AddRange(tags.Where(item => !childTags.Contains(item)));
            
            //todo: add child tags 
        }

        public async Task<BlazorInstanceFacade> GetByTag(BlazorTag tag, CancellationToken cancellationToken = default)
        {
            
            foreach(var blazorInstance in this.blazorInstances)
            {
                if ((await blazorInstance.BlazorTags()).Any(a => a.Equals(tag)))
                {
                    return blazorInstance;
                }
            }
            return null;

        }

        public async Task<TagCount> GetTagQueueCount(BlazorTag tag, CancellationToken cancellationToken = default)
        {
            return new TagCount (){Count=  taskQueue.Count };
        }

        public async Task<List<BlazorTag>> BlazorTags(CancellationToken cancellationToken = default)
        {
            return tags;
        }
    }
    
}
