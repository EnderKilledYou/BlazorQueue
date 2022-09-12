using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using ServiceStack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
  

        public async Task<BlazorInstanceFacade> GetFreestBlazorInstance(BlazorTag tag)
        {
            var instance = blazorInstances.Where(a => a.BlazorTags.Any(a => a.Name == tag.Name)).OrderBy(a=>a.GetTagQueueCount(tag).Count).FirstOrDefault();
            if (instance != null)
            {
                return instance;
            }
            
            instance = await parent.GetByTagAsync(tag);
            while (instance == null)
            {
                var tmp = await parent.GetParentAsync();
                if(tmp == null)
                {
                    break;
                }
                instance = await tmp.GetByTagAsync(tag);
            }
            return instance;
        }

    }
    public interface IHasBlazorQueueGuid
    {
        public Guid Guid { get; set; }
    }

  
   
    public class BlazorInstanceFacade : HasBlazorQueueGuid, IAsyncDisposable
    {
        public List<BlazorTag> BlazorTags { get; init; } =  new();
        public async Task<BlazorInstanceFacade> GetParentAsync( CancellationToken cancellationToken = default(CancellationToken))
        {
            return await connection.InvokeAsync<BlazorInstanceFacade>("GetParent", cancellationToken:cancellationToken);
        }
        public async Task<TagCount> GetTagQueueCountAsync(BlazorTag tag,CancellationToken cancellationToken = default(CancellationToken))
        
        {
            return await connection.InvokeAsync<TagCount>("GetTagQueueCount",tag, cancellationToken: cancellationToken);
            
        }

        private readonly HubConnection connection;



        public async Task<BlazorInstanceFacade> GetByTagAsync(BlazorTag tag, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await connection.InvokeAsync<BlazorInstanceFacade>("GetByTag", tag, cancellationToken: cancellationToken);
        }
        public BlazorInstanceFacade GetByTag(BlazorTag tag, CancellationToken cancellationToken)
        {
            using var result = GetByTagAsync(tag, cancellationToken);
            result.Wait();
            return result.Result;
        }
        public TagCount GetTagQueueCount(BlazorTag tag, CancellationToken cancellationToken = default(CancellationToken))
        {
            using var result = GetTagQueueCountAsync(tag, cancellationToken);
            result.Wait();
            return result.Result;
        }
        public BlazorInstanceFacade GetParent(CancellationToken cancellationToken = default(CancellationToken))
        {
            using var result = GetParentAsync(cancellationToken);
            result.Wait();
            return result.Result;
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            return connection.DisposeAsync();
        }

        public BlazorInstanceFacade(string HostUrl, string HubName, string token)
        {
            connection = new HubConnectionBuilder()
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(10) })
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
