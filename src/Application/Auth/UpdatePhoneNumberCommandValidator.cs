using BaseRepository.Application.Abstractions;
using FluentValidation;

namespace BaseRepository.Application.Auth;

public class UpdatePhoneNumberCommandValidator : AbstractValidator<UpdatePhoneNumberCommand>
{
    public UpdatePhoneNumberCommandValidator(IPhoneNumberValidator phoneNumberValidator)
    {
        RuleFor(x => x)
            .Must(x => phoneNumberValidator.IsValid(x.PhoneNumber!, x.Region))
            .WithMessage("'PhoneNumber' is not a valid phone number for the given region.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
