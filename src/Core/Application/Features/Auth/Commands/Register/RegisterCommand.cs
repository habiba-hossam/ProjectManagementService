namespace ProjectManagementAPI.Core.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string FullName, string Email, string Password) : MediatR.IRequest<AuthResponseDto>;
