using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OCB_API.Models;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OCB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLoginController : ControllerBase
    {
        private readonly OCBContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        public UserLoginController(OCBContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<string>>> Login([FromBody] Login userLogin)
        {
            var existingUser = await _context.UserLoginTable.SingleOrDefaultAsync(u => u.UserName == userLogin.UserName && u.Password == userLogin.Password);

            if (existingUser != null)
            {
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );
                var dataUser = _context.UserTable.SingleOrDefault(z => z.Id == _context.UserLoginTable.SingleOrDefault(x => x.UserName == userLogin.UserName).UserId);
                var roleUser = _context.UserLoginTable.SingleOrDefault(x => x.UserName == userLogin.UserName).RoleUser;
                return Ok(new ApiResponse<LoginRespone> 
                { 
                    Status = true, StatusCode = 200, Message = "Đăng nhập thành công!",
                    Data = new LoginRespone
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        data= dataUser,
                        role = roleUser
                    } 
                });
            }

            return Ok(new ApiResponse<string> { Status = false, StatusCode = 401, Message = "Đăng nhập thất bại, vui lòng kiểm tra lại tên đăng nhập và mật khẩu!" });
        }

        // POST: api/register-info
        [HttpPost("register-info")]
        public async Task<ActionResult<ApiResponse<InfoRegister>>> PostUser(InfoRegister info)
        {

            if (!EmailExists(info.Email))
            {
                _context.InfoRegisterTable.Add(info);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string> { Status = true, StatusCode = 200, Message = "Đã đăng kí nhận thông tin thành công!" });

            } else
            {
                return Ok(new ApiResponse<string> { Status = true, StatusCode = 400, Message = "Email đã được đăng kí, vui lòng chọn email khác!" });
            } 
        }

        private bool EmailExists(string email)
        {
            return _context.InfoRegisterTable.Any(e => e.Email == email);
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<string>>> PostForgotPassword(string email)
        {
            // Validate the incoming user contact data
            if (!_context.UserTable.Any(e => e.Email == email))
            {
                return Ok(new ApiResponse<string> { Status = true, StatusCode = 400, Message = "Tài khoản với địa chỉ email này không tồn tại!" });

            }
            else
            {
                var user = await _context.UserTable.Where(x=> x.Email == email).FirstOrDefaultAsync();

                if (user != null)
                {
                    var userLogin = await _context.UserLoginTable.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
                    userLogin.Password = RandomString(6);
                    _context.Entry(userLogin).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    var emailBody = $"Tài khoản đăng nhập: {userLogin.UserName}\n Mật khẩu mới của bạn là: {userLogin.Password}";

                    _emailService.SendContactEmail("Admin OCB", email, "OCB thông tin mật khẩu mới", emailBody);

                    return Ok(new ApiResponse<object> { Status = true, StatusCode = 200, Message = "Gửi thông tin thành công! Vui lòng kiểm tra email!", Data = null });

                }
                else
                {
                    return Ok(new ApiResponse<string> { Status = true, StatusCode = 400, Message = "Tài khoản với địa chỉ email này không tồn tại!" });
                }

                
            }

        }

        private string RandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return  new String(stringChars);
        }

        [HttpPost("contact-info")]
        public async Task<ActionResult<ApiResponse<InfoContact>>> PostInfoContact([FromBody] InfoContact infoContact)
        {
            // Validate the incoming user contact data
            if (_context.InfoContactTable.Any(e => e.Email == infoContact.Email))
            {
                return Ok(new ApiResponse<string> { Status = true, StatusCode = 400, Message = "Email đã được đăng kí, vui lòng chọn email khác!" });
            
            } else
            {

                _context.InfoContactTable.Add(infoContact);
                await _context.SaveChangesAsync();

                var emailBody = $"Tên liên hệ: {infoContact.Name}\nEmail: {infoContact.Email}\nChức vụ: {infoContact.Subject}\nTin nhắn: {infoContact.Message}";

                _emailService.SendContactEmail(infoContact.Name,infoContact.Email, "OCB gửi thông tin liên hệ", emailBody);

                return Ok(new ApiResponse<object> { Status = true, StatusCode = 200, Message = "Gửi thông tin thành công! Vui lòng kiểm tra email!", Data = null });
            }

        }
    }

    public class LoginRespone
    {
        public string token { get; set; }
        public User? data { get; set; }
        public string role { get; set; }
    }
}
