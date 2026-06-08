using AutoMapper;
using FitnessPlatform.API.Data;
using FitnessPlatform.API.DTOs;
using FitnessPlatform.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly AppDbContext _context;

         private readonly IMapper _mapper;
        public AuthController(TokenService tokenService, UserManager<AppUser> userManager, IMapper mapper, AppDbContext context)


        {
            _tokenService = tokenService;
            _userManager = userManager;
            _mapper = mapper;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if(user == null ) return Unauthorized("Invalid email");

            var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if(!passwordValid) return Unauthorized("Invalid password");

            var userDto = await CreateUserDtoWithTokensAsync(user);

            return Ok(userDto);
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await _userManager.FindByEmailAsync(registerDto.Email) != null)
                return BadRequest("Email already in use");

            var user = new AppUser
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                UserName = registerDto.Email
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if(!result.Succeeded) return BadRequest(result.Errors);

            // Opcional: atribuir role
            await _userManager.AddToRoleAsync(user, "Client");

            var userDto = await CreateUserDtoWithTokensAsync(user);

            return Ok(userDto);

        }

        [HttpPost("refresh")]
        public async Task<ActionResult<UserDto>> Refresh(RefreshTokenDto dto)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .SingleOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

            if(storedToken == null) return Unauthorized("Invalid refresh token");
            if(!storedToken.IsActive) return Unauthorized("Refresh token expired or revoked");

            // Rotação: revoga o token usado e emite um novo par
            var newRefreshToken = _tokenService.GenerateRefreshToken(storedToken.UserId);

            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.ReplacedByToken = newRefreshToken.Token;

            _context.RefreshTokens.Add(newRefreshToken);

            var accessToken = await _tokenService.GenerateAccessToken(storedToken.User);

            await _context.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(storedToken.User);
            userDto.Token = accessToken;
            userDto.RefreshToken = newRefreshToken.Token;

            var roles = await _userManager.GetRolesAsync(storedToken.User);
            userDto.Role = roles.FirstOrDefault() ?? string.Empty;

            return Ok(userDto);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(RefreshTokenDto dto)
        {
            var storedToken = await _context.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

            if(storedToken == null) return NotFound("Refresh token not found");
            if(!storedToken.IsActive) return BadRequest("Refresh token already inactive");

            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<UserDto> CreateUserDtoWithTokensAsync(AppUser user)
        {
            var userDto = _mapper.Map<UserDto>(user);

            userDto.Token = await _tokenService.GenerateAccessToken(user);

            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            userDto.RefreshToken = refreshToken.Token;

            var roles = await _userManager.GetRolesAsync(user);
            userDto.Role = roles.FirstOrDefault() ?? string.Empty;

            return userDto;
        }
    }
}
