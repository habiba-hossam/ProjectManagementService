namespace ProjectManagementAPI.Core.Application.Features.Auth.Commands;

// Shared response DTO used by both Register and Login
public class AuthResponseDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public string Token { get; set; } = String.Empty;
    public string Role { get; set; } = String.Empty;
}   
