using BaseRepository.Application.Behaviors;
using BaseRepository.Application.Cqrs.Commands;
using BaseRepository.Application.Cqrs.Queries;
using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Common;
using BaseRepository.Domain.Entities;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace BaseRepository.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Auto-discovers every AbstractValidator<T> in this assembly - drop a validator next
        // to the DTO/command it validates and it's picked up with no extra registration.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }

    /// <summary>
    /// Registers the generic Create/Update/Delete/GetById/GetList handlers for one entity.
    /// Call this once per entity you want CRUD for, e.g.
    /// <c>services.AddCrudHandlers&lt;Product, int, ProductDto&gt;();</c>
    /// Add an <c>AbstractValidator&lt;CreateCommand&lt;Product,int,ProductDto&gt;&gt;</c> (and/or
    /// for Update) if that entity's writes need validation - AddApplication() picks it up
    /// automatically via assembly scanning, and does nothing if none exists.
    /// </summary>
    public static IServiceCollection AddCrudHandlers<TEntity, TKey, TDto>(this IServiceCollection services)
        where TEntity : BaseEntity<TKey>
    {
        // Mapster maps onto non-public setters too, so an in-place Adapt(entity) during Update
        // would otherwise overwrite the tracked entity's real Id with the DTO's (usually
        // unset/default) Id and make EF Core reject the update as "changing the key".
        TypeAdapterConfig<TDto, TEntity>.NewConfig().Ignore(dest => dest.Id!);

        services.AddScoped<IRequestHandler<CreateCommand<TEntity, TKey, TDto>, TDto>, CreateCommandHandler<TEntity, TKey, TDto>>();
        services.AddScoped<IRequestHandler<UpdateCommand<TEntity, TKey, TDto>, TDto>, UpdateCommandHandler<TEntity, TKey, TDto>>();
        services.AddScoped<IRequestHandler<DeleteCommand<TEntity, TKey>, Unit>, DeleteCommandHandler<TEntity, TKey>>();
        services.AddScoped<IRequestHandler<GetByIdQuery<TEntity, TKey, TDto>, TDto>, GetByIdQueryHandler<TEntity, TKey, TDto>>();
        services.AddScoped<IRequestHandler<GetListQuery<TEntity, TKey, TDto>, PagedResult<TDto>>, GetListQueryHandler<TEntity, TKey, TDto>>();

        return services;
    }
}
