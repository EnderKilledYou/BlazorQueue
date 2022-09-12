using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Tasks;


namespace BlazorQueue.ServiceInterface
{
    public class BlazorInstanceHub : Hub, IAmABlazor
    {
        private readonly BlazorInstance me;

        public BlazorInstanceHub(BlazorInstance me)
        {
            this.me = me;
        }

        public async Task<BlazorInstanceFacade> GetParent(CancellationToken cancellationToken = default)
        {
            return me.GetParent();
        }
        public async Task<BlazorInstanceFacade> GetByTag(BlazorTag tag, CancellationToken cancellationToken = default)
        {
            return await me.GetFreestBlazorInstance(tag);
        }
        public async Task<TagCount> GetTagQueueCount(BlazorTag tag, CancellationToken cancellationToken = default)
        {
            
            return null;
            //return await me.GetTagQueueCount(tag);
        }
    }
}
