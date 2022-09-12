using ServiceStack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public BlazorInstance(BlazorInstanceFacade blazorInstanceFacade, List<BlazorInstanceFacade> blazorInstances = null )
        {
            this.parent = blazorInstanceFacade;
            this.blazorInstances = blazorInstances ?? new();
           
        }

        private readonly BlazorInstanceFacade parent;
        private readonly List<BlazorInstanceFacade> blazorInstances;

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
            while (instance == null)
            {
                var tmp = await parent.GetParent();
                if (tmp == null)
                {
                    break;
                }
                instance = await tmp.GetByTag(tag);
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
            var instance = blazorInstances.Where(a => a.BlazorTags.Any(a => a.Name == tag.Name)).OrderBy(a=>a.GetTagQueueCountSync(tag).Count).FirstOrDefault();
            if (instance != null)
            {
                return instance;
            }
            
            instance = await parent.GetByTag(tag);
            while (instance == null)
            {
                var tmp = await parent.GetParent();
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
