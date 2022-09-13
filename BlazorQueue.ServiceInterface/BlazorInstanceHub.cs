using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using ServiceStack;
using ServiceStack.Host;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace BlazorQueue.ServiceInterface
{
    public class BlazorInstanceHub : ServiceGatewayHub
    {
        public BlazorInstanceHub(IServiceGatewayAsync serviceGatewayAsync) : base(serviceGatewayAsync)
        {
        }
    }
    public class ServiceGatewayHub : Hub
    {
        private IServiceGatewayAsync serviceGatewayAsync;

        public ServiceGatewayHub(IServiceGatewayAsync serviceGatewayAsync)
        {
            this.serviceGatewayAsync = serviceGatewayAsync;

        }

        public async Task<TResponse> SendAsync<TResponse>(IReturn<TResponse> requestDto)
        {
            return await serviceGatewayAsync.SendAsync<TResponse>(requestDto);
        }

        public async Task<List<TResponse>> SendAllAsync<TResponse>(IEnumerable<IReturn<TResponse>> requestDtos)
        {

            return await serviceGatewayAsync.SendAllAsync<TResponse>(requestDtos);
        }

        public async Task PublishAsync(object requestDto)
        {
            await serviceGatewayAsync.PublishAsync(requestDto);
        }

        public async Task PublishAllAsync(IEnumerable<object> requestDtos)
        {
            await serviceGatewayAsync.PublishAllAsync(requestDtos);
        }

    }
}
