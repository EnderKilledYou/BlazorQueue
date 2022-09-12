namespace BlazorQueue.ServiceInterface
{
    public class GetByTagRequest : HasBlazorQueueGuid
    {
        public GetByTagRequest(BlazorTag tag)
        {
            Tag = tag;
        }

        public BlazorTag Tag { get; }
    }
}