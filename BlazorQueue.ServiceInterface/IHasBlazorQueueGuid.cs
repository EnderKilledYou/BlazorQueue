using System;


namespace BlazorQueue.ServiceInterface
{
    public interface IHasBlazorQueueGuid
    {
        public Guid Guid { get; set; }
    }
}
