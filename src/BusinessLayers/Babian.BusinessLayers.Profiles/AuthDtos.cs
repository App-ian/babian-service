using System;

namespace Babian.BusinessLayers.Profiles;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string? Role = "Barman");
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Email, string FirstName, string LastName, string Role, Guid Id, string? RestaurantId = null, string? ApiKey = null);
