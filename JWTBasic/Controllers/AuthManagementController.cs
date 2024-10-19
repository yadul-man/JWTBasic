using JWTBasic.Configurations;
using JWTBasic.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWTBasic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthManagementController : ControllerBase
    {
        private readonly ILogger<AuthManagementController> _logger;

        /*
            UserManager<TUser> is a service provided by ASP.NET Core Identity that allows you to manage users and handle common operations such as creating, finding, updating, and deleting user records. 
            eg:- var newUser = new IdentityUser { UserName = "john_doe", Email = "john@example.com" };
                 var result = await _userManager.CreateAsync(newUser, "Password123!");
        */
        private readonly UserManager<IdentityUser> _userManager;

        private readonly JWTConfig _jwtConfig;

        public AuthManagementController(ILogger<AuthManagementController> logger, UserManager<IdentityUser> userManager, IOptionsMonitor<JWTConfig> optionsMonitor)
        {
            _logger = logger;
            _userManager = userManager;

            /*
                IOptionsMonitor<JWTConfig> is used to access the JWTConfig settings, which contain the secret key used to sign JWT tokens. These settings are retrieved from the appsettings.json file. 
            */
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        [HttpPost]
        [Route(template: "Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDTO userRegistrationRequestDTO)
        {
            /*
                If incoming request is valid:
                Check if email already exists, if not create a new user and generate token
            */
            if (ModelState.IsValid)
            {
                var emailExist = await _userManager.FindByEmailAsync(userRegistrationRequestDTO.Email);
                if (emailExist != null)
                {
                    return BadRequest(error: "Email already exists.");
                }

                var newUser = new IdentityUser()
                {
                    Email = userRegistrationRequestDTO.Email,
                    UserName = userRegistrationRequestDTO.Email
                };

                var isCreated = await _userManager.CreateAsync(newUser, userRegistrationRequestDTO.Password);
                if (isCreated.Succeeded)
                {
                    // Generate token
                    var token = GenerateJWTToken(newUser);

                    return Ok(new RegistrationRequestResponse()
                    {
                        Result = true,
                        Token = token
                    });
                }

                return BadRequest(error: new {
                    message = "Error creating the user, please try again later.",
                    errors = isCreated.Errors.Select(x => x.Description).ToList() 
                });
            }

            return BadRequest(error: "Invalid request payload.\n" + ModelState);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDTO userLoginRequestDTO)
        {
            /*
                If incoming request is valid:
                Check if credentials are valid, generate token and send it as response
            */
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(userLoginRequestDTO.Email);
                if (existingUser == null)
                {
                    return BadRequest(error: "Invalid credentials.");
                }

                var passwordValid = await _userManager.CheckPasswordAsync(existingUser, userLoginRequestDTO.Password);
                if (passwordValid)
                {
                    var token = GenerateJWTToken(existingUser);
                    return Ok(new LoginRequestResponse()
                    {
                        Token = token,
                        Result = true
                    });
                }
            }

            return BadRequest(error: "Invalid request payload.\n" + ModelState);
        }

        /*
            Generates a JWT (JSON Web Token) for a user by using the JwtSecurityTokenHandler class. It starts by encoding a secret key from the configuration. 
            Then, it creates claims that store user-specific information, the user's ID, email, and a unique token ID (Jti). 
            These claims are packed into a ClaimsIdentity object. The token expiration time is 4 hours from the current time.
            The token is signed using the HMAC SHA256 algorithm, with the secret key encoded earlier.
            Once all this information (claims, expiration, signing credentials) is packed into a SecurityTokenDescriptor, the CreateToken method generates the token, which is then written as a string in JWT format using the WriteToken method. 
        */
        private string GenerateJWTToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(type: "Id", value: user.Id),
                    new Claim(type: JwtRegisteredClaimNames.Sub, value: user.Email),
                    new Claim(type: JwtRegisteredClaimNames.Email, value: user.Email),
                    new Claim(type: JwtRegisteredClaimNames.Jti, value: Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(4),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), algorithm: SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);
            return jwtToken;
        }
    }
}
