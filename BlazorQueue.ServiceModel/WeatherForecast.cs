using ServiceStack;
using System;

namespace BlazorQueue.ServiceModel;
public interface IConnectToHub
{
    public string HostUrl { get; }
    public string HubName { get; }
    public string Token { get; }
}
public class WeatherForecast
{
    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public string Summary { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class WhoParentRequest : IReturn<WhoParentResponse>
{
}

public class WhoParentResponse
{
    public IConnectToHub Parent { get; set; }
}