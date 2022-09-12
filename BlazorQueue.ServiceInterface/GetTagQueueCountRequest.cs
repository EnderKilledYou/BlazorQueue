namespace BlazorQueue.ServiceInterface
{
    public
        class GetTagQueueCountRequest : HasBlazorQueueGuid
    {
        public GetTagQueueCountRequest(BlazorTag tag)
        {
            Tag = tag;
        }

        public BlazorTag Tag { get; }
    }
}