using Converter.Core.Contracts;
using FluentValidation;

namespace Converter.Api.Validation;

public sealed class InputDtoValidator : AbstractValidator<InputDto>
{
    private static readonly DateOnly MinDate = new(2024, 8, 24);

    public InputDtoValidator()
    {
        RuleFor(x => x.Status)
            .Equal(3).WithMessage("Status must be 3.");

        RuleFor(x => x.PublishDate)
            .Must(d => DateOnly.FromDateTime(d.UtcDateTime) >= MinDate)
            .WithMessage("PublishDate must be on or after 2024-08-24.");

        RuleFor(x => x.TestRun)
            .Equal(true).WithMessage("Request must be a test run (TestRun=true).");
    }
}
