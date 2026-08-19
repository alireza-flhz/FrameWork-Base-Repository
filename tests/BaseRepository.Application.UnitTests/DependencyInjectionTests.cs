using BaseRepository.Application;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BaseRepository.Application.UnitTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ReturnsTheSameServiceCollectionForChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddApplication();

        Assert.Same(services, result);
    }
}
