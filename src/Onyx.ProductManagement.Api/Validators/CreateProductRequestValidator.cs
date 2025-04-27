using FluentValidation;
using Onyx.ProductManagement.Api.Endpoints.v1.Products;

namespace Onyx.ProductManagement.Api.Validators;

internal class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Product price must be greater than zero.");

        RuleFor(x => x.Colour)
            .NotEmpty().WithMessage("Product colour is required.")
            .MaximumLength(50).WithMessage("Product colour cannot exceed 50 characters.");
    }
}