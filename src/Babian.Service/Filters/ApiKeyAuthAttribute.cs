using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Babian.Infrastructure.Persistence;
using System.Security.Claims;

namespace Babian.Service.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    private const string ApiKeyHeaderName = "x-api-key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Result = new UnauthorizedObjectResult("API Key manquante dans le header x-api-key.");
            return;
        }

        var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.ApiKey == extractedApiKey.ToString() && !string.IsNullOrEmpty(u.RestaurantId));

        if (user == null)
        {
            context.Result = new UnauthorizedObjectResult("API Key invalide ou non rattachée à un compte restaurant.");
            return;
        }

        // Inject claims so that Request.HttpContext.User has the RestaurantId
        var claims = new[] 
        { 
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("RestaurantId", user.RestaurantId)
        };
        var identity = new ClaimsIdentity(claims, "ApiKey");
        context.HttpContext.User = new ClaimsPrincipal(identity);

        await next();
    }
}
