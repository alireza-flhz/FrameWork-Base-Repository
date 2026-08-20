using FluentValidation;

namespace BaseRepository.Application.Common.Validation;

/// <summary>
/// Ready-made FluentValidation rules for common Iranian formats, wrapping the pure checks in
/// BaseRepository.Domain.Common. Usage: <c>RuleFor(x => x.Phone).IranianMobileNumber();</c>
/// Fully-qualified below on purpose - each rule method shares its name with the Domain type it
/// wraps, which would otherwise shadow that type inside this class.
/// </summary>
public static class ValidationRuleExtensions
{
    public static IRuleBuilderOptions<T, string> IranianMobileNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder
            .Must(BaseRepository.Domain.Common.IranianMobileNumber.IsValid)
            .WithMessage("'{PropertyName}' must be a valid Iranian mobile number.");

    public static IRuleBuilderOptions<T, string> IranianNationalCode<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder
            .Must(BaseRepository.Domain.Common.IranianNationalCode.IsValid)
            .WithMessage("'{PropertyName}' is not a valid Iranian national code.");
}
