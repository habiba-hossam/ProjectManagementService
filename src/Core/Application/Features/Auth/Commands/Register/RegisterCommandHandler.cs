using AutoMapper;
using MediatR;
using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Domain.Entities;

namespace ProjectManagementAPI.Core.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IJwtService jwtService, IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
            throw new ConflictException($"A user with email '{request.Email}' already exists.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = "User"
        };

        await _userRepository.AddAsync(user, cancellationToken);

        var token = _jwtService.GenerateToken(user);
        return _mapper.Map<AuthResponseDto>(user, opt => opt.AfterMap((src, dest) => dest.Token = token));
    }
}
