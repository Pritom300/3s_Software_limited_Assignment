using API.DTOs;
using API.Helpers;
using AutoMapper;
using Azure.Core;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Cryptography;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtService _jwtService;
        private readonly IMapper _mapper;

        public AuthController(IUnitOfWork unitOfWork, JwtService jwtService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        //api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("Email already in use");

            //Hash password
            var salt = RandomNumberGenerator.GetBytes(16);
            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: dto.Password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            var user = _mapper.Map<User>(dto);
            user.PasswordHash = hashed;
            user.PasswordSalt = Convert.ToBase64String(salt);

            //Refresh tokens mechanism
            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpiryTime = refreshToken.Expires;

            //Refresh tokens mechanism end


            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            var token = _jwtService.GenerateToken(user);

            //return Ok(new { Token = token });
            return Ok(new
            {
                AccessToken = token,
                RefreshToken = refreshToken.Token
            });
        }

        // api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            var salt = Convert.FromBase64String(user.PasswordSalt!);
            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: dto.Password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 32));

            if (hashed != user.PasswordHash)
                return Unauthorized("Invalid email or password");

            
            // New refresh token
            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpiryTime = refreshToken.Expires;

            // End refresh token

            await _unitOfWork.CompleteAsync();

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                AccessToken = token,
                RefreshToken = refreshToken.Token
            });

            //return Ok(new { Token = token });
        }


        //Refresh Token

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var user = (await _unitOfWork.Users.GetAllAsync())
                .FirstOrDefault(u => u.RefreshToken == refreshToken);

            if (user == null || !_jwtService.ValidateRefreshToken(user, refreshToken))
                return Unauthorized(new { message = "Invalid refresh token" });

            var newAccessToken = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenExpiryTime = newRefreshToken.Expires;
            await _unitOfWork.CompleteAsync();

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            });
        }
    }
}

