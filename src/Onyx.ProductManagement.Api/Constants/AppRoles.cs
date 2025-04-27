namespace Onyx.ProductManagement.Api.Constants;

internal static class AppRoles
{
    public const string ProductWriteAccess = "ProductWriteAccess";
    public const string ProductReadAccess = "ProductReadAccess";
    public const string ProductAdminAccess = "ProductAdminAccess";
}

internal static class RoleGroups
{
    public static readonly string[] ReadRoles = [AppRoles.ProductReadAccess, AppRoles.ProductWriteAccess, AppRoles.ProductAdminAccess];
    public static readonly string[] WriteRoles = [AppRoles.ProductWriteAccess, AppRoles.ProductAdminAccess];
}