
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            this.tokenService = tokenService;
            this.context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO userRegisterData)
        {
            using var hmac = new HMACSHA512();
            var user = new AppUser();
            if (await userExists(userRegisterData.username) != true)
            {
                user = new AppUser
                {
                    UserName = userRegisterData.username.ToLower(),
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userRegisterData.password)),
                    PaasswordSalt = hmac.Key
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();
                return  new UserDTO
                {
                    Username = user.UserName,
                    Token = tokenService.CreateToken(user)
                };

            }
            else
            {
                return BadRequest("Username is taken");
            }


        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO logindto)
        {
            var user = await context.Users.FirstOrDefaultAsync(q => q.UserName == logindto.username);
            if (user == null) return Unauthorized("invalid username");
            using var hmac = new HMACSHA512(user.PaasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(logindto.password));
            for(int i = 0 ; i< computedHash.Length; i++)
            {
                if(computedHash[i]!= user.PasswordHash[i])
                {
                    return Unauthorized("invalid password") ;
                }
            }
             return  new UserDTO
                {
                    Username = user.UserName,
                    Token = tokenService.CreateToken(user)
                };
        }
        private async Task<bool> userExists(string username)
        {
            return await context.Users.AnyAsync(q => q.UserName == username.ToLower());
        }

    }
}