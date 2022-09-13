using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorQueue.ServiceInterface
{
    public interface IAmABlazor
    {

        public Task<BlazorInstanceFacade> GetParent( CancellationToken cancellationToken = default);

        public Task AddChild(BlazorInstanceFacade child, CancellationToken cancellationToken = default);

        public Task<BlazorInstanceFacade> GetByTag(BlazorTag tag, CancellationToken cancellationToken = default);

        public Task<TagCount> GetTagQueueCount(BlazorTag tag, CancellationToken cancellationToken = default);
        
    }
    public interface IHaveBlazorTags
    {
        public   Task<List<BlazorTag>> BlazorTags(CancellationToken cancellationToken = default)
    }
}