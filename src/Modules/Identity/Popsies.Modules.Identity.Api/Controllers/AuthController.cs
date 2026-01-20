using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Popsies.Modules.Identity.Application.Common.Repositories;
using Popsies.Modules.Identity.Application.UseCases.CreateGuest;
using Popsies.Modules.Identity.Application.UseCases.Login;
using Popsies.Modules.Identity.Application.UseCases.RefreshToken;
using Popsies.Modules.Identity.Application.UseCases.Register;
using Popsies.Modules.Identity.Contracts;

namespace Popsies.Modules.Identity.Api.Controllers;

[ApiController]
[Route("api/identity/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IUserRepository _userRepository;

    public AuthController(ISender sender, IUserRepository userRepository)
    {
        _sender = sender;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterUserResponse>> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Username,
            request.Email,
            request.Password,
            request.ConfirmPassword);

        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        var userId = result.Value;
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        var response = new RegisterUserResponse(
            userId,
            user!.Username.FullUsername,
            user.Email.Value,
            "Registration successful. Please verify your email.");

        return CreatedAtAction(nameof(Register), new { id = userId }, response);
    }

    /// <summary>
    /// Login with email or username
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var deviceInfo = request.DeviceInfo ?? Request.Headers.UserAgent.ToString();

        var command = new LoginCommand(
            request.UsernameOrEmail,
            request.Password,
            deviceInfo);

        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var loginResult = result.Value;
        var response = new LoginResponse(
            loginResult.UserId,
            loginResult.AccessToken,
            loginResult.RefreshToken,
            loginResult.ExpiresAt);

        return Ok(response);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var deviceInfo = request.DeviceInfo ?? Request.Headers.UserAgent.ToString();

        var command = new RefreshTokenCommand(
            request.RefreshToken,
            deviceInfo);

        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        var tokenResult = result.Value;
        var response = new RefreshTokenResponse(
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.ExpiresAt);

        return Ok(response);
    }

    /// <summary>
    /// Create a guest session
    /// </summary>
    [HttpPost("guest")]
    [ProducesResponseType(typeof(CreateGuestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateGuestResponse>> CreateGuest(
        [FromBody] CreateGuestRequest request,
        CancellationToken cancellationToken)
    {
        var deviceInfo = request.DeviceInfo ?? Request.Headers.UserAgent.ToString();

        var command = new CreateGuestCommand(
            request.DisplayName,
            deviceInfo);

        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        var guestResult = result.Value;
        var response = new CreateGuestResponse(
            guestResult.GuestId,
            guestResult.AccessToken,
            guestResult.RefreshToken,
            guestResult.ExpiresAt);

        return CreatedAtAction(nameof(CreateGuest), new { id = guestResult.GuestId }, response);
    }
}
