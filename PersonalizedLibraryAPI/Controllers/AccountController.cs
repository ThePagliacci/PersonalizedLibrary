using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PersonalizedLibraryAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonalizedLibraryAPI.DTOs.Account;
using PersonalizedLibraryAPI.Repository.IRepository;
using System.IdentityModel.Tokens.Jwt;

namespace PersonalizedLibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenServiceRepository _tokenServiceRepository;
        public AccountController(UserManager<AppUser> userManager, 
                                ITokenServiceRepository tokenServiceRepository,
                                SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenServiceRepository = tokenServiceRepository;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if(!ModelState.IsValid) return BadRequest(ModelState);
                var appUser = new AppUser
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email
                };
                var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);
                if(createdUser.Succeeded)
                {
                    return Ok(
                        new NewUserDto
                        {
                            UserName = appUser.UserName,
                            Email = appUser.Email,
                            Token = _tokenServiceRepository.CreateToken(appUser)
                        }
                    );
                }
                else return StatusCode(500, createdUser.Errors); 
            }catch(Exception e)
            {
                return StatusCode(500, e);
            }
        }
     
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.UserName);

            if(user == null) return Unauthorized("Yetkisiz Kullanıcı Adı!");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if(!result.Succeeded) return Unauthorized("kullanıcı adı bulunamadı ve/veya şifre yanlış");

            return Ok(
                new NewUserDto
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = _tokenServiceRepository.CreateToken(user)
                }
            );
        }

        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] string token)
        {
            try
            {
                var principal = _tokenServiceRepository.ValidateToken(token);
                return Ok(new
                {
                    IsValid = true,
                    Email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value,
                    Username = principal.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { IsValid = false, Message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            await _signInManager.SignOutAsync();

            return Ok("başarıyla çıkış yapıldı");
        }
    }
}