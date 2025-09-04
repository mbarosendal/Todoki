namespace Todoki.Server.Controllers
{
    using global::Todoki.Data.Shared.ViewModels;
    using global::Todoki.Data.ViewModels;
    using global::Todoki.Server.Data.Helpers;
    using global::Todoki.Server.Data.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.RateLimiting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    namespace Todoki.Server.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class AuthenticationController : ControllerBase
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly ApplicationDbContext _context;
            private readonly IConfiguration _configuration;
            private readonly TokenValidationParameters _tokenValidationParameterse; 
            private readonly ILogger<AuthenticationController> _logger;

            public AuthenticationController(UserManager<ApplicationUser> userManager,
                RoleManager<IdentityRole> roleManager,
                ApplicationDbContext context,
                IConfiguration configuration,
                TokenValidationParameters tokenValidationParameterse,
                ILogger<AuthenticationController> logger)
            {
                _userManager = userManager;
                _roleManager = roleManager;
                _context = context;
                _configuration = configuration;
                _tokenValidationParameterse = tokenValidationParameterse;
                _logger = logger;
            }

            [EnableRateLimiting("AuthPolicy")]
            [HttpPostAttribute("register-user")]
            public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Register failed: Register attempt with invalid model for email: {Email}", registerRequest.EmailAddress);
                    return BadRequest("Please provide all the required fields");
                }

                var userExists = await _userManager.FindByEmailAsync(registerRequest.EmailAddress);

                if (userExists != null)
                {
                    _logger.LogWarning("Register failed: Register attempt with existing email: {Email}", registerRequest.EmailAddress);
                    return BadRequest($"User {registerRequest.EmailAddress} already exists");
                }

                ApplicationUser newUser = new ApplicationUser()
                {
                    Email = registerRequest.EmailAddress,
                    UserName = registerRequest.EmailAddress,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await _userManager.CreateAsync(newUser, registerRequest.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, UserRoles.User);

                    _logger.LogInformation("New user created: {UserId} ({Email})", newUser.Id, newUser.Email);
                    return Ok("User created");
                }

                _logger.LogError("Failed to create user for email {Email}. Errors: {ErrorCodes}",
                                 registerRequest.EmailAddress,
                                 string.Join(", ", result.Errors.Select(e => e.Code)));
                return BadRequest(result.Errors);
            }

            [Authorize(Roles = "Admin")]
            [HttpPost("assign-role")]
            public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
            {
                var user = await _userManager.FindByIdAsync(request.EmailAddress);
                if (user == null)
                {
                    _logger.LogWarning("AssignRole failed: The user to be assigned role was not found: {Email}",
                        request.EmailAddress);
                    return NotFound("Invalid request.");
                }

                var roleExists = await _roleManager.RoleExistsAsync(request.Role);
                if (!roleExists)
                {
                    _logger.LogWarning("AssignRole failed: The role was not found: {Role}",
                        request.Role);
                    return NotFound("Invalid request.");
                }

                var result = await _userManager.AddToRoleAsync(user, request.Role);
                if (!result.Succeeded)
                {
                    _logger.LogError("AssignRole failed: Failed to assign role for user {Email}. Errors: {@Errors}",
                        request.EmailAddress, result.Errors);
                    return BadRequest("Invalid assignment");
                }

                _logger.LogInformation("User {Email} ({UserId}) was assigned role {Role}",
                                       request.EmailAddress, user.Id, request.Role);
                return Ok($"Role '{request.Role}' assigned to user {request.EmailAddress}.");
            }

            // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/logging-in-users?autoSkip=true&resume=false&u=57075649
            [EnableRateLimiting("AuthPolicy")]
            [HttpPost("login-user")]
            public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Login failed: Invalid model provided for email {Email}", loginRequest.EmailAddress);
                    return BadRequest("Please provide all required fields.");
                }

                var userExists = await _userManager.FindByEmailAsync(loginRequest.EmailAddress);

                if (userExists != null && await _userManager.CheckPasswordAsync(userExists, loginRequest.Password))
                {
                    // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/generating-an-access-token?autoSkip=true&resume=false&u=57075649
                    var tokenValue = await GenerateJWTTokenAsync(userExists, null);

                    _logger.LogInformation("User {UserId} ({Email}) logged in successfully", userExists.Id, loginRequest.EmailAddress);
                    return Ok(tokenValue);
                }

                _logger.LogWarning("Login failed: Invalid credentials provided.");
                return Unauthorized();
            }

            [HttpPost("logout")]
            [AllowAnonymous]
            public async Task<IActionResult> Logout([FromBody] LogoutRequest logoutRequest)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Please provide all required fields");
                }

                var refreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == logoutRequest.RefreshToken && !rt.IsRevoked);

                if (refreshToken == null)
                {
                    return BadRequest("Invalid refresh token");
                }

                refreshToken.IsRevoked = true;
                _context.RefreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync();

                return Ok("User logged out successfully");
            }

            [HttpPost("refresh-token")]
            public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Please provide all required fields");
                }

                var result = await VerifyAndGenerateTokenAsync(tokenRequest);

                return Ok(result);

            }

            private async Task<AuthResponse> VerifyAndGenerateTokenAsync(TokenRequest tokenRequest)
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();

                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);

                if (storedToken == null)
                {
                    return new AuthResponse()
                    {
                        IsSuccess = false,
                        Error = "Invalid refresh token"
                    };
                }

                if (storedToken.IsRevoked || storedToken.DateExpire < DateTime.UtcNow)
                {
                    return new AuthResponse()
                    {
                        IsSuccess = false,
                        Error = "Refresh token expired or revoked"
                    };
                }

                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
                if (dbUser == null)
                {
                    return new AuthResponse()
                    {
                        IsSuccess = false,
                        Error = "User not found"
                    };
                }

                try
                {
                    var tokenCheckResult = jwtTokenHandler.ValidateToken(
                        tokenRequest.Token,
                        _tokenValidationParameterse,
                        out var validatedToken);

                    var jwtToken = validatedToken as JwtSecurityToken;
                    var jti = jwtToken?.Claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

                    if (storedToken.JwtId != jti)
                    {
                        return new AuthResponse()
                        {
                            IsSuccess = false,
                            Error = "Token mismatch"
                        };
                    }

                    storedToken.IsRevoked = true;
                    _context.RefreshTokens.Update(storedToken);
                    await _context.SaveChangesAsync();

                    return await GenerateJWTTokenAsync(dbUser, null);
                }
                catch (SecurityTokenExpiredException)
                {
                    var jwtToken = jwtTokenHandler.ReadJwtToken(tokenRequest.Token);
                    var jti = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

                    if (storedToken.JwtId != jti)
                    {
                        return new AuthResponse()
                        {
                            IsSuccess = false,
                            Error = "Token mismatch"
                        };
                    }

                    storedToken.IsRevoked = true;
                    _context.RefreshTokens.Update(storedToken);
                    await _context.SaveChangesAsync();

                    return await GenerateJWTTokenAsync(dbUser, null);
                }
                catch (Exception ex)
                {
                    // Invalid token format
                    return new AuthResponse()
                    {
                        IsSuccess = false,
                        Error = "Invalid token"
                    };
                }
            }

            // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/generating-an-access-token?autoSkip=true&resume=false&u=57075649
            private async Task<AuthResponse> GenerateJWTTokenAsync(ApplicationUser user, RefreshToken rToken)
            {
                var jti = Guid.NewGuid().ToString();

                _logger.LogInformation("User has Id: " + user.Id);

                var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
            };

                if (rToken != null)
                {
                    rToken.JwtId = jti;
                }

                // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/adding-role-claims-to-tokens?autoSkip=true&resume=false&u=57075649 
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration.GetValue<string>("JWT:Issuer"),     
                    audience: _configuration.GetValue<string>("JWT:Audience"),
                    expires: DateTime.UtcNow.AddMinutes(30),                
                    claims: authClaims,                
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

                // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/refreshing-expired-tokens?autoSkip=true&resume=false&u=57075649
                if (rToken != null)
                {
                    var rTokenResponse = new AuthResponse()
                    {
                        Token = jwtToken,
                        RefreshToken = rToken.Token,
                        ExpiresAt = token.ValidTo

                    };
                    return rTokenResponse;
                }

                // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/generating-and-storing-refresh-tokens?autoSkip=true&resume=false&u=57075649
                var refreshToken = new RefreshToken()
                {
                    JwtId = jti,                                            
                    IsRevoked = false,                                     
                    UserId = user.Id,                                     
                    DateAdded = DateTime.UtcNow,                       
                    DateExpire = DateTime.UtcNow.AddMonths(6),            
                    Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString(),
                };

                await _context.RefreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();

                var response = new AuthResponse()
                {
                    Token = jwtToken,                 
                    RefreshToken = refreshToken.Token, 
                    ExpiresAt = token.ValidTo

                };

                return response;

            }
        }
    }
}
