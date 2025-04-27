namespace Onyx.ProductManagement.Api.Common;

internal record ValidationFailureResponse
{
    public string PropertyName { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}