using API.DTOs;
using API.Helpers;
using Domain.Entities;
using Domain.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
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

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            var token = _jwtService.GenerateToken(user);

            return Ok(new { Token = token });
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

            var token = _jwtService.GenerateToken(user);
            return Ok(new { Token = token });
        }
    }
}
