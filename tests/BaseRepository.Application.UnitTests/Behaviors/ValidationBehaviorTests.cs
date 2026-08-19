using System;
using System.Threading;
using System.Threading.Tasks;
using BaseRepository.Application.Behaviors;
using BaseRepository.Application.Messaging;
using FluentValidation;
using Xunit;

namespace BaseRepository.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    private class SampleRequest : IRequest<string>
    {
        public string Name { get; init; } = string.Empty;
    }

    private class SampleRequestValidator : AbstractValidator<SampleRequest>
    {
        public SampleRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    [Fact]
    public async Task Handle_WithNoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>(Array.Empty<IValidator<SampleRequest>>());

        var result = await behavior.Handle(
            new SampleRequest { Name = "" },
            () => Task.FromResult("next-called"),
            CancellationToken.None);

        Assert.Equal("next-called", result);
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ThrowsValidationException()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>(new[] { new SampleRequestValidator() });

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new SampleRequest { Name = "" }, () => Task.FromResult("next-called"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithPassingValidator_CallsNext()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>(new[] { new SampleRequestValidator() });

        var result = await behavior.Handle(
            new SampleRequest { Name = "valid" },
            () => Task.FromResult("next-called"),
            CancellationToken.None);

        Assert.Equal("next-called", result);
    }
}
