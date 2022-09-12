using System;

namespace BlazorQueue.ServiceInterface
{
    public class HasBlazorQueueGuid : IHasBlazorQueueGuid 
    {
        public Guid Guid { get; set; }
    }
    public class GetParentRequest : HasBlazorQueueGuid
    {
     
    }
}