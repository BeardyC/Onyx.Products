namespace Onyx.ProductManagement.Api.Common;

public record ApiError(string Message);
public record DuplicateProductError(string Message) : ApiError(Message);
