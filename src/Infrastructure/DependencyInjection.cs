using System;
using BaseRepository.Application.Abstractions;
using BaseRepository.Infrastructure.Persistence;
using BaseRepository.Infrastructure.Persistence.Interceptors;
using BaseRepository.Infrastructure.Phone;
using BaseRepository.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaseRepository.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Cross-cutting infrastructure services that don't need a DbContext. Password hashing and
    /// JWT issuing for Auth (Application.Auth) live here; email, file storage, and other
    /// external clients land here too as they're added.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Singleton: PhoneNumberUtil.GetInstance() already loads/caches all metadata once and
        // is documented as thread-safe, so there's no reason to rebuild it per request.
        services.AddSingleton<IPhoneNumberValidator, PhoneNumberValidator>();

        return services;
    }

    /// <summary>
    /// Wires EF Core persistence for <typeparamref name="TContext"/>: the audit/soft-delete
    /// interceptor, the generic repository, and the unit of work. Call this once your project
    /// has a concrete <see cref="DbContext"/>, e.g.
    /// <c>services.AddPersistence&lt;AppDbContext&gt;(options => options.UseSqlite(connectionString));</c>
    /// </summary>
    public static IServiceCollection AddPersistence<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
        where TContext : DbContext
    {
        services.AddSingleton<AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<TContext>((sp, options) =>
        {
            configureDbContext(options);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
