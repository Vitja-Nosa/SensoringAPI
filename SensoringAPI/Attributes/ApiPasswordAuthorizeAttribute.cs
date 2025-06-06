using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SensoringAPI.Attributes;

public class ApiPasswordAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly bool _writeRequired;

    public ApiPasswordAuthorizeAttribute(bool writeRequired = false)
    {
        _writeRequired = writeRequired;
    }

    public void OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var readPassword = config["Auth:ReadPassword"];
        var writePassword = config["Auth:WritePassword"];

        if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Password", out var providedPassword))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (_writeRequired)
        {
            if (providedPassword != writePassword)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }
        else
        {
            if (providedPassword != readPassword && providedPassword != writePassword)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }
    }
}

