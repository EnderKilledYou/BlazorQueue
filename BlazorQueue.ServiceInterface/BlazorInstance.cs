using Microsoft.AspNetCore.SignalR.Client;
using ServiceStack.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace BlazorQueue.ServiceInterface
{
    public class BlazorTag
    {
        public string Name { get; set; }
    }
    public class BlazorInstance
    {

        //"https://localhost:7278/StreamHub"

        public BlazorInstance(BlazorInstanceFacade blazorInstanceFacade, List<BlazorInstance> blazorInstances = null, List<BlazorTag> blazorTags = null)
        {
            this.parent = blazorInstanceFacade;
            this.blazorInstances = blazorInstances ?? new();
            this.blazorTags = blazorTags ?? new();
        }

        private readonly BlazorInstanceFacade parent;
        private readonly List<BlazorInstance> blazorInstances;
        private readonly List<BlazorTag> blazorTags;


        public async Task<BlazorInstanceFacade> GetFreestBlazorInstance(BlazorTag tag)
        {
            var instance = await parent.GetByTag(tag);
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

    }
    public interface IHasBlazorQueueGuid
    {
        public Guid Guid { get; set; }
    }
    public class BlazorRPC<T,TRequest> where TRequest : IHasBlazorQueueGuid where T: IHasBlazorQueueGuid
    {


        private readonly HubConnection connection;
        private T? value;
        private string eventName;
        private CancellationTokenSource cancellationTokenSource;
        private readonly int timeOut;

        public BlazorRPC(HubConnection connection, CancellationTokenSource cancellationTokenSource = null, int timeOut = -1)
        {
            this.connection = connection;
            
            this.cancellationTokenSource = cancellationTokenSource ?? new();
            this.timeOut = timeOut;
        }

        public async Task Put(TRequest request)
        {
         
            connection.On<T>(request.Guid.ToString(), (message) =>
            {
                value = message;
                cancellationTokenSource.Cancel();
            });
            await connection.SendAsync(typeof(TRequest).FullName,request);
        }
        public async Task<T> Get()
        {
            try
            {
 
                await Task.Delay(timeOut, cancellationTokenSource.Token);
            }
            catch
            {

            }
            connection.Remove(value?.Guid.ToString());
            return value;
        }

    }
    public class BlazorInstanceFacade : HasBlazorQueueGuid, IAsyncDisposable
    {

        public async Task<BlazorInstanceFacade> GetParent()
        {
            var rpc =new  BlazorRPC<BlazorInstanceFacade, GetParentRequest>(connection,timeOut:10* 1000);
            await rpc.Put(new GetParentRequest());
            return await rpc.Get();
        }

        private readonly HubConnection connection;



        public async Task<BlazorInstanceFacade> GetByTag(BlazorTag tag)
        {
            var rpc = new BlazorRPC<BlazorInstanceFacade, GetByTagRequest>(connection, timeOut: 10 * 1000);
            await rpc.Put(new GetByTagRequest(tag));
            return await rpc.Get();
        }

 

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            return connection.DisposeAsync();
        }

        public BlazorInstanceFacade(string HostUrl, string HubName, string token)
        {
            connection = new HubConnectionBuilder()
                .WithUrl(new Uri(HostUrl + HubName), options =>
                {
                    options.Headers.Add("Authorization", "bearer " + token);
                }).Build();
 
            

            connection.StartAsync().Wait();
        }
    }
    public class HubHelper
    {

  
    }
}
