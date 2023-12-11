using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController:BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService tokenService;
        private readonly IMapper _mapper;

        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            _mapper = mapper;
            this.tokenService = tokenService;
            _context = context;
            
        }

        [HttpPost("register")] //POST:api/account/register
        public async Task<ActionResult<UserDto>>Register(RegisterDto registerDto)
        {

            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken"); // register the user if they do not already exist

            var user = _mapper.Map<AppUser>(registerDto);
            using var hmac = new HMACSHA512();

           //check the password against waht we have in the database
   
            user.UserName = registerDto.Username.ToLower();
            //need to calculate a hashing algorithm from a password
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;
        
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                Token = tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => 
            x.UserName == loginDto.Username);

             if(user == null) return Unauthorized("invalide username");

             using var hmac = new HMACSHA512(user.PasswordSalt);

             var ComputeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i = 0; i < ComputeHash.Length; i++) 
            {
                if (ComputeHash[i] != user.PasswordHash[i]) return Unauthorized("invalide password");
            }

            return new UserDto //properties returned when a user signs in
            {
                Username = user.UserName,
                Token = tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        private async Task<bool> UserExists(string username)
        {
          return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}