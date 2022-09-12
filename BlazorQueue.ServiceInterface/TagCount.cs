namespace BlazorQueue.ServiceInterface
{
    public class TagCount :HasBlazorQueueGuid
    {
        public int Count { get; set; }
    }
}