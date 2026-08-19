using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaseRepository.Infrastructure;

/// <summary>
/// DbContext, generic repository, specification evaluator and UnitOfWork registrations
/// land here starting Phase 1.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
