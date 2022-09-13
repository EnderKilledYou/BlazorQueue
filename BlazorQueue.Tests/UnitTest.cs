using NUnit.Framework;
using ServiceStack;
using ServiceStack.Testing;
using BlazorQueue.ServiceInterface;
using BlazorQueue.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace BlazorQueue.Tests;

public class UnitTest
{
    private readonly ServiceStackHost appHost;

    public UnitTest()
    {
        appHost = new BasicAppHost().Init();
        
        appHost.Container.AddTransient<MyServices>();
        appHost.Container.AddTransient<BlazorInstanceHub>();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => appHost.Dispose();

    [Test]
    public void Test_tags_equal()
    {
        BlazorTag a = new BlazorTag { Name = "a" };
        BlazorTag b = new BlazorTag { Name = "a" };
        Assert.That(a.Equals(b), Is.True);
    }

    [Test]
    public void Test_tags_contains()
    {
        var a = new BlazorTag[] { new BlazorTag { Name = "a" } };
        BlazorTag b = new BlazorTag { Name = "a" };
        Assert.That(a.Contains(b), Is.True);
    }
 
    [Test]
    public void Can_call_MyServices()
    {
        
        var hub = appHost.Container.Resolve<BlazorInstanceHub>();

        
        
        Assert.That(hub, Is.Not.Null);
    }
}
