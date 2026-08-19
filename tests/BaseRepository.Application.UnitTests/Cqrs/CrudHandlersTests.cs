using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Cqrs.Commands;
using BaseRepository.Application.Cqrs.Queries;
using BaseRepository.Application.UnitTests.TestSupport;
using BaseRepository.Domain.Exceptions;
using Xunit;

namespace BaseRepository.Application.UnitTests.Cqrs;

public class CrudHandlersTests
{
    private readonly InMemoryRepository<SampleEntity, int> _repository = new();
    private readonly InMemoryUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Create_AddsTheEntityAndReturnsItsDto()
    {
        var handler = new CreateCommandHandler<SampleEntity, int, SampleDto>(_repository, _unitOfWork);
        var command = new CreateCommand<SampleEntity, int, SampleDto>(new SampleDto { Name = "first" });

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("first", result.Name);
        var stored = await _repository.ListAsync(CancellationToken.None);
        Assert.Single(stored);
    }

    [Fact]
    public async Task Update_OnMissingEntity_ThrowsNotFoundException()
    {
        var handler = new UpdateCommandHandler<SampleEntity, int, SampleDto>(_repository, _unitOfWork);
        var command = new UpdateCommand<SampleEntity, int, SampleDto>(id: 999, new SampleDto { Name = "x" });

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Update_OnExistingEntity_AppliesTheDtoAndPersists()
    {
        await _repository.AddAsync(new SampleEntity(1, "original"), CancellationToken.None);
        var handler = new UpdateCommandHandler<SampleEntity, int, SampleDto>(_repository, _unitOfWork);
        var command = new UpdateCommand<SampleEntity, int, SampleDto>(id: 1, new SampleDto { Name = "changed" });

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("changed", result.Name);
        var stored = await _repository.GetByIdAsync(1, CancellationToken.None);
        Assert.Equal("changed", stored!.Name);
    }

    [Fact]
    public async Task Delete_OnExistingEntity_RemovesIt()
    {
        await _repository.AddAsync(new SampleEntity(1, "to-delete"), CancellationToken.None);
        var handler = new DeleteCommandHandler<SampleEntity, int>(_repository, _unitOfWork);

        await handler.Handle(new DeleteCommand<SampleEntity, int>(1), CancellationToken.None);

        var stored = await _repository.GetByIdAsync(1, CancellationToken.None);
        Assert.Null(stored);
    }

    [Fact]
    public async Task Delete_OnMissingEntity_ThrowsNotFoundException()
    {
        var handler = new DeleteCommandHandler<SampleEntity, int>(_repository, _unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new DeleteCommand<SampleEntity, int>(999), CancellationToken.None));
    }

    [Fact]
    public async Task GetById_OnMissingEntity_ThrowsNotFoundException()
    {
        var handler = new GetByIdQueryHandler<SampleEntity, int, SampleDto>(_repository);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetByIdQuery<SampleEntity, int, SampleDto>(999), CancellationToken.None));
    }

    [Fact]
    public async Task GetById_OnExistingEntity_ReturnsItsDto()
    {
        await _repository.AddAsync(new SampleEntity(1, "found-me"), CancellationToken.None);
        var handler = new GetByIdQueryHandler<SampleEntity, int, SampleDto>(_repository);

        var result = await handler.Handle(new GetByIdQuery<SampleEntity, int, SampleDto>(1), CancellationToken.None);

        Assert.Equal("found-me", result.Name);
    }

    [Fact]
    public async Task GetList_ReturnsPagedResultsWithCorrectMetadata()
    {
        for (var i = 1; i <= 5; i++)
            await _repository.AddAsync(new SampleEntity(i, $"item-{i}"), CancellationToken.None);

        var handler = new GetListQueryHandler<SampleEntity, int, SampleDto>(_repository);
        var page = await handler.Handle(new GetListQuery<SampleEntity, int, SampleDto>(pageIndex: 2, pageSize: 2), CancellationToken.None);

        Assert.Equal(5, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(2, page.PageIndex);
    }
}
