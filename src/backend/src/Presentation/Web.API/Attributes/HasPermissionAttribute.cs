using Microsoft.AspNetCore.Authorization;

namespace Web.API.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute
{
    public string Permission { get; } = permission;
}
