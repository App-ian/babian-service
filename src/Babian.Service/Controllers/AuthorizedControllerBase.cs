using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Babian.Service.Controllers;

/// <summary>
/// Classe de base pour tous les contrôleurs nécessitant une authentification JWT.
/// Expose CurrentUserId extrait du token, évitant la duplication de code.
/// </summary>
[Authorize]
public abstract class AuthorizedControllerBase : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
