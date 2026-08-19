using BaseRepository.Application.Cqrs.Commands;
using FluentValidation;

namespace BaseRepository.Api.FunctionalTests.TestSupport;

public class SampleCreateValidator : AbstractValidator<CreateCommand<SampleEntity, int, SampleDto>>
{
    public SampleCreateValidator()
    {
        RuleFor(x => x.Dto.Name).NotEmpty();
    }
}
