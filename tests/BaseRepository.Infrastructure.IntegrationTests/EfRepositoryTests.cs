using System;
using System.Linq;
using System.Threading.Tasks;
using BaseRepository.Application.Abstractions;
using BaseRepository.Infrastructure.IntegrationTests.TestSupport;
using BaseRepository.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseRepository.Infrastructure.IntegrationTests;

public class EfRepositoryTests : IDisposable
{
    private readonly SqliteTestDatabase _database;
    private readonly IRepository<TestEntity, int> _repository;

    public EfRepositoryTests()
    {
        _database = new SqliteTestDatabase();
        _repository = new EfRepository<TestEntity, int>(_database.Context);
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task AddAsync_ThenSaveChanges_PersistsTheEntityAndStampsCreatedAt()
    {
        var entity = new TestEntity { Name = "first" };

        await _repository.AddAsync(entity);
        await _database.Context.SaveChangesAsync();

        Assert.True(entity.Id > 0);
        Assert.NotEqual(default, entity.CreatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsThePersistedEntity()
    {
        var entity = new TestEntity { Name = "lookup-me" };
        await _repository.AddAsync(entity);
        await _database.Context.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(entity.Id);

        Assert.NotNull(found);
        Assert.Equal("lookup-me", found!.Name);
    }

    [Fact]
    public async Task Update_MarksEntityModified_AndStampsLastModifiedAtWithoutTouchingCreatedAt()
    {
        var entity = new TestEntity { Name = "original" };
        await _repository.AddAsync(entity);
        await _database.Context.SaveChangesAsync();
        var createdAt = entity.CreatedAt;

        entity.Name = "changed";
        _repository.Update(entity);
        await _database.Context.SaveChangesAsync();

        Assert.Equal(createdAt, entity.CreatedAt);
        Assert.NotNull(entity.LastModifiedAt);
    }

    [Fact]
    public async Task Remove_WithSoftDeleteEntity_MarksAsDeletedInsteadOfHardDeleting()
    {
        var entity = new TestEntity { Name = "to-delete" };
        await _repository.AddAsync(entity);
        await _database.Context.SaveChangesAsync();
        var id = entity.Id;

        _repository.Remove(entity);
        await _database.Context.SaveChangesAsync();
        _database.Context.ChangeTracker.Clear();

        var found = await _repository.GetByIdAsync(id);
        Assert.Null(found);

        var raw = await _database.Context.TestEntities
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id);
        Assert.NotNull(raw);
        Assert.True(raw!.IsDeleted);
        Assert.NotNull(raw.DeletedAt);
    }

    [Fact]
    public async Task ListAsync_WithSpecification_FiltersAndOrders()
    {
        await _repository.AddAsync(new TestEntity { Name = "banana" });
        await _repository.AddAsync(new TestEntity { Name = "apple" });
        await _repository.AddAsync(new TestEntity { Name = "cherry" });
        await _database.Context.SaveChangesAsync();

        var spec = new TestEntitiesByNameSpecification(nameContains: "a");
        var results = await _repository.ListAsync(spec);

        Assert.Equal(2, results.Count);
        Assert.Equal("apple", results[0].Name);
        Assert.Equal("banana", results[1].Name);
    }

    [Fact]
    public async Task PaginatedListAsync_ReturnsCorrectPageMetadata()
    {
        for (var i = 1; i <= 5; i++)
            await _repository.AddAsync(new TestEntity { Name = $"item-{i:00}" });
        await _database.Context.SaveChangesAsync();

        var spec = new TestEntitiesByNameSpecification(nameContains: "item", skip: 2, take: 2);
        var page = await _repository.PaginatedListAsync(spec);

        Assert.Equal(5, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(2, page.PageIndex);
        Assert.Equal(2, page.PageSize);
        Assert.Equal("item-03", page.Items[0].Name);
    }

    [Fact]
    public async Task CountAsync_And_AnyAsync_IgnorePaging()
    {
        await _repository.AddAsync(new TestEntity { Name = "match-1" });
        await _repository.AddAsync(new TestEntity { Name = "match-2" });
        await _database.Context.SaveChangesAsync();

        var spec = new TestEntitiesByNameSpecification(nameContains: "match", skip: 0, take: 1);

        Assert.Equal(2, await _repository.CountAsync(spec));
        Assert.True(await _repository.AnyAsync(spec));
    }
}
