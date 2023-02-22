using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MovieReactAPI.Configuration;
using MovieReactAPI.DTO_s;
using MovieReactAPI.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MovieReactAPI.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManeger;
        private readonly ApplicationDbContext context;
        private readonly JWTConfiguration jWTConfiguration;
        private readonly IMapper mapper;
        public AccountController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManeger,
           IOptions<JWTConfiguration> options,
           ApplicationDbContext context,
           IMapper mapper)
        {
            this.userManager = userManager;
            this.signInManeger = signInManeger;
            this.jWTConfiguration = options.Value;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpPost("create")]
        public async Task<ActionResult<AuthenticationResponse>> Create([FromBody] UserCredentionals userCredentionals)
        {
            var user = new IdentityUser { UserName = userCredentionals.Email, Email = userCredentionals.Email };

            var result = await userManager.CreateAsync(user, userCredentionals.Password);

            if (result.Succeeded)
            {
                return await BuildToken(userCredentionals);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody]
        UserCredentionals userCredentionals)
        {
            var result = await signInManeger.PasswordSignInAsync(userCredentionals.Email, userCredentionals.Password,
                isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return await BuildToken(userCredentionals);
            }
            else
            {
                return BadRequest("Incorrect Passsword");
            }
        }

        //[HttpGet("profile")]
        //[Authorize]
        //public async Task<ActionResult<CredentionalsDTO>> GetProfile()
        //{

        //}

        [HttpGet("listUsers")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyConstants.AdminPolicy)]
        public async Task<ActionResult<List<UserDTO>>> GetListUsers([FromQuery] PaginationDTO paginationDTO)
        {
            var quariable = context.Users.AsQueryable();

            await HttpContext.InsertParametersPaginationInHeader(quariable);
            var users = await quariable.OrderBy(x => x.Email).Paginate(paginationDTO)
                .ToListAsync();

            return mapper.Map<List<UserDTO>>(users);
        }

        [HttpPost("makeAdmin")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyConstants.AdminPolicy)]
        public async Task<ActionResult> MakeAdmin([FromBody] string userID)
        {
            var user = await userManager.FindByIdAsync(userID);
            await userManager.AddClaimAsync(user!, new Claim("role", "admin"));
            return NoContent();
        }

        [HttpPost("removeAdmin")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyConstants.AdminPolicy)]
        public async Task<ActionResult> RemoveAdmin([FromBody] string userID)
        {
            var user = await userManager.FindByIdAsync(userID);
            await userManager.RemoveClaimAsync(user!, new Claim("role", "admin"));
            return NoContent();
        }

        private async Task<AuthenticationResponse> BuildToken(UserCredentionals userCredentionals)
        {
            var claims = new List<Claim>();

            claims.Add(new Claim("email", userCredentionals.Email));

            var user = await userManager.FindByNameAsync(userCredentionals.Email);
            var claimsList = await userManager.GetClaimsAsync(user!);

            foreach (var claim in claimsList)
            {
                claims.Add(claim);
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jWTConfiguration.key));
            var creads = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(3);
            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: creads);

            return new AuthenticationResponse()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }

    }
}
