using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Cqrs;
using BaseRepository.Application.Cqrs.Commands;
using BaseRepository.Application.Cqrs.Queries;
using BaseRepository.Application.Messaging;
using BaseRepository.Domain.Common;
using BaseRepository.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseRepository.Api.Controllers;

/// <summary>
/// Full REST CRUD for one entity, routed through the generic CQRS handlers from
/// BaseRepository.Application.Cqrs. Derive a concrete controller with its own [Route], e.g.
/// <c>[Route("api/products")] public class ProductsController : BaseCrudController&lt;Product,int,ProductDto&gt;</c>.
/// </summary>
[ApiController]
public abstract class BaseCrudController<TEntity, TKey, TDto> : ControllerBase
    where TEntity : BaseEntity<TKey>
{
    protected ISender Sender { get; }

    protected BaseCrudController(ISender sender)
    {
        Sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TDto>>> GetList(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetListQuery<TEntity, TKey, TDto>(pageIndex, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TDto>> GetById(TKey id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetByIdQuery<TEntity, TKey, TDto>(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TDto>> Create([FromBody] TDto dto, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new CreateCommand<TEntity, TKey, TDto>(dto), cancellationToken);

        if (result is IHasId<TKey> withId)
            return CreatedAtAction(nameof(GetById), new { id = withId.Id }, result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TDto>> Update(TKey id, [FromBody] TDto dto, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new UpdateCommand<TEntity, TKey, TDto>(id, dto), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(TKey id, CancellationToken cancellationToken = default)
    {
        await Sender.Send(new DeleteCommand<TEntity, TKey>(id), cancellationToken);
        return NoContent();
    }
}
