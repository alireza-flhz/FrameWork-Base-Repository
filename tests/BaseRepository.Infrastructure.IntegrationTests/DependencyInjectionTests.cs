using BaseRepository.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BaseRepository.Infrastructure.IntegrationTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_ReturnsTheSameServiceCollectionForChaining()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var result = services.AddInfrastructure(configuration);

        Assert.Same(services, result);
    }
}
