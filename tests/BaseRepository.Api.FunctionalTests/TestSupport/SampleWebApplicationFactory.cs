using BaseRepository.Application;
using BaseRepository.Application.Cqrs.Commands;
using BaseRepository.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BaseRepository.Api.FunctionalTests.TestSupport;

/// <summary>
/// Wires a real SQLite-backed entity (SampleEntity/SampleDto/SamplesController) into the
/// shipped Api pipeline, so these tests exercise the whole stack - real HTTP, real
/// controller, real mediator/validation pipeline, real EF Core, real exception mapping -
/// not just each layer in isolation.
/// </summary>
public class SampleWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public SampleWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddControllers().AddApplicationPart(typeof(SamplesController).Assembly);

            services.AddPersistence<SampleDbContext>(options => options.UseSqlite(_connection));
            services.AddCrudHandlers<SampleEntity, int, SampleDto>();
            services.AddScoped<IValidator<CreateCommand<SampleEntity, int, SampleDto>>, SampleCreateValidator>();

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<SampleDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
