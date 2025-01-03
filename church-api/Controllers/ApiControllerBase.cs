using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace church_api.Controllers;

//[Authorize(Policy = AuthPolicies.ValidUsers)]
[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase, IDisposable
{
    private bool _disposed;

    ~ApiControllerBase()
    {
        Dispose(false);
    }

    protected bool IsAdminRole()
    {
        return IsInRole(AuthRoles.PilotAdmins);
    }

    protected bool IsInRole(string role)
    {
        var roles = GetUserRoles();
        return roles.Any(x => x.Value == role);
    }

    protected IEnumerable<Claim> GetUserRoles()
    {
        if (HttpContext.User.Identity is not ClaimsIdentity identity)
        {
            return [];
        }

        return identity.FindAll(ClaimTypes.Role);
    }

    protected CancellationTokenSource CancellationTokenSource { get; } = new();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            CancellationTokenSource.Cancel();
        }

        _disposed = true;
    }
}
