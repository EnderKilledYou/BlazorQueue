using BlazorQueue.ServiceModel;
using ServiceStack;

namespace BlazorQueue.ServiceInterface;

public class BlazorInstanceService : Service
{
    private BlazorInstance BlazorInstance;

    public BlazorInstanceService(BlazorInstance blazorInstance)
    {
        BlazorInstance = blazorInstance;
    }

    public object Any(WhoParentRequest request)
    {
        return new WhoParentResponse()
        {
            Parent = (IConnectToHub)BlazorInstance.Parent
        };
    }
}