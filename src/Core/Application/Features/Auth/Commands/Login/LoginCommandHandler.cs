using AutoMapper;
using FluentValidation;
using MediatR;
using ProjectManagementAPI.Core.Application.Common.Exceptions;
using ProjectManagementAPI.Core.Application.Common.Interfaces;
using ProjectManagementAPI.Core.Application.Features.Auth.Commands.Register;

namespace ProjectManagementAPI.Core.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    public LoginCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IJwtService jwtService, IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        var token = _jwtService.GenerateToken(user);
        return _mapper.Map<AuthResponseDto>(user, opt => opt.AfterMap((src, dest) => dest.Token = token));
    }
}
