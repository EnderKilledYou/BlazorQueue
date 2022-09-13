using Microsoft.AspNetCore.SignalR.Client;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;


namespace BlazorQueue.ServiceInterface
{
  
    public class BlazorInstanceFacade : HasBlazorQueueGuid, IAsyncDisposable ,  IJsonOnDeserialized, IEquatable<BlazorInstanceFacade>, IServiceGatewayAsync, IConnectToHub
    {

       
        public async Task<List<BlazorTag>> BlazorTags(CancellationToken cancellationToken = default)
        {
            return await connection.InvokeAsync<List<BlazorTag>>(nameof(BlazorTags), cancellationToken: cancellationToken);
        }
        public string HostUrl { get; }
        public string HubName { get; }
        public string Token { get; }

        private   HubConnection connection;


         
        public ValueTask DisposeAsync()
        {
            var val = connection.DisposeAsync();
            GC.SuppressFinalize(this);
            return val;
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
         
        public override bool Equals(object obj)
        {
            return Equals(obj as BlazorInstanceFacade);
        }

        public bool Equals(BlazorInstanceFacade other)
        {
            return other is not null &&
                   Guid.Equals(other.Guid);
        }

        public static bool operator ==(BlazorInstanceFacade left, BlazorInstanceFacade right)
        {
            return EqualityComparer<BlazorInstanceFacade>.Default.Equals(left, right);
        }

        public static bool operator !=(BlazorInstanceFacade left, BlazorInstanceFacade right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        public async Task<TResponse> SendAsync<TResponse>(object requestDto, CancellationToken token = default)
        {
            return await connection.InvokeAsync<TResponse>("SendAsync",requestDto, token);
        }

        public async Task<List<TResponse>> SendAllAsync<TResponse>(IEnumerable<object> requestDtos, CancellationToken token = default)
        {
            return await connection.InvokeAsync<List<TResponse>>("SendAllAsync", requestDtos, token);
        }

        public async Task PublishAsync(object requestDto, CancellationToken token = default)
        {
            await connection.SendAsync("PublishAsync", requestDto, token);
        }

        public async Task PublishAllAsync(IEnumerable<object> requestDtos, CancellationToken token = default)
        {
            await connection.SendAsync("PublishAllAsync", requestDtos, token);
        }
    }
}
