using BaseRepository.Api.Controllers;
using BaseRepository.Application.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace BaseRepository.Api.FunctionalTests.TestSupport;

[Route("api/samples")]
public class SamplesController : BaseCrudController<SampleEntity, int, SampleDto>
{
    public SamplesController(ISender sender) : base(sender)
    {
    }
}
