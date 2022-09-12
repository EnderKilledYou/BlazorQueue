using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;


namespace BlazorQueue.ServiceInterface
{
    public class BlazorInstanceFacade : HasBlazorQueueGuid, IAsyncDisposable , IAmABlazor, IJsonOnDeserialized
    {
        public List<BlazorTag> BlazorTags { get; init; } =  new();
        public string HostUrl { get; }
        public string HubName { get; }
        public string Token { get; }

        private   HubConnection connection;



        public async Task<BlazorInstanceFacade> GetByTag(BlazorTag tag, CancellationToken cancellationToken = default)
        {
            return await connection.InvokeAsync<BlazorInstanceFacade>("GetByTag", tag, cancellationToken: cancellationToken);
        }
 
    
        public async Task<BlazorInstanceFacade> GetParent(CancellationToken cancellationToken = default)
        {
            return await connection.InvokeAsync<BlazorInstanceFacade>("GetParent", cancellationToken: cancellationToken);

        }

        public ValueTask DisposeAsync()
        {
            var val = connection.DisposeAsync();
            GC.SuppressFinalize(this);
            return val;
        }

        public   TagCount GetTagQueueCountSync(BlazorTag tag, CancellationToken cancellationToken = default)
        {
            var task = connection.InvokeAsync<TagCount>("GetTagQueueCount", tag, cancellationToken: cancellationToken);
            task.Wait(cancellationToken);
            return task.Result;
        }

        public async Task<TagCount> GetTagQueueCount(BlazorTag tag, CancellationToken cancellationToken = default)
        {

            return await connection.InvokeAsync<TagCount>("GetTagQueueCount", tag, cancellationToken: cancellationToken);
        }

        public BlazorInstanceFacade(string HostUrl, string HubName, string token)
        {
            
            this.HostUrl = HostUrl;
            this.HubName = HubName;
            Token = token;
        }
        public async Task Start()
        {
            if (connection != null && connection.State != HubConnectionState.Disconnected)
            {
                throw new Exception("Already connected or connecting");
            }
            connection = new HubConnectionBuilder()
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(1) })
                .WithUrl(new Uri(HostUrl + HubName), options =>
                {
                    options.Headers.Add("Authorization", "bearer " + Token);
                }).Build();
            await connection.StartAsync();
        }
        public async Task Stop()
        {
            await connection.StopAsync();
        }

        public void OnDeserialized()
        {
            Start().Wait();
        }
    }
}
