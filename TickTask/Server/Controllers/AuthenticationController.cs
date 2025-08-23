namespace TickTask.Server.Controllers
{
    using global::TickTask.Data.ViewModels;
    using global::TickTask.Server.Data.Helpers;
    using global::TickTask.Server.Data.Models;
    using global::TickTask.Shared.Data.ViewModels;
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

    // AUTHENTICATION CONTROLLER - FLOW EXPLANATION
    // Think of this like a bouncer at a club who:
    // 1. Checks IDs (login)
    // 2. Gives you a wristband (JWT token) 
    // 3. Lets you back in if you have the right wristband
    // 4. Can kick you out (logout/revoke)

    // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/adding-the-authentication-controller?autoSkip=true&resume=false&u=57075649
    namespace TickTask.Server.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class AuthenticationController : ControllerBase
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly ApplicationDbContext _context;
            private readonly IConfiguration _configuration;
            private readonly TokenValidationParameters _tokenValidationParameterse; // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/injecting-tokenvalidationparameters?autoSkip=true&resume=false&u=57075649
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

            // STEP 1: REGISTER USER - "Sign up for membership"
            // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/registering-new-users-using-usermanager?autoSkip=true&resume=false&u=57075649
            [EnableRateLimiting("AuthPolicy")]
            [HttpPostAttribute("register-user")]
            public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
            {
                // Validation: "Fill out the form completely"
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Register failed: Register attempt with invalid model for email: {Email}", registerRequest.EmailAddress);
                    return BadRequest("Please provide all the required fields");
                }

                // Check if already exists: "Sorry, that email is already taken"
                var userExists = await _userManager.FindByEmailAsync(registerRequest.EmailAddress);

                if (userExists != null)
                {
                    _logger.LogWarning("Register failed: Register attempt with existing email: {Email}", registerRequest.EmailAddress);
                    return BadRequest($"User {registerRequest.EmailAddress} already exists");
                }

                // Create the user: "Welcome! Creating your account..."
                ApplicationUser newUser = new ApplicationUser()
                {
                    Email = registerRequest.EmailAddress,
                    UserName = registerRequest.EmailAddress,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                // Try to save to database: "Saving your info..."
                var result = await _userManager.CreateAsync(newUser, registerRequest.Password);

                if (result.Succeeded)
                {
                    // Everybody becomes a "User" first
                    await _userManager.AddToRoleAsync(newUser, UserRoles.User);

                    _logger.LogInformation("New user created: {UserId} ({Email})", newUser.Id, newUser.Email);
                    return Ok("User created");
                }

                _logger.LogError("Failed to create user for email {Email}. Errors: {ErrorCodes}",
                                 registerRequest.EmailAddress,
                                 string.Join(", ", result.Errors.Select(e => e.Code)));
                return BadRequest(result.Errors);
            }

            // STEP 1.5: Upgrade (or downgrade) - "You know the owner? Let me just upgrade that for you"
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

            // STEP 2: LOGIN - "Show me your ID, get a wristband"
            // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/logging-in-users?autoSkip=true&resume=false&u=57075649
            [EnableRateLimiting("AuthPolicy")]
            [HttpPost("login-user")]
            public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
            {
                // Validation: "Please provide email and password"
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Login failed: Invalid model provided for email {Email}", loginRequest.EmailAddress);
                    return BadRequest("Please provide all required fields.");
                }

                // Find user: "Let me look you up..."
                var userExists = await _userManager.FindByEmailAsync(loginRequest.EmailAddress);

                // Check password: "Does your password match?"
                if (userExists != null && await _userManager.CheckPasswordAsync(userExists, loginRequest.Password))
                {
                    // Success! Give them tokens: "Here's your wristband and backup wristband"
                    // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/generating-an-access-token?autoSkip=true&resume=false&u=57075649
                    var tokenValue = await GenerateJWTTokenAsync(userExists, null);

                    _logger.LogInformation("User {UserId} ({Email}) logged in successfully", userExists.Id, loginRequest.EmailAddress);
                    return Ok(tokenValue);
                    // demonstration in postman of getting token at login and using it: https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/generating-an-access-token?autoSkip=true&resume=false&u=57075649
                }

                // Wrong credentials: "Nope, try again"
                _logger.LogWarning("Login failed: Invalid credentials provided.");
                return Unauthorized();
            }

            // STEP 3: LOGOUT - "Cancel your wristband"
            [HttpPost("logout")]
            public async Task<IActionResult> Logout([FromBody] LogoutRequest logoutRequest)
            {
                // Validation: "Please provide refresh token"
                if (!ModelState.IsValid)
                {
                    return BadRequest("Please provide all required fields");
                }

                // Find the refresh token: "Let me find that backup wristband..."
                var refreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == logoutRequest.RefreshToken && !rt.IsRevoked);

                if (refreshToken == null)
                {
                    return BadRequest("Invalid refresh token");
                }

                // Cancel it: "Marking your wristband as invalid"
                refreshToken.IsRevoked = true;
                _context.RefreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync();

                return Ok("User logged out successfully");
            }

            // STEP 4: REFRESH TOKEN - "Exchange your backup wristband for a new one"
            [HttpPost("refresh-token")]
            public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
            {
                // Validation: "Please provide both tokens"
                if (!ModelState.IsValid)
                {
                    return BadRequest("Please provide all required fields");
                }

                var result = await VerifyAndGenerateTokenAsync(tokenRequest);

                return Ok(result);

            }

            // WRISTBAND EXCHANGER - "Check backup, give new main wristband"
            private async Task<AuthResponse> VerifyAndGenerateTokenAsync(TokenRequest tokenRequest)
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();

                // Find backup wristband: "Let me look up that backup..."
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

                // Check if backup is still valid: "Is your backup expired or cancelled?"
                if (storedToken.IsRevoked || storedToken.DateExpire < DateTime.UtcNow)
                {
                    return new AuthResponse()
                    {
                        IsSuccess = false,
                        Error = "Refresh token expired or revoked"
                    };
                }

                // Find the user: "Who does this backup belong to?"
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
                    // Check if main and backup wristbands match: "Do these belong together?"
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

                    // Success! Cancel old backup and give new wristbands
                    storedToken.IsRevoked = true;
                    _context.RefreshTokens.Update(storedToken);
                    await _context.SaveChangesAsync();

                    return await GenerateJWTTokenAsync(dbUser, null);
                }
                catch (SecurityTokenExpiredException)
                {
                    // Main wristband expired, but that's OK for refresh
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

                    // Cancel old backup and give new wristbands
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

            // THE WRISTBAND FACTORY - "Makes the actual wristbands"
            // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/generating-an-access-token?autoSkip=true&resume=false&u=57075649
            private async Task<AuthResponse> GenerateJWTTokenAsync(ApplicationUser user, RefreshToken rToken)
            {
                // Create unique ID for this token pair
                var jti = Guid.NewGuid().ToString();

                // What info goes on the wristband: "Name, ID, email, etc."
                var authClaims = new List<Claim>
            {
                //new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, jti) // Unique token ID
            };

                // Link refresh token to this JWT
                if (rToken != null)
                {
                    rToken.JwtId = jti;
                }

                // Add user's roles: "User, Admin, etc."
                // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/adding-role-claims-to-tokens?autoSkip=true&resume=false&u=57075649 
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                // Sign the token with our secret key: "Official club stamp"
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

                // Create the actual wristband
                var token = new JwtSecurityToken(
                    issuer: _configuration.GetValue<string>("JWT:Issuer"),        // Who made this
                    audience: _configuration.GetValue<string>("JWT:Audience"),    // Who it's for
                    expires: DateTime.UtcNow.AddMinutes(30),                      // When it expires
                    claims: authClaims,                                           // What's on it
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256) // Signed
                );

                var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

                // If we're refreshing, return the existing backup wristband
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

                // Create new backup wristband (refresh token)
                // https://www.linkedin.com/learning/asp-dot-net-core-token-based-authentication/generating-and-storing-refresh-tokens?autoSkip=true&resume=false&u=57075649
                var refreshToken = new RefreshToken()
                {
                    JwtId = jti,                                              // Links to the JWT
                    IsRevoked = false,                                        // Not cancelled yet
                    UserId = user.Id,                                         // Who owns it
                    DateAdded = DateTime.UtcNow,                              // When created
                    DateExpire = DateTime.UtcNow.AddMonths(6),                // Expires in 6 months
                    Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString(), // Random string
                };

                // Save backup wristband to database
                await _context.RefreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();

                // Return both wristbands
                var response = new AuthResponse()
                {
                    Token = jwtToken,                  // Main wristband (30 minutes)
                    RefreshToken = refreshToken.Token, // Backup wristband (6 months)
                    ExpiresAt = token.ValidTo

                };

                return response;

            }
        }
    }

    // MISSING FOR PRODUCTION:

    // ✔️✔️✔️ 1. RATE LIMITING - "Don't let someone try to guess passwords 1000 times per second"
    // Add: builder.Services.AddRateLimiter()

    // 2. LOGGING - "Keep track of who's trying to log in"
    // Add: _logger.LogWarning("Failed login attempt for {Email}", loginRequest.EmailAddress);

    // 3. EMAIL VERIFICATION - "Make sure the email is real"
    // Add: Email confirmation flow

    // 4. PASSWORD RESET - "Forgot password" functionality
    // Add: Password reset endpoints

    // 5. CORS CONFIGURATION - "Which websites can call your API"
    // Add: builder.Services.AddCors()

    // 6. INPUT VALIDATION - "Make sure passwords are strong"
    // Add: Password complexity requirements

    // ✔️✔️✔️ 7. HTTPS ONLY - "Force encrypted connections"
    // Already done with RequireHttpsMetadata

    // 8. SECRETS MANAGEMENT - "Don't store secrets in code"
    // Move JWT:SecretKey to Azure Key Vault or environment variables

}
