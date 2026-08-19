using Microsoft.Extensions.DependencyInjection;

namespace BaseRepository.Application;

/// <summary>
/// MediatR, FluentValidation and mapping registrations land here starting Phase 2.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
