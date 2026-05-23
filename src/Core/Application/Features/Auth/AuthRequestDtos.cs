namespace ProjectManagementAPI.Core.Application.Features.Auth.Commands;

public record RegisterRequest(string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);
