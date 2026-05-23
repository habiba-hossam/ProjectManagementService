namespace ProjectManagementAPI.Core.Application.Features.Auth.Commands;

// Shared response DTO used by both Register and Login
public record AuthResponseDto(Guid UserId, string FullName, string Email, string Token, string Role);
